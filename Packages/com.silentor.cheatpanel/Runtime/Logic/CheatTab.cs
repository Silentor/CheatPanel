using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class CheatTab
    {
        public readonly string              Name;
        public readonly Button              CheatTabButton;
        public readonly List<VisualElement> PredefinedCheats = new();
        public readonly List<CheatGroup>         CheatsGroups           = new();

        public CheatTab(String name )
        {
            Name           = name;
            CheatTabButton = new Button();
            CheatTabButton.text = name;
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

        private void InvalidateUI( )
        {
            _contentUI = null;
        }

        private VisualElement _contentUI;

        private VisualElement GenerateContentUI( )
        {
            var content = new VisualElement();

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
    }
}