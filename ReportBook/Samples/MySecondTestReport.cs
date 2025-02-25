namespace ReportBook.Samples
{
    using ReportBook.Attributes;
    using ReportBook.Models;

    public class MySecondTestReport : Report
    {
        [ColumnCaption("Second Description")]
        public string Description { get; set; }

        public double Amount;
    }
}
