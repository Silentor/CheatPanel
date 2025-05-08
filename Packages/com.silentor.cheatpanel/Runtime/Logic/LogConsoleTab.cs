using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Silentor.CheatPanel.UI;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel
{
    [GeneratePropertyBag]
    public partial class LogConsoleTab : CheatTab, IDataSourceViewHashProvider, IDisposable
    {
        public const string LogUssClassName = "log";
        public const string LogItemUssClassName = LogUssClassName + "__item";

        [CreateProperty]
        public Boolean IsRecording 
        {
            get => _isRecording;
            set
            {
                if ( value )
                    StartLogging();
                else
                    StopLogging();
            }
        }

        [CreateProperty]
        public Boolean IsAutoscroll
        {
            get => _isAutoscroll;
            set
            {
                _isAutoscroll = value;
                if( value )
                    _log.ScrollToItem( _logBuffer.Count );
            }
        }

        public LogConsoleTab( ) : base ("Log")
        {
            _logBuffer = new List<LogItem>( _capacity );
            _writeBuffer = new List<LogItem>();

            Application.logMessageReceivedThreaded += LogMessageReceivedThreaded;
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

            StartLogging();
        }

        public void StartLogging( )
        {
            if ( _isDisposed )
                throw new ObjectDisposedException( nameof(LogConsoleTab) );

            if( _isRecording )
                return;

            _isRecording = true;
            Publish();
            UpdateLogBuffer( );
        }

        public void StopLogging( )
        {
            if ( _isDisposed )
                throw new ObjectDisposedException( nameof(LogConsoleTab) );

            if( !_isRecording )
                return;

            _isRecording = false;
            Publish();
        }

        public override void Dispose( )
        {
            base.Dispose();

            Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
            StopLogging();
            _isDisposed = true;
        }

        private int _capacity = 1000;
        private readonly List<LogItem> _writeBuffer;
        private readonly List<LogItem> _logBuffer;
        private readonly Int32 _mainThreadId;
        private readonly Object _lock = new ( );
        private Boolean _isRecording;
        private Boolean _isAutoscroll;
        private Boolean _isDisposed;
        private Boolean _isLogItemsWaiting;
        private ListView _log;
        private Int64 _version;

        protected override VisualElement GenerateCustomContent( )
        {
            var instance = Resources.Content.Instantiate();
            instance.dataSource = this;

            _log = instance.Q<ListView>(  );
            _log.makeItem += () => new LogItemElement();
            _log.bindItem += ( logItem, index ) =>
            {
                var logItemElement = (LogItemElement)logItem;
                var logData = _logBuffer[ index ];
                var timeText = $"[{logData.Time:HH:mm:ss}]";        //todo cache for consecutive items
                logItemElement.SetLogItem( timeText, logData.Log, logData.StackTrace, logData.LogType );
            };
            _log.unbindItem += ( logItem, index ) =>
            {
                var logItemElement = (LogItemElement)logItem;
                logItemElement.Recycle();
            };

            _log.itemsSource = _logBuffer;

            return instance;
        }

        private void LogMessageReceivedThreaded(String condition, String stackTrace, LogType type )
        {
            if( _isDisposed || !_isRecording )
                return;

            Assert.IsTrue( _writeBuffer.Count < _capacity, "Write buffer is full, something wrong with UpdateLogBuffer( )" );

            var newMessage = new LogItem( DateTime.Now, condition, stackTrace, type, Time.frameCount, Thread.CurrentThread.ManagedThreadId );
            lock ( _lock )
            {
                _writeBuffer.Add( newMessage );
                _isLogItemsWaiting = true;
            }
        }

        private async void UpdateLogBuffer( )
        {
            while ( !_isDisposed )
            {
                if ( _isRecording )
                {
                    if ( _isLogItemsWaiting )
                    {
                        lock ( _lock )
                        {
                            var itemsToDelete = _logBuffer.Count + _writeBuffer.Count - _capacity;
                            if( itemsToDelete > 0 )                                
                                _logBuffer.RemoveRange( _logBuffer.Count - itemsToDelete, itemsToDelete );

                            _logBuffer.AddRange( _writeBuffer );
                            _writeBuffer.Clear();
                            _isLogItemsWaiting = false;
                        }

                        if( IsVisible )
                            _log.RefreshItems();
                    }
                }
                
                await Task.Delay( 100, CancellationToken.None );
            }
        }

        public readonly struct LogItem
        {
            public readonly DateTime Time;
            public readonly String Log;
            public readonly String StackTrace;
            public readonly LogType LogType;
            public readonly Int32 FrameCount;
            public readonly int ThreadId;

            public LogItem( DateTime time, String log, String stackTrace, LogType logType, int frameCount, Int32 threadId )
            {
                Time = time;
                Log = log;
                StackTrace = stackTrace;
                LogType = logType;
                FrameCount = frameCount;
                ThreadId = threadId;
            }
        }

        private static class Resources
        {
            public static readonly VisualTreeAsset Content = UnityEngine.Resources.Load<VisualTreeAsset>( "UnityLogTab" );
            public static readonly VisualTreeAsset LogItem = UnityEngine.Resources.Load<VisualTreeAsset>( "UnityLogItem" );
        }

#region IDataSourceViewHashProvider

        Int64  IDataSourceViewHashProvider.GetViewHashCode( )
        {
            return _version;
        }

        private void Publish( )
        {
            unchecked { _version++; }
        }

#endregion
    }
}