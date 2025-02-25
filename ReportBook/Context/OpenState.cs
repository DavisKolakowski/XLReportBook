namespace ReportBook.Context
{
    using System.Collections.Generic;
    using System.IO;

    public class OpenState : ReportBookState
    {
        public override void Clear(ReportBookContext context)
        {
            context.ClearInternal();
        }

        public override void Refresh<TSchema>(ReportBookContext context, IEnumerable<TSchema> data)
        {
            context.RefreshInternal(data);
        }

        public override void SaveToStream(ReportBookContext context, Stream stream)
        {
            context.TransitionTo(new ExportingState());
            context.SaveToStream(stream);
        }

        public override void SaveToFile(ReportBookContext context, string filePath)
        {
            context.TransitionTo(new ExportingState());
            context.SaveToFile(filePath);
        }
    }
}
