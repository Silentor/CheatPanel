using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
                RebuildFilteredList();
            }
        }

        [CreateProperty]
        public bool ShowWarnings
        {
            get => _showWarnings;
            set
            {
                _showWarnings = value;
                RebuildFilteredList();
            }
        }

        [CreateProperty]
        public bool ShowErrors
        {
            get => _showErrors;
            set
            {
                _showErrors = value;
                RebuildFilteredList();
            }
        }

        [CreateProperty]
        public String SearchField
        {
            get => _searchField;
            set
            {
                _searchField = value;
                RebuildFilteredList();
            }
        }


        public LogConsoleTab( ) : base ("Log")
        {
            _fullLog = new List<LogItem>( _capacity );
            _filteredLog = new List<LogItem>( _capacity );
            _writeBufferLocked = new List<LogItem>();
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
            UpdateLogBufferAsync( );
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

            Publish();
            RebuildFilteredList();
        }

        public override void Dispose( )
        {
            base.Dispose();

            Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
            StopLogging();
            _isDisposed = true;
        }

        private int _capacity = 1000;
        private readonly List<LogItem> _writeBufferLocked;
        private readonly List<LogItem> _writeBuffer;
        private readonly List<LogItem> _fullLog;
        private readonly List<LogItem> _filteredLog;

        private readonly Int32 _mainThreadId;
        private readonly Object _lock = new ( );
        private Boolean _isRecording;
        private Boolean _isAutoscroll;
        private Boolean _isDisposed;
        private Boolean _isWriteBufferEmpty = true;

        private int _infosCount;
        private int _warningsCount;
        private int _errorsCount;

        private ListView _log;
        private Int64 _version;
        private Boolean _showInfos = true;
        private Boolean _showWarnings = true;
        private Boolean _showErrors = true;
        private String _searchField;

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
                logItemElement.SetLogItem( timeText, logData.ThreadId != _mainThreadId ? $"||{logData.ThreadId}" : null, logData.Log, logData.StackTrace, logData.LogType );
            };
            _log.unbindItem += ( logItem, index ) =>
            {
                var logItemElement = (LogItemElement)logItem;
                logItemElement.Recycle();
            };

            _log.itemsSource = _filteredLog;

            var clearBtn = instance.Q<Button>( "ClearBtn" );
            clearBtn.clicked += ClearLog;

            var saveLogBtn = instance.Q<Button>( "SaveLogBtn" );
            saveLogBtn.clicked += SaveLogToFile;

            return instance;
        }

        private void SaveLogToFile( )
        {
            var persistentPath = Application.persistentDataPath;
            var logFileName    = $"Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            var path           = System.IO.Path.Combine( persistentPath, logFileName );

            try
            {
                using ( var fileWriter = System.IO.File.CreateText( path ) )
                {
                    foreach ( var logItem in _fullLog )
                    {
                        if ( logItem.ThreadId != _mainThreadId )
                            fileWriter.WriteLine( $"[{logItem.Time:HH:mm:ss.fff}]  {logItem.LogType} {logItem.ThreadId} {logItem.Log}" );
                        else
                            fileWriter.WriteLine( $"[{logItem.Time:HH:mm:ss.fff}]  {logItem.LogType} {logItem.Log}" );
                        if ( !String.IsNullOrEmpty( logItem.StackTrace ) )
                        {
                            using var sr = new System.IO.StringReader( logItem.StackTrace );
                            while ( sr.ReadLine() is { } line )
                            {
                                fileWriter.WriteLine( "    " + line );
                            }
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                Debug.LogError( $"[{nameof(LogConsoleTab)}] Error creating file for dumping Unity log: {e}" );
            }

            Debug.Log( $"[{nameof(LogConsoleTab)}] Log saved to file {path}" );
        }

        private void ClearLog( )
        {
            _infosCount = _warningsCount = _errorsCount = 0;
            _fullLog.Clear();
            _filteredLog.Clear();
            RebuildFilteredList();
            Publish();
        }

        private void LogMessageReceivedThreaded(String condition, String stackTrace, LogType type )
        {
            if( _isDisposed || !_isRecording )
                return;

            Assert.IsTrue( _writeBufferLocked.Count < _capacity, "Write buffer is full, something wrong with UpdateLogBuffer( )" );

            LogItem newMessage;
            var logThreadId = Thread.CurrentThread.ManagedThreadId;
            if( logThreadId == _mainThreadId)
                newMessage = new LogItem( DateTime.Now, condition, stackTrace, type, Time.frameCount, _mainThreadId );
            else
                newMessage = new LogItem( DateTime.Now, condition, stackTrace, type, 0 /*No frame info*/, logThreadId );

            lock ( _lock )
            {
                _writeBufferLocked.Add( newMessage );
                _isWriteBufferEmpty = false;
            }
        }

        private async void UpdateLogBufferAsync( )
        {
            while ( !_isDisposed )
            {
                if ( _isRecording )
                {
                    if ( !_isWriteBufferEmpty )
                    {
                        //Acquire new item from locked buffer
                        lock ( _lock )
                        {
                            _writeBuffer.Clear();
                            _writeBuffer.AddRange( _writeBufferLocked );
                            _writeBufferLocked.Clear();
                            _isWriteBufferEmpty = true;
                        }

                        //Remove old entries
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
                                                        
                            _fullLog.RemoveRange( 0, itemsToDelete );
                        }

                        //Add new entries
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

                        if ( IsVisible )
                        {
                            Publish();
                            UpdateFilteredList( _writeBuffer );
                        }
                    }
                }
                
                await Task.Delay( 500, CancellationToken.None );
            }
        }

        //Update filtered list with new values
        private void UpdateFilteredList( List<LogItem> newEntries )
        {
            //Some fast path's
            if ( _fullLog.Count == 0 )
            {
                if( _filteredLog.Count != 0 )
                {
                    _filteredLog.Clear();
                    _log.RefreshItems();
                    return;
                } 
            }
            else if( _filteredLog.Count == 0 )
            {
                foreach ( var logItem in _fullLog )
                {
                    if( IsLogItemVisible(logItem) )
                        _filteredLog.Add( logItem );
                }
                _log.RefreshItems();
                return;
            }
            
            //Remove the very old entries
            var wasDeleted = false;
            if ( _filteredLog.Count > 0 )
            {
                var oldestFullLogEntryTime = _fullLog[ 0 ].Time;
                if( _filteredLog[0].Time < oldestFullLogEntryTime )            //Very old entries found at the beginning of the filtered log
                {
                    if( _filteredLog[^1].Time < oldestFullLogEntryTime )        //Entire filtered log is very old
                    {
                        _filteredLog.Clear();
                        wasDeleted = true;
                    }
                    else
                    {
                        var indexOfFirstActualItem = _filteredLog.FindIndex( li => li.Time > oldestFullLogEntryTime );
                        _filteredLog.RemoveRange( 0, indexOfFirstActualItem );
                        wasDeleted = true;
                    }
                }
            }

            var wasAdded = false;
            foreach ( var newEntry in newEntries )
            {
                if ( IsLogItemVisible( newEntry ) )
                {
                    _filteredLog.Add( newEntry );
                    wasAdded = true;
                }
            }

            if( wasAdded || wasDeleted )
                _log.RefreshItems();
        }

        //Completely rebuild filtered list
        private void RebuildFilteredList( )
        {
            _filteredLog.Clear();
            foreach ( var logItem in _fullLog )
            {
                if( IsLogItemVisible( logItem ) )
                    _filteredLog.Add( logItem );
            }

            _log.RefreshItems();
        }

        private bool IsLogItemVisible( LogItem logItem )
        {
            if( logItem.LogType == LogType.Log )
            {
                if ( !_showInfos )
                    return false;
            }
            else if( logItem.LogType == LogType.Warning )
            {
                if ( !_showWarnings )
                    return false;
            }
            else 
            {
                if(  !_showErrors )
                    return false;
            }

            if ( !string.IsNullOrEmpty( _searchField ) && !logItem.Log.Contains( _searchField, StringComparison.OrdinalIgnoreCase ) && !logItem.StackTrace.Contains( _searchField, StringComparison.OrdinalIgnoreCase ) ) 
                return false;

            return true;
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