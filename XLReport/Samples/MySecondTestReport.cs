namespace XLReport.Samples
{
    using XLReport.Attributes;
    using XLReport.Models;

    public class MySecondTestReport : Report
    {
        [ColumnCaption("Second Description")]
        public string Description { get; set; }

        public double Amount;
    }
}
