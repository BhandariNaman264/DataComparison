using Microsoft.EntityFrameworkCore;

namespace CPToolServerSide.Models
{
    public partial class CPTDBContext : DbContext
    {

        public CPTDBContext()
        {

        }
        public CPTDBContext(DbContextOptions<CPTDBContext> options) : base(options)
        {

        }

        public DbSet<Input> Input { get; set; }

        public DbSet<AnalyzePSR> AnalyzePSR { get; set; }

        public DbSet<AnalyzeBRR> AnalyzeBRR { get; set; }

        public DbSet<AnalyzeJSR> AnalyzeJSR { get; set; }

        public DbSet<AnalyzeSCR> AnalyzeSCR { get; set; }

        public DbSet<AnalyzeAE> AnalyzeAE { get; set; }

        public DbSet<CPToolServerSide.Models.AnalyzeE> AnalyzeE { get; set; }

    }
}
