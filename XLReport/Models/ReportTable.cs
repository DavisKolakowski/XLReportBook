namespace XLReport.Models
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System;
    using XLReport.Attributes;

    public abstract class ReportTable<TSchema> where TSchema : Report, new()
    {
        protected DataTable table;
        public DataTable Table => table;

        public ReportTable()
        {
            InitializeTable();
        }

        private void InitializeTable()
        {
            Type reportType = typeof(TSchema);
            string tableName = reportType.Name;

            table = new DataTable(tableName);

            DataColumn idColumn = new DataColumn(Report.OrderByKey, typeof(int))
            {
                AutoIncrement = true,
                AutoIncrementSeed = 1,
                AutoIncrementStep = 1,
                ReadOnly = true,
                AllowDBNull = false,
                Unique = true
            };
            table.Columns.Add(idColumn);
            table.PrimaryKey = new[] { idColumn };

            foreach (var prop in reportType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                AddColumnForMember(prop.PropertyType, prop.Name, prop);
            }

            foreach (var field in reportType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                AddColumnForMember(field.FieldType, field.Name, field);
            }
        }

        private void AddColumnForMember(Type memberType, string memberName, MemberInfo memberInfo)
        {
            if (memberName == Report.OrderByKey)
            {
                throw new InvalidOperationException($"Member '{Report.OrderByKey}' is reserved for the identity column.");
            }

            if (table.Columns.Contains(memberName))
            {
                throw new InvalidOperationException($"Duplicate column name '{memberName}' in schema.");
            }

            Type columnType = Nullable.GetUnderlyingType(memberType) ?? memberType;
            var col = new DataColumn(memberName, columnType)
            {
                AllowDBNull = !memberType.IsValueType || Nullable.GetUnderlyingType(memberType) != null,
                ReadOnly = false,
                Unique = false
            };

            var captionAttr = memberInfo.GetCustomAttribute<ColumnCaptionAttribute>();
            if (captionAttr != null)
            {
                col.Caption = captionAttr.Caption;
            }

            var dateTimeModeAttr = memberInfo.GetCustomAttribute<DateTimeModeAttribute>();
            if (dateTimeModeAttr != null)
            {
                DateTimeModeAttribute.Validate(memberInfo);
                col.DateTimeMode = dateTimeModeAttr.Mode;
            }
            else if (memberType == typeof(DateTime))
            {
                col.DateTimeMode = DataSetDateTime.Local;
            }

            table.Columns.Add(col);
        }

        public DataColumnCollection Schema => table.Columns;
        public DataRowCollection Data => table.Rows;

        public List<TSchema> ToList()
        {
            var list = new List<TSchema>();
            foreach (DataRow row in Data)
            {
                var item = new TSchema();
                foreach (DataColumn col in Schema)
                {
                    if (col.ColumnName != Report.OrderByKey)
                    {
                        var prop = typeof(TSchema).GetProperty(col.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        var field = prop == null ? typeof(TSchema).GetField(col.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) : null;
                        object value = row[col.ColumnName] is DBNull ? null : row[col.ColumnName];
                        if (prop != null && prop.CanWrite)
                        {
                            prop.SetValue(item, value);
                        }
                        else if (field != null)
                        {
                            field.SetValue(item, value);
                        }
                    }
                }
                list.Add(item);
            }
            return list;
        }

        public void Refresh(IEnumerable<TSchema> data)
        {
            Data.Clear();
            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var prop in typeof(TSchema).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (table.Columns.Contains(prop.Name))
                    {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                }
                foreach (var field in typeof(TSchema).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (table.Columns.Contains(field.Name))
                    {
                        row[field.Name] = field.GetValue(item) ?? DBNull.Value;
                    }
                }
                table.Rows.Add(row);
            }
        }
    }
}
