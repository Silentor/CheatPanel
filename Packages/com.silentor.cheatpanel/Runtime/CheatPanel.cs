using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
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
        /// <summary>
        /// Add cheats to the cheat panel.
        /// </summary>
        /// <param name="cheats"></param>
        public void AddCheats( ICheats cheats )
        {
            AddCheatsInternal( cheats );
            if( _settings.IsMaximized )
            {
                ShowPanel();
            }
        } 

        private          UIDocument     _doc;
        private          SettingsDTO    _settings;
        private          Settings       _settingsManager;
        private          float          _updatedTimeScale = -1f;
        private          int            _updatedFps       = -2;
        private          FpsMeter       _fpsMeter;
        private readonly List<CheatTab> _tabs = new();
        private          CheatTab       _selectedTab;


        private void Awake( )
        {
            _doc             = GetComponent<UIDocument>();
            _settingsManager = new Settings();
            _settings        = _settingsManager.GetSettings();
            _fpsMeter        = new FpsMeter();
            _fpsMeter.StartMeter();

            var systemTab = new CheatTab("System");
            systemTab.PredefinedCheats.Add( CreateSystemTabPredefinedContent() );
            _tabs.Add( systemTab );
            _selectedTab = systemTab;
            
            ShowPanel( );
        }

        private void OnDestroy( )
        {
            _fpsMeter.StopMeter();
        }

        private void ShowPanel( )
        {
            if( _settings.IsMaximized )
            {
                var root = _doc.rootVisualElement;
                root.Clear();
                var panelInstance = Resources.CheatPanelMax.Instantiate( );
                //panelInstance.style.paddingTop = Screen.height - Screen.safeArea.yMax;
                root.Add( panelInstance );
                var tabView = root.Q<VisualElement>( "CheatTabView" );
                tabView.Q("Header").Q( "ToolBar" ).Q<Button>( "MinimizeBtn" ).clicked += CheatPanelOnMinMaxToggleClicked;

                foreach ( var cheatTab in _tabs )                    
                    AddTab( tabView, cheatTab );
                var contentContainer = tabView.Q<VisualElement>( "Content" );
                SelectTab( contentContainer, _selectedTab );
            }
            else
            {
                var root = _doc.rootVisualElement;
                root.Clear();
                var panelInstance = Resources.CheatPanelMin.Instantiate( );
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

        private VisualElement CreateSystemTabPredefinedContent( )
        {
            var instance = Resources.SystemTab.Instantiate( );

            var sysInfoBox       = instance.Q<VisualElement>( "DeviceInfo" );
            var sysInfoLabel     = sysInfoBox.Q<Label>( "DeviceInfoValue" );
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

            var fpsBox = instance.Q<VisualElement>( "FPS" );
            var fpsStatsLbl = fpsBox.Q<Label>( "FPSStatsLbl" );
            var fpsSlider = fpsBox.Q<Slider>( "FPSSlider" );
            var fpsTargetLbl = fpsBox.Q<Label>( "TargetFPS" );
            fpsSlider.RegisterValueChangedCallback( evt => Application.targetFrameRate = (int)evt.newValue );
            // Mobile oriented controls
            fpsBox.Q<Button>( "FPS_X" ).clicked += () => Application.targetFrameRate = -1;
            fpsBox.Q<Button>( "FPS_15" ).clicked += () => Application.targetFrameRate = 15;
            fpsBox.Q<Button>( "FPS_30" ).clicked += () => Application.targetFrameRate = 30;
            fpsBox.Q<Button>( "FPS_60" ).clicked += () => Application.targetFrameRate = 60;
            fpsStatsLbl.schedule.Execute( () => fpsStatsLbl.text = GetFPSStats() ).Every( 1000 );
            fpsBox.schedule.Execute( ( ) =>
            {
                if ( Application.targetFrameRate != _updatedFps )
                {
                    _updatedFps = Application.targetFrameRate;
                    fpsSlider.SetValueWithoutNotify( _updatedFps );
                    fpsTargetLbl.text = $"FPS: {_updatedFps}";
                }
            } ).Every( 100 );
            //todo add desktop controls (vSync)

            return instance;
        }

        private void AddTab( VisualElement cheatTabView, CheatTab tab )
        {
            var tabBtnContainer = cheatTabView.Q<VisualElement>( "TabsScroll" );
            var tabBtn = new Button( );
            tabBtn.AddToClassList( "CheatBtn" );
            tabBtnContainer.Add( tabBtn );
            tabBtn.text    =  tab.Name;
            var contentContainer = cheatTabView.Q<VisualElement>( "Content" );
            tabBtn.clicked += () => TabBtnOnclicked( contentContainer, tab );
        }

        private void SelectTab( VisualElement contentContainer, CheatTab selectedTab )
        {
            foreach ( var cheatTab in _tabs )
            {
                cheatTab.CheatTabButton.EnableInClassList( "CheatBtn--pressed", cheatTab == selectedTab );
            }
            contentContainer.Clear();
            var tabContent = selectedTab.GetUI();
            contentContainer.Add( tabContent );
        }

        private void TabBtnOnclicked( VisualElement contentContainer, CheatTab selectedTab )
        {
            SelectTab( contentContainer, selectedTab );
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

        private string GetFPSStats( )
        {
            return $"FPS {_fpsMeter.AverageFPS}, min {_fpsMeter.SlowestFPS}";
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

        private void AddCheatsInternal( ICheats cheats )
        {
            var defaultTabName = cheats.GetType().Name;
            if( defaultTabName.EndsWith( "Cheats", StringComparison.InvariantCultureIgnoreCase ) )
                defaultTabName = defaultTabName[ ..^6 ];

            var members = cheats.GetType().GetMembers( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
            //Debug.Log($"Processing cheats from {cheats.GetType().Name}, members count: {members.Length}");
            foreach ( var member in members )
            {
                //Debug.Log(member.Name);

                if ( Cheat.IsValidCheat( member ) )
                {
                    var cheat           = new Cheat( cheats, member, destroyCancellationToken );
                    var tabNameForCheat = cheat.TabName ?? defaultTabName;
                    var tab             = GetOrCreateTab( tabNameForCheat );
                    tab.Add( cheat );
                }
            }
        }

        private CheatTab GetOrCreateTab( String tabName)
        {
            if( String.IsNullOrEmpty( tabName ) ) throw new ArgumentNullException( nameof(tabName) );

            var tab = _tabs.FirstOrDefault( t => t.Name == tabName );
            if ( tab == null )
            {
                tab = new CheatTab( tabName );
                _tabs.Add( tab );
            }

            return tab;
        } 
       
    }
}
