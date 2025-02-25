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

    public abstract class ReportBookContext
    {
        protected Dictionary<Type, ReportStore> reportStoresByType = new Dictionary<Type, ReportStore>();
        private ReportBookState currentState;

        protected ReportBookContext()
        {
            InitializeStores();
            currentState = new OpenState();
        }

        private void InitializeStores()
        {
            Type type = GetType();
            foreach (var prop in type.GetProperties())
            {
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ReportStore<>))
                {
                    var reportType = prop.PropertyType.GetGenericArguments()[0];
                    var storeName = prop.Name;
                    var store = (ReportStore)Activator.CreateInstance(prop.PropertyType, storeName);
                    prop.SetValue(this, store);
                    reportStoresByType[reportType] = store;
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
            if (reportStoresByType.TryGetValue(typeof(TSchema), out var store))
            {
                if (store is ReportStore<TSchema> typedStore)
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
            foreach (var store in reportStoresByType.Values)
            {
                store.Data.Rows.Clear();
            }
        }

        internal void ExportToExcel(string filePath)
        {
            using (var workbook = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                ExportToExcelInternal(workbook);
            }
        }

        internal void ExportToExcel(Stream stream)
        {
            using (var workbook = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                ExportToExcelInternal(workbook);
            }
        }

        private void ExportToExcelInternal(SpreadsheetDocument workbook)
        {
            var workbookPart = workbook.AddWorkbookPart();
            workbook.WorkbookPart.Workbook = new Workbook();
            workbook.WorkbookPart.Workbook.Sheets = new Sheets();

            uint sheetId = 1;
            foreach (var store in reportStoresByType.Values)
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

                var sheet = new Sheet { Id = relationshipId, SheetId = sheetId, Name = store.StoreName };
                sheets.Append(sheet);

                var headerRow = CreateHeaderRow(store.Data);
                sheetData.AppendChild(headerRow);

                foreach (DataRow row in store.Data.Rows)
                {
                    var newRow = CreateDataRow(row, store.Data);
                    sheetData.AppendChild(newRow);
                }

                sheetId++;
            }
        }

        private Row CreateHeaderRow(DataTable table)
        {
            var headerRow = new Row();
            foreach (DataColumn column in table.Columns)
            {
                var cell = new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(GetColumnCaption(column) ?? column.ColumnName)
                };
                headerRow.AppendChild(cell);
            }
            return headerRow;
        }

        private Row CreateDataRow(DataRow dataRow, DataTable table)
        {
            var newRow = new Row();
            foreach (DataColumn column in table.Columns)
            {
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

        private void SetCellValue(Cell cell, object value, Type dataType)
        {
            switch (Type.GetTypeCode(dataType))
            {
                case TypeCode.DBNull:
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue("");
                    break;
                case TypeCode.Boolean:
                    cell.DataType = CellValues.Boolean;
                    cell.CellValue = new CellValue(Convert.ToBoolean(value));
                    break;
                case TypeCode.Int32:
                    cell.DataType = CellValues.Number;
                    cell.CellValue = new CellValue(Convert.ToInt32(value));
                    break;
                case TypeCode.Double:
                    cell.DataType = CellValues.Number;
                    cell.CellValue = new CellValue(Convert.ToDouble(value));
                    break;
                case TypeCode.Decimal:
                    cell.DataType = CellValues.Number;
                    cell.CellValue = new CellValue(Convert.ToDecimal(value));
                    break;
                case TypeCode.DateTime:
                    cell.DataType = CellValues.Date;
                    cell.CellValue = new CellValue(Convert.ToDateTime(value));
                    break;
                default:
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(Convert.ToString(value));
                    break;
            }
        }
    }
}
