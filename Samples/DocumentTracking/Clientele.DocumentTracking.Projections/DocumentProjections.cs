using System;
using System.Collections.Generic;
using System.Linq;

using Clientele.Core.Persistance;
using Clientele.DocumentTracking.DataModel.Model;

using DocumentTracking.Contracts;

using Hermes.Messaging;

namespace Clientele.DocumentTracking.Projections
{
    public class DocumentProjections 
        : IHandleMessage<DocumentReceived>
        , IHandleMessage<DocumentUpdated>
    {
        private readonly IRepository<Document> documents;

        public DocumentProjections(IRepository<Document> documents)
        {
            this.documents = documents;
        }

        public void Handle(DocumentReceived message)
        {
            var document = GetDocument(message.DocumentId);
            document.Source = message.Source;

            document.ActivityHistory.Add(new DocumentActivity
            {
                ActivityDate = message.OccurredAt,
                Description = "Document received.",
                DocumentId = message.DocumentId,
            });

            documents.Add(document);
        }

        public void Handle(DocumentUpdated message)
        {
            Document document = documents.Get(message.DocumentId);

            document.IfaNumber = message.IfaNumber;
            document.PolicyNumber = message.PolicyNumber;

            document.ActivityHistory.Add(new DocumentActivity
            {
                ActivityDate = message.OccurredAt,
                Description = "Document information updated.",
                DocumentId = message.DocumentId,
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
                    ActivityHistory = new HashSet<DocumentActivity>()
                };

                documents.Add(document);
            }

            return document;
        }
    }
}
