using System;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;
using Screen = UnityEngine.Device.Screen;
using SystemInfo = UnityEngine.Device.SystemInfo;
using Application = UnityEngine.Device.Application;

namespace Silentor.CheatPanel
{
    public class CheatPanel : MonoBehaviour
    {

        public VisualTreeAsset CheatPanelMaximized;
        public VisualTreeAsset CheatPanelMinimized;
        public VisualTreeAsset SystemTab;

        private UIDocument  _doc;
        private SettingsDTO _settings;
        private Settings    _settingsManager;
        private float       _updatedTimeScale = -1f;

        private void Awake( )
        {
            _doc             = GetComponent<UIDocument>();
            _settingsManager = new Settings();
            _settings        = _settingsManager.GetSettings();
            
            ShowPanel( );
        }

        private void ShowPanel( )
        {
            if( _settings.IsMaximized )
            {
                var root = _doc.rootVisualElement;
                root.Clear();
                var panelInstance = CheatPanelMaximized.Instantiate( );
                //panelInstance.style.paddingTop = Screen.height - Screen.safeArea.yMax;
                root.Add( panelInstance );
                var tabView = root.Q<TabView>( "CheatTabView" );
                var systemTab = CreateSystemTab( );
                tabView.Add( systemTab );
            }
            else
            {
                var root = _doc.rootVisualElement;
                root.Clear();
                var panelInstance = CheatPanelMinimized.Instantiate( );
                //panelInstance.style.paddingTop = Screen.height - Screen.safeArea.yMax;
                root.Add( panelInstance );
                var maxBtn = root.Q<Button>( "MaxBtn" );
                maxBtn.clicked += CheatPanelOnMinMaxToggleClicked;
            }
        }

        private void CheatPanelOnMinMaxToggleClicked( )
        {
            _settings.IsMaximized = !_settings.IsMaximized;
            _settingsManager.UpdateSettings();
            ShowPanel();
        }

        private Tab CreateSystemTab( )
        {
            var  result   = new Tab("System");
            var instance = SystemTab.Instantiate( );

            var sysInfoBox       = instance.Q<VisualElement>( "DeviceInfo" );
            var sysInfoLabel     = sysInfoBox.Q<Label>( "DeviceInfoValue" );
            var minBtn           = sysInfoBox.Q<Button>( "MinimizeBtn" );
            minBtn.clicked += CheatPanelOnMinMaxToggleClicked;
            var expandSysInfoBtn = sysInfoBox.Q<Button>( "ExpandBtn" );
            expandSysInfoBtn.clicked += ( ) => ExpandSysInfoOnClicked( sysInfoLabel, expandSysInfoBtn );

            var timescaleBox = instance.Q<VisualElement>( "TimeScale" );
            var timescaleLabel = timescaleBox.Q<Label>( "TimeScaleLbl" );
            var timescaleSlider = timescaleBox.Q<Slider>( "TimeScaleSlider" );
            timescaleSlider.RegisterValueChangedCallback( evt => Time.timeScale = evt.newValue );
            timescaleBox.Q<Button>( "TS_0" ).clicked   += ( ) => Time.timeScale = 0;
            timescaleBox.Q<Button>( "TS_0-1" ).clicked += ( ) => Time.timeScale = 0.1f;
            timescaleBox.Q<Button>( "TS_1" ).clicked   += ( ) => Time.timeScale = 1;
            _updatedTimeScale  =  -1;                                           //To force update
            timescaleBox.schedule.Execute( () =>
            {
                if( Math.Abs( Time.timeScale - _updatedTimeScale ) > 0.01f )
                {
                    _updatedTimeScale   = Time.timeScale;
                    timescaleLabel.text = $"TimeScale: {_updatedTimeScale:0.#}";
                    timescaleSlider.SetValueWithoutNotify( _updatedTimeScale );
                }
            } ).Every( 100 );

            result.Add( instance );
            return result;
        }

        private void ExpandSysInfoOnClicked( Label deviceInfoLabel, Button expandSysInfoBtn )
        {
            var wasExpanded = deviceInfoLabel.style.display == DisplayStyle.Flex;
            if ( wasExpanded )
            {
                deviceInfoLabel.style.display = DisplayStyle.None;
                expandSysInfoBtn.text = "Expand";
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
            var deviceInfo = $"Device: {SystemInfo.deviceModel}\n"            +
                             $"CPU: {SystemInfo.processorType}\n"             +
                             $"GPU: {SystemInfo.graphicsDeviceName}\n"        +
                             $"Name: {SystemInfo.deviceName}\n"               +
                             $"DUID: {SystemInfo.deviceUniqueIdentifier}\n"   +
                             $"OS: {SystemInfo.operatingSystem}\n"            +
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

    }
}
