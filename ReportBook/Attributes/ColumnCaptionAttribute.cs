namespace ReportBook.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnCaptionAttribute : Attribute
    {
        public string Caption { get; }
        public ColumnCaptionAttribute(string caption) => Caption = caption;
    }
}
