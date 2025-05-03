using System;
using System.Reflection;
using System.Threading;
using Silentor.CheatPanel.Utils;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class CheatPropertyField : Cheat
    {
        public CheatPropertyField( PropertyInfo cheatProperty, ICheats cheatObject, CancellationToken cancel ): base( cheatProperty, cheatObject, cancel )
        {
            Assert.IsTrue( cheatProperty.CanRead || cheatProperty.CanWrite );

            _cheatProperty = cheatProperty;
            _cheatObject   = cheatObject;
            _refreshTiming = cheatProperty.CanRead
                    ? Attr?.RefreshTime ?? RefreshUITiming.PerSecond
                    : RefreshUITiming.Never;
            //_min           = min;
        }

        private readonly RefreshUITiming _refreshTiming;
        private readonly PropertyInfo    _cheatProperty;
        private readonly ICheats         _cheatObject;
        private readonly Single          _min;
        private CheatFieldWrapperBase        _cheatFieldWrapper;

        protected override VisualElement GenerateUI( )
        {
            var cheatName = Name;
            var propType = _cheatProperty.PropertyType;

            if( propType == typeof(Single) )
            {
                var  field = new FloatField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldSimpleWrapper<Single>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Double) )
            {
                var  field = new DoubleField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldSimpleWrapper<Double>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Int32) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldSimpleWrapper<Int32>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Int64) )
            {
                var  field = new LongField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldSimpleWrapper<Int64>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(UInt32) )
            {
                var  field = new UnsignedIntegerField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldSimpleWrapper<UInt32>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(UInt64) )
            {
                var  field = new UnsignedLongField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldSimpleWrapper<UInt64>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Byte) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldWrapper<Int32, Byte>( field, _cheatProperty, _cheatObject, i => i.ClampToUInt8(), b => b );
                return field;
            }
            else if( propType == typeof(SByte) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldWrapper<Int32, SByte>( field, _cheatProperty, _cheatObject, i => i.ClampToInt8(), b => b );
                return field;
            }
            else if( propType == typeof(UInt16) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldWrapper<Int32, UInt16>( field, _cheatProperty, _cheatObject, i => i.ClampToUInt16(), b => b );
                return field;
            }
            else if( propType == typeof(Int16) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldWrapper<Int32, Int16>( field, _cheatProperty, _cheatObject, i => i.ClampToInt16(), b => b );
                return field;
            }
            else if( propType == typeof(Boolean) )
            {
                var  field = new Toggle( cheatName );
                field.AddToClassList( "CheatLine" );        //Special case
                field.AddToClassList( "CheatToggle" );
                _cheatFieldWrapper = new PropertyFieldSimpleWrapper<Boolean>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(String) )
            {
                var  field = new TextField( cheatName );
                StyleField( field );
                _cheatFieldWrapper = new PropertyFieldSimpleWrapper<String>( field, _cheatProperty, _cheatObject );
                return field;
            }

            return null;
        }

        protected override void RefreshUI( )
        {
            if( _cheatFieldWrapper != null )
                _cheatFieldWrapper.RefreshFieldUI();
        }

        protected override RefreshUITiming GetRefreshUITiming( )
        {
            return _refreshTiming;
        }

        private static void StyleField<TField>( TextInputBaseField<TField> field )
        {
            field.AddToClassList( "CheatLine" );
            field.AddToClassList( "CheatTextBox" );
            field.isDelayed = true;
        }
    }
}