using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CPToolServerSide.Models
{
    public class AnalyzeBRR
    {
        [Key]
        public int ID { get; set; }

        public int Comparison_ID { get; set; }

        public int Line { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Client { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string EmployeeId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string EmploymentStatusId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string EffectiveStart { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string EffectiveEnd { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Table { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string Discrepancy { get; set; }

    }
}
