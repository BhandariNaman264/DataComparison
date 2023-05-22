using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CPToolServerSide.Models
{
    public class Input
    {

        [Key]
        public int ID { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string User_ID { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string User_Name { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string User_Email { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Client { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string DBName_1 { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string DBName_2 { get; set; }


        [Column(TypeName = "nvarchar(100)")]
        public string ControlDB_1 { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ControlDB_2 { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ControlDBServer_1 { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ControlDBServer_2 { get; set; }


        [Column(TypeName = "nvarchar(100)")]
        public string ControlDBServer_Server1 { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string ControlDBServer_Server2 { get; set; }




        [Column(TypeName = "nvarchar(200)")]
        public string Task_Name { get; set; }

        public bool ForceCompareOnly { get; set; }

        public bool RunTask_1 { get; set; }

        public bool RunTask_2 { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string Start_Time { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string End_Time { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string Date_Relative_To_Today { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Org { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Policy { get; set; }

        public int Pay_Group_Calendar_Id { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Export_Mode { get; set; }

        public bool Mock_Transmit { get; set; }

        [Column(TypeName = "nvarchar(300)")]
        public string Export_File_Name { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string Job { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Date { get; set; }


        [Column(TypeName = "nvarchar(100)")]
        public string LogId1 { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Status1 { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string LogId2 { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Status2 { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Results { get; set; }

        // Comparison is used to know the status of Job and Comparison.
        // Compared = 0: Comparison not made yet
        // Compared = 1: Comparison has been made
        // Compared = 2: Comparison has been made and we have got Results (SUCCESS or WARNING)
        // Compared = 3: Atleast one version Job has failed or Queue has Failed and we can't run comparison
        public int Compared { get; set; }

        // Analyzed is used to check weahter Analyze for this Comparison ID has already started or not
        // Analyze = 0: Analyze has not started yet
        // Analyze = 1: Analyze has started
        // Analyze = 2: Analyze has completed
        public int Analyzed { get; set; }

        [Column(TypeName = "nvarchar(300)")]
        public string Version1_Path { get; set; }

        [Column(TypeName = "nvarchar(300)")]
        public string Version2_Path { get; set; }

        [Column(TypeName = "nvarchar(300)")]
        public string Analyze_Path { get; set; }

    }
}
