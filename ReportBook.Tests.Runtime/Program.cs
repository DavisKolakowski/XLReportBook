namespace ReportBook.Tests.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using ReportBook.Models;
    using ReportBook.Samples;

    public class Program
    {
        public static void Main()
        {
            var context = new MyReportContext();
            context.Refresh(new[]
            {
                new MyFirstTestReport { Name = "Alice", CreatedDate = new DateTime(2025, 2, 25, 1, 3, 51, DateTimeKind.Utc), Value = 10 },
                new MyFirstTestReport { Name = "Bob", CreatedDate = new DateTime(2025, 2, 24, 1, 3, 51, DateTimeKind.Utc), Value = 20 }
            });
            context.Refresh(new[]
            {
                new MySecondTestReport { Description = "Item A", Amount = 5.5 },
                new MySecondTestReport { Description = "Item B", Amount = 15.5 }
            });

            var firstList = context.TestFirstReport.ToList();
            Console.WriteLine("First Report List:");
            foreach (var item in firstList)
            {
                Console.WriteLine($"Name: {item.Name}, CreatedDate: {item.CreatedDate} (Kind: {item.CreatedDate.Kind}), Value: {item.Value}");
            }

            Console.WriteLine("Second Report Table:");
            foreach (DataRow row in context.TestSecondReport.Data.Rows)
            {
                Console.WriteLine($"ID: {row["InternalId"]}, Description: {row["Description"]}, Amount: {row["Amount"]}");
            }

            using (var stream = new MemoryStream())
            {
                context.SaveToStream(stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }

            context.SaveToFile("reportbook.xlsx");

            context.Clear();
            Console.WriteLine($"First Report Count after Clear: {context.TestFirstReport.Data.Rows.Count}");
        }
    }
}
