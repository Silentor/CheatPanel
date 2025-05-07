using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = System.Object;

namespace Silentor.CheatPanel
{
    public class LogConsoleTab : IDisposable
    {
        public LogConsoleTab( )
        {
            _logBuffer = new Queue<LogItem>( _capacity );
            Application.logMessageReceivedThreaded += LogMessageReceivedThreaded;
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public void Dispose( )
        {
            Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
        }

        private int _capacity = 1000;
        private readonly Queue<LogItem> _logBuffer;
        private readonly Int32 _mainThreadId;
        private readonly Object _lock = new Object( );

        private void LogMessageReceivedThreaded(String condition, String stackTrace, LogType type )
        {
            var newMessage = new LogItem( condition, stackTrace, type, Thread.CurrentThread.ManagedThreadId );
            lock ( _lock )
            {
                while( _logBuffer.Count >= _capacity )                    
                    _logBuffer.Dequeue( );
                _logBuffer.Enqueue( newMessage );
            }
        }

        public readonly struct LogItem
        {
            public readonly String Log;
            public readonly String StackTrace;
            public readonly LogType LogType;
            public readonly int ThreadId;

            public LogItem(String log, String stackTrace, LogType logType, Int32 threadId )
            {
                Log = log;
                StackTrace = stackTrace;
                LogType = logType;
                ThreadId = threadId;
            }
        }
    }
}