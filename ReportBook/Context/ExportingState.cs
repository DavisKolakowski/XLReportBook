namespace ReportBook.Context
{
    using System.Collections.Generic;
    using System.IO;
    using System;

    public class ExportingState : ReportBookState
    {
        public override void Clear(ReportBookContext context)
        {
            throw new InvalidOperationException("Cannot clear data while exporting.");
        }

        public override void Refresh<TSchema>(ReportBookContext context, IEnumerable<TSchema> data)
        {
            throw new InvalidOperationException("Cannot refresh data while exporting.");
        }

        public override void SaveToStream(ReportBookContext context, Stream stream)
        {
            context.ExportToExcel(stream);
            context.TransitionTo(new OpenState());
        }

        public override void SaveToFile(ReportBookContext context, string filePath)
        {
            context.ExportToExcel(filePath);
            context.TransitionTo(new OpenState());
        }
    }
}
