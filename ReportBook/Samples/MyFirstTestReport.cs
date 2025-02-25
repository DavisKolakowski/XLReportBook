namespace ReportBook.Samples
{
    using System.Data;
    using System;

    using ReportBook.Attributes;
    using ReportBook.Models;

    public class MyFirstTestReport : Report
    {
        [ColumnCaption("First Name")]
        public string Name { get; set; }

        [DateTimeMode(DataSetDateTime.Utc)]
        public DateTime CreatedDate { get; set; }

        public int Value;
    }
}
