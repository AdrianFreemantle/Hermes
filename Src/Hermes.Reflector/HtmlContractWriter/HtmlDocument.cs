using System;
using System.Text;

namespace Hermes.Reflector.HtmlContractWriter
{
    public class HtmlDocument
    {
        private const string TableTag = "table";
        private const string TableHeaderTag = "thead";
        private const string TableHeadingTag = "th";
        private const string TableBodyTag = "tbody";
        private const string TableRowTag = "trow";
        private const string AnchorTag = "a";
        private const string DivTag = "div";
        private const string ListTag = "ul";
        private const string ListItemTag = "li";
        private const string HorizontalRowTag = "hr";
        private const string HeadingOneTag = "h1";
        private const string HeadingTwoTag = "h2";

        readonly StringBuilder sb = new StringBuilder();

        public HtmlDocument(string title)
        {
            CreateHtmlHeader(title);
        }

        private void CreateHtmlHeader(string title)
        {
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine(String.Format("<title>{0}</title>", title));
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
        }

        private void AppendHtmlFooter()
        {
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
        }

        public void HeadingOne(string value)
        {
            HtmlSection.AddSection(sb, HeadingOneTag).AddContent(value).Dispose();
        }

        public void HeadingTwo(string value)
        {
            HtmlSection.AddSection(sb, HeadingTwoTag).AddContent(value).Dispose();
        }

        public void HorizontalRow()
        {
            HtmlSection.AddTag(sb, HorizontalRowTag);
        }

        public HtmlSection Div()
        {
            return HtmlSection.AddSection(sb, DivTag);
        }

        public HtmlSection List()
        {
            return HtmlSection.AddSection(sb, ListTag);
        }

        public HtmlSection ListItem()
        {
            return HtmlSection.AddSection(sb, ListItemTag);
        }

        public HtmlSection TableHeading()
        {
            return HtmlSection.AddSection(sb, TableHeadingTag);
        }

        public HtmlSection TableBody()
        {
            return HtmlSection.AddSection(sb, TableBodyTag);
        }

        public HtmlSection TableRow()
        {
            return HtmlSection.AddSection(sb, TableRowTag);
        }

        public HtmlSection Anchor()
        {
            return HtmlSection.AddSection(sb, AnchorTag);
        }

        public HtmlSection Table()
        {
            return HtmlSection.AddSection(sb, TableTag);
        }

        public HtmlSection TableHeader()
        {
            return HtmlSection.AddSection(sb, TableHeaderTag);
        }

        public string ToDocument()
        {
            AppendHtmlFooter();
            return ToString();
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }
}