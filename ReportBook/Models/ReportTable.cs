namespace ReportBook.Models
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System;
    using ReportBook.Attributes;

    public abstract class ReportTable<TSchema> where TSchema : Report, new()
    {
        protected DataTable table;
        public DataTable Table => table;

        private Dictionary<string, MemberInfo> memberMap;

        public ReportTable()
        {
            InitializeTable();
        }

        private void InitializeTable()
        {
            Type reportType = typeof(TSchema);
            string tableName = reportType.Name;
            table = new DataTable(tableName);

            DataColumn idColumn = CreateIdentityColumn();
            table.Columns.Add(idColumn);
            table.PrimaryKey = new[] { idColumn };

            memberMap = reportType
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field)
                .ToDictionary(m => m.Name);

            foreach (var member in memberMap.Values)
            {
                AddColumnForMember(member);
            }
        }

        private DataColumn CreateIdentityColumn()
        {
            return new DataColumn(Report.IdentityColumnKey, typeof(int))
            {
                AutoIncrement = true,
                AutoIncrementSeed = 1,
                AutoIncrementStep = 1,
                ReadOnly = true,
                AllowDBNull = false,
                Unique = true
            };
        }

        private void AddColumnForMember(MemberInfo member)
        {
            if (member.Name == Report.IdentityColumnKey)
            {
                throw new InvalidOperationException($"Member '{Report.IdentityColumnKey}' is reserved for the identity column.");
            }

            if (table.Columns.Contains(member.Name))
            {
                throw new InvalidOperationException($"Duplicate column name '{member.Name}' in schema.");
            }

            Type memberType = member is PropertyInfo prop ? prop.PropertyType : ((FieldInfo)member).FieldType;
            Type columnType = Nullable.GetUnderlyingType(memberType) ?? memberType;

            var col = new DataColumn(member.Name, columnType)
            {
                AllowDBNull = !memberType.IsValueType || Nullable.GetUnderlyingType(memberType) != null,
                ReadOnly = false,
                Unique = false
            };

            var captionAttr = member.GetCustomAttribute<ColumnCaptionAttribute>();
            if (captionAttr != null)
            {
                col.Caption = captionAttr.Caption;
            }

            if (columnType == typeof(DateTime) || columnType == typeof(DateTime?))
            {
                ConfigureDateTimeColumn(col, member);
            }

            table.Columns.Add(col);
        }

        private void ConfigureDateTimeColumn(DataColumn col, MemberInfo member)
        {
            var dateTimeModeAttr = member.GetCustomAttribute<DateTimeModeAttribute>();
            if (dateTimeModeAttr != null)
            {
                col.DateTimeMode = dateTimeModeAttr.Mode;
            }
            else
            {
                col.DateTimeMode = DataSetDateTime.Local;
            }
        }

        public DataColumnCollection Schema => table.Columns;
        public DataRowCollection Data => table.Rows;

        public List<TSchema> ToList()
        {
            var list = new List<TSchema>();
            foreach (DataRow row in Data)
            {
                var item = new TSchema();
                foreach (var kvp in memberMap)
                {
                    if (kvp.Key == Report.IdentityColumnKey)
                        continue;

                    if (Schema.Contains(kvp.Key))
                    {
                        object value = row[kvp.Key] is DBNull ? null : row[kvp.Key];
                        SetMemberValue(item, kvp.Value, value);
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
                foreach (var kvp in memberMap)
                {
                    if (Schema.Contains(kvp.Key))
                    {
                        object value = GetMemberValue(item, kvp.Value) ?? DBNull.Value;
                        row[kvp.Key] = value;
                    }
                }
                table.Rows.Add(row);
            }
        }

        private object GetMemberValue(object instance, MemberInfo member)
        {
            if (member is PropertyInfo prop)
            {
                return prop.GetValue(instance);
            }
            else if (member is FieldInfo field)
            {
                return field.GetValue(instance);
            }
            throw new InvalidOperationException("Unsupported member type.");
        }

        private void SetMemberValue(object instance, MemberInfo member, object value)
        {
            if (member is PropertyInfo prop && prop.CanWrite)
            {
                prop.SetValue(instance, value);
            }
            else if (member is FieldInfo field)
            {
                field.SetValue(instance, value);
            }
        }
    }
}
