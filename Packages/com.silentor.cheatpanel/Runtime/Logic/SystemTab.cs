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
        private float _lastTImeScale = -1f;
        private int   _lastTargetFps       = -2;
        private Int64 _version;
        private Boolean _updateFPSHistogram = true;
        private FpsMeter.EFPSStats     _FPSHistogramMode = FpsMeter.EFPSStats.Worst;
        private FpsHistogram _histo;
        private FpsMeter.Stats[] _pausedHistogrammData;
        private Boolean _isDisposed;

        [CreateProperty ]
        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        [CreateProperty]
        public String TimeScaleLabel
        {
            get => $"TimeScale: {TimeScale:0.#}";
        }

        [CreateProperty ]
        public int TargetFPS
        {
            get => Application.targetFrameRate;
            set => Application.targetFrameRate = value;
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


        public SystemTab( FpsMeter fpsMeter, Settings settings ) : base ( "System" )
        {
            _fpsMeter = fpsMeter;
            _settings = settings;
        }

        protected override VisualElement GenerateCustomContent( )
        {
            var instance = Resources.Content.Instantiate( );
            instance.dataSource = this;

            var sysInfoBox       = instance.Q<VisualElement>( "DeviceInfo" );
            var sysInfoLabel     = sysInfoBox.Q<Label>( "DeviceInfoValue" );
            var expandSysInfoBtn = sysInfoBox.Q<Button>( "ExpandBtn" );
            expandSysInfoBtn.clicked += ( ) => ExpandSysInfoOnClicked( sysInfoLabel, expandSysInfoBtn );

            var timescaleBox = instance.Q<VisualElement>( "TimeScale" );
            timescaleBox.Q<Button>( "TS_0" ).clicked   += ( ) => Time.timeScale = 0;
            timescaleBox.Q<Button>( "TS_0-1" ).clicked += ( ) => Time.timeScale = 0.1f;
            timescaleBox.Q<Button>( "TS_1" ).clicked   += ( ) => Time.timeScale = 1;

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

            //todo add desktop controls (vSync)

            CheckUpdatesAsync( );

            return instance;
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
                             $" UID: {SystemInfo.deviceUniqueIdentifier}\n" +
                             $"OS: {SystemInfo.operatingSystem}\n"          +
                             //GetApplicationId() +
                             $"App Version: {Application.version}\n"          +
                             $"Lang: {Application.systemLanguage}\n"          +
                             $"Unity: {Application.unityVersion}\n"           +
                             $"Pers path: {Application.persistentDataPath}\n" +
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
                    if( Math.Abs( Time.timeScale - _lastTImeScale ) > 0.01f )
                    {
                        _lastTImeScale = Time.timeScale;
                        //Notify( nameof(TimeScale) );
                        //Notify( nameof(TimeScaleLabel) );
                        Publish();
                    }

                    if ( Application.targetFrameRate != _lastTargetFps )
                    {
                        _lastTargetFps = Application.targetFrameRate;
                        Publish();
                    }
                }

                await Task.Delay( 100, CancellationToken.None );
            }
        }

        private static class Resources
        {
            public static readonly VisualTreeAsset Content = UnityEngine.Resources.Load<VisualTreeAsset>( "SystemTab" );
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