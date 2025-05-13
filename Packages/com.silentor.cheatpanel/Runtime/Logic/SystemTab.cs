using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Silentor.CheatPanel.UI;
using Silentor.CheatPanel.Utils;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Screen = UnityEngine.Device.Screen;
using SystemInfo = UnityEngine.Device.SystemInfo;
using Application = UnityEngine.Device.Application;
using Debug = UnityEngine.Debug;
using Object = System.Object;

[assembly: GeneratePropertyBagsForAssembly] 

namespace Silentor.CheatPanel
{
    /// <summary>
    /// Controller for System Tab in Cheat Panel
    /// </summary>
    [GeneratePropertyBag]
    public partial class SystemTab : CheatTab, IDataSourceViewHashProvider, /*INotifyBindablePropertyChanged,*/ IDisposable   //TODO INotifyBindablePropertyChanged doesnt work, investigate
    {
        private readonly FpsMeter _fpsMeter;
        private readonly Settings _settings;
        private Int64 _version;
        private Boolean _updateFPSHistogram = true;
        private FpsMeter.EFPSStats     _FPSHistogramMode = FpsMeter.EFPSStats.Worst;
        private FpsHistogram _histo;
        private FpsMeter.Stats[] _pausedHistogrammData;
        private Boolean _isDisposed;
        private ILogger _logger;


        private SmartProperty<Single> _timeScaleProperty;
        private SmartProperty<Int32> _targetFrameRateProperty;
        private SmartProperty<Boolean> _logEnabledProperty;
        private SmartPropertyEnum<ELogTypeSorted> _logLevelProperty;
        private SmartPropertyEnum<EStackTraceLogType> _logStackProperty;
        private SmartPropertyEnum<EStackTraceLogType> _logErrorStackProperty;
        private SmartProperty<Boolean> _devConsoleEnabledProperty;
        private SmartProperty<Boolean> _devConsoleVisibleProperty;

        [CreateProperty ]
        public float TimeScale
        {
            get => _timeScaleProperty.Value;
            set => _timeScaleProperty.Value = value;
        }

        [CreateProperty]
        public String TimeScaleLabel
        {
            get => $"TimeScale: {TimeScale:0.#}";
        }

        [CreateProperty ]
        public int TargetFPS
        {
            get => _targetFrameRateProperty.Value;
            set => _targetFrameRateProperty.Value = value;
        }

        [CreateProperty]
        public String TargetFPSLabel
        {
            get => $"Target FPS: {Application.targetFrameRate}";
        }

        [CreateProperty]
        public string FPSStatsString
        {
            get => $"FPS avg {_fpsMeter.AverageFPS}, 90% {_fpsMeter.Percentile90FPS}, 99% {_fpsMeter.Percentile99FPS}";
        }

        [CreateProperty]
        public Enum FPSHistoModeIndex
        {
            get => _FPSHistogramMode;
            set
            {
                _FPSHistogramMode = (FpsMeter.EFPSStats)value;
                _settings.GetSettings().FPSHistogrammMode = (int)_FPSHistogramMode;
                if( !FPSUpdateHistoMode )
                    _histo.SetFPS( _pausedHistogrammData, _fpsMeter.LastStatsCapacity, 1f / OnDemandRendering.effectiveRenderFrameRate, (FpsMeter.EFPSStats)FPSHistoModeIndex );
            }
        }

        [CreateProperty]
        public Boolean FPSUpdateHistoMode
        {
            get => _updateFPSHistogram;
            set
            {
                _updateFPSHistogram = value;
                _settings.GetSettings().UpdateFPSHistogramm = value;
                if ( !_updateFPSHistogram )
                {
                    _pausedHistogrammData = _fpsMeter.LastStats.ToArray();
                }
            }
        }

        [CreateProperty]
        public StackTraceLogType LogStackTraceType
        {
            get => Application.GetStackTraceLogType( LogType.Log );
            set => Application.SetStackTraceLogType( LogType.Log, value );
        }

        [CreateProperty]
        public Boolean LogEnabled
        {
            get => _logEnabledProperty.Value;
            set => _logEnabledProperty.Value = value;
        }

        [CreateProperty]
        public ELogTypeSorted LogLevel
        {
            get => _logLevelProperty.Value;
            set => _logLevelProperty.Value = value;
        }

        [CreateProperty]
        public EStackTraceLogType LogStackTrace
        {
            get => _logStackProperty.Value;
            set => _logStackProperty.Value = value;
        }

        [CreateProperty]
        public EStackTraceLogType ErrorStackTrace
        {
            get => _logErrorStackProperty.Value;
            set => _logErrorStackProperty.Value = value;
        }

        [CreateProperty]
        public Boolean DevConsoleEnabled
        {
            get => _devConsoleEnabledProperty.Value;
            set => _devConsoleEnabledProperty.Value = value;
        }

        [CreateProperty]
        public Boolean DevConsoleVisible
        {
            get => _devConsoleVisibleProperty.Value;
            set => _devConsoleVisibleProperty.Value = value;
        }

        public SystemTab( FpsMeter fpsMeter, Settings settings, ILogger logger = null ) : base ( "System" )
        {
            _fpsMeter = fpsMeter;
            _settings = settings;
            _logger = logger ?? Debug.unityLogger;
        }

        protected override VisualElement GenerateCustomContent( )
        {
            var instance = Resources.Content.Instantiate( );
            instance.dataSource = this;

            //Device, OS and app section
            var sysInfoBox       = instance.Q<VisualElement>( "DeviceInfo" );
            var sysInfoLabel     = sysInfoBox.Q<Label>( "DeviceInfoValue" );
            var expandSysInfoBtn = sysInfoBox.Q<Button>( "ExpandBtn" );
            expandSysInfoBtn.clicked += ( ) => ExpandSysInfoOnClicked( sysInfoLabel, expandSysInfoBtn );

            var timescaleBox = instance.Q<VisualElement>( "TimeScale" );
            timescaleBox.Q<Button>( "TS_0" ).clicked   += ( ) => Time.timeScale = 0;
            timescaleBox.Q<Button>( "TS_0-1" ).clicked += ( ) => Time.timeScale = 0.1f;
            timescaleBox.Q<Button>( "TS_1" ).clicked   += ( ) => Time.timeScale = 1;
            _timeScaleProperty = new SmartProperty<Single>( () => Time.timeScale, v => Time.timeScale = v );

            //FPS section
            var fpsBox = instance.Q<VisualElement>( "FPS" );
            // Mobile oriented controls
            fpsBox.Q<Button>( "FPS_X" ).clicked += () => Application.targetFrameRate = -1;
            fpsBox.Q<Button>( "FPS_15" ).clicked += () => Application.targetFrameRate = 15;
            fpsBox.Q<Button>( "FPS_30" ).clicked += () => Application.targetFrameRate = 30;
            fpsBox.Q<Button>( "FPS_60" ).clicked += () => Application.targetFrameRate = 60;
            _histo = fpsBox.Q<FpsHistogram>( "FpsHistogram" );
            _fpsMeter.Updated +=  fpsMeter  =>
            {
                if ( FPSUpdateHistoMode && IsVisible )
                {
                    //var timer = Stopwatch.StartNew();
                    _histo.SetFPS( fpsMeter.LastStats, fpsMeter.LastStatsCapacity, 1f / OnDemandRendering.effectiveRenderFrameRate, (FpsMeter.EFPSStats)FPSHistoModeIndex );
                    //timer.Stop();
                    //Debug.Log( $"fps histo {timer.Elapsed.TotalMicroseconds()} mks" ); 
                }

                Publish();
            };
            _FPSHistogramMode = _settings.GetSettings().GetFPSHistogrammMode();
            _updateFPSHistogram = _settings.GetSettings().UpdateFPSHistogramm;
            _targetFrameRateProperty = new SmartProperty<Int32>( () => Application.targetFrameRate, v => Application.targetFrameRate = v );

            //todo add desktop controls (vSync)

            //Logger section
            _logger = Debug.unityLogger;
            _logEnabledProperty = new SmartProperty<bool>( () => _logger.logEnabled, value => _logger.logEnabled = value );
            _logLevelProperty = new SmartPropertyEnum<ELogTypeSorted>( () => LogTypeConvert( _logger.filterLogType ), value => _logger.filterLogType = LogTypeConvert( value ) );
            _logStackProperty = new SmartPropertyEnum<EStackTraceLogType>( ( ) => StackTraceConvert( Application.GetStackTraceLogType( LogType.Log ) ),
                    value  =>
                    {
                        var valueConverted = StackTraceConvert( value );
                        Application.SetStackTraceLogType( LogType.Log, valueConverted );
                        Application.SetStackTraceLogType( LogType.Warning, valueConverted );
                    } );
            _logErrorStackProperty = new SmartPropertyEnum<EStackTraceLogType>( ( ) => StackTraceConvert( Application.GetStackTraceLogType( LogType.Error ) ),
                    value  =>
                    {
                        var valueConverted = StackTraceConvert( value );
                        Application.SetStackTraceLogType( LogType.Error, valueConverted );
                        Application.SetStackTraceLogType( LogType.Exception, valueConverted );
                        Application.SetStackTraceLogType( LogType.Assert, valueConverted );
                    } );
            _devConsoleEnabledProperty = new SmartProperty<Boolean>( () => Debug.developerConsoleEnabled, v => Debug.developerConsoleEnabled = v );
            _devConsoleVisibleProperty = new SmartProperty<Boolean>( () => Debug.developerConsoleVisible, v => Debug.developerConsoleVisible = v );
            
            CheckUpdatesAsync( );

            return instance;
        }

        protected override Button GenerateTabButton( )
        {
            var btn = new Button();
            btn.style.backgroundImage = Background.FromSprite( Resources.TabButtonIcon );
            btn.style.backgroundSize  = new StyleBackgroundSize( new BackgroundSize( BackgroundSizeType.Contain ) );
            btn.tooltip = "Unity system settings tab";
            return btn;
        }

        private void ExpandSysInfoOnClicked( Label deviceInfoLabel, Button expandSysInfoBtn )
        {
            var wasExpanded = deviceInfoLabel.style.display == DisplayStyle.Flex;
            if ( wasExpanded )
            {
                deviceInfoLabel.style.display = DisplayStyle.None;
                expandSysInfoBtn.text         = "Expand";
            }
            else
            {
                deviceInfoLabel.style.display = DisplayStyle.Flex;
                expandSysInfoBtn.text         = "Collapse";
                deviceInfoLabel.text          = GetDeviceAndAppInfo();
            }
        }

        private string GetDeviceAndAppInfo( )
        {
            var deviceInfo = $"Device: {SystemInfo.deviceModel}\n"          +
                             $"CPU: {SystemInfo.processorType}\n"           +
                             $"GPU: {SystemInfo.graphicsDeviceName}\n"      +
                             $"Name: {SystemInfo.deviceName}\n"             +
                             $"UID: {SystemInfo.deviceUniqueIdentifier}\n" +
                             $"OS: {SystemInfo.operatingSystem}\n"          +
                             //GetApplicationId() +
                             $"App Version: {Application.version}\n"          +
                             $"Lang: {Application.systemLanguage}\n"          +
                             $"Unity: {Application.unityVersion}\n"           +
                             $"Pers path: {Application.persistentDataPath}\n" +
                             (!String.IsNullOrEmpty(Application.consoleLogPath) ? $"Log path: {Application.consoleLogPath}\n" : "") +
                             $"Screen res: {Screen.currentResolution}\n"      +
                             $"DPI: {Screen.dpi}\n";

            return deviceInfo;
        }

        private String GetApplicationId( )
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_TVOS || UNITY_STANDALONE_OSX
            return $"App id {Application.identifier}\n";
#endif
            return String.Empty;
        }

        private static LogType LogTypeConvert( ELogTypeSorted logType )
        {
            return logType switch
            {
                ELogTypeSorted.Log      => LogType.Log,
                ELogTypeSorted.Warn     => LogType.Warning,
                ELogTypeSorted.Assert   => LogType.Assert,
                ELogTypeSorted.Error    => LogType.Error,
                ELogTypeSorted.Exception => LogType.Exception,
                _                       => throw new ArgumentOutOfRangeException( nameof(logType), logType, null )
            };
        }
        
        private static ELogTypeSorted LogTypeConvert( LogType logType )
        {
            return logType switch
            {
                LogType.Log      => ELogTypeSorted.Log,
                LogType.Warning  => ELogTypeSorted.Warn,
                LogType.Assert   => ELogTypeSorted.Assert,
                LogType.Error    => ELogTypeSorted.Error,
                LogType.Exception => ELogTypeSorted.Exception,
                _               => throw new ArgumentOutOfRangeException( nameof(logType), logType, null )
            };
        }

        private static StackTraceLogType StackTraceConvert( EStackTraceLogType logType )
        {
            return logType switch
            {
                EStackTraceLogType.None  => StackTraceLogType.None,
                EStackTraceLogType.Script => StackTraceLogType.ScriptOnly,
                EStackTraceLogType.Full   => StackTraceLogType.Full,
                _                        => throw new ArgumentOutOfRangeException( nameof(logType), logType, null )
            };
        }

        private static EStackTraceLogType StackTraceConvert( StackTraceLogType logType )
        {
            return logType switch
            {
                StackTraceLogType.None  => EStackTraceLogType.None,
                StackTraceLogType.ScriptOnly => EStackTraceLogType.Script,
                StackTraceLogType.Full   => EStackTraceLogType.Full,
                _                        => throw new ArgumentOutOfRangeException( nameof(logType), logType, null )
            };
        }

        /// <summary>
        /// Frequently check for updates of some system settings
        /// </summary>
        /// <param name="cancel"></param>
        private async void CheckUpdatesAsync( )
        {
            while ( !_isDisposed )
            {
                if ( IsVisible )
                {
                    if( _timeScaleProperty.CheckExternalChanges() )
                    {
                        //Notify( nameof(TimeScale) );
                        //Notify( nameof(TimeScaleLabel) );
                        Publish();
                    }

                    if ( _targetFrameRateProperty.CheckExternalChanges() )
                    {
                        Publish();
                    }

                    if( _logEnabledProperty.CheckExternalChanges() || _logLevelProperty.CheckExternalChanges() )
                        Publish();

                    if( _logStackProperty.CheckExternalChanges() || _logErrorStackProperty.CheckExternalChanges() )
                        Publish();

                    if( _devConsoleEnabledProperty.CheckExternalChanges() || _devConsoleVisibleProperty.CheckExternalChanges() )
                        Publish();

                }

                await Task.Delay( 100, CancellationToken.None );
            }
        }

        private static class Resources
        {
            public static readonly VisualTreeAsset Content = UnityEngine.Resources.Load<VisualTreeAsset>( "SystemTab" );
            public static readonly Sprite TabButtonIcon = UnityEngine.Resources.Load<Sprite>( "BuildSettings.Editor" );
        }

        public enum ELogTypeSorted
        {
            Log,
            Warn,
            Assert,
            Error,
            Exception
        }

        public enum EStackTraceLogType
        {
            None,
            Script,
            Full,
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

#region INotifyBindablePropertyChanged

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        // private void Notify([CallerMemberName] string property = "")
        // {
        //     propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        // }

#endregion

#region IDisposable

        public override void Dispose( )
        {
            base.Dispose();

            _isDisposed = true;
        }

#endregion


    }
}