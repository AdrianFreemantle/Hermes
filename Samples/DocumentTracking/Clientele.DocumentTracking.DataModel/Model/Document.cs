using System;
using System.Collections.Generic;

namespace Clientele.DocumentTracking.DataModel.Model
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public string IfaNumber { get; set; }
        public string PolicyNumber { get; set; }

        public ICollection<DocumentActivity> ActivityHistory { get; set; }

        public Document()
        {
            ActivityHistory = new HashSet<DocumentActivity>();
            IfaNumber = string.Empty;
            PolicyNumber = string.Empty;
        }
    }
}
