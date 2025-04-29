using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class CheatTab
    {
        public readonly  string     Name;
        

        public readonly  Button              CheatTabButton;
        public readonly  List<VisualElement> PredefinedCheats = new();
        public readonly  List<CheatGroup>    CheatsGroups     = new();

        public Boolean IsVisible { get; private set; }

        public CheatTab(String name, CancellationToken cancel )
        {
            Name                = name;
            _cancel             = cancel;
            CheatTabButton      = new Button();
            CheatTabButton.text = name;
            CheatTabButton.AddToClassList( TabButtonUssClassName );
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

        public VisualElement GetUI( )
        {
            return _contentUI ??= GenerateContentUI();
        }

        public void Show( )
        {
            IsVisible = true;
            if( _contentUI == null )
                _contentUI = GenerateContentUI();
        }

        public void Hide( )
        {
            IsVisible = false;
        }

        private          VisualElement     _contentUI;
        private readonly CancellationToken _cancel;
        private const    String            TabUssClassName        = "tab";
        private const    String            TabContentUssClassName = "tab__content";
        private const    String            TabButtonUssClassName  = "tab__button";

        private VisualElement GenerateContentUI( )
        {
            var content = new VisualElement();
            content.AddToClassList( TabContentUssClassName );

            foreach ( var predefinedCheatUI in PredefinedCheats )
            {
                content.Add( predefinedCheatUI );
            }

            foreach ( var cheatGroup in CheatsGroups )
            {
                content.Add( cheatGroup.GetUI() );
            }

            return content;
        }

        private void InvalidateUI( )
        {
            _contentUI = null;
        }
        
    }
}