using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class JournalPreflightResult
    {
        public int SourceRows { get; set; }

        public int ValidRows { get; set; }

        public int RejectedRows { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public List<string> Errors { get; } = new List<string>();
    }
}