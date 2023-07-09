using System.ComponentModel;

namespace Scraping.Models
{
    public class BankData
    {
        public int Id { get; set; }

        [Description("תאריך התנועה")]
        public string ActionDate { get; set; }

        [Description("סכום תנועה")]
        public string  ActionAmount { get; set; }

        [Description("פרטים/תיאור תנועה")]
        public string ActionDetailes { get; set; }  
    }
}
