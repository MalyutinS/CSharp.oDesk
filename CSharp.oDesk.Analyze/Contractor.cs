using System;

namespace CSharp.oDesk.Analyze
{
    public class Contractor
    {
        public Guid Id { get; set; }
        public string OdeskId { get; set; }
        public double Rate { get; set; }
        public double Feedback { get; set; }
        public string Country { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime MemberSince { get; set; }
        public int PortfolioItemsCount { get; set; }
        public int TestPassedCount { get; set; }
        public string ProfileType { get; set; }
        public string Skill { get; set; }
    }
}
