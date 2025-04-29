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

        [Cheat("Player")]
        public void Kill( )
        {
            Debug.Log("Doing Cheat ...");
        }
        
        [Cheat("Player")]
        public void Ressurect( )
        {
            Debug.Log("Doing Cheat 2...");
        }

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
            set => _isImmortal = value;
        }
        
        [Cheat("Player")]
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
        [Range(1, 7)]
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
        
        [Cheat("Resources", TabName = "Resources")]
        public void AddMoney( [CheatValue(100, 1000, 10000)] int amount )
        {
            _money += amount;
            Debug.Log( $"added {amount} money, total {_money}" );
        }

        [Cheat(CheatName = "cheat name test +")]
        public void DoThird2( )
        {
            Debug.Log("Doing Cheat 3...");
        }
        
        public void DoThird3( )
        {
            Debug.Log("Doing Cheat 3...");
        }
        
        public void DoThird4( )
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
