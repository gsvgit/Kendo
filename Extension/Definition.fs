namespace Extension

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.InterfaceGenerator

module Definition =
    let (=@) (name: string) t =
        let names = name.Split '.'
        names.[names.Length - 1] =@ t
        |> WithSetterInline (System.String.Format("void ($this.{0} = $value)", name))
        |> WithGetterInline (System.String.Format("$this.{0}", name))

    let TabStrip =
        Class "kendo.ui.TabStrip"
        |+> [Constructor T<Dom.Element>]
        |+> Protocol [
            "options.animation.open" =@ T<bool>
            "options.animation.close" =@ T<bool>
            "options.select" =@ T<obj>
        ]

    let FieldType =
        Pattern.Config "kendo.ui.FieldType" {
            Required = ["editable", T<bool>; "type", T<string>]
            Optional = []
        }

    let Model =
        let model = Type.New()
        Pattern.Config "kendo.ui.Model" {
            Required = ["fields", T<obj>]
            Optional = ["id", T<string>]
        }
        |=> model
        |+> ["define" => T<obj> ^-> model]

    let Schema =
        Pattern.Config "kendo.ui.Schema" {
            Required = ["model", Model.Type]
            Optional = []
        }

    let Attributes =
        Pattern.Config "kendo.ui.Attributes" {
            Required = ["class", T<string>]
            Optional = []
        }

    let Command =
        Pattern.Config "kendo.ui.Command" {
            Required = ["name", T<string>; "click", T<obj> ^-> T<unit>]
            Optional = []
        }

    let Column =
        Generic / fun t ->
            Pattern.Config "kendo.ui.Column" {
                Required = []
                Optional =
                    [
                        "field", T<string>
                        "title", T<string>
                        "command", Command.Type
                        "editor", T<JQuery.JQuery> * T<obj> ^-> T<unit>
                        "filterable", T<bool>
                        "format", T<string>
                        "sortable", T<bool>
                        "template", t ^-> T<string>
                        "width", T<int>
                        "attributes", Attributes.Type
                    ]
            }

    let DataSource =
        Generic / fun t ->
            Pattern.Config "kendo.data.DataSource" {
                Required = []
                Optional =
                    [
                        "data", Type.ArrayOf t
                        "pageSize", T<int>
                        "aggregate", T<obj>
                        "filter", T<obj>
                        "group", T<obj>
                        "onChange", T<unit -> unit>
                        "onError", T<obj -> unit>
                        "page", T<int>
                        "schema", Schema.Type
                    ]
            }

    let DropDownValue =
        Generic / fun t ->
            Pattern.Config "kendo.ui.DropDownValue" {
                Required =
                    [
                        "text", T<string>
                        "value", t
                    ]
                Optional = []
            }

    let DropDownConfiguration =
        Generic / fun t ->
            Pattern.Config "kendo.ui.DropDownConfiguration" {
                Required =
                    [
                        "dataTextField", T<string>
                        "dataValueField", T<string>
                        "dataSource", Type.ArrayOf (DropDownValue t)
                    ]
                Optional = []
            }

    let DropDownList =
        Generic / fun t ->
            Class "kendo.ui.DropDownList"
            |+> [
                Constructor (T<Dom.Element> * DropDownConfiguration t)
                Constructor (T<JQuery.JQuery> * DropDownConfiguration t)
            ]

    let ToolButton =
        Pattern.Config "kendo.ui.ToolbarElement" {
            Required = []
            Optional =
                [
                    "template", T<obj> //T<unit> ^-> T<obj>
                    "name", T<string>
                ]
        }

    let GridConfiguration =
        Generic / fun t ->
            Pattern.Config "kendo.ui.GridConfiguration" {
                Required = []
                Optional =
                    [
                        "columns", Type.ArrayOf (Column t)
                        "dataSource", (DataSource t).Type
                        "selectable", T<string>
                        "change", T<obj -> unit>
                        "resizable", T<bool>
                        "filterable", T<bool>
                        "reorderable", T<bool>
                        "editable", T<bool>
                        "groupable", T<bool>
                        "scrollable", T<bool>
                        "sortable", T<bool>
                        "pageable", T<obj>
                        "toolbar", Type.ArrayOf ToolButton
                    ]
            }

    let Grid =
        Generic / fun t ->
            Class "kendo.ui.Grid"
            |+> [
                Constructor T<Dom.Element>
                Constructor (T<Dom.Element> * GridConfiguration t)
            ]
            |+> Protocol [
                "select" => T<unit> ^-> T<obj>
                "dataItem" => T<obj> ^-> t
                "saveRow" => T<unit> ^-> T<unit>
            ]

    let WindowConfiguration =
        Pattern.Config "kendo.ui.WindowConfiguration" {
            Required =
                [
                    "title", T<string>
                    "width", T<string>
                    "close", T<unit> ^-> T<unit>
                    "actions", Type.ArrayOf T<string>
                ]
            Optional =
                [
                    "animation", T<bool>
                    "draggable", T<bool>
                    "modal", T<bool>
                    "resizable", T<bool>
                ]
        }

    let Window =
        Class "kendo.ui.Window"
        |+> [
            Constructor (T<Dom.Element> * WindowConfiguration)
        ]
        |+> Protocol [
            "open" => T<unit> ^-> T<unit>
            "destroy" => T<unit> ^-> T<unit>
            "center" => T<unit> ^-> T<unit>
            "bind" => (T<string> * (T<unit> ^-> T<unit>)) ^-> T<unit>
        ]

    let MenuConfiguration =
        Pattern.Config "kendo.ui.MenuConfiguration" {
            Required = ["select", T<obj> ^-> T<unit>]
            Optional = ["animation.open.duration", T<int>]
        }

    let Menu =
        Class "kendo.ui.Menu"
        |+> [
            Constructor (T<Dom.Element> * MenuConfiguration)
        ]

    let kResource name file =
        sprintf "http://cdn.kendostatic.com/2013.3.1119/%s" file
        |> Resource name

    let KendoAPI = kResource "KendoAPI" "js/kendo.web.min.js"
    let ThemeCommon = kResource "ThemeCommon" "styles/kendo.common.min.css"
    let Jquery = Resource "Jquery" "http://code.jquery.com/jquery-1.10.2.min.js"

    let Kendo =
        Class "kendo"
        |+> [
            "culture" => T<string> ^-> T<unit>
            "template" => T<string> ^-> T<obj>
            "toString" => T<obj> * T<string> ^-> T<string>
        ]
        |> WithSourceName "Kendo"
        |> Requires [Jquery; KendoAPI; ThemeCommon]

    let Assembly =
        Assembly [
            Namespace "WebSharper.Kendo.Extension" [
                Generic - DataSource
                Kendo
            ]
            Namespace "WebSharper.Kendo.Extension.UI" [
                Attributes
                TabStrip
                Command
                Model
                FieldType
                Schema
                ToolButton
                WindowConfiguration
                Window
                MenuConfiguration
                Menu
                Generic - Column
                Generic - GridConfiguration
                Generic - Grid
                Generic - DropDownValue
                Generic - DropDownConfiguration
                Generic - DropDownList
            ]
            Namespace "WebSharper.Kendo.Dependencies" [
                Jquery
                KendoAPI
            ]
            Namespace "WebSharper.Kendo.Culture" [
                ThemeCommon
                kResource "English" "js/cultures/kendo.culture.en-CA.min.js"
                kResource "French" "js/cultures/kendo.culture.fr-CA.min.js"
            ]
            Namespace "WebSharper.Kendo.Themes" [
                kResource "Default" "styles/kendo.default.min.css"
                kResource "Silver" "styles/kendo.silver.min.css"
            ]
        ]

open IntelliFactory.WebSharper.InterfaceGenerator

[<Sealed>]
type Extension() =
    interface IExtension with
        member x.Assembly = Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()