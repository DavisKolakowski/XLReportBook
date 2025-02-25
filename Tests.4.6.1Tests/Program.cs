namespace XLReport.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using XLReport.Tests.Runner;
    using XLReport.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using XLReport.Attributes;
    using System.Data;
    using XLReport.Context;
    using XLReport.Samples;

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

        public class InvalidAttributeReport : Report
        {
            [DateTimeMode(DataSetDateTime.Utc)]
            public int InvalidProperty { get; set; }
        }
        #endregion

        #region Table and Column Naming Tests
        [TestMethod]
        public void TableName_ShouldMatchClassName()
        {
            var store = new ReportStore<TestReport>("TestStore");
            string tableName = store.Data.TableName;
            Assert.AreEqual("TestReport", tableName, "Table name should match the report class name.");
        }

        [TestMethod]
        public void ColumnNames_ShouldMatchPropertyNames()
        {
            var store = new ReportStore<TestReport>("TestStore");

            var columnNames = store.Data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            var propertyNames = typeof(TestReport).GetProperties().Select(p => p.Name).ToList();

            CollectionAssert.Contains(columnNames, "TestName", "Column name should match property name.");
            CollectionAssert.Contains(columnNames, "TestDate", "Column name should match property name.");
            CollectionAssert.Contains(columnNames, "TestValue", "Column name should match property name.");
            CollectionAssert.Contains(columnNames, "InternalId", "InternalId should be in DataTable as read-only.");
            CollectionAssert.DoesNotContain(propertyNames, "InternalId", "InternalId should not be a mapped property.");
        }
        #endregion

        #region Attribute Application Tests
        [TestMethod]
        public void ColumnCaptionAttribute_ShouldSetCaption()
        {
            var store = new ReportStore<TestReport>("TestStore");
            var column = store.Data.Columns["TestName"];
            Assert.AreEqual("Test Name", column.Caption, "Column caption should match the ColumnCaptionAttribute value.");
        }

        [TestMethod]
        public void DateTimeModeAttribute_ShouldSetMode()
        {
            var store = new ReportStore<TestReport>("TestStore");
            var column = store.Data.Columns["TestDate"];
            Assert.AreEqual(DataSetDateTime.Utc, column.DateTimeMode, "DateTimeMode should match the attribute value.");
        }
        #endregion

        #region Data Mapping Tests
        [TestMethod]
        public void Refresh_ShouldPopulateDataTableCorrectly()
        {
            var store = new ReportStore<TestReport>("TestStore");
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
            var store = new ReportStore<TestReport>("TestStore");
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
            var store = new ReportStore<TestReport>("TestStore");
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
            var context = new MyReportCollection();
            var store = context.TestFirstReportStore;
            store.Refresh(new[] { new MyFirstTestReport { Name = "Test", CreatedDate = DateTime.UtcNow, Value = 42 } });
            context.Clear();
            Assert.AreEqual(0, store.Data.Rows.Count, "DataTable should be empty after Clear.");
        }
        #endregion

        #region Nullability Tests
        [TestMethod]
        public void Nullability_ShouldReflectPropertyTypes()
        {
            var store = new ReportStore<NullabilityReport>("NullabilityStore");
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
            var store = new ReportStore<EmptyReport>("EmptyStore");
            var columns = store.Data.Columns.Count;
            Assert.AreEqual(1, columns, "Empty report should only have InternalId column.");
            Assert.IsTrue(store.Data.Columns.Contains("InternalId"), "InternalId should be present.");
        }

        [TestMethod]
        public void RefreshWithEmptyList_ShouldNotThrow()
        {
            var store = new ReportStore<TestReport>("TestStore");
            var emptyData = new List<TestReport>();
            store.Refresh(emptyData);
            Assert.AreEqual(0, store.Data.Rows.Count, "DataTable should remain empty with empty input.");
        }
        #endregion
    }
}
