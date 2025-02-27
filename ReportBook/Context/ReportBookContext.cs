[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ReportBook.Tests")]
namespace ReportBook.Context
{
    using System.Collections.Generic;
    using System;
    using ReportBook.Models;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using DocumentFormat.OpenXml;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Globalization;
    using ReportBook.Attributes;
    using System.Reflection;

    public abstract class ReportBookContext
    {
        protected Dictionary<Type, ReportSheet> reportSheetsByType = new Dictionary<Type, ReportSheet>();
        private ReportBookState currentState;

        protected ReportBookContext()
        {
            InitializeSheets();
            currentState = new OpenState();
        }

        private void InitializeSheets()
        {
            var sheetNames = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in GetType().GetProperties())
            {
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ReportSheet<>))
                {
                    var sheetNameAttr = prop.GetCustomAttribute<SheetNameAttribute>();
                    if (sheetNameAttr == null)
                    {
                        throw new InvalidOperationException($"Property {prop.Name} must have a SheetNameAttribute.");
                    }

                    var sheetName = sheetNameAttr.Name;

                    if (sheetNames.ContainsKey(sheetName))
                    {
                        throw new InvalidOperationException(
                            $"Duplicate sheet name '{sheetName}' found for properties {sheetNames[sheetName].Name} and {prop.Name}.");
                    }
                    sheetNames[sheetName] = prop;

                    var reportType = prop.PropertyType.GetGenericArguments()[0];
                    var store = (ReportSheet)Activator.CreateInstance(prop.PropertyType, sheetName);
                    prop.SetValue(this, store);

                    reportSheetsByType[reportType] = store;
                }
            }
        }

        internal void TransitionTo(ReportBookState state)
        {
            currentState = state;
        }

        public void SaveToFile(string filePath)
        {
            currentState.SaveToFile(this, filePath);
        }

        public void SaveToStream(Stream stream)
        {
            currentState.SaveToStream(this, stream);
        }

        public void Refresh<TSchema>(IEnumerable<TSchema> data) where TSchema : Report, new()
        {
            currentState.Refresh(this, data);
        }

        public void Clear()
        {
            currentState.Clear(this);
        }

        internal void RefreshInternal<TSchema>(IEnumerable<TSchema> data) where TSchema : Report, new()
        {
            if (reportSheetsByType.TryGetValue(typeof(TSchema), out var store))
            {
                if (store is ReportSheet<TSchema> typedStore)
                {
                    typedStore.Refresh(data);
                }
            }
            else
            {
                throw new ArgumentException($"No ReportStore registered for type {typeof(TSchema).Name}");
            }
        }

        internal void ClearInternal()
        {
            foreach (var store in reportSheetsByType.Values)
            {
                store.Data.Rows.Clear();
            }
        }

        internal void ExportToExcel(string filePath)
        {
            try
            {
                using (var workbook = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    ExportToExcelInternal(workbook);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to export to Excel file '{filePath}': {ex.Message}", ex);
            }
        }

        internal void ExportToExcel(Stream stream)
        {
            try
            {
                using (var workbook = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    ExportToExcelInternal(workbook);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to export to Excel stream: " + ex.Message, ex);
            }
        }

        private void ExportToExcelInternal(SpreadsheetDocument workbook)
        {
            var workbookPart = workbook.AddWorkbookPart();
            workbook.WorkbookPart.Workbook = new Workbook();
            workbook.WorkbookPart.Workbook.Sheets = new Sheets();

            uint sheetId = 1;
            foreach (var reportSheet in reportSheetsByType.Values)
            {
                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                sheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                if (sheets.Elements<Sheet>().Any())
                {
                    sheetId = sheets.Elements<Sheet>().Max(s => s.SheetId.Value) + 1;
                }

                var sheet = new Sheet { Id = relationshipId, SheetId = sheetId, Name = reportSheet.Name };
                sheets.Append(sheet);

                var headerRow = CreateHeaderRow(reportSheet.Data);
                sheetData.AppendChild(headerRow);

                foreach (DataRow row in reportSheet.Data.Rows)
                {
                    var newRow = CreateDataRow(row, reportSheet.Data);
                    sheetData.AppendChild(newRow);
                }

                sheetId++;
            }
        }

        internal Row CreateHeaderRow(DataTable table)
        {
            var headerRow = new Row();
            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName == Report.IdentityColumnKey)
                {
                    continue;
                }
                var cell = new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(GetColumnCaption(column) ?? column.ColumnName)
                };
                headerRow.AppendChild(cell);
            }
            return headerRow;
        }

        internal Row CreateDataRow(DataRow dataRow, DataTable table)
        {
            var newRow = new Row();
            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName == Report.IdentityColumnKey)
                {
                    continue;
                }
                var cell = new Cell();
                object value = dataRow[column];
                SetCellValue(cell, value, column.DataType);
                newRow.AppendChild(cell);
            }
            return newRow;
        }

        private string GetColumnCaption(DataColumn column)
        {
            return column.Caption;
        }

        public void SetCellValue(Cell cell, object value, Type type)
        {
            if (value == null || value == DBNull.Value)
            {
                return;
            }

            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    cell.DataType = new EnumValue<CellValues>(CellValues.Boolean);
                    cell.CellValue = new CellValue(Convert.ToBoolean(value, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Double:
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    cell.CellValue = new CellValue(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Decimal:
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    cell.CellValue = new CellValue(Convert.ToDecimal(value, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Int32:
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    cell.CellValue = new CellValue(Convert.ToInt32(value, CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Int64:
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    cell.CellValue = new CellValue(Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString());
                    break;
                case TypeCode.DateTime:
                    cell.DataType = new EnumValue<CellValues>(CellValues.Date);
                    cell.CellValue = new CellValue(Convert.ToDateTime(value, CultureInfo.InvariantCulture).ToOADate().ToString(CultureInfo.InvariantCulture));
                    break;
                default:
                    cell.DataType = new EnumValue<CellValues>(CellValues.String);
                    cell.CellValue = new CellValue(Convert.ToString(value, CultureInfo.InvariantCulture));
                    break;
            }
        }
    }
}
