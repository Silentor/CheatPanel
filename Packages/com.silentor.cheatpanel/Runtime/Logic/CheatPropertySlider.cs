using System;
using System.Reflection;
using System.Threading;
using Silentor.CheatPanel.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class CheatPropertySlider : Cheat
    {
        public CheatPropertySlider( PropertyInfo cheatProperty, RangeAttribute rangeAttr, ICheats cheatObject, CancellationToken cancel ): base( cheatProperty, cheatObject, cancel )
        {
            Assert.IsTrue( cheatProperty.CanRead || cheatProperty.CanWrite );

            _cheatProperty = cheatProperty;
            _cheatObject   = cheatObject;
            _refreshTiming = cheatProperty.CanRead
                    ? Attr?.RefreshTime ?? RefreshUITiming.PerSecond
                    : RefreshUITiming.None;
            _min           = rangeAttr.min;
            _max           = rangeAttr.max;
        }

        private          Action          _refreshFieldUI;
        private readonly RefreshUITiming _refreshTiming;
        private readonly PropertyInfo    _cheatProperty;
        private readonly ICheats         _cheatObject;
        private readonly Single          _min;
        private readonly Single          _max;


        protected override VisualElement GenerateUI( )
        {
            var valueLabel = new Label( );
            var slider     = PrepareSlider( valueLabel );
            if( slider == null )
                return null;                            //Incompatible cheat property type, ignore cheat

            var container = new VisualElement( );
            container.Add( slider );
            container.Add( valueLabel );
            container.AddToClassList( "CheatLine" );
            valueLabel.AddToClassList( "CheatLabel" );
            slider.AddToClassList( "CheatSlider" );
            return container;
        }

        private VisualElement PrepareSlider( Label valueLabel )
        {
            var propType = _cheatProperty.PropertyType;
            if( propType == typeof(Single) )
            {
                var  field = GetFloatSlider( );
                _refreshFieldUI = GetRefreshUI( field, valueLabel );
                PrepareUpdateCheatProperty( field );
                return field;
            }
            else if( propType == typeof(Double) )
            {
                var  field = GetFloatSlider();
                _refreshFieldUI = GetRefreshUI<float, double>( field, valueLabel, d => d.ClampToFloat() );
                PrepareUpdateCheatProperty<float, double>( field, d => d );
                return field;
            }
            else if( propType == typeof(Int32) )
            {
                var  field = GetIntSlider(  );
                _refreshFieldUI = GetRefreshUI( field, valueLabel );
                PrepareUpdateCheatProperty( field );
                return field;
            }
            else if( propType == typeof(Int64) )
            {
                var  field = GetIntSlider(  );
                _refreshFieldUI = GetRefreshUI<Int32, Int64>( field, valueLabel, ul => ul.ClampToInt32() );
                PrepareUpdateCheatProperty<Int32, Int64>( field, i => i );
                return field;
            }
            else if( propType == typeof(UInt32) )
            {
                var  field = GetIntSlider(  );
                _refreshFieldUI = GetRefreshUI<Int32, UInt32>( field, valueLabel, ui => ui.ClampToInt32() );
                PrepareUpdateCheatProperty<Int32, UInt32>( field, i => i.ClampToUInt32() );
                return field;
            }
            else if( propType == typeof(UInt64) )
            {
                var  field = GetIntSlider(  );
                _refreshFieldUI = GetRefreshUI<Int32, UInt64>( field, valueLabel, ul => ul.ClampToInt32() );
                PrepareUpdateCheatProperty<Int32, UInt64>( field, i => i.ClampToUInt64() );
                return field;
            }
            else if( propType == typeof(Byte) )
            {
                var  field = GetIntSlider(  );
                _refreshFieldUI = GetRefreshUI<int, byte>( field, valueLabel, v => v );
                PrepareUpdateCheatProperty( field, i => i.ClampToUInt8() );
                return field;
            }
            else if( propType == typeof(SByte) )
            {
                var  field = GetIntSlider(  );
                _refreshFieldUI = GetRefreshUI<int, sbyte>( field, valueLabel, v => v );
                PrepareUpdateCheatProperty( field, i => i.ClampToInt8() );
                return field;
            }
            else if( propType == typeof(UInt16) )
            {
                var  field = GetIntSlider(  );
                _refreshFieldUI = GetRefreshUI<int, ushort>( field, valueLabel, v => v );
                PrepareUpdateCheatProperty( field, i => i.ClampToUInt16() );
                return field;
            }
            else if( propType == typeof(Int16) )
            {
                var  field = GetIntSlider(  );
                _refreshFieldUI = GetRefreshUI<int, short>( field, valueLabel, v => v );
                PrepareUpdateCheatProperty( field, i => i.ClampToInt16() );
                return field;
            }

            return null;
        }

        protected override void RefreshUI( )
        {
            _refreshFieldUI?.Invoke( );
        }

        protected override RefreshUITiming GetRefreshUITiming( )
        {
            return _refreshTiming;
        }

        private Slider GetFloatSlider(  )
        {
            var slider = new Slider( Name, _min, _max );
            return slider;
        }

        private SliderInt GetIntSlider(  )
        {
            var slider = new SliderInt( Name, (int)_min, (int)_max );
            return slider;
        }

        private Action GetRefreshUI<TField>( BaseSlider<TField> field, Label label ) where TField : IComparable<TField>
        {
            if( _cheatProperty.CanRead )
            {
                var getter   = (Func<TField>)Delegate.CreateDelegate( typeof(Func<TField>), _cheatObject, _cheatProperty.GetGetMethod() );
                return ( ) =>
                {
                    var value = getter();
                    field.SetValueWithoutNotify( value );
                    label.text = value.ToString();
                };
            }

            return null;
        }

        private Action GetRefreshUI<TField, TProp>( BaseSlider<TField> field, Label label, Func<TProp, TField> propToFieldConversion ) where TField : IComparable<TField>
        {
            if( _cheatProperty.CanRead )
            {
                var getter = (Func<TProp>)Delegate.CreateDelegate( typeof(Func<TProp>), _cheatObject, _cheatProperty.GetGetMethod() );
                return ( ) =>
                {
                    var value = propToFieldConversion( getter() );
                    field.SetValueWithoutNotify( value );
                    label.text = value.ToString();
                };
            }

            return null;
        }

        private void PrepareUpdateCheatProperty<TField>( BaseSlider<TField> field ) where TField : IComparable<TField>
        {
            if( _cheatProperty.CanWrite )
            {
                var setter = (Action<TField>)Delegate.CreateDelegate( typeof(Action<TField>), _cheatObject, _cheatProperty.GetSetMethod() );
                field.RegisterValueChangedCallback( evt => setter( evt.newValue ) );
            }
            else
            {
                field.SetEnabled( false );
            }
        }

        private void PrepareUpdateCheatProperty<TField, TProp>( BaseSlider<TField> field, Func<TField, TProp> fieldToPropConversion ) where TField : IComparable<TField>
        {
            if( _cheatProperty.CanWrite )
            {
                var setter = (Action<TProp>)Delegate.CreateDelegate( typeof(Action<TProp>), _cheatObject, _cheatProperty.GetSetMethod() );
                field.RegisterValueChangedCallback( evt => setter( fieldToPropConversion ( evt.newValue ) ) );
            }
            else
            {
                field.SetEnabled( false );
            }
        }
      
    }
}