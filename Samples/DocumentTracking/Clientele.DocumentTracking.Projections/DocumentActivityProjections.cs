using System;
using System.Collections.Generic;
using System.Linq;

using Clientele.Core.Persistance;
using Clientele.DocumentTracking.DataModel.Model;

using DocumentTracking.Contracts;

using Hermes.Messaging;

namespace Clientele.DocumentTracking.Projections
{
    public class DocumentActivityProjections
        : IHandleMessage<DocumentImported>
            , IHandleMessage<DocumentViewed>
    {
        private readonly IRepository<DocumentActivity> documentActivities;
        private readonly IRepository<Document> documents;

        public DocumentActivityProjections(IRepository<DocumentActivity> documentActivities, IRepository<Document> documents)
        {
            this.documentActivities = documentActivities;
            this.documents = documents;
        }

        public void Handle(DocumentImported message)
        {
            CreateDocumentIfItDoesNotExist(message.DocumentId);

            documentActivities.Add(new DocumentActivity
            {
                ActivityDate = message.OccurredAt,
                DocumentId = message.DocumentId,
                Description = "Document imported to document store",
            });
        }        

        public void Handle(DocumentViewed message)
        {
            CreateDocumentIfItDoesNotExist(message.DocumentId);

            documentActivities.Add(new DocumentActivity
            {
                ActivityDate = message.OccurredAt,
                DocumentId = message.DocumentId,
                Description = "Document viewed by user " + message.UserName,
            });
        }

        private void CreateDocumentIfItDoesNotExist(Guid documentId)
        {
            var document = documents.SingleOrDefault(d => d.Id == documentId);

            if (document != null)
            {
                return;
            }

            document = new Document
            {
                Id = documentId,
                ActivityHistory = new HashSet<DocumentActivity>()
            };

            documents.Add(document);
        }
    }
}