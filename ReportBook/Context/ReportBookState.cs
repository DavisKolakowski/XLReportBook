namespace ReportBook.Context
{
    using ReportBook.Models;
    using System.Collections.Generic;
    using System.IO;

    public abstract class ReportBookState
    {
        public abstract void Clear(ReportBookContext context);
        public abstract void Refresh<TSchema>(ReportBookContext context, IEnumerable<TSchema> data) where TSchema : Report, new();
        public abstract void SaveToStream(ReportBookContext context, Stream stream);
        public abstract void SaveToFile(ReportBookContext context, string filePath);
    }
}
