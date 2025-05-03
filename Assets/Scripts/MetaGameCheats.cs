using System;
using UnityEngine;
using UnityEngine.Scripting;
using Object = System.Object;

namespace Silentor.CheatPanel.DevProject
{
    public class MetaGameCheats : ICheats
    {
        private bool   _isImmortal; 
        private float  _playerSpeed = 5.55f; 
        private int    _hp          = 100;
        private string _name        = "MyName";
        private int    _money       = 69;

        // public string MethodParams( int par1 )
        // {
        //     return $"called {nameof(MethodParams)} with {par1}";
        // } 

        // [Cheat("Player")]
        // public void Kill( )
        // {
        //     Debug.Log("Doing Cheat ...");
        // }
        //
        // [Cheat("Player")]
        // public String KillWithResult( )
        // {
        //     return "Doing Cheat ...";
        // }
        //
        // public void NoCheatAttr( )
        // {
        //     Debug.Log("Doing Cheat 2...");
        // }
        //
        [Cheat("Player")]
        public bool Immortal
        {
            get => _isImmortal;
            set => _isImmortal = value;
        }
        
        [Cheat("Player")]
        public bool Immortal2
        {
            get => _isImmortal;
            //set => _isImmortal = value;
        }
        
        //[Cheat("Player")]
        public float Speed
        {
            get => _playerSpeed;
            set => _playerSpeed = value;
        }
        
        [Cheat("Player")]
        public float Speed2
        {
            get => _playerSpeed;
            set => _playerSpeed = value;
        }
        
        
        [Cheat("Player")]
        [Range(1, 7)]
        public float SpeedSlider
        {
            get => _playerSpeed;
            set => _playerSpeed = value;
        }
        
        [Cheat("Player")]
        [Range(-1, 7)]
        public byte ByteSlider
        {
            get => (Byte)_hp;
            set => _hp = value;
        }
        
        [Cheat("Player")]
        public float SpeedRO
        {
            get => _playerSpeed;
        }
        
        [Cheat("Player")]
        public float SpeedWO
        {
            set => _playerSpeed = value;            //Because why not
        }
        
        [Cheat("Player")]
        public double SpeedDouble
        {
            get => _playerSpeed;
            set => _playerSpeed = (float)value;
        }
        
        [Cheat("Player")]
        public Byte HP_Byte
        {
            get => (byte)_hp;
            set => _hp = value;
        }
        
        [Cheat("Player")]
        public short HP_short
        {
            get => (short)_hp;
            set => _hp = value;
        }
        
        [Cheat("Player")]
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        //
        // [Cheat("Resources", TabName = "Resources")]
        // public String AddMoney( [CheatValue(100, 1000, 10000)] int amount )
        // {
        //     _money += amount;
        //     return $"added {amount} money, total {_money}" ;
        // }
        //
        // [Cheat(CheatName = "with result")]
        // public int DoThird2( )
        // {
        //     return 42 ;
        // }
        //
        // [Cheat(CheatName = "very long result")]
        // public string DoThird3( )
        // {
        //     return "lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
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
