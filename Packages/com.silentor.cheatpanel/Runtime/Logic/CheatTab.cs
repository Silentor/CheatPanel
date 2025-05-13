using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    [DebuggerDisplay("{Name}")]
    public class CheatTab : IDisposable
    {
        public readonly  string     Name;
        public readonly  List<CheatGroup>    CheatsGroups     = new();
        public virtual int Order => 0;

        public Boolean IsVisible { get; private set; }

        public CheatTab( String name )
        {
            Name                = name;
        }

        public void Add( Cheat cheat )
        {
            Assert.IsTrue( cheat.TabName == null || cheat.TabName == Name );

            CheatGroup group = null;
            if ( cheat.GroupName == null )
            {
                if ( CheatsGroups.Any() && CheatsGroups.Last().Name == null )
                {
                    group = CheatsGroups.Last();
                }
            }
            else
            {
                group = CheatsGroups.FirstOrDefault( g => g.Name == cheat.GroupName );
            }

            if( group == null )
            {
                group = new CheatGroup( cheat.GroupName );
                CheatsGroups.Add( group );
            }

            group.AddCheat( cheat );

            InvalidateUI();
        }

        public Boolean Remove( ICheats cheats )
        {
            var wasChanged = false;

            for ( var i = 0; i < CheatsGroups.Count; i++ )
            {
                var cheatGroup = CheatsGroups[ i ];
                wasChanged |= cheatGroup.RemoveCheats( cheats );
            }

            if ( wasChanged )
            {
                CheatsGroups.RemoveAll( cg => cg.Cheats.Count == 0 );
                InvalidateUI();
            }

            return wasChanged;
        }

        public Button GetTabButton(  )
        {
            return _buttonUI ??= GenerateTabButton();
        }

        public VisualElement GetTabContent( )
        {
            return _contentUI ??= GenerateContentUI();
        }

        public virtual void Show( )
        {
            IsVisible = true;
            if( _contentUI == null )
                _contentUI = GenerateContentUI();
        }

        public void Hide( )
        {
            IsVisible = false;
        }

        private          VisualElement      _contentUI;
        private          Button             _buttonUI;

        protected virtual VisualElement GenerateContentUI( )
        {
            var content = new ScrollView();
            content.mode = ScrollViewMode.Vertical;

            var customContent = GenerateCustomContent( );
            if( customContent != null )                
                content.Add( customContent );

            foreach ( var cheatGroup in CheatsGroups )
            {
                content.Add( cheatGroup.GetUI() );
            }

            return content;
        }

        protected virtual VisualElement GenerateCustomContent( )
        {
            return null;
        }

        protected virtual Button GenerateTabButton( )
        {
            var tabBtn      = new Button();
            tabBtn.text = Name;
            return tabBtn;
        }

        private void InvalidateUI( )
        {
            _contentUI = null;
        }

        public virtual void Dispose( )
        {
            // TODO release managed resources here

            foreach ( var cheatsGroup in CheatsGroups )
            {
                cheatsGroup.Dispose();
            }
        }

        public class CheatTabOrderComparer : IComparer<CheatTab>
        {
            public static readonly CheatTabOrderComparer Instance = new CheatTabOrderComparer();

            public int Compare(CheatTab x, CheatTab y)
            {
                return x.Order.CompareTo( y.Order );
            }
        }
    }
}