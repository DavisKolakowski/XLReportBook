namespace XLReport.Context
{
    using XLReport.Models;

    public class ConcreteReportTable<TSchema> : ReportTable<TSchema> where TSchema : Report, new() { }
}
