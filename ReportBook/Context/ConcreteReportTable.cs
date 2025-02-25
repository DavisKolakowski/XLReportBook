namespace ReportBook.Context
{
    using ReportBook.Models;

    public class ConcreteReportTable<TSchema> : ReportTable<TSchema> where TSchema : Report, new() { }
}
