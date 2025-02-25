namespace XLReport.Samples
{
    using System.Data;
    using System;

    using XLReport.Attributes;
    using XLReport.Models;

    public class MyFirstTestReport : Report
    {
        [ColumnCaption("First Name")]
        public string Name { get; set; }

        [DateTimeMode(DataSetDateTime.Utc)]
        public DateTime CreatedDate { get; set; }

        public int Value;
    }
}
