namespace ReportBook.Samples
{
    using ReportBook.Context;
    using ReportBook.Models;

    public class MyReportContext : ReportBookContext
    {
        public ReportStore<MyFirstTestReport> TestFirstReport { get; set; }
        public ReportStore<MySecondTestReport> TestSecondReport { get; set; }
    }
}
