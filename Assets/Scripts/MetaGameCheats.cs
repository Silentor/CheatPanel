using UnityEngine;

namespace Silentor.CheatPanel.DevProject
{
    public class MetaGameCheats : ICheats
    {
        private bool _isImmortal; 

        // [Cheat("Player")]
        // public void Kill( )
        // {
        //     Debug.Log("Doing Cheat ...");
        // }
        //
        // [Cheat("Player")]
        // public void Ressurect( )
        // {
        //     Debug.Log("Doing Cheat 2...");
        // }

        [Cheat("Player")]
        public bool Immortal
        {
            get => _isImmortal;
            set => _isImmortal = value;
        }

        // public void DoThird2( )
        // {
        //     Debug.Log("Doing Cheat 3...");
        // }
        //
        // public void DoThird3( )
        // {
        //     Debug.Log("Doing Cheat 3...");
        // }
        //
        // public void DoThird4( )
        // {
        //     Debug.Log("Doing Cheat 3...");
        // }
        //
        // [Cheat()]
        // public void DoWithAttrNull( )
        // {
        //     Debug.Log("Doing Cheat 4...");
        // }
        //
        // [Cheat("Group1")]
        // public void DoWithAttrGr1( )
        // {
        // }
        //
        // [Cheat("Group1")]
        // public void DoWithAttrGr2( )
        // {
        // }
        //
        // public void DoNoAttr( )
        // {
        //     Debug.Log("Doing Cheat 3...");
        // }
    }
}
