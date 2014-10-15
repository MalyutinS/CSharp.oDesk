using System;

namespace CSharp.oDesk.Analyze
{
    public class Contractor
    {
        public Guid Id { get; set; }
        public string OdeskId { get; set; }
        public double TotalHours { get; set; }
        public int EngSkill { get; set; }
        public string Country { get; set; }
        public double TotalFeedback { get; set; }
        public int IsAffiliated { get; set; }
        public double AdjScore { get; set; }
        public double AdjScoreRecent { get; set; }
        public long LastWorkedTs { get; set; }
        public string LastWorked { get; set; }
        public int PortfolioItemsCount { get; set; }
        public string UiProfileAccess { get; set; }
        public int BilledAssignments { get; set; }
        public double BillRate { get; set; }
        public int RecNo { get; set; }
        public string City { get; set; }
        public string LastActivity { get; set; }
        public string ShortName { get; set; }
        
    }
}
