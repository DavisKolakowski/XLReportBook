namespace XLReport.Samples
{
    using XLReport.Context;
    using XLReport.Models;

    public class MyReportCollection : ReportBookCollectionContext
    {
        public ReportStore<MyFirstTestReport> TestFirstReportStore { get; set; }
        public ReportStore<MySecondTestReport> TestSecondReportStore { get; set; }
    }
}
