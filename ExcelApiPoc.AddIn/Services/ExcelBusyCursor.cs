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

            try
            {
                _application.Cursor = _previousCursor;
            }
            finally
            {
                _disposed = true;
            }
        }
    }

    internal sealed class ExcelApplicationStateScope : IDisposable
    {
        private readonly Excel.Application _application;
        private readonly bool _previousScreenUpdating;
        private readonly bool _previousEnableEvents;
        private readonly bool _disableEvents;
        private bool _disposed;

        public ExcelApplicationStateScope(
            Excel.Application application,
            bool disableEvents = true)
        {
            _application = application ??
                throw new ArgumentNullException(nameof(application));

            _disableEvents = disableEvents;
            _previousScreenUpdating = _application.ScreenUpdating;
            _previousEnableEvents = _application.EnableEvents;

            _application.ScreenUpdating = false;

            if (_disableEvents)
                _application.EnableEvents = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                if (_disableEvents)
                {
                    try
                    {
                        _application.EnableEvents =
                            _previousEnableEvents;
                    }
                    catch
                    {
                        // Best effort: do not mask the original operation error.
                    }
                }

                try
                {
                    _application.ScreenUpdating =
                        _previousScreenUpdating;
                }
                catch
                {
                    // Best effort: do not mask the original operation error.
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
