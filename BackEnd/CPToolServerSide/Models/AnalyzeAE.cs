using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CPToolServerSide.Models
{
    public class AnalyzeAE
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
        public string Table { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string Discrepancy { get; set; }
    }
}
