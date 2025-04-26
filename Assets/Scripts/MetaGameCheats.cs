using UnityEngine;

namespace Silentor.CheatPanel.DevProject
{
    public class MetaGameCheats : ICheats
    {
        public void DoCheat( )
        {
            Debug.Log("Doing Cheat ...");
        }

        public void DoAnother( )
        {
            Debug.Log("Doing Cheat 2...");
        }

        public void DoThird( )
        {
            Debug.Log("Doing Cheat 3...");
        }

        [Cheat()]
        public void DoWithAttrNull( )
        {
            Debug.Log("Doing Cheat 4...");
        }

        [Cheat("Group1")]
        public void DoWithAttrGr1( )
        {
        }

        [Cheat("Group1")]
        public void DoWithAttrGr2( )
        {
        }

        public void DoNoAttr( )
        {
            Debug.Log("Doing Cheat 3...");
        }
    }
}
