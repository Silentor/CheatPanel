using System;
using System.Reflection;
using System.Threading;
using Silentor.CheatPanel.Binders;
using Silentor.CheatPanel.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class CheatSlider : Cheat
    {
        public CheatSlider( PropertyInfo cheatProperty, RangeAttribute rangeAttr, ICheats cheatObject ): base( cheatProperty, cheatObject )
        {
            Assert.IsTrue( cheatProperty.CanRead || cheatProperty.CanWrite );

            _cheatProperty = cheatProperty;
            _cheatObject   = cheatObject;
            _refreshTiming = cheatProperty.CanRead
                    ? Attr?.RefreshTime ?? RefreshUITiming.PerSecond
                    : RefreshUITiming.Never;
            _min           = rangeAttr.min;
            _max           = rangeAttr.max;
        }

        public CheatSlider( FieldInfo cheatField, RangeAttribute rangeAttr, ICheats cheatObject ): base( cheatField, cheatObject )
        {
            _cheatField     = cheatField;
            _cheatObject   = cheatObject;
            _refreshTiming = cheatField.IsInitOnly || cheatField.IsLiteral
                    ? RefreshUITiming.OneTime
                    : Attr?.RefreshTime ?? RefreshUITiming.PerSecond;
            _min = rangeAttr.min;
            _max = rangeAttr.max;
        }


        private readonly RefreshUITiming       _refreshTiming;
        private readonly PropertyInfo          _cheatProperty;
        private readonly FieldInfo              _cheatField;
        private readonly ICheats               _cheatObject;
        private readonly Single                _min;
        private readonly Single                _max;
        private          CheatControlBinderBase _cheatControlBinder;


        protected override VisualElement GenerateUI( )
        {
            var slider     = PrepareSlider( );
            if( slider == null )
                return null;                            //Incompatible cheat property type, ignore cheat

            slider.AddToClassList( "CheatLine" );
            return slider;
        }

        private VisualElement PrepareSlider( )
        {
            var cheatType = _cheatProperty != null ? _cheatProperty.PropertyType : _cheatField.FieldType;
            if( cheatType == typeof(Single) )
            {
                var  control = GetFloatSlider( );
                _cheatControlBinder = GetSimpleBinder( control );
                return control;
            }
            else if( cheatType == typeof(Double) )
            {
                var  control = GetFloatSlider();
                _cheatControlBinder = GetBinder<Single, Double>( control, f => f, d => d.ClampToFloat() );
                return control;
            }
            else if( cheatType == typeof(Int32) )
            {
                var  control = GetIntSlider(  );
                _cheatControlBinder = GetSimpleBinder( control );
                return control;
            }
            else if( cheatType == typeof(Int64) )
            {
                var  control = GetIntSlider(  );
                _cheatControlBinder = GetBinder<Int32, Int64>( control, f => f, p => p.ClampToInt32() );
                return control;
            }
            else if( cheatType == typeof(UInt32) )
            {
                var  control = GetIntSlider(  );
                _cheatControlBinder = GetBinder( control, f => f.ClampToUInt32(), p => p.ClampToInt32() );
                return control;
            }
            else if( cheatType == typeof(UInt64) )
            {
                var  control = GetIntSlider(  );
                _cheatControlBinder = GetBinder( control, f => f.ClampToUInt64(), p => p.ClampToInt32() );
                return control;
            }
            else if( cheatType == typeof(Byte) )
            {
                var  control = GetIntSlider(  );
                _cheatControlBinder = GetBinder( control, f => f.ClampToByte(), p => p );
                return control;
            }
            else if( cheatType == typeof(SByte) )
            {
                var  control = GetIntSlider(  );
                _cheatControlBinder = GetBinder( control, f => f.ClampToSByte(), p => p );
                return control;
            }
            else if( cheatType == typeof(UInt16) )
            {
                var  control = GetIntSlider(  );
                _cheatControlBinder = GetBinder( control, f => f.ClampToUInt16(), p => p );
                return control;
            }
            else if( cheatType == typeof(Int16) )
            {
                var  control = GetIntSlider(  );
                _cheatControlBinder = GetBinder( control, f => f.ClampToInt16(), p => p );
                return control;
            }

            return null;
        }

        protected override void RefreshUI( )
        {
            if( _cheatControlBinder != null )
                _cheatControlBinder.RefreshControl();
        }

        protected override RefreshUITiming GetRefreshUITiming( )
        {
            return _refreshTiming;
        }

        private Slider GetFloatSlider(  )
        {
            var slider = new Slider( Name, _min, _max );
            slider.showInputField = true;
            return slider;
        }

        private SliderInt GetIntSlider(  )
        {
            var slider = new SliderInt( Name, (int)_min, (int)_max );
            slider.showInputField = true;
            return slider;
        }

        private CheatControlBinderBase GetSimpleBinder<TControl>( BaseField<TControl> control )
        {
            if( _cheatProperty != null )
                return new PropertySimpleBinder<TControl>( control, _cheatProperty, _cheatObject );
            else
                return new FieldSimpleBinder<TControl>( control, _cheatField, _cheatObject );
        }

        private CheatControlBinderBase GetBinder<TControl, TCheat>( BaseField<TControl> control, Func<TControl, TCheat> controlToCheat, Func<TCheat, TControl> cheatToControl )
        {
            if( _cheatProperty != null )
                return new PropertyBinder<TControl, TCheat>( control, _cheatProperty, _cheatObject, controlToCheat, cheatToControl );
            else
                return new FieldBinder<TControl, TCheat>( control, _cheatField, _cheatObject, controlToCheat, cheatToControl );
        }


    }
}