using System;
using System.Reflection;
using Silentor.CheatPanel.Binders;
using Silentor.CheatPanel.UI;
using Silentor.CheatPanel.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class CheatTextBox : Cheat
    {
        public CheatTextBox( FieldInfo cheatField, ICheats cheatObject ) : base( cheatField, cheatObject )
        {
            _cheatField = cheatField;
            _cheatObject   = cheatObject;
            _refreshTiming = cheatField.IsInitOnly | cheatField.IsLiteral
                    ? RefreshUITiming.OneTime
                    : Attr?.RefreshTime ?? RefreshUITiming.PerSecond;
            //_min           = min;
        }

        public CheatTextBox( PropertyInfo cheatProperty, ICheats cheatObject ): base( cheatProperty, cheatObject )
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
        private readonly FieldInfo       _cheatField;
        private readonly ICheats         _cheatObject;
        private readonly Single          _min;
        private CheatControlBinderBase   _cheatControlBinder;

        protected override VisualElement GenerateUI( )
        {
            var isProperty = _cheatProperty != null;
            var cheatName = Name;
            var cheatType = isProperty ? _cheatProperty.PropertyType : _cheatField.FieldType;

            if( cheatType == typeof(Single) )
            {
                var  control = new FloatField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if( cheatType == typeof(Double) )
            {
                var control = new DoubleField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty ,control );
                return control;
            }
            else if( cheatType == typeof(Int32) )
            {
                var control = new IntegerField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if( cheatType == typeof(Int64) )
            {
                var  control = new LongField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if( cheatType == typeof(UInt32) )
            {
                var  control = new UnsignedIntegerField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if( cheatType == typeof(UInt64) )
            {
                var  control = new UnsignedLongField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if( cheatType == typeof(Byte) )
            {
                var  control = new IntegerField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetBinder( isProperty, control, i => i.ClampToByte(), b => b );
                return control;
            }
            else if( cheatType == typeof(SByte) )
            {
                var  control = new IntegerField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetBinder( isProperty, control, i => i.ClampToSByte(), b => b );
                return control;
            }
            else if( cheatType == typeof(UInt16) )
            {
                var  control = new IntegerField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetBinder( isProperty, control, i => i.ClampToUInt16(), b => b );
                return control;
            }
            else if( cheatType == typeof(Int16) )
            {
                var  control = new IntegerField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetBinder( isProperty, control, i => i.ClampToInt16(), b => b );
                return control;
            }
            else if( cheatType == typeof(Boolean) )
            {
                var  control = new Toggle( cheatName );
                control.AddToClassList( "CheatLine" );        //Special case styling for toggle
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if( cheatType == typeof(String) )
            {
                var  control = new TextField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType == typeof(Vector3) )
            {
                var  control = new Vector3Field( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType == typeof(Vector2) )
            {
                var  control = new Vector2Field( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType == typeof(Vector4) )
            {
                var  control = new Vector4Field( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control);
                return control;
            }
            else if ( cheatType == typeof(Vector3Int) )
            {
                var  control = new Vector3IntField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType == typeof(Vector2Int) )
            {
                var  control = new Vector2IntField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType == typeof(Rect) )
            {
                var  control = new RectField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType == typeof(Bounds) )
            {
                var  control = new BoundsField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType == typeof(RectInt) )
            {
                var  control = new RectIntField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType == typeof(BoundsInt) )
            {
                var  control = new BoundsIntField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType == typeof(Color) )
            {
                var  control = new MiniColorField( cheatName );
                StyleField( control );
                _cheatControlBinder = GetSimpleBinder( isProperty, control );
                return control;
            }
            else if ( cheatType.IsEnum )
            {
                var value = cheatType.GetEnumValues().GetValue( 0 ); //We need this boxing to init field with values
                var  control = new EnumField( cheatName, (Enum)value );
                StyleField( control );
                _cheatControlBinder = isProperty 
                        ? new PropertyEnumBinder( control, _cheatProperty, _cheatObject )
                        : new FieldEnumBinder( control, _cheatField, _cheatObject );
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

        private CheatControlBinderBase GetSimpleBinder<TCheat>( Boolean isProperty, BaseField<TCheat> control )
        {
            if( isProperty )
                return new PropertySimpleBinder<TCheat>( control, _cheatProperty, _cheatObject );
            else
                return new FieldSimpleBinder<TCheat>( control, _cheatField, _cheatObject );
        }

        private CheatControlBinderBase GetBinder<TCheat, TControl>( Boolean isProperty, BaseField<TControl> control, Func<TControl, TCheat> controlToCheat, Func<TCheat, TControl> cheatToControl )
        {
            if( isProperty )
                return new PropertyBinder<TControl, TCheat>( control, _cheatProperty, _cheatObject, controlToCheat, cheatToControl );
            else
                return new FieldBinder<TControl, TCheat>( control, _cheatField, _cheatObject, controlToCheat, cheatToControl );
        }

        public override String ToString( )
        {
            return $"{_cheatObject.GetType().Name} {_cheatProperty} ({GetType().Name})";
        }

        private static void StyleField<TField>( TextInputBaseField<TField> field )
        {
            field.AddToClassList( "CheatLine" );
            field.isDelayed = true;
        }

        private static void StyleField<TField>( BaseField<TField> field )
        {
            field.AddToClassList( "CheatLine" );
        }
    }
}