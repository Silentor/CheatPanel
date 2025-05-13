using System;
using System.Collections.Generic;
using Silentor.CheatPanel.UI;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class CheatGroup : IDisposable
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

        public Boolean RemoveCheats( ICheats cheats )
        {
            var wasChanged = false;
            for ( int i = 0; i < Cheats.Count; i++ )
            {
                if ( Cheats[ i ].CheatObject == cheats )
                {
                    Cheats.RemoveAt( i );
                    InvalidateUI();
                    wasChanged = true;
                    i--;
                }
            }

            return wasChanged;
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

        public void Dispose( )
        {
            // TODO release managed resources here
            foreach ( var cheat in Cheats )
            {
                cheat.Dispose();
            }
        }
    }
}