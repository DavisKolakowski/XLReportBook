namespace ReportBook.Attributes
{
    using System.Reflection;
    using System;
    using System.Data;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DateTimeModeAttribute : Attribute
    {
        public DataSetDateTime Mode { get; }
        public DateTimeModeAttribute(DataSetDateTime mode) => Mode = mode;

        public static void Validate(MemberInfo member)
        {
            var type = member is PropertyInfo prop ? prop.PropertyType : ((FieldInfo)member).FieldType;
            if (type != typeof(DateTime))
            {
                throw new ArgumentException($"{nameof(DateTimeModeAttribute)} can only be applied to DateTime members.");
            }
        }
    }
}
