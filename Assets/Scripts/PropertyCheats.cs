using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel.DevProject
{
    public class PropertyCheats : ICheats
    {
        private bool   _isImmortal; 
        private float  _playerSpeed = 5.55f; 
        private int    _hp          = 100;
        private string _name        = "MyName";
        private int    _money       = 69;

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

        [Cheat("Advanced")] public Vector3 Vec3Cheat { get; set; }
        [Cheat("Advanced")] public Vector2 Vec2Cheat { get; set; }
        [Cheat("Advanced")] public Vector4 Vec4Cheat { get; set; }
        [Cheat("Advanced")] public Rect RectCheat { get; set; }
        [Cheat("Advanced")] public Bounds BoundsCheat { get; set; }
        [Cheat("Advanced")] public Vector2Int Vec2ICheat { get; set; }
        [Cheat("Advanced")] public Vector3Int Vec3ICheat { get; set; }
        [Cheat("Advanced")] public RectInt   RectIntCheat   { get; set; }
        [Cheat("Advanced")] public BoundsInt BoundsIntCheat { get; set; }
        [Cheat("Advanced")] public EasingMode EnumCheat { get; set; }

        [Cheat("Advanced")] private Color ColorCheat { get; set; } = Color.magenta;
    }

}
