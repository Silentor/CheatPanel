using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
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

        /// <summary>
        /// Show temporary text in the bottom of the cheat panel.
        /// </summary>
        /// <param name="result"></param>
        internal void ShowResult( String result )
        {
            if( _resultLbl != null )
            {
                _resultLbl.AddToClassList( TabViewResultShowUssClassName );
                _resultLbl.text = result;
                _resultTransitionTask?.Pause();                                 //Yes, it's a recommended way to stop transition
                _resultTransitionTask = _resultLbl.schedule.Execute( ( ) =>
                {
                    _resultLbl.RemoveFromClassList( TabViewResultShowUssClassName );

                } ).StartingIn( 5000 );
            }
        }

        private          UIDocument                  _doc;
        private          SettingsDTO                 _settings;
        private          Settings                    _settingsManager;
        private          float                       _updatedTimeScale = -1f;
        private          int                         _updatedFps       = -2;
        private          FpsMeter                    _fpsMeter;
        private readonly List<CheatTab>              _tabs = new();
        private          CheatTab                    _selectedTab;
        private          Label                       _resultLbl;
        private          IVisualElementScheduledItem _resultTransitionTask;

        private const String TabViewResultUssClassName = "TabView__result";
        private const String TabViewResultShowUssClassName = TabViewResultUssClassName + "--show";


        private void Awake( )
        {
            _doc             = GetComponent<UIDocument>();
            _settingsManager = new Settings();
            _settings        = _settingsManager.GetSettings();
            _fpsMeter        = new FpsMeter();
            _fpsMeter.StartMeter();

            var systemTab = new CheatTab("System", destroyCancellationToken ) ;
            systemTab.PredefinedCheats.Add( CreateSystemTabPredefinedContent() );
            _tabs.Add( systemTab );
            _selectedTab = systemTab;
            
            ShowPanel( );
        }

        private void OnDestroy( )
        {
            _fpsMeter.Dispose();
        }

        private void ShowPanel( )
        {
            if( _settings.IsMaximized )
            {
                var root = _doc.rootVisualElement;
                root.Clear();
                var panelInstance = Resources.CheatPanelMax.Instantiate( );
                var tabView       = panelInstance.Q<VisualElement>( "CheatTabView" );
                var minimizeBtn   = panelInstance.Q<Button>( "MinimizeBtn" );
                _resultLbl    = panelInstance.Q<Label>( "Result" );
                minimizeBtn.clicked += CheatPanelOnMinMaxToggleClicked;
                var contentContainer = panelInstance.Q<VisualElement>( "Content" );

                //panelInstance.style.paddingTop = Screen.height - Screen.safeArea.yMax;
                root.Add( panelInstance );

                foreach ( var cheatTab in _tabs )                    
                    AddTab( tabView, contentContainer, cheatTab );
                SelectTab( contentContainer, _selectedTab );
            }
            else
            {
                foreach ( var tab in _tabs )                    
                    tab.Hide();

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

        private void AddTab( VisualElement cheatTabView, VisualElement contentContainer, CheatTab tab )
        {
            var tabBtnContainer = cheatTabView.Q<VisualElement>( "TabsScroll" );
            var tabBtn = tab.CheatTabButton;
            tabBtnContainer.Add( tabBtn );
            tabBtn.clicked += () => TabBtnOnclicked( contentContainer, tab );
        }

        private void SelectTab( VisualElement contentContainer, CheatTab selectedTab )
        {
            foreach ( var cheatTab in _tabs )
            {
                if( cheatTab == selectedTab )
                {
                    cheatTab.CheatTabButton.AddToClassList( "CheatBtn--pressed" );
                    cheatTab.Show();
                }
                else
                {
                    cheatTab.CheatTabButton.RemoveFromClassList( "CheatBtn--pressed" );
                    cheatTab.Hide();
                }
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
            return $"FPS avg {_fpsMeter.AverageFPS}, 90% {_fpsMeter.Percentile90FPS}, 99% {_fpsMeter.Percentile99FPS}";
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
                    var cheat           = Cheat.CreateCheat( member, cheats, this, destroyCancellationToken );
                    if( cheat == null )         //Still not valid
                        continue;

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
                tab = new CheatTab( tabName, destroyCancellationToken );
                _tabs.Add( tab );
            }

            return tab;
        } 
       
    }
}
