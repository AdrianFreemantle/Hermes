using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hermes.Reflector.HtmlContractWriter
{
    public class HtmlContractWriter : IContractWriter
    {
        private readonly string outputPath;

        public HtmlContractWriter(string outputPath)
        {
            this.outputPath = outputPath;

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Directory.CreateDirectory(outputPath);
        }

        public void WriteDetails(MessageType messageType, ICollection<Type> contractTypes, ICollection<HandlerDetail> handlers, ICollection<MessageOriginator> originators)
        {
            HtmlDocument document = new HtmlDocument(contractTypes.First().FullName);

            document.HeadingOne(contractTypes.First().FullName);

            document.HorizontalRow();
            document.HeadingTwo("Contracts");

            using (document.Div())
            {
                using (document.List())
                {
                    foreach (var contractType in contractTypes)
                    {
                        using (document.ListItem().AddContent(contractType.FullName)) { }
                    }
                }
            }

            document.HorizontalRow();
            document.HeadingTwo("Handlers");

            using (document.Div())
            {
                using (document.List())
                {
                    foreach (HandlerDetail handlerDetail in handlers)
                    {
                        using (document.ListItem().AddContent(handlerDetail.HandlerType.FullName)) { }
                    }
                }
            }

            document.HorizontalRow();
            document.HeadingTwo("Originators");

            using (document.Div())
            {
                using (document.List())
                {
                    foreach (MessageOriginator messageOriginator in originators)
                    {
                        var content = String.Format("{0}.{1}: {2}({3} m);", messageOriginator.OriginatorType.FullName, messageOriginator.OriginatorMethod.Name, messageOriginator.BusMethod.Name, messageOriginator.MessageType.Name);
                        using (document.ListItem().AddContent(content)) { }
                    }
                }
            }

            var html = document.ToDocument();

            var fileName = Path.Combine(Environment.CurrentDirectory, outputPath, contractTypes.First().FullName + ".html");

            File.WriteAllText(fileName, html);
        }
    }
}
