using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spring.Json;

namespace CSharp.oDesk.Analyze
{
    public class Assignment
    {
        public Guid Id { get; set; }

        public string ContractorId { get; set; }

        public string JobType { get; set; }

        public double Rate { get; set; }

        public string RecNo { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public double TotalHours { get; set; }
        
        public double TotalCharge { get; set; }
        
    }
}
