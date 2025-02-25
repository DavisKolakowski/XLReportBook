namespace ReportBook.Context
{
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using System;
    using ReportBook.Models;

    public abstract class ReportStore
    {
        public abstract string StoreName { get; }
        public abstract DataTable Data { get; }
    }

    public class ReportStore<TSchema> : ReportStore where TSchema : Report, new()
    {
        public ReportTable<TSchema> ReportTable { get; private set; }
        public override string StoreName { get; }

        public ReportStore(string storeName)
        {
            StoreName = storeName ?? typeof(TSchema).Name;
            ReportTable = new ConcreteReportTable<TSchema>();
        }

        public override DataTable Data => ReportTable.Table;

        public void Refresh(IEnumerable<TSchema> data) => ReportTable.Refresh(data);
        public List<TSchema> ToList() => ReportTable.ToList();
    }
}
