namespace ReportBook.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ReportBook.Tests.Runtime;
    using ReportBook.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ReportBook.Attributes;
    using System.Data;
    using ReportBook.Context;
    using ReportBook.Samples;
    using System.IO;
    using ReportBook.Samples.Reports;
    using DocumentFormat.OpenXml.Spreadsheet;

    [TestClass()]
    public class Program
    {
        #region Test Models
        public class TestReport : Report
        {
            [ColumnCaption("Test Name")]
            public string TestName { get; set; }

            [DateTimeMode(DataSetDateTime.Utc)]
            public DateTime TestDate { get; set; }

            public int TestValue { get; set; }
        }

        public class DuplicateColumnReport : Report
        {
            [ColumnCaption("Duplicate")]
            public string Column1 { get; set; }

            [ColumnCaption("Duplicate")]
            public int Column2 { get; set; }
        }

        public class NullabilityReport : Report
        {
            public string NullableString { get; set; }
            public int NonNullableInt { get; set; }
            public int? NullableInt { get; set; }
        }

        public class EmptyReport : Report
        {
        }

        public class ReservedColumnNameReport : Report
        {
            public int InternalId { get; set; }
        }

        public class VariousTypesReport : Report
        {
            public int IntProperty { get; set; }
            public string StringProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
            public bool BoolProperty { get; set; }
            public decimal DecimalProperty { get; set; }
            public double DoubleProperty { get; set; }
            public float FloatProperty { get; set; }
            public long LongProperty { get; set; }
            public short ShortProperty { get; set; }
            public byte ByteProperty { get; set; }
            public char CharProperty { get; set; }
            public Guid GuidProperty { get; set; }
            public int? NullableIntProperty { get; set; }
            public DateTime? NullableDateTimeProperty { get; set; }
        }

        public class MyFirstTestReport : Report
        {
            [ColumnCaption("First Name")]
            public string Name { get; set; }

            [DateTimeMode(DataSetDateTime.Utc)]
            public DateTime CreatedDate { get; set; }

            public int Value;
        }

        public class MySecondTestReport : Report
        {
            [ColumnCaption("Second Description")]
            public string Description { get; set; }

            public double Amount;
        }

        public class MyReportContext : ReportBookContext
        {
            [SheetName("FirstSheet")]
            public ReportSheet<MyFirstTestReport> TestFirstReport { get; set; }

            [SheetName("SecondSheet")]
            public ReportSheet<MySecondTestReport> TestSecondReport { get; set; }
        }
        public class DuplicateSheetNameContext : ReportBookContext
        {
            [SheetName("DuplicateSheet")]
            public ReportSheet<MyFirstTestReport> TestFirstReport { get; set; }

            [SheetName("DuplicateSheet")]
            public ReportSheet<MySecondTestReport> TestSecondReport { get; set; }
        }
        public class CaseInsensitiveDuplicateContext : ReportBookContext
        {
            [SheetName("SheetName")]
            public ReportSheet<MyFirstTestReport> TestFirstReport { get; set; }

            [SheetName("sheetname")]
            public ReportSheet<MySecondTestReport> TestSecondReport { get; set; }
        }
        public class MissingSheetNameContext : ReportBookContext
        {
            public ReportSheet<MyFirstTestReport> TestFirstReport { get; set; }
        }
        #endregion

        #region Table and Column Naming Tests
        [TestMethod]
        public void TableName_ShouldMatchClassName()
        {
            var store = new ReportSheet<TestReport>("TestStore");
            string tableName = store.Data.TableName;
            Assert.AreEqual("TestReport", tableName, "Table name should match the report class name.");
        }

        [TestMethod]
        public void ColumnNames_ShouldMatchPropertyNames()
        {
            var store = new ReportSheet<TestReport>("TestStore");

            var columnNames = store.Data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            var propertyNames = typeof(TestReport).GetProperties().Select(p => p.Name).ToList();

            CollectionAssert.Contains(columnNames, "TestName", "Column name should match property name.");
            CollectionAssert.Contains(columnNames, "TestDate", "Column name should match property name.");
            CollectionAssert.Contains(columnNames, "TestValue", "Column name should match property name.");
            CollectionAssert.Contains(columnNames, "InternalId", "InternalId should be in DataTable as read-only.");
            CollectionAssert.DoesNotContain(propertyNames, "InternalId", "InternalId should not be a mapped property.");
        }

        [TestMethod]
        public void VariousDataTypes_ShouldMapCorrectly()
        {
            var store = new ReportSheet<VariousTypesReport>("VariousTypesStore");
            var columns = store.Data.Columns;

            Assert.AreEqual(typeof(int), columns["IntProperty"].DataType);
            Assert.AreEqual(typeof(string), columns["StringProperty"].DataType);
            Assert.AreEqual(typeof(DateTime), columns["DateTimeProperty"].DataType);
            Assert.AreEqual(typeof(bool), columns["BoolProperty"].DataType);
            Assert.AreEqual(typeof(decimal), columns["DecimalProperty"].DataType);
            Assert.AreEqual(typeof(double), columns["DoubleProperty"].DataType);
            Assert.AreEqual(typeof(float), columns["FloatProperty"].DataType);
            Assert.AreEqual(typeof(long), columns["LongProperty"].DataType);
            Assert.AreEqual(typeof(short), columns["ShortProperty"].DataType);
            Assert.AreEqual(typeof(byte), columns["ByteProperty"].DataType);
            Assert.AreEqual(typeof(char), columns["CharProperty"].DataType);
            Assert.AreEqual(typeof(Guid), columns["GuidProperty"].DataType);
            Assert.AreEqual(typeof(int), columns["NullableIntProperty"].DataType);
            Assert.AreEqual(typeof(DateTime), columns["NullableDateTimeProperty"].DataType);

            Assert.IsFalse(columns["IntProperty"].AllowDBNull, "Non-nullable int should not allow null.");
            Assert.IsTrue(columns["StringProperty"].AllowDBNull, "String should allow null.");
            Assert.IsFalse(columns["DateTimeProperty"].AllowDBNull, "Non-nullable DateTime should not allow null.");
            Assert.IsFalse(columns["BoolProperty"].AllowDBNull, "Bool should not allow null.");
            Assert.IsFalse(columns["DecimalProperty"].AllowDBNull, "Decimal should not allow null.");
            Assert.IsFalse(columns["DoubleProperty"].AllowDBNull, "Double should not allow null.");
            Assert.IsFalse(columns["FloatProperty"].AllowDBNull, "Float should not allow null.");
            Assert.IsFalse(columns["LongProperty"].AllowDBNull, "Long should not allow null.");
            Assert.IsFalse(columns["ShortProperty"].AllowDBNull, "Short should not allow null.");
            Assert.IsFalse(columns["ByteProperty"].AllowDBNull, "Byte should not allow null.");
            Assert.IsFalse(columns["CharProperty"].AllowDBNull, "Char should not allow null.");
            Assert.IsFalse(columns["GuidProperty"].AllowDBNull, "Guid should not allow null.");
            Assert.IsTrue(columns["NullableIntProperty"].AllowDBNull, "Nullable int should allow null.");
            Assert.IsTrue(columns["NullableDateTimeProperty"].AllowDBNull, "Nullable DateTime should allow null.");
        }
        #endregion

        #region Attribute Application Tests
        [TestMethod]
        public void ColumnCaptionAttribute_ShouldSetCaption()
        {
            var store = new ReportSheet<TestReport>("TestStore");
            var column = store.Data.Columns["TestName"];
            Assert.AreEqual("Test Name", column.Caption, "Column caption should match the ColumnCaptionAttribute value.");
        }

        [TestMethod]
        public void DateTimeModeAttribute_ShouldSetMode()
        {
            var store = new ReportSheet<TestReport>("TestStore");
            var column = store.Data.Columns["TestDate"];
            Assert.AreEqual(DataSetDateTime.Utc, column.DateTimeMode, "DateTimeMode should match the attribute value.");
        }
        #endregion

        #region Data Mapping Tests
        [TestMethod]
        public void Refresh_ShouldPopulateDataTableCorrectly()
        {
            var store = new ReportSheet<TestReport>("TestStore");
            var testData = new List<TestReport>
            {
                new TestReport { TestName = "Alice", TestDate = DateTime.UtcNow, TestValue = 10 }
            };
            store.Refresh(testData);
            Assert.AreEqual(1, store.Data.Rows.Count, "DataTable should contain one row after Refresh.");
            var row = store.Data.Rows[0];
            Assert.AreEqual("Alice", row["TestName"], "TestName should match the input data.");
            Assert.AreEqual(10, row["TestValue"], "TestValue should match the input data.");
        }

        [TestMethod]
        public void ToList_ShouldRetrieveDataCorrectly()
        {
            var store = new ReportSheet<TestReport>("TestStore");
            var testData = new List<TestReport>
            {
                new TestReport { TestName = "Bob", TestDate = DateTime.UtcNow.AddDays(-1), TestValue = 20 }
            };
            store.Refresh(testData);
            var result = store.ToList();
            Assert.AreEqual(1, result.Count, "ToList should return one item.");
            Assert.AreEqual("Bob", result[0].TestName, "TestName should match the original data.");
            Assert.AreEqual(20, result[0].TestValue, "TestValue should match the original data.");
        }
        #endregion

        #region Identity Column Tests
        [TestMethod]
        public void IdentityColumn_ShouldNotMapToProperty()
        {
            var store = new ReportSheet<TestReport>("TestStore");
            var testData = new List<TestReport> { new TestReport { TestName = "Test" } };
            store.Refresh(testData);
            var result = store.ToList();
            var properties = typeof(TestReport).GetProperties().Select(p => p.Name).ToList();
            CollectionAssert.DoesNotContain(properties, "InternalId", "InternalId should not be a property of the report class.");
            Assert.IsNotNull(store.Data.Columns["InternalId"], "InternalId should exist in DataTable but not in the report object.");
        }
        #endregion

        #region Clearing Data Tests
        [TestMethod]
        public void Clear_ShouldRemoveAllData()
        {
            var context = new MyReportContext();
            var store = context.TestFirstReport;
            store.Refresh(new[] { new MyFirstTestReport { Name = "Test", CreatedDate = DateTime.UtcNow, Value = 42 } });
            context.Clear();
            Assert.AreEqual(0, store.Data.Rows.Count, "DataTable should be empty after Clear.");
        }
        #endregion

        #region Nullability Tests
        [TestMethod]
        public void Nullability_ShouldReflectPropertyTypes()
        {
            var store = new ReportSheet<NullabilityReport>("NullabilityStore");
            var nullableStringColumn = store.Data.Columns["NullableString"];
            var nonNullableIntColumn = store.Data.Columns["NonNullableInt"];
            var nullableIntColumn = store.Data.Columns["NullableInt"];
            Assert.IsTrue(nullableStringColumn.AllowDBNull, "Reference types should allow null.");
            Assert.IsFalse(nonNullableIntColumn.AllowDBNull, "Non-nullable value types should not allow null.");
            Assert.IsTrue(nullableIntColumn.AllowDBNull, "Nullable value types should allow null.");
        }
        #endregion

        #region Edge Case Tests
        [TestMethod]
        public void EmptyReport_ShouldHaveOnlyIdentityColumn()
        {
            var store = new ReportSheet<EmptyReport>("EmptyStore");
            var columns = store.Data.Columns.Count;
            Assert.AreEqual(1, columns, "Empty report should only have InternalId column.");
            Assert.IsTrue(store.Data.Columns.Contains("InternalId"), "InternalId should be present.");
        }

        [TestMethod]
        public void RefreshWithEmptyList_ShouldNotThrow()
        {
            var store = new ReportSheet<TestReport>("TestStore");
            var emptyData = new List<TestReport>();
            store.Refresh(emptyData);
            Assert.AreEqual(0, store.Data.Rows.Count, "DataTable should remain empty with empty input.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DuplicateSheetNames_ShouldThrowException()
        {
            new DuplicateSheetNameContext();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CaseInsensitiveDuplicateSheetNames_ShouldThrowException()
        {
            new CaseInsensitiveDuplicateContext();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MissingSheetNameAttribute_ShouldThrowException()
        {
            new MissingSheetNameContext();
        }

        [TestMethod]
        public void CreateHeaderRow_ShouldExcludeInternalId()
        {
            var context = new MyReportContext();
            var store = context.TestFirstReport;
            store.Refresh(new[] { new MyFirstTestReport { Name = "Test", CreatedDate = DateTime.UtcNow, Value = 42 } });
            var table = store.Data;
            var headerRow = context.CreateHeaderRow(table);
            var expectedColumnCount = table.Columns.Count - 1;
            var actualCellCount = headerRow.Elements<Cell>().Count();
            Assert.AreEqual(expectedColumnCount, actualCellCount, "Header row should exclude InternalId column.");
        }

        [TestMethod]
        public void CreateDataRow_ShouldExcludeInternalId()
        {
            var context = new MyReportContext();
            var store = context.TestFirstReport;
            store.Refresh(new[] { new MyFirstTestReport { Name = "Test", CreatedDate = DateTime.UtcNow, Value = 42 } });
            var table = store.Data;
            var dataRow = table.Rows[0];
            var excelRow = context.CreateDataRow(dataRow, table);
            var expectedColumnCount = table.Columns.Count - 1;
            var actualCellCount = excelRow.Elements<Cell>().Count();
            Assert.AreEqual(expectedColumnCount, actualCellCount, "Data row should exclude InternalId column.");
        }
        #endregion

        #region Integration Tests
        [TestMethod]
        public void Test_StandardizedAddressableAudienceReport_ShouldSaveToStream()
        {
            var context = new StandardizedAddressableAudienceReportBookContext();

            var reports = new List<StandardizedAddressableAudienceReport>
            {
                new StandardizedAddressableAudienceReport
                {
                    AQID = "AQ123",
                    Market = "Market1",
                    SalesAlias = "Alias1",
                    AaHhCount = 1000,
                    UniverseCount = null,
                    IncidenceLevelPct = 20.0m
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "AQ456",
                    Market = "Market2",
                    SalesAlias = "Alias2",
                    AaHhCount = 2000,
                    UniverseCount = 10000,
                    IncidenceLevelPct = 20.0m
                }
            };

            context.Refresh(reports);
            var retrievedReports = context.StandardizedAddressableAudienceReport.ToList();

            Assert.AreEqual(reports.Count, retrievedReports.Count, "The number of retrieved reports should match the input count.");
            for (int i = 0; i < reports.Count; i++)
            {
                Assert.AreEqual(reports[i].AQID, retrievedReports[i].AQID, $"AQID mismatch at index {i}.");
                Assert.AreEqual(reports[i].Market, retrievedReports[i].Market, $"Market mismatch at index {i}.");
                Assert.AreEqual(reports[i].SalesAlias, retrievedReports[i].SalesAlias, $"SalesAlias mismatch at index {i}.");
                Assert.AreEqual(reports[i].AaHhCount, retrievedReports[i].AaHhCount, $"AaHhCount mismatch at index {i}.");
                Assert.AreEqual(reports[i].UniverseCount, retrievedReports[i].UniverseCount, $"UniverseCount mismatch at index {i}.");
                Assert.AreEqual(reports[i].IncidenceLevelPct, retrievedReports[i].IncidenceLevelPct, $"IncidenceLevelPct mismatch at index {i}.");
            }

            using (var stream = new MemoryStream())
            {
                context.SaveToStream(stream);
                Assert.IsTrue(stream.Length > 0, "The stream should contain data after SaveToStream.");
            }

            context.Clear();
            Assert.AreEqual(0, context.StandardizedAddressableAudienceReport.Data.Rows.Count, "The report store should be empty after Clear.");
        }

        [TestMethod]
        public void Test_StandardizedAddressableAudienceReport_ShouldSaveToExcel()
        {
            var context = new StandardizedAddressableAudienceReportBookContext();

            var reports = new List<StandardizedAddressableAudienceReport>
            {
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K2",
                    Market = "Washington, DC (Hagerstown)",
                    SalesAlias = "Experian HH 75k Washington DC",
                    AaHhCount = 500000L,
                    UniverseCount = null,
                    IncidenceLevelPct = 0.75m
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K3",
                    Market = "Denver",
                    SalesAlias = "gfdgf",
                    AaHhCount = null,
                    UniverseCount = 1000000L,
                    IncidenceLevelPct = null
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K6",
                    Market = "Denver",
                    SalesAlias = "gfdgf",
                    AaHhCount = 750000L,
                    UniverseCount = null,
                    IncidenceLevelPct = 0.85m
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K8",
                    Market = "Boston (Manchester)",
                    SalesAlias = "Mock Boston Segment",
                    AaHhCount = null,
                    UniverseCount = 800000L,
                    IncidenceLevelPct = null
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K9",
                    Market = "Chicago",
                    SalesAlias = "Mock Chicago Segment",
                    AaHhCount = 600000L,
                    UniverseCount = null,
                    IncidenceLevelPct = 0.65m
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K8",
                    Market = "Atlanta, San Francisco-Oak-San Jose",
                    SalesAlias = "Mock Atlanta SF Segment",
                    AaHhCount = null,
                    UniverseCount = 1200000L,
                    IncidenceLevelPct = null
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K7",
                    Market = "Atlanta, San Francisco-Oak-San Jose",
                    SalesAlias = null,
                    AaHhCount = 900000L,
                    UniverseCount = null,
                    IncidenceLevelPct = 0.90m
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K7",
                    Market = "Atlanta, San Francisco-Oak-San Jose",
                    SalesAlias = "Mock Atlanta SF Segment 2",
                    AaHhCount = null,
                    UniverseCount = 1100000L,
                    IncidenceLevelPct = null
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K4",
                    Market = "Colorado Springs-Pueblo",
                    SalesAlias = "Mock Colorado Segment",
                    AaHhCount = 450000L,
                    UniverseCount = null,
                    IncidenceLevelPct = 0.70m
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K1",
                    Market = "Chicago",
                    SalesAlias = null,
                    AaHhCount = null,
                    UniverseCount = 950000L,
                    IncidenceLevelPct = null
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A01K0",
                    Market = "Atlanta, San Francisco-Oak-San Jose",
                    SalesAlias = "Mock Atlanta SF Segment 3",
                    AaHhCount = 850000L,
                    UniverseCount = null,
                    IncidenceLevelPct = 0.80m
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A0111",
                    Market = "Chicago",
                    SalesAlias = "Mock Chicago Segment 2",
                    AaHhCount = null,
                    UniverseCount = 900000L,
                    IncidenceLevelPct = null
                },
                new StandardizedAddressableAudienceReport
                {
                    AQID = "A0111",
                    Market = "Chicago",
                    SalesAlias = null,
                    AaHhCount = 700000L,
                    UniverseCount = null,
                    IncidenceLevelPct = 0.75m
                }
            };

            context.Refresh(reports);
            var retrievedReports = context.StandardizedAddressableAudienceReport.ToList();

            Assert.AreEqual(reports.Count, retrievedReports.Count, "The number of retrieved reports should match the input count.");
            for (int i = 0; i < reports.Count; i++)
            {
                Assert.AreEqual(reports[i].AQID, retrievedReports[i].AQID, $"AQID mismatch at index {i}.");
                Assert.AreEqual(reports[i].Market, retrievedReports[i].Market, $"Market mismatch at index {i}.");
                Assert.AreEqual(reports[i].SalesAlias, retrievedReports[i].SalesAlias, $"SalesAlias mismatch at index {i}.");
                Assert.AreEqual(reports[i].AaHhCount, retrievedReports[i].AaHhCount, $"AaHhCount mismatch at index {i}.");
                Assert.AreEqual(reports[i].UniverseCount, retrievedReports[i].UniverseCount, $"UniverseCount mismatch at index {i}.");
                Assert.AreEqual(reports[i].IncidenceLevelPct, retrievedReports[i].IncidenceLevelPct, $"IncidenceLevelPct mismatch at index {i}.");
            }

            string tempFilePath = Path.Combine(Path.GetTempPath(), "test_saa_report.xlsx");
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            context.SaveToFile(tempFilePath);

            Assert.IsTrue(File.Exists(tempFilePath), "Excel file should be created.");
            Assert.IsTrue(new FileInfo(tempFilePath).Length > 0, "Excel file should not be empty.");

            // File.Delete(tempFilePath);
        }
        #endregion

        #region Additional Error Handling Tests
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReservedColumnName_ShouldThrowException()
        {
            new ReportSheet<ReservedColumnNameReport>("ReservedStore");
        }

        [TestMethod]
        public void MultipleReportStores_ShouldWorkIndependently()
        {
            var context = new MyReportContext();

            var firstData1 = new List<MyFirstTestReport>
            {
                new MyFirstTestReport { Name = "Alice", CreatedDate = DateTime.UtcNow, Value = 10 }
            };
            var secondData1 = new List<MySecondTestReport>
            {
                new MySecondTestReport { Description = "Test1", Amount = 100.0 }
            };

            context.Refresh(firstData1);
            context.Refresh(secondData1);

            Assert.AreEqual(1, context.TestFirstReport.Data.Rows.Count, "First report should have 1 row.");
            Assert.AreEqual(1, context.TestSecondReport.Data.Rows.Count, "Second report should have 1 row.");

            var firstData2 = new List<MyFirstTestReport>
            {
                new MyFirstTestReport { Name = "Bob", CreatedDate = DateTime.UtcNow, Value = 20 }
            };
            context.Refresh(firstData2);

            Assert.AreEqual(1, context.TestFirstReport.Data.Rows.Count, "First report should still have 1 row after refresh.");
            Assert.AreEqual("Bob", context.TestFirstReport.ToList()[0].Name, "First report data should be updated.");
            Assert.AreEqual(1, context.TestSecondReport.Data.Rows.Count, "Second report should remain unaffected.");
            Assert.AreEqual("Test1", context.TestSecondReport.ToList()[0].Description, "Second report data should be unchanged.");

            context.Clear();
            Assert.AreEqual(0, context.TestFirstReport.Data.Rows.Count, "First report should be empty after clear.");
            Assert.AreEqual(0, context.TestSecondReport.Data.Rows.Count, "Second report should be empty after clear.");
        }

        [TestMethod]
        public void SaveToFile_ShouldCreateExcelFile()
        {
            var context = new MyReportContext();
            var firstData = new List<MyFirstTestReport>
            {
                new MyFirstTestReport { Name = "Alice", CreatedDate = DateTime.UtcNow, Value = 10 }
            };
            context.Refresh(firstData);

            string tempFilePath = Path.Combine(Path.GetTempPath(), "test_report.xlsx");
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            context.SaveToFile(tempFilePath);

            Assert.IsTrue(File.Exists(tempFilePath), "Excel file should be created.");
            Assert.IsTrue(new FileInfo(tempFilePath).Length > 0, "Excel file should not be empty.");

            File.Delete(tempFilePath);
        }

        [TestMethod]
        public void SaveToStream_ShouldWriteToStream()
        {
            var context = new MyReportContext();
            var firstData = new List<MyFirstTestReport>
            {
                new MyFirstTestReport { Name = "Alice", CreatedDate = DateTime.UtcNow, Value = 10 }
            };
            context.Refresh(firstData);

            using (var stream = new MemoryStream())
            {
                context.SaveToStream(stream);
                Assert.IsTrue(stream.Length > 0, "Stream should contain data after SaveToStream.");
            }
        }
        #endregion
    }
}
