using System;
using Clientele.DocumentTracking.ApplicationService;
using Clientele.DocumentTracking.ApplicationService.Commands;
using Clientele.Ocr.Contracts.Events;
using Hermes.Logging;
using Hermes.Messaging;

namespace Clientele.DocumentTracking.OcrService
{
    public class OcrService 
        : IHandleMessage<ExportCompleted> 
        , IHandleMessage<ExportFailed>
        , IHandleMessage<RecognitionDone>
        , IHandleMessage<RecognitionFailed>
        , IHandleMessage<VerificationDone>
    {
        private readonly DocumentTrackingService trackingService;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (OcrService));

        public OcrService(DocumentTrackingService trackingService)
        {
            this.trackingService = trackingService;
        }

        private const string Source = "OCR";
        private const string SystemUser = "OCR System";

        public void Handle(ExportCompleted message)
        {
            Logger.Info("Handling Export Completed event");

            var command = new RegisterDocumentActivity
            {
                Description = String.Format("Document exported to {0}", message.FilePath),
                DocumentId = message.DocumentId,
                OccurredAt = message.OccurredAt,
                Source = Source,
                User = SystemUser
            };

            trackingService.Handle(command);
        }

        public void Handle(ExportFailed message)
        {
            Logger.Info("Handling Export Failed event");

            var command = new RegisterDocumentActivity
            {
                Description = "Document export failed",
                DocumentId = message.DocumentId,
                OccurredAt = message.OccurredAt,
                Source = Source,
                User = SystemUser
            };

            trackingService.Handle(command);
        }

        public void Handle(RecognitionDone message)
        {
            Logger.Info("Handling Recognition Done event");

            var command = new RegisterDocumentReceived
            {
                DocumentId = message.DocumentId,
                OccurredAt = message.OccurredAt,
                Source = Source,
                User = SystemUser
            };

            trackingService.Handle(command);
        }

        public void Handle(RecognitionFailed message)
        {
            Logger.Info("Handling Recognition Failed event");

            var command = new RegisterDocumentActivity
            {
                Description = "Document recognition failed",
                DocumentId = message.DocumentId,
                OccurredAt = message.OccurredAt,
                Source = Source,
                User = SystemUser
            };

            trackingService.Handle(command);
        }

        public void Handle(VerificationDone message)
        {
            Logger.Info("Handling Verification Done event");

            var command = new UpdateDocumentMetaData
            {
                DocumentId = message.DocumentId,
                OccurredAt = message.OccurredAt,
                Source = Source,
                User = message.VerfiedByUser,
            };

            string ifaNumber;
            string policyNumber;

            message.MetaData.TryGetValue("IfaNumber", out ifaNumber);            
            message.MetaData.TryGetValue("PolicyNumber", out policyNumber);

            command.IfaNumber = ifaNumber ?? String.Empty;
            command.PolicyNumber = policyNumber ?? String.Empty; 

            trackingService.Handle(command);
        }
    }
}
