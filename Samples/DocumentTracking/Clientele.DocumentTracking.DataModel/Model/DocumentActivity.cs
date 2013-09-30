using System;

namespace Clientele.DocumentTracking.DataModel.Model
{
    public class DocumentActivity
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string User { get; set; }
        public String Source { get; set; }
        public DateTime ActivityDate { get; set; }

        public Guid DocumentId { get; set; }
        public Document Document { get; set; }
    }
}