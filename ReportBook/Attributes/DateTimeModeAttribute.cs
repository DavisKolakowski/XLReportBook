namespace ReportBook.Attributes
{
    using System.Reflection;
    using System;
    using System.Data;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DateTimeModeAttribute : Attribute
    {
        public DataSetDateTime Mode { get; }
        public DateTimeModeAttribute(DataSetDateTime mode) => Mode = mode;
    }
}
