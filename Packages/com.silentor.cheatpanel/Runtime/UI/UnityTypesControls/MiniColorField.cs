using System;
using System.Runtime.CompilerServices;
using Silentor.CheatPanel.Utils;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Silentor.CheatPanel.UI
{
    [UxmlElement]
    public partial class MiniColorField: BaseField<Color>
    {
        public new static readonly string ussClassName = "mini-color-field";
        public static readonly string sampleUssClassName = ussClassName + "__sample";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        private readonly IntegerField _rField;
        private readonly IntegerField _gField;
        private readonly IntegerField _bField;
        private readonly IntegerField _aField;
        private readonly VisualElement _sample;

        public MiniColorField() : this( "ColorField" ){}

        public MiniColorField(String label ) : base( label, GetVisualInput() )
        {
            AddToClassList( ussClassName );
            styleSheets.Add( Resources.StyleSheet );

            var editors = this.Query<IntegerField>(  ).ToList();
            _rField = editors[0];
            _rField.RegisterValueChangedCallback( ( evt) => {
                Color32 color32 = value;
                color32.r   = evt.newValue.ClampToByte();
                value = color32;
            } );
            _gField = editors[1];
            _gField.RegisterValueChangedCallback( ( evt) => {
                Color32 color32 = value;
                color32.g = evt.newValue.ClampToByte();
                value     = color32;
            } );
            _bField = editors[2];
            _bField.RegisterValueChangedCallback( ( evt) => {
                Color32 color32 = value;
                color32.b = evt.newValue.ClampToByte();
                value     = color32;
            } );
            _aField = editors[3];
            _aField.RegisterValueChangedCallback( ( evt) => {
                Color32 color32 = value;
                color32.a = evt.newValue.ClampToByte();
                value     = color32;
            } );
            _sample = this.Q<VisualElement>( className: sampleUssClassName );
        }

        private static VisualElement GetVisualInput( )
        {
            var input = new VisualElement();
            input.AddToClassList( inputUssClassName );
            input.AddToClassList( "unity-composite-field__input" );
            var colorSample = new VisualElement();
            colorSample.AddToClassList( sampleUssClassName );
            colorSample.name = "Sample";
            var redValue = new IntegerField( "R", 3 );
            redValue.name = "RedValue";
            var greenValue = new IntegerField( "G", 3 );
            greenValue.name = "GreenValue";
            var blueValue = new IntegerField( "B", 3 );
            blueValue.name = "BlueValue";
            var alphaValue = new IntegerField( "A", 3 );
            alphaValue.name = "AlphaValue";

            input.Add( colorSample );
            input.Add( redValue );
            input.Add( greenValue );
            input.Add( blueValue );
            input.Add( alphaValue );
            return input;
        }

        public override void SetValueWithoutNotify(Color newValue )
        {
            base.SetValueWithoutNotify( newValue );

            Color32 color32 = newValue;
            _sample.style.backgroundColor = newValue;
            _rField.SetValueWithoutNotify( color32.r );
            _gField.SetValueWithoutNotify( color32.g );
            _bField.SetValueWithoutNotify( color32.b );
            _aField.SetValueWithoutNotify( color32.a );
        }

        private static class Resources
        {
            public static readonly StyleSheet StyleSheet = UnityEngine.Resources.Load<StyleSheet>( "MiniColorField" );
        }
    }
}