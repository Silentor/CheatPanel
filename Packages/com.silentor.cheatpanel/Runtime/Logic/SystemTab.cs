using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using Screen = UnityEngine.Device.Screen;
using SystemInfo = UnityEngine.Device.SystemInfo;
using Application = UnityEngine.Device.Application;
using Object = System.Object;


namespace Silentor.CheatPanel
{
    /// <summary>
    /// Controller for System Tab in Cheat Panel
    /// </summary>
    public class SystemTab : IDataSourceViewHashProvider, /*INotifyBindablePropertyChanged,*/ IDisposable   //TODO INotifyBindablePropertyChanged doesnt work, investigate
    {
        private readonly FpsMeter _fpsMeter;
        private float _updatedTimeScale = -1f;
        private int   _updatedFps       = -2;
        private Int64 _version;
        private readonly CancellationTokenSource _cancelUpdates;

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

        public SystemTab( FpsMeter fpsMeter )
        {
            _fpsMeter = fpsMeter;
            _cancelUpdates = new CancellationTokenSource();
        }

        public VisualElement CreateContent( )
        {
            var instance = Resources.Content.Instantiate( );
            instance.dataSource = this;

            var sysInfoBox       = instance.Q<VisualElement>( "DeviceInfo" );
            var sysInfoLabel     = sysInfoBox.Q<Label>( "DeviceInfoValue" );
            var expandSysInfoBtn = sysInfoBox.Q<Button>( "ExpandBtn" );
            expandSysInfoBtn.clicked += ( ) => ExpandSysInfoOnClicked( sysInfoLabel, expandSysInfoBtn );

            var timescaleBox = instance.Q<VisualElement>( "TimeScale" );
            //var timescaleLabel = timescaleBox.Q<Label>( "TimeScaleLbl" );
            //var timescaleSlider = timescaleBox.Q<Slider>( "TimeScaleSlider" );
            //timescaleSlider.RegisterValueChangedCallback( evt => Time.timeScale = evt.newValue );
            timescaleBox.Q<Button>( "TS_0" ).clicked   += ( ) => Time.timeScale = 0;
            timescaleBox.Q<Button>( "TS_0-1" ).clicked += ( ) => Time.timeScale = 0.1f;
            timescaleBox.Q<Button>( "TS_1" ).clicked   += ( ) => Time.timeScale = 1;
            // timescaleBox.schedule.Execute( () =>
            // {
            //     if( Math.Abs( Time.timeScale - _updatedTimeScale ) > 0.01f )
            //     {
            //         _updatedTimeScale   = Time.timeScale;
            //         timescaleLabel.text = $"TimeScale: {_updatedTimeScale:0.#}";
            //         timescaleSlider.SetValueWithoutNotify( _updatedTimeScale );
            //     }
            // } ).Every( 100 );

            var fpsBox = instance.Q<VisualElement>( "FPS" );
            var fpsStatsLbl = fpsBox.Q<Label>( "FPSStatsLbl" );
            //var fpsSlider = fpsBox.Q<Slider>( "FPSSlider" );
            //var fpsTargetLbl = fpsBox.Q<Label>( "TargetFPS" );
            //fpsSlider.RegisterValueChangedCallback( evt => Application.targetFrameRate = (int)evt.newValue );
            // Mobile oriented controls
            fpsBox.Q<Button>( "FPS_X" ).clicked += () => Application.targetFrameRate = -1;
            fpsBox.Q<Button>( "FPS_15" ).clicked += () => Application.targetFrameRate = 15;
            fpsBox.Q<Button>( "FPS_30" ).clicked += () => Application.targetFrameRate = 30;
            fpsBox.Q<Button>( "FPS_60" ).clicked += () => Application.targetFrameRate = 60;
            //fpsStatsLbl.schedule.Execute( () => fpsStatsLbl.text = GetFPSStats() ).Every( 1000 );
            _fpsMeter.Updated +=  _  => { Publish(); };
            // fpsBox.schedule.Execute( ( ) =>
            // {
            //     if ( Application.targetFrameRate != _updatedFps )
            //     {
            //         _updatedFps = Application.targetFrameRate;
            //         fpsSlider.SetValueWithoutNotify( _updatedFps );
            //         fpsTargetLbl.text = $"FPS: {_updatedFps}";
            //     }
            // } ).Every( 100 );
            //todo add desktop controls (vSync)

            CheckUpdatesAsync( _cancelUpdates.Token );

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
        private async void CheckUpdatesAsync( CancellationToken cancel )
        {
            while ( !cancel.IsCancellationRequested )
            {
                if( Math.Abs( Time.timeScale - _updatedTimeScale ) > 0.01f )
                {
                    _updatedTimeScale = Time.timeScale;
                    //Notify( nameof(TimeScale) );
                    //Notify( nameof(TimeScaleLabel) );
                    Publish();
                }

                if ( Application.targetFrameRate != _updatedFps )
                {
                    _updatedFps = Application.targetFrameRate;
                    Publish();
                }

                await Task.Delay( 100, cancel );
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

        public void Dispose( )
        {
            _cancelUpdates.Cancel();
            _cancelUpdates.Dispose();
        }

#endregion


    }
}