namespace ReportBook.Context
{
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using System;
    using ReportBook.Models;
    using ReportBook.Attributes;

    public abstract class ReportSheet
    {
        public abstract string Name { get; }
        public abstract DataTable Data { get; }
    }

    public class ReportSheet<TSchema> : ReportSheet where TSchema : Report, new()
    {
        public ReportTable<TSchema> ReportTable { get; private set; }
        public override string Name { get; }

        public ReportSheet(string sheetName)
        {
            Name = sheetName ?? throw new ArgumentNullException(nameof(sheetName), "Sheet name cannot be null.");
            ReportTable = new ConcreteReportTable<TSchema>();
        }

        public override DataTable Data => ReportTable.Table;

        public void Refresh(IEnumerable<TSchema> data) => ReportTable.Refresh(data);
        public List<TSchema> ToList() => ReportTable.ToList();
    }
}
