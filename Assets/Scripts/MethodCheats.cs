using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel.DevProject
{
    public class MethodCheats : ICheats
    {
        private int    _money       = 69;

        public string MethodManyParams( BackgroundSizeType @enum, byte par1 = 66, Boolean par2 = true )
        {
            return $"called {nameof(MethodManyParams)} with {@enum}, {par1}, {par2}";
        } 

        [Cheat("Group1")]
        public void Kill( )
        {
            Debug.Log("Doing Cheat ...");
        }
        
        [Cheat("Group1", CheatName = "Custom Name")]
        public String KillWithResult( )
        {
            return "Doing Cheat ...";
        }
        
        public void NoCheatAttr( )
        {
            Debug.Log("Doing Cheat 2...");
        }

        
        [Cheat("Group2")]
        public String AddMoney( [CheatValue(100, 1000, 10000)] int amount )
        {
            _money += amount;
            return $"added {amount} money, total {_money}" ;
        }
        
        [Cheat(CheatName = "NoGroupCustomName, int result")]
        public int DoThird2( )
        {
            return 42 ;
        }
        
        [Cheat(CheatName = "very long result")]
        public string DoThird3( )
        {
            return "lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
        }
        
        [Cheat()]
        private void PrivateMethod( )
        {
            Debug.Log("Doing Cheat 4...");
        }

        [Cheat()]
        protected void ProtectedMethod( )
        {
            Debug.Log("Doing Cheat 4...");
        }

        [Cheat()]
        internal void InternalMethod( )
        {
            Debug.Log("Doing Cheat 4...");
        }

        [Cheat("Group1")]
        public void DoWithAttrGr1( )
        {
        }
    }
}
