using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Scout.Utils.Logging.Seriglog.Tests
{
    /// <summary>
    /// Фэйковый синк для тестирования Serilog. 
    /// </summary>
    public class FakeSink : ILogEventSink
    {
        readonly Action<LogEvent> _writeAction;

        public FakeSink(Action<LogEvent> writeAction)
        {
            if (writeAction == null) throw new ArgumentNullException(nameof(writeAction));
            _writeAction = writeAction;
        }

        public void Emit(LogEvent logEvent)
        {
            _writeAction.Invoke(logEvent);
        }
    }
}