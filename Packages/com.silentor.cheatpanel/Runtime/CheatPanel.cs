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
        private          FpsMeter                    _fpsMeter;
        private readonly List<CheatTab>              _tabs = new();
        private          CheatTab                    _selectedTab;
        private          Label                       _resultLbl;
        private          IVisualElementScheduledItem _resultTransitionTask;
        private SystemTab _systemTab;

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
            _systemTab = new SystemTab( _fpsMeter, systemTab );
            systemTab.PredefinedCheats.Add( _systemTab.CreateContent( ) );
            _tabs.Add( systemTab );
            _selectedTab = systemTab;
            
            ShowPanel( );
        }

        private void OnDestroy( )
        {
            _fpsMeter.Dispose();
            _systemTab.Dispose();
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
