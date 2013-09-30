using System;
using System.Collections.Generic;
using System.Linq;
using Clientele.Core.Persistance;
using Clientele.DocumentTracking.ApplicationService.Commands;
using Clientele.DocumentTracking.DataModel.Model;
using Hermes.Logging;

namespace Clientele.DocumentTracking.ApplicationService
{
    public class DocumentTrackingService
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (DocumentTrackingService));

        private readonly IRepository<Document> documents;

        public DocumentTrackingService(IUnitOfWork unitOfWork)
        {
            documents = unitOfWork.GetRepository<Document>();
        }

        public void Handle(RegisterDocumentReceived command)
        {
            Logger.Info("Registering Document Received");

            var document = GetDocument(command.DocumentId);
            document.Source = command.Source;

            document.ActivityHistory.Add(new DocumentActivity
            {
                ActivityDate = command.OccurredAt,
                Description = "Document received.",
                DocumentId = command.DocumentId,
                Source = command.Source,
                User = command.User
            });

            documents.Add(document);
        }

        public void Handle(UpdateDocumentMetaData command)
        {
            Logger.Info("Updating Document Metadata");

            Document document = GetDocument(command.DocumentId);

            if (String.IsNullOrWhiteSpace(document.Source))
            {
                document.Source = command.Source;
            }

            document.IfaNumber = command.IfaNumber;
            document.PolicyNumber = command.PolicyNumber;

            document.ActivityHistory.Add(new DocumentActivity
            {
                ActivityDate = command.OccurredAt,
                Description = "Document information updated.",
                DocumentId = command.DocumentId,
                Source = command.Source,
                User = command.User
            });
        }


        public void Handle(RegisterDocumentActivity command)
        {
            Logger.Info("Registering Document Activity");

            Document document = GetDocument(command.DocumentId);

            if (String.IsNullOrWhiteSpace(document.Source))
            {
                document.Source = command.Source;
            }

            document.ActivityHistory.Add(new DocumentActivity
            {
                ActivityDate = command.OccurredAt,
                DocumentId = command.DocumentId,
                Description = command.Description,
                Source = command.Source,
                User = command.User
            });
        }

        private Document GetDocument(Guid documentId)
        {
            var document = documents.SingleOrDefault(d => d.Id == documentId);

            if (document == null)
            {
                document = new Document
                {
                    Id = documentId,
                };

                documents.Add(document);
            }

            return document;
        }
    }
}
