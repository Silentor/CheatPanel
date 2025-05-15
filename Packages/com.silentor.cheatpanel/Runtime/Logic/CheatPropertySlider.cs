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
    public class CheatPropertySlider : Cheat
    {
        public CheatPropertySlider( PropertyInfo cheatProperty, RangeAttribute rangeAttr, ICheats cheatObject ): base( cheatProperty, cheatObject )
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

        private readonly RefreshUITiming       _refreshTiming;
        private readonly PropertyInfo          _cheatProperty;
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
            var propType = _cheatProperty.PropertyType;
            if( propType == typeof(Single) )
            {
                var  field = GetFloatSlider( );
                _cheatControlBinder = new PropertySimpleBinder<Single>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Double) )
            {
                var  field = GetFloatSlider();
                _cheatControlBinder = new PropertyBinder<Single, Double>( field, _cheatProperty, _cheatObject, f => f, d => d.ClampToFloat() );
                return field;
            }
            else if( propType == typeof(Int32) )
            {
                var  field = GetIntSlider(  );
                _cheatControlBinder = new PropertySimpleBinder<Int32>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Int64) )
            {
                var  field = GetIntSlider(  );
                _cheatControlBinder = new PropertyBinder<Int32, Int64>( field, _cheatProperty, _cheatObject, f => f, p => p.ClampToInt32() );
                return field;
            }
            else if( propType == typeof(UInt32) )
            {
                var  field = GetIntSlider(  );
                _cheatControlBinder = new PropertyBinder<Int32, UInt32>( field, _cheatProperty, _cheatObject, f => f.ClampToUInt32(), p => p.ClampToInt32() );
                return field;
            }
            else if( propType == typeof(UInt64) )
            {
                var  field = GetIntSlider(  );
                _cheatControlBinder = new PropertyBinder<Int32, UInt64>( field, _cheatProperty, _cheatObject, f => f.ClampToUInt64(), p => p.ClampToInt32() );
                return field;
            }
            else if( propType == typeof(Byte) )
            {
                var  field = GetIntSlider(  );
                _cheatControlBinder = new PropertyBinder<Int32, Byte>( field, _cheatProperty, _cheatObject, f => f.ClampToByte(), p => p );
                return field;
            }
            else if( propType == typeof(SByte) )
            {
                var  field = GetIntSlider(  );
                _cheatControlBinder = new PropertyBinder<Int32, SByte>( field, _cheatProperty, _cheatObject, f => f.ClampToSByte(), p => p );
                return field;
            }
            else if( propType == typeof(UInt16) )
            {
                var  field = GetIntSlider(  );
                _cheatControlBinder = new PropertyBinder<Int32, UInt16>( field, _cheatProperty, _cheatObject, f => f.ClampToUInt16(), p => p );
                return field;
            }
            else if( propType == typeof(Int16) )
            {
                var  field = GetIntSlider(  );
                _cheatControlBinder = new PropertyBinder<Int32, Int16>( field, _cheatProperty, _cheatObject, f => f.ClampToInt16(), p => p );
                return field;
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
            //slider.showInputField = true;
            return slider;
        }

        private SliderInt GetIntSlider(  )
        {
            var slider = new SliderInt( Name, (int)_min, (int)_max );
            //slider.showInputField = true;
            return slider;
        }
    }
}