using System;

namespace CSharp.oDesk.Analyze
{
    class Job
    {
        public Guid Id { get; set; }
        public string OdeskId { get; set; }
        public string Title { get; set; }
        public string OdeskCategory { get; set; }
        public string OdeskSubcategory { get; set; }
        public DateTime DateCreated { get; set; }
        public int Budjet { get; set; }
        public string ClientCountry { get; set; }
        public string Skill { get; set; }
    }
}
