namespace XLReportTests
{
    using DocumentFormat.OpenXml.Spreadsheet;
    using XLReport.Builders;
    using XLReport.Configurations;
    using XLReport.Context;

    [TestClass]
    public sealed class Main
    {
        // Test Models (Simplified for testing purposes)
        public class TestModelOne
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class TestModelTwo
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
        }

        public class TestModelOneConfiguration : ReportTypeConfiguration<TestModelOne>
        {

            protected override void OnConfigure(ReportTypeBuilder<TestModelOne> builder)
            {
                builder.ToSheet("Index").WithTabColor();
                builder.Property(m => m.Name).WithColumnName("Test Name");
            }
        }

        public class TestModelTwoConfiguration : ReportTypeConfiguration<TestModelTwo>
        {
            protected override void OnConfigure(ReportTypeBuilder<TestModelTwo> builder)
            {
                // No overrides, uses defaults
            }
        }

        public class TestWorkbookContext : XLReportWorkbookContext
        {
            public XLReportSheet<TestModelOne> SheetOne => GetSheet<TestModelOne>();
            public XLReportSheet<TestModelTwo> SheetTwo => GetSheet<TestModelTwo>();

            protected override void OnXLReportCreating(XLReportBuilder builder)
            {
                builder.ApplyConfiguration(new TestModelOneConfiguration());
                builder.ApplyConfiguration(new TestModelTwoConfiguration());
            }
        }

        [TestMethod]
        public void TestColumnConfigurationOverride()
        {
            // Arrange
            var workbook = new TestWorkbookContext();
            var sheetOne = workbook.SheetOne;

            // Act & Assert
            var idColumn = sheetOne.Table.Columns["Id"];
            Assert.IsNotNull(idColumn, "Column 'Id' should exist with default name.");
            Assert.AreEqual(typeof(int), idColumn.DataType, "Column 'Id' should be of type int.");

            var nameColumn = sheetOne.Table.Columns["Full Name"];
            Assert.IsNotNull(nameColumn, "Column 'Full Name' should exist due to configuration override.");
            Assert.AreEqual(typeof(string), nameColumn.DataType, "Column 'Full Name' should be of type string.");
        }

        [TestMethod]
        public void TestDefaultColumnConfiguration()
        {
            // Arrange
            var workbook = new TestWorkbookContext();
            var sheetTwo = workbook.SheetTwo;

            // Act & Assert
            var productIdColumn = sheetTwo.Table.Columns["ProductId"];
            Assert.IsNotNull(productIdColumn, "Column 'ProductId' should exist with default name.");
            Assert.AreEqual(typeof(int), productIdColumn.DataType, "Column 'ProductId' should be of type int.");

            var productNameColumn = sheetTwo.Table.Columns["ProductName"];
            Assert.IsNotNull(productNameColumn, "Column 'ProductName' should exist with default name.");
            Assert.AreEqual(typeof(string), productNameColumn.DataType, "Column 'ProductName' should be of type string.");
        }
        [TestMethod]
        public void TestDataLoading()
        {
            // Arrange
            var workbook = new TestWorkbookContext();
            var sheetOne = workbook.SheetOne;
            var data = new List<TestModelOne>
            {
                new TestModelOne { Id = 1, Name = "John Doe" },
                new TestModelOne { Id = 2, Name = "Jane Smith" }
            };

            // Act
            sheetOne.LoadData(data);

            // Assert
            Assert.AreEqual(2, sheetOne.Table.Rows.Count, "Two rows should be loaded into the table.");

            var firstRow = sheetOne.Table.Rows[0];
            Assert.AreEqual(1, firstRow["Id"], "First row's 'Id' should be 1.");
            Assert.AreEqual("John Doe", firstRow["Full Name"], "First row's 'Full Name' should be 'John Doe'.");

            var secondRow = sheetOne.Table.Rows[1];
            Assert.AreEqual(2, secondRow["Id"], "Second row's 'Id' should be 2.");
            Assert.AreEqual("Jane Smith", secondRow["Full Name"], "Second row's 'Full Name' should be 'Jane Smith'.");
        }

        [TestMethod]
        public void TestDataRetrieval()
        {
            // Arrange
            var workbook = new TestWorkbookContext();
            var sheetOne = workbook.SheetOne;
            var data = new List<TestModelOne>
            {
                new TestModelOne { Id = 1, Name = "John Doe" }
            };

            // Act
            sheetOne.LoadData(data);
            var retrievedData = sheetOne.GetData();

            // Assert
            Assert.AreEqual(1, retrievedData.Count(), "One item should be retrieved from the table.");
            var item = retrievedData.First();
            Assert.AreEqual(1, item.Id, "Retrieved item's Id should be 1.");
            Assert.AreEqual("John Doe", item.Name, "Retrieved item's Name should be 'John Doe'.");
        }

        [TestMethod]
        public void TestSheetManagement()
        {
            // Arrange
            var workbook = new TestWorkbookContext();

            // Act & Assert
            Assert.IsNotNull(workbook.SheetOne, "SheetOne should be accessible via the workbook context.");
            Assert.IsNotNull(workbook.SheetTwo, "SheetTwo should be accessible via the workbook context.");

            Assert.AreEqual("SheetOne", workbook.SheetOne.Table.TableName, "SheetOne should have table name 'SheetOne'.");
            Assert.AreEqual("SheetTwo", workbook.SheetTwo.Table.TableName, "SheetTwo should have table name 'SheetTwo'.");
        }

        [TestMethod]
        public void TestDataTableProperties()
        {
            // Arrange
            var workbook = new TestWorkbookContext();
            var sheetOne = workbook.SheetOne;

            // Act & Assert
            Assert.IsNotNull(sheetOne.Table.PrimaryKey, "Primary key should be set for the DataTable.");
            Assert.AreEqual("InternalId", sheetOne.Table.PrimaryKey[0].ColumnName, "Primary key column should be 'InternalId'.");

            var internalIdColumn = sheetOne.Table.Columns["InternalId"];
            Assert.IsNotNull(internalIdColumn, "Column 'InternalId' should exist as the primary key.");
            Assert.AreEqual(typeof(Guid), internalIdColumn.DataType, "Column 'InternalId' should be of type Guid.");

            var idColumn = sheetOne.Table.Columns["Id"];
            Assert.AreEqual(typeof(int), idColumn.DataType, "Column 'Id' should be of type int.");

            var nameColumn = sheetOne.Table.Columns["Full Name"];
            Assert.AreEqual(typeof(string), nameColumn.DataType, "Column 'Full Name' should be of type string.");
        }
    }
}
