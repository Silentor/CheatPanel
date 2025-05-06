using System;
using System.Collections.Generic;
using Silentor.CheatPanel.UI;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class CheatGroup
    {
        public readonly string        Name;
        public readonly List<Cheat>   Cheats = new();

        public CheatGroup(String name )
        {
            Name = name;
        }

        public void AddCheat( Cheat cheat )
        {
            Cheats.Add( cheat );
            InvalidateUI( );
        }

        public VisualElement GetUI( )
        {
            return _cachedUI ??= GenerateUI( );
        }

        private void InvalidateUI( )
        {
            _cachedUI = null;
        }

        private VisualElement _cachedUI;


        /// <summary>
        /// Generate UI for this cheats group
        /// </summary>
        /// <returns></returns>
        private VisualElement GenerateUI( )
        {
            if( Cheats.Count == 0 )
                return new VisualElement();

            var group = new CheatGroupControl( Name );
            var content = group.Content;
            if ( Name == null )     //Unnamed group, no caption
            {
                foreach ( var cheat in Cheats )
                {
                    content.Add( cheat.GetUI() );
                }
            }
            else
            {
                foreach ( var cheat in Cheats )
                {
                    content.Add( cheat.GetUI() );
                }
            }

            return group;
        }
    }
}