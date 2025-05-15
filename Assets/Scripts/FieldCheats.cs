using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel.DevProject
{
    public class FieldCheats : ICheats
    {
        public string Name = "MyName";
        public bool Immortal;
        public float Speed;

        [Cheat("RO")]
        public const String ConstCheat = "ConstCheat";
        [Cheat("RO")]
        public readonly float SpeedRO;

        [Cheat( "Private" )]
        private float PrivateFieldWithAttrib;
        
        [Cheat("Range")]
        [Range( 1, 7 )]
        public float SpeedSlider;

        [Cheat( "Min" )]
        [Min( 5 )]
        public byte Min_5Value = 5;

        [Cheat("Unity")] public Vector3 Vec3Cheat;
        [Cheat("Unity")] public Vector2 Vec2Cheat;
        [Cheat("Unity")] public Vector4 Vec4Cheat;
        [Cheat("Unity")] public Rect RectCheat;
        [Cheat("Unity")] public Bounds BoundsCheat;
        [Cheat("Unity")] public Vector2Int Vec2ICheat;
        [Cheat("Unity")] public Vector3Int Vec3ICheat;
        [Cheat("Unity")] public RectInt   RectIntCheat;
        [Cheat("Unity")] public BoundsInt BoundsIntCheat;
        [Cheat("Unity")] public EasingMode EnumCheat;

        [Cheat("Unity")] 
        private Color ColorCheat = Color.magenta;
        private Color32 Color32Cheat = Color.green;

        [Cheat("Adv types")] public Guid GuidCheat = Guid.NewGuid( );
        [Cheat("Adv types")] public DateTime DateTimeCheat;
        [Cheat("Adv types")] public DateTimeOffset DTOffset;
        [Cheat("Adv types")] public TimeSpan TimeSpanCheat;
    }
}
