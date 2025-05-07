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
        private CheatFieldBinderBase        _cheatFieldBinder;

        protected override VisualElement GenerateUI( )
        {
            var cheatName = Name;
            var propType = _cheatProperty.PropertyType;

            if( propType == typeof(Single) )
            {
                var  field = new FloatField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Single>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Double) )
            {
                var  field = new DoubleField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Double>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Int32) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Int32>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Int64) )
            {
                var  field = new LongField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Int64>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(UInt32) )
            {
                var  field = new UnsignedIntegerField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<UInt32>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(UInt64) )
            {
                var  field = new UnsignedLongField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<UInt64>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(Byte) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertyBinder<Int32, Byte>( field, _cheatProperty, _cheatObject, i => i.ClampToUInt8(), b => b );
                return field;
            }
            else if( propType == typeof(SByte) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertyBinder<Int32, SByte>( field, _cheatProperty, _cheatObject, i => i.ClampToInt8(), b => b );
                return field;
            }
            else if( propType == typeof(UInt16) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertyBinder<Int32, UInt16>( field, _cheatProperty, _cheatObject, i => i.ClampToUInt16(), b => b );
                return field;
            }
            else if( propType == typeof(Int16) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertyBinder<Int32, Int16>( field, _cheatProperty, _cheatObject, i => i.ClampToInt16(), b => b );
                return field;
            }
            else if( propType == typeof(Boolean) )
            {
                var  field = new Toggle( cheatName );
                field.AddToClassList( "CheatLine" );        //Special case
                //field.AddToClassList( "CheatToggle" );
                _cheatFieldBinder = new PropertySimpleBinder<Boolean>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if( propType == typeof(String) )
            {
                var  field = new TextField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<String>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType == typeof(Vector3) )
            {
                var  field = new Vector3Field( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Vector3>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType == typeof(Vector2) )
            {
                var  field = new Vector2Field( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Vector2>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType == typeof(Vector4) )
            {
                var  field = new Vector4Field( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Vector4>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType == typeof(Vector3Int) )
            {
                var  field = new Vector3IntField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Vector3Int>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType == typeof(Vector2Int) )
            {
                var  field = new Vector2IntField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Vector2Int>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType == typeof(Rect) )
            {
                var  field = new RectField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Rect>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType == typeof(Bounds) )
            {
                var  field = new BoundsField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<Bounds>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType == typeof(RectInt) )
            {
                var  field = new RectIntField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<RectInt>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType == typeof(BoundsInt) )
            {
                var  field = new BoundsIntField( cheatName );
                StyleField( field );
                _cheatFieldBinder = new PropertySimpleBinder<BoundsInt>( field, _cheatProperty, _cheatObject );
                return field;
            }
            else if ( propType.IsEnum )
            {
                var value = propType.GetEnumValues().GetValue( 0 ); //We need this boxing to init field with values
                var  field = new EnumField( cheatName, (Enum)value );
                StyleField( field );
                _cheatFieldBinder = new PropertyEnumBinder( field, _cheatProperty, _cheatObject );
                return field;
            }


            return null;
        }

        protected override void RefreshUI( )
        {
            if( _cheatFieldBinder != null )
                _cheatFieldBinder.RefreshFieldUI();
        }

        protected override RefreshUITiming GetRefreshUITiming( )
        {
            return _refreshTiming;
        }

        private static void StyleField<TField>( TextInputBaseField<TField> field )
        {
            field.AddToClassList( "CheatLine" );
            //field.AddToClassList( "CheatTextBox" );
            field.isDelayed = true;
        }

        private static void StyleField<TField>( BaseField<TField> field )
        {
            field.AddToClassList( "CheatLine" );
            //field.AddToClassList( "CheatTextBox" );
        }
    }
}