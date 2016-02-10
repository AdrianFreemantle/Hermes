using System;
using System.Text;

namespace Hermes.Reflector.HtmlContractWriter
{
    public class HtmlSection : IDisposable
    {
        private const string OpenTag = "<";
        private const string CloseTag = ">";
        private const string TermindateTag = "</";

        private static string indentation = "    ";
        private static string currentIndentation = indentation;

        private readonly StringBuilder sb;
        private readonly string type;

        private HtmlSection(StringBuilder sb, string type)
        {
            this.sb = sb;
            this.type = type;
        }

        public static HtmlSection AddSection(StringBuilder sb, string type)
        {
            var section = new HtmlSection(sb, type);
            section.OpenSection();
            return section;
        }

        public static void AddTag(StringBuilder sb, string tag)
        {
            sb.AppendLine(String.Format("{0}{1}{2}{3}{4}", currentIndentation, indentation, OpenTag, tag, CloseTag));
        }

        public HtmlSection AddContent(string text)
        {
            sb.AppendLine(String.Format("{0}{1}", currentIndentation, text));
            return this;
        }

        private void TerminateSection()
        {
            currentIndentation = currentIndentation.Remove(0, indentation.Length);
            sb.AppendLine(String.Format("{0}{1}{2}{3}", currentIndentation, TermindateTag, type, CloseTag));
        }

        private void OpenSection()
        {
            sb.AppendLine(String.Format("{0}{1}{2}{3}", currentIndentation, OpenTag, type, CloseTag));
            currentIndentation += indentation;
        }

        public void Dispose()
        {
            TerminateSection();
        }
    }
}