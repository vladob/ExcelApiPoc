using System;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal sealed class ExcelBusyCursor : IDisposable
    {
        private readonly Excel.Application _application;
        private readonly Excel.XlMousePointer _previousCursor;
        private bool _disposed;

        public ExcelBusyCursor(Excel.Application application)
        {
            _application = application ??
                throw new ArgumentNullException(nameof(application));
            _previousCursor = _application.Cursor;
            _application.Cursor = Excel.XlMousePointer.xlWait;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _application.Cursor = _previousCursor;
            _disposed = true;
        }
    }
}
