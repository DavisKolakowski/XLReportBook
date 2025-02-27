namespace ReportBook.Attributes
{
    using System;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class SheetNameAttribute : Attribute
    {
        public string Name { get; }

        public SheetNameAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Sheet name cannot be null or empty", nameof(name));
            }
            if (name.Length > 31)
            {
                throw new ArgumentException("Sheet name cannot exceed 31 characters", nameof(name));
            }
            if (name.IndexOfAny(new char[] { '/', '\\', '?', '*', ':', '[', ']' }) >= 0)
            {
                throw new ArgumentException("Sheet name contains invalid characters", nameof(name));
            }
            if (name.StartsWith("'") || name.EndsWith("'"))
            {
                throw new ArgumentException("Sheet name cannot begin or end with an apostrophe (')", nameof(name));
            }
            if (name.Equals("History", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Sheet name 'History' is reserved and cannot be used", nameof(name));
            }

            Name = name;
        }
    }
}
