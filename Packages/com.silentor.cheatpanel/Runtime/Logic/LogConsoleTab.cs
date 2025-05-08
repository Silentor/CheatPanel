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
                    _log.ScrollToItem( _fullLog.Count );
            }
        }

        [CreateProperty]
        public int InfosCount => _infosCount;

        [CreateProperty]
        public int WarningsCount => _warningsCount;

        [CreateProperty]
        public int ErrorsCount => _errorsCount;

        [CreateProperty]
        public bool ShowInfos
        {
            get => _showInfos;
            set
            {
                _showInfos = value;
                UpdateLogControl();
            }
        }

        [CreateProperty]
        public bool ShowWarningss
        {
            get => _showWarnings;
            set
            {
                _showWarnings = value;
                UpdateLogControl();
            }
        }

        [CreateProperty]
        public bool ShowErrors
        {
            get => _showErrors;
            set
            {
                _showErrors = value;
                UpdateLogControl();
            }
        }


        public LogConsoleTab( ) : base ("Log")
        {
            _fullLog = new List<LogItem>( _capacity );
            _filteredLog = new List<LogItem>( _capacity );
            _writeBuffer = new List<LogItem>();

            Application.logMessageReceivedThreaded += LogMessageReceivedThreaded;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;

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

        public override void Show( )
        {
            base.Show();

            UpdateLogControl();
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
        private readonly List<LogItem> _fullLog;
        private readonly List<LogItem> _filteredLog;

        private readonly Int32 _mainThreadId;
        private readonly Object _lock = new ( );
        private Boolean _isRecording;
        private Boolean _isAutoscroll;
        private Boolean _isDisposed;
        private Boolean _isWriteBufferNotEmpty;

        private int _infosCount;
        private int _warningsCount;
        private int _errorsCount;

        private ListView _log;
        private Int64 _version;
        private Boolean _showInfos = true;
        private Boolean _showWarnings = true;
        private Boolean _showErrors = true;

        protected override VisualElement GenerateCustomContent( )
        {
            var instance = Resources.Content.Instantiate();
            instance.dataSource = this;

            _log = instance.Q<ListView>(  );
            _log.makeItem += () => new LogItemElement();
            _log.bindItem += ( logItem, index ) =>
            {
                var logItemElement = (LogItemElement)logItem;
                var logData = _filteredLog[ index ];
                var timeText = $"[{logData.Time:HH:mm:ss}]";        //todo cache for consecutive items
                logItemElement.SetLogItem( timeText, logData.Log, logData.StackTrace, logData.LogType );
            };
            _log.unbindItem += ( logItem, index ) =>
            {
                var logItemElement = (LogItemElement)logItem;
                logItemElement.Recycle();
            };

            _log.itemsSource = _filteredLog;

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
                _isWriteBufferNotEmpty = true;
            }
        }

        private async void UpdateLogBuffer( )
        {
            while ( !_isDisposed )
            {
                if ( _isRecording )
                {
                    if ( _isWriteBufferNotEmpty )
                    {
                        lock ( _lock )
                        {
                            var itemsToDelete = _fullLog.Count + _writeBuffer.Count - _capacity;
                            if ( itemsToDelete > 0 )
                            {
                                for ( int i = _fullLog.Count - itemsToDelete; i < _fullLog.Count; i++ )
                                {
                                    if ( _fullLog[ i ].LogType == LogType.Log )
                                        _infosCount--;
                                    else if ( _fullLog[ i ].LogType == LogType.Warning )
                                        _warningsCount--;
                                    else 
                                        _errorsCount--;
                                }
                                                            
                                _fullLog.RemoveRange( _fullLog.Count - itemsToDelete, itemsToDelete );
                            }

                            foreach ( var item in _writeBuffer )
                            {
                                if ( item.LogType == LogType.Log )
                                    _infosCount++;
                                else if ( item.LogType == LogType.Warning )
                                    _warningsCount++;
                                else
                                    _errorsCount++;
                            }

                            _fullLog.AddRange( _writeBuffer );
                            _writeBuffer.Clear();
                            _isWriteBufferNotEmpty = false;
                        }

                        Publish();
                        if( IsVisible )
                            UpdateLogControl();
                    }
                }
                
                await Task.Delay( 500, CancellationToken.None );
            }
        }

        private void UpdateLogControl( )
        {
            _filteredLog.Clear();

            if ( _showInfos && _showWarnings && _showErrors )
            {
                _filteredLog.AddRange( _fullLog );
            }
            else if ( !_showInfos && !_showWarnings && !_showErrors )
            {
            }
            else 
            {
                foreach ( var logItem in _fullLog )
                {
                    if( logItem.LogType == LogType.Log )
                    {
                        if ( _showInfos )
                            _filteredLog.Add( logItem );
                    }
                    else if( logItem.LogType == LogType.Warning )
                    {
                        if ( _showWarnings )
                            _filteredLog.Add( logItem );
                    }
                    else 
                    {
                        if(  _showErrors )
                            _filteredLog.Add( logItem );
                    }
                }
            }

            _log.RefreshItems();
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