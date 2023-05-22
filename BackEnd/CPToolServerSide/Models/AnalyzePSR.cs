using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CPToolServerSide.Models
{
    public class AnalyzePSR
    {
        [Key]
        public int ID { get; set; }

        public int Comparison_ID { get; set; }

        public int Line { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Client { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Employee_ID { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Buisness_Date { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Employee_Pay_AdjustID { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string Discrepancy { get; set; }

    }
}
