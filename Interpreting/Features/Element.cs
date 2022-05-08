namespace Point.Interpreting.Features
{
    public class Element
    {
        public static string[] Known =
        {
            "!--",
            "--",
            "!doctype",
            "a",
            "abbr",
            "acronym",
            "address",
            "applet",
            "area",
            "article",
            "aside",
            "audio",
            "b",
            "base",
            "basefont",
            "bb",
            "bdo",
            "big",
            "blockquote",
            "body",
            "br",
            "button",
            "canvas",
            "caption",
            "center",
            "cite",
            "code",
            "col",
            "colgroup",
            "colgroup",
            "command",
            "datagrid",
            "datalist",
            "input",
            "dd",
            "del",
            "details",
            "dfn",
            "dialog",
            "dir",
            "div",
            "dl",
            "dt",
            "em",
            "embed",
            "eventsource",
            "fieldset",
            "figcaption",
            "figure",
            "figcaption",
            "font",
            "footer",
            "form",
            "frame",
            "frameset",
            "frame",
            "h1",
            "h2",
            "h3",
            "h3",
            "h4",
            "h6",
            "head",
            "header",
            "hgroup",
            "hr",
            "html",
            "i",
            "iframe",
            "img",
            "input",
            "ins",
            "isindex",
            "kbd",
            "keygen",
            "label",
            "input",
            "legend",
            "fieldset",
            "li",
            "link",
            "map",
            "mark",
            "menu",
            "meta",
            "meter",
            "nav",
            "noframes",
            "frame",
            "noscript",
            "object",
            "ol",
            "optgroup",
            "option",
            "select",
            "optgroup",
            "datalist",
            "output",
            "p",
            "param",
            "pre",
            "progress",
            "q",
            "rp",
            "rt",
            "ruby",
            "s",
            "samp",
            "script",
            "section",
            "select",
            "small",
            "source",
            "span",
            "strike",
            "strong",
            "style",
            "sub",
            "sup",
            "table",
            "tbody",
            "td",
            "textarea",
            "tfoot",
            "th",
            "thead",
            "time",
            "title",
            "tr",
            "track",
            "audio",
            "video",
            "tt",
            "u",
            "ul",
            "var",
            "video",
            "wbr",
        };

        public static string[] Voided =
        {
            "area",
            "base",
            "br",
            "col",
            "embed",
            "hr",
            "img",
            "input",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr"
        };
        public readonly string tag;
        //public readonly string id;
        //public readonly List<string> classes;
        public readonly Dictionary<string, string> attributes;
        public readonly List<string> content;
        public Element(string tag, string id, List<string> classes, Dictionary<string, string> attributes, List<string> content)
        {
            this.tag = tag;
            //this.id = id;
            //this.classes = classes;
            this.attributes = attributes;
            if (id.Length > 0)
                this.attributes["id"] = id;

            if (classes.Count > 0)
                this.attributes["class"] = string.Join(" ", classes);

            this.content = content;

            if (!Known.Contains(tag.ToLower()))
                Global.Output(ConsoleColor.DarkYellow, $"[WARN] Unknown tag '{tag}', did you mean '{Known.FindNearestString(tag)}'?");
        }

        public override string ToString()
        {
            if (Voided.Contains(tag.ToLower()))
                return $"<{tag}{(attributes.Count > 0 ? " " : "")}{string.Join(" ", attributes.Select(x => x.Key + "=\"" + x.Value + "\""))} />";
            else
                return $"<{tag}{(attributes.Count > 0 ? " " : "")}{string.Join(" ", attributes.Select(x => x.Key + "=\"" + x.Value + "\""))}>{string.Join("", content)}</{tag}>";
        }
    }
}
