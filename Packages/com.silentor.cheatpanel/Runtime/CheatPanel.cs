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

        private VisualElement _maximizedPanelUI;
        private VisualElement _contentContainer;
        private VisualElement _tabButtonsContainer;

        private VisualElement _minimizedPanelUI;

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

            _systemTab = new SystemTab( _fpsMeter, _settingsManager );
            _tabs.Add( _systemTab );

            var logTab = new LogConsoleTab(  );
            _tabs.Add( logTab );

            _selectedTab = logTab;
            
            ShowPanel( );
        }

        private void OnDestroy( )
        {
            _fpsMeter.Dispose();

            foreach ( var cheatTab in _tabs )                
                cheatTab.Dispose();
        }

        private void ShowPanel( )
        {
            if( _settings.IsMaximized )
            {
                if ( _maximizedPanelUI == null )
                {
                    var panelInstance = Resources.CheatPanelMax.Instantiate( );
                    var minimizeBtn   = panelInstance.Q<Button>( "MinimizeBtn" );
                    minimizeBtn.clicked += CheatPanelOnMinMaxToggleClicked;
                    _resultLbl          =  panelInstance.Q<Label>( "Result" );
                    _tabButtonsContainer = panelInstance.Q<VisualElement>( "TabsScroll" );
                    _contentContainer = panelInstance.Q<VisualElement>( "Content" );
                    _maximizedPanelUI = panelInstance;
                }

                var root = _doc.rootVisualElement;
                root.Clear();
                //panelInstance.style.paddingTop = Screen.height - Screen.safeArea.yMax;
                root.Add( _maximizedPanelUI );

                UpdateTabsButtons();
                SelectTab( _contentContainer, _selectedTab );
            }
            else
            {
                if ( _minimizedPanelUI == null )
                {
                    var panelInstance = Resources.CheatPanelMin.Instantiate( );
                    var maxBtn = panelInstance.Q<Button>( "MaxBtn" );
                    maxBtn.clicked += CheatPanelOnMinMaxToggleClicked;
                    _minimizedPanelUI = panelInstance;
                }

                if( _selectedTab != null )
                    _selectedTab.Hide();

                var root = _doc.rootVisualElement;
                root.Clear();
                //panelInstance.style.paddingTop = Screen.height - Screen.safeArea.yMax;
                root.Add( _minimizedPanelUI );
            }
        }

        private void CheatPanelOnMinMaxToggleClicked( )
        {
            _settings.IsMaximized = !_settings.IsMaximized;
            _settingsManager.UpdateSettings();
            ShowPanel();
        }

        private void UpdateTabsButtons( )
        {
            if ( _tabButtonsContainer != null )
            {
                if ( _tabs.Count == 0 )
                {
                    _tabButtonsContainer.Clear();
                    return;
                }

                //Make sure that tab buttons is equals to the tabs in list
                var tabButtons = _tabButtonsContainer.Query<Button>(  ).ToList();
                for ( int i = 0; i < _tabs.Count; i++ )
                {
                    var tab = _tabs[i];
                    var tabButton = tab.GetTabButton();
                    var btnIndex = tabButtons.IndexOf( tabButton );
                    if ( btnIndex < 0 )     //New tab was added, should add new button
                    {
                        tabButtons.Insert( i, tabButton );
                        tabButton.clicked += () => TabBtnOnclicked( _contentContainer, tab );
                    }
                    else if ( btnIndex != i )           //Fix buttons order
                    {
                        tabButtons.RemoveAt( btnIndex );
                        tabButtons.Insert( i, tabButton );
                    }
                }

                //If there are some buttons from deleted tabs that are not in tabs list, remove them
                while ( tabButtons.Count > _tabs.Count )
                {
                    tabButtons.RemoveAt( tabButtons.Count - 1 );
                }

                _tabButtonsContainer.Clear();
                foreach ( var tabButton in tabButtons )
                {
                    _tabButtonsContainer.Add( tabButton );    
                }
            }
        }

        private void SelectTab( VisualElement contentContainer, CheatTab selectedTab )
        {
            foreach ( var cheatTab in _tabs )
            {
                if( cheatTab == selectedTab )
                {
                    cheatTab.GetTabButton().AddToClassList( "CheatBtn--pressed" );
                    cheatTab.Show();
                }
                else
                {
                    cheatTab.GetTabButton().RemoveFromClassList( "CheatBtn--pressed" );
                    cheatTab.Hide();
                }
            }
            contentContainer.Clear();
            var tabContent = selectedTab.GetTabContent();
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

            var oldTabsCount = _tabs.Count;
            var members = cheats.GetType().GetMembers( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
            //Debug.Log($"Processing cheats from {cheats.GetType().Name}, members count: {members.Length}");
            foreach ( var member in members )
            {
                //Debug.Log(member.Name);

                if ( Cheat.IsValidCheat( member ) )
                {
                    var cheat           = Cheat.CreateCheat( member, cheats, this );
                    if( cheat == null )         //Still not valid, ignore
                        continue;

                    var tabNameForCheat = cheat.TabName ?? defaultTabName;
                    var tab             = GetOrCreateTab( tabNameForCheat );
                    tab.Add( cheat );
                }
            }

            if( _tabs.Count != oldTabsCount )
                UpdateTabsButtons();
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
