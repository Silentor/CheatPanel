using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Silentor.CheatPanel.UI;
using Unity.Profiling;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
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
                ProcessFilterChange();
            }
        }

        [CreateProperty]
        public bool ShowWarnings
        {
            get => _showWarnings;
            set
            {
                _showWarnings = value;
                ProcessFilterChange();
            }
        }

        [CreateProperty]
        public bool ShowErrors
        {
            get => _showErrors;
            set
            {
                _showErrors = value;
                ProcessFilterChange();
            }
        }

        [CreateProperty]
        public String SearchField
        {
            get => _searchField;
            set
            {
                _searchField = value;
                ProcessFilterChange();
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
            ProcessFilterChange();
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
        private Boolean _isAutoscroll = true;
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
        private Boolean _isFiltered;

        private String _cachedTimeToSeconds;
        private Int32 _cachedTimeToSecondsHash = -1;

        private readonly ProfilerMarker _logItemSampler = new ( ProfilerCategory.Scripts, $"{nameof(LogConsoleTab)}.{nameof(LogMessageReceivedThreaded)}" );
        private readonly ProfilerMarker _updateFilteredListSampler = new ( ProfilerCategory.Scripts, $"{nameof(LogConsoleTab)}.{nameof(UpdateList)}" );
        private readonly ProfilerMarker _rebuildFilteredListSampler = new ( ProfilerCategory.Scripts, $"{nameof(LogConsoleTab)}.{nameof(ProcessFilterChange)}" ); 

        protected override VisualElement GenerateCustomContent( )
        {
            var instance = Resources.Content.Instantiate();
            instance.dataSource = this;

            _log = instance.Q<ListView>(  );
            _log.makeItem += () => new LogItemElement();
            _log.bindItem += ( logItem, index ) =>
            {
                var logItemElement = (LogItemElement)logItem;
                var logData = _isFiltered ? _filteredLog[ index ] : _fullLog[ index ];
                var timeText = GetCachedTimeToSeconds( logData.Time );
                logItemElement.SetLogItem( timeText, logData.ThreadId != _mainThreadId ? $"||{logData.ThreadId}" : null, logData.Log, logData.StackTrace, logData.LogType );
            };
            _log.unbindItem += ( logItem, index ) =>
            {
                var logItemElement = (LogItemElement)logItem;
                logItemElement.Recycle();
            };
            _log.makeNoneElement += () =>
            {
                var emptyLogHelpLbl = new Label( "Shows Unity log messages. Double tap on message to display stack trace. Long press to copy message to clipboard." );
                emptyLogHelpLbl.style.whiteSpace = WhiteSpace.Normal;
                emptyLogHelpLbl.style.unityFontStyleAndWeight = FontStyle.Italic;
                return emptyLogHelpLbl;
            };

            _log.itemsSource = _isFiltered ? _filteredLog : _fullLog;

            var clearBtn = instance.Q<Button>( "ClearBtn" );
            clearBtn.clicked += ClearLog;

            var saveLogBtn = instance.Q<Button>( "SaveLogBtn" );
            saveLogBtn.clicked += SaveLogToFile;

            return instance;
        }

        protected override Button GenerateTabButton( )
        {
            var btn = new Button();
            btn.style.backgroundImage = Background.FromSprite( Resources.TabButtonIcon );
            btn.style.backgroundSize = new StyleBackgroundSize( new BackgroundSize( BackgroundSizeType.Contain ) );
            btn.tooltip = "Unity log tab";
            return btn;
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
            _log.RefreshItems();
            Publish();
        }

        private void LogMessageReceivedThreaded(String condition, String stackTrace, LogType type )
        {
            if( _isDisposed || !_isRecording )
                return;

            _logItemSampler.Begin();

            LogItem newMessage;
            var logThreadId = Thread.CurrentThread.ManagedThreadId;
            if( logThreadId == _mainThreadId)
                newMessage = new LogItem( DateTime.Now, condition, stackTrace, type, Time.frameCount, _mainThreadId );//todo cache DateTime string for consecutive items
            else
                newMessage = new LogItem( DateTime.Now, condition, stackTrace, type, 0 /*No frame info from other threads*/, logThreadId );

            lock ( _lock )
            {
                if( _writeBufferLocked.Count >= _capacity )
                {
                    //Something wrong with UpdateLogBuffer( ), messages are not consumed fast enough. Drop the latest message
                }
                else
                {
                    _writeBufferLocked.Add( newMessage );
                    _isWriteBufferEmpty = false;
                }
            }

            _logItemSampler.End();
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
                            UpdateList( _writeBuffer );
                            if( IsAutoscroll )
                                _log.ScrollToItem( _log.itemsSource.Count - 1 );
                        }
                    }
                }
                
                await Task.Delay( 500, CancellationToken.None );
            }
        }

        //Update list with new values
        private void UpdateList( List<LogItem> newEntries )
        {
            _updateFilteredListSampler.Begin();

            //Some fast path's
            if ( !_isFiltered )
            {
                _log.RefreshItems();            //RefreshItem() doesn't work here

                _updateFilteredListSampler.End();
                return;
            }
            
            if ( _fullLog.Count == 0 )
            {
                if( _filteredLog.Count != 0 )
                {
                    _filteredLog.Clear();
                    _log.RefreshItems();
                    _updateFilteredListSampler.End();
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
                _updateFilteredListSampler.End();
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
                        var indexOfFirstActualItem = _filteredLog.FindIndex( li => li.Time >= oldestFullLogEntryTime );
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

            _updateFilteredListSampler.End();
        }

        private void ProcessFilterChange( )
        {
            _rebuildFilteredListSampler.Begin();

            _isFiltered = IsFiltered();

            if ( _isFiltered )
            {
                _filteredLog.Clear();
                foreach ( var logItem in _fullLog )
                {
                    if( IsLogItemVisible( logItem ) )
                        _filteredLog.Add( logItem );
                }

                _log.itemsSource = _filteredLog;
                _log.RefreshItems();
            }
            else
            {
                _log.itemsSource = _fullLog;
                _log.RefreshItems();
            }

            if( IsAutoscroll )
                _log.ScrollToItem( _log.itemsSource.Count - 1 );

            _rebuildFilteredListSampler.End();
        }

        private bool IsLogItemVisible( LogItem logItem )
        {
            if ( !_isFiltered )
                return true;

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

        private Boolean IsFiltered( )
        {
            return !_showInfos || !_showWarnings || !_showErrors || !string.IsNullOrEmpty( _searchField );
        }

        private String GetCachedTimeToSeconds( DateTime dateTime )
        {
            var timeHash = (dateTime.Hour << 16) | (dateTime.Minute << 8) | dateTime.Second;
            if( timeHash != _cachedTimeToSecondsHash )
            {
                _cachedTimeToSecondsHash = timeHash;
                _cachedTimeToSeconds = $"[{dateTime:HH:mm:ss}]";
            }

            return _cachedTimeToSeconds;
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
            public static readonly Sprite TabButtonIcon = UnityEngine.Resources.Load<Sprite>( "UnityEditor.ConsoleWindow@2x" );
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