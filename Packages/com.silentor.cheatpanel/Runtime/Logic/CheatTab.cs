using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class CheatTab : IDisposable
    {
        public readonly  string     Name;
        public readonly  List<CheatGroup>    CheatsGroups     = new();

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
                InvalidateUI();
            }

            group.AddCheat( cheat );
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

        private VisualElement GenerateContentUI( )
        {
            var content = new VisualElement();
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
    }
}