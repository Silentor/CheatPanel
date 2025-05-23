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
        public const string DockTopUssClassName = "root--top";
        public const string DockBottomUssClassName = "root--bottom";
        public const string DockFullUssClassName = "root--full";

        public const string TabBtnUssClassName = "tab-view__tab-btn";
        public const string SelectedTabBtnUssClassName = "tab-view__tab-btn--selected";

        public const String CheatPanelResultShowUssClassName = "result--visible";

        /// <summary>
        /// Add cheats to the cheat panel.
        /// </summary>
        /// <param name="cheats"></param>
        public void AddCheats( ICheats cheats, String name = null )
        {
            var oldTabsCount = _tabs.Count;
            AddCheatsInternal( cheats, name );
            if( _settings.IsMaximized )
            {
                if( oldTabsCount != _tabs.Count )
                    UpdateTabsButtons();
                SelectTab( _contentContainer, _selectedTab );
            }
        }

        public void RemoveCheats( ICheats cheats )
        {
            var oldTabsCount = _tabs.Count;
            var oldSelectedTabIndex = _tabs.IndexOf( _selectedTab );
            var wasChanged = false;

            foreach ( var cheatTab in _tabs )
            {
                wasChanged |= cheatTab.Remove( cheats );
            }

            if ( wasChanged )
            {
                _tabs.RemoveAll( t => t.GetType() == typeof(CheatTab) && t.CheatsGroups.Count == 0 );
                if ( !_tabs.Contains( _selectedTab ) )
                {
                    var newSelectedTabIndex = Math.Clamp( oldSelectedTabIndex, 0, _tabs.Count - 1 );
                    _selectedTab = _tabs[newSelectedTabIndex];
                }

                if( _settings.IsMaximized )
                {
                    if( oldTabsCount != _tabs.Count )
                        UpdateTabsButtons();
                    SelectTab( _contentContainer, _selectedTab );
                }
            }
        }

        public void RemoveCheats<TCheats>() where TCheats : ICheats
        {
            var cheatObjects = _tabs.SelectMany( t => t.CheatsGroups ).SelectMany( g => g.Cheats ).Select( c => c.CheatObject ).OfType<TCheats>().Distinct( ).ToArray();
            foreach ( var cheatObject in cheatObjects )                
                RemoveCheats( cheatObject );
        }

        /// <summary>
        /// Show temporary text in the bottom of the cheat panel.
        /// </summary>
        /// <param name="result"></param>
        internal void ShowResult( String result )
        {
            if( _resultLbl != null )
            {
                _resultLbl.AddToClassList( CheatPanelResultShowUssClassName );
                _resultLbl.text = result;
                _resultTransitionTask?.Pause();                                 //Yes, it's a recommended way to stop transition
                _resultTransitionTask = _resultLbl.schedule.Execute( ( ) =>
                {
                    _resultLbl.RemoveFromClassList( CheatPanelResultShowUssClassName );

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
        private         EDockMode                   _dockMode;

        private VisualElement _maximizedPanelTemplateContainer;
        private VisualElement _contentContainer;
        private VisualElement _tabButtonsContainer;
        private Button _dockBtn;

        private VisualElement _minimizedPanelUI;

        private SystemTab _systemTab;
        


        private void Awake( )
        {
            _doc             = GetComponent<UIDocument>();
            _settingsManager = new Settings();
            _settings        = _settingsManager.GetSettings();
            _dockMode = _settings.GetEnumValueSafe<EDockMode>( _settings.DockMode );

            _fpsMeter        = new FpsMeter();
            _fpsMeter.StartMeter();

            _systemTab = new SystemTab( _fpsMeter, _settingsManager );
            _tabs.Add( _systemTab );

            var logTab = new LogConsoleTab(  );
            _tabs.Add( logTab );

            _selectedTab = logTab;
            
            ShowPanel( );
            ChangeDockMode( null, _dockMode );
        }

        private void OnDestroy( )
        {
            _fpsMeter.Dispose();

            foreach ( var cheatTab in _tabs )                
                cheatTab.Dispose();

            _settings.DockMode = (int)_dockMode;
            _settingsManager.UpdateSettings();
        }

        private void ShowPanel( )
        {
            if( _settings.IsMaximized )
            {
                if ( _maximizedPanelTemplateContainer == null )
                {
                    var panelInstance = Resources.CheatPanelMax.Instantiate( );
                    panelInstance.pickingMode = PickingMode.Ignore;                                 //Fix TemplateContainer
                    var minimizeBtn   = panelInstance.Q<Button>( "MinimizeBtn" );
                    minimizeBtn.clicked += CheatPanelOnMinMaxToggleClicked;
                    _dockBtn      = panelInstance.Q<Button>( "DockBtn" );
                    _dockBtn.clicked += CheatPanelOnDockBtnClicked;
                    _resultLbl          =  panelInstance.Q<Label>( "Result" );
                    _tabButtonsContainer = panelInstance.Q<VisualElement>( "TabsScroll" );
                    _contentContainer = panelInstance.Q<VisualElement>( "Content" );
                    _maximizedPanelTemplateContainer = panelInstance;
                }

                var root = _doc.rootVisualElement;
                root.Clear();
                //panelInstance.style.paddingTop = Screen.height - Screen.safeArea.yMax;
                root.Add( _maximizedPanelTemplateContainer );

                UpdateTabsButtons();
                SelectTab( _contentContainer, _selectedTab );
                ChangeDockMode( null, _dockMode );
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

        private void CheatPanelOnDockBtnClicked( )
        {
            var oldDockMode = _dockMode;
            var nextDockMode = GetNextDockMode( oldDockMode );
            ChangeDockMode( oldDockMode,  nextDockMode );
        }

        private void CheatPanelOnMinMaxToggleClicked( )
        {
            _settings.IsMaximized = !_settings.IsMaximized;
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
                        tabButton.AddToClassList( TabBtnUssClassName );
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
                    cheatTab.GetTabButton().AddToClassList( SelectedTabBtnUssClassName );
                    cheatTab.Show();
                }
                else
                {
                    cheatTab.GetTabButton().RemoveFromClassList( SelectedTabBtnUssClassName );
                    cheatTab.Hide();
                }
            }
            contentContainer.Clear();
            var tabContent = selectedTab.GetTabContent();
            contentContainer.Add( tabContent );
            _selectedTab = selectedTab;
        }

        private void TabBtnOnclicked( VisualElement contentContainer, CheatTab selectedTab )
        {
            SelectTab( contentContainer, selectedTab );
        }

        private void AddCheatsInternal( ICheats cheats, String name )
        {
            var defaultTabName = name ?? GetDefaultTabName( cheats );

            var members = cheats.GetType().GetMembers( BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ).ToList();
            members = CheckAccessibility( members );
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

            static String GetDefaultTabName( ICheats cheats )
            {
                var typeName = cheats.GetType().Name;
                if( typeName.EndsWith( "Cheats", StringComparison.InvariantCultureIgnoreCase ) )
                    typeName = typeName[0..^6];
                return typeName;
            }
        }

        private List<MemberInfo> CheckAccessibility( List<MemberInfo> members )
        {
            var allowPrivate = false;
            var allowFromBaseType = false;

            for ( int i = 0; i < members.Count; i++ )
            {
                var member  = members[i];

                //Access cheats from base type of ICheats need special permission
                if( !allowFromBaseType && !(typeof(ICheats).IsAssignableFrom(member.DeclaringType) ))
                {
                    members.RemoveAt( i-- );
                    continue;
                }

                var isAbstract = false;
                var isPublic = false;
                var markedAsCheat = member.IsDefined( typeof(CheatAttribute), true );

                if ( member is MethodInfo method )
                {
                    isPublic = method.IsPublic;
                    isAbstract = method.IsAbstract;
                }
                else if ( member is PropertyInfo property )
                {
                    isPublic = (property.CanRead && property.GetMethod.IsPublic) || (property.CanWrite && property.SetMethod.IsPublic);
                    isAbstract = (property.CanRead && property.GetMethod.IsAbstract) || (property.CanWrite && property.SetMethod.IsAbstract);
                }
                else if ( member is FieldInfo field )
                {
                    isPublic = field.IsPublic;
                }

                if( isAbstract)     //Definitely no
                {
                    members.RemoveAt( i-- );
                    continue;
                }

                if ( !isPublic )
                {
                    if( !allowPrivate && !markedAsCheat )
                    {
                        members.RemoveAt( i-- );
                        continue;
                    }
                }
            }

            return members;
        }

        private CheatTab GetOrCreateTab( String tabName)
        {
            if( String.IsNullOrEmpty( tabName ) ) throw new ArgumentNullException( nameof(tabName) );

            var tab = _tabs.FirstOrDefault( t => t.Name == tabName );
            if ( tab == null )
            {
                tab = new CheatTab( tabName );
                _tabs.Add( tab );
                _tabs.Sort( CheatTab.CheatTabOrderComparer.Instance );
            }

            return tab;
        } 

        private void ChangeDockMode( EDockMode? oldDockMode, EDockMode dockMode )
        {
            if ( _dockMode == dockMode && oldDockMode != null )
                return;

            if( oldDockMode.HasValue )
                _maximizedPanelTemplateContainer.RemoveFromClassList( GetDockStyle( oldDockMode.Value ) );
            _maximizedPanelTemplateContainer.AddToClassList( GetDockStyle( dockMode ) );
            var nextDockMode = GetNextDockMode(dockMode);
            _dockBtn.text = GetDockIconSymbol( nextDockMode );
            _dockMode = dockMode;

            String GetDockStyle( EDockMode dockMode )
            {
                return dockMode switch
                {
                    EDockMode.Top => DockTopUssClassName,
                    EDockMode.Bottom => DockBottomUssClassName,
                    EDockMode.Full => DockFullUssClassName,
                    _ => throw new ArgumentOutOfRangeException( nameof(dockMode), dockMode, null )
                };
            }

            String GetDockIconSymbol( EDockMode dockMode )
            {
                return dockMode switch
                       {
                               EDockMode.Top    => "↑",
                               EDockMode.Bottom => "↓",
                               EDockMode.Full   => "□",
                               _                => throw new ArgumentOutOfRangeException( nameof(dockMode), dockMode, null )
                       };
            }
        }

        private EDockMode GetNextDockMode( EDockMode dockMode )
        {
            return (EDockMode)(((int)dockMode + 1) % Enum.GetValues( typeof(EDockMode) ).Length);
        }
       
    }

    public enum EDockMode
    {
        Top,
        Bottom,
        Full,
    }
}
