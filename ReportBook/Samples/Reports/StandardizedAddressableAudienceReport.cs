namespace ReportBook.Samples.Reports
{
    using ReportBook.Attributes;
    using ReportBook.Models;

    public class StandardizedAddressableAudienceReport : Report
    {
        [ColumnCaption("AQID [FOR CM USE IN AOS]")]
        public string AQID { get; set; }

        [ColumnCaption("Market(s)")]
        public string Market { get; set; }

        [ColumnCaption("Audience Segment Name")]
        public string SalesAlias { get; set; } = string.Empty;

        [ColumnCaption("AA HH Counts (AQID in W)")]
        public long? AaHhCount { get; set; }

        [ColumnCaption("Universe (Please use DMA or State in B)")]
        public long? UniverseCount { get; set; }

        [ColumnCaption("Incidence Level (Geo)")]
        public decimal? IncidenceLevelPct { get; set; }

    }
}
