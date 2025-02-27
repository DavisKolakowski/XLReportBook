namespace ReportBook.Samples
{
    using ReportBook.Attributes;
    using ReportBook.Context;
    using ReportBook.Samples.Reports;

    public class StandardizedAddressableAudienceReportBookContext : ReportBookContext
    {
        [SheetName("TestSAAReportTab")]
        public ReportSheet<StandardizedAddressableAudienceReport> StandardizedAddressableAudienceReport { get; set; }
    }
}
