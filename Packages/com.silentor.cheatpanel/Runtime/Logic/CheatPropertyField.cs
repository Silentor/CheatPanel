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

        private          Action          _refreshFieldUI;
        private readonly RefreshUITiming _refreshTiming;
        private readonly PropertyInfo    _cheatProperty;
        private readonly ICheats         _cheatObject;
        private readonly Single          _min;

        protected override VisualElement GenerateUI( )
        {
            var cheatName = Name;
            var propType = _cheatProperty.PropertyType;

            if( propType == typeof(Single) )
            {
                var  field = new FloatField( cheatName ){ isDelayed = true };
                StyleField( field );
                _refreshFieldUI = GetRefreshUI( field );
                PrepareUpdateCheatProperty( field );
                return field;
            }
            else if( propType == typeof(Double) )
            {
                var  field = new DoubleField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI( field );
                PrepareUpdateCheatProperty( field );
                return field;
            }
            else if( propType == typeof(Int32) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI( field );
                PrepareUpdateCheatProperty( field );
                return field;
            }
            else if( propType == typeof(Int64) )
            {
                var  field = new LongField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI( field );
                PrepareUpdateCheatProperty( field );
                return field;
            }
            else if( propType == typeof(UInt32) )
            {
                var  field = new UnsignedIntegerField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI( field );
                PrepareUpdateCheatProperty( field );
                return field;
            }
            else if( propType == typeof(UInt64) )
            {
                var  field = new UnsignedLongField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI( field );
                PrepareUpdateCheatProperty( field  );
                return field;
            }
            else if( propType == typeof(Byte) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI<int, byte>( field, v => v );
                PrepareUpdateCheatProperty( field, v => v.ClampToUInt8() );
                return field;
            }
            else if( propType == typeof(SByte) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI<int, sbyte>( field, v => v );
                PrepareUpdateCheatProperty( field, v => v.ClampToInt8() );
                return field;
            }
            else if( propType == typeof(UInt16) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI<int, ushort>( field, v => v );
                PrepareUpdateCheatProperty( field, v => v.ClampToUInt16() );
                return field;
            }
            else if( propType == typeof(Int16) )
            {
                var  field = new IntegerField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI<int, short>( field, v => v );
                PrepareUpdateCheatProperty( field, v => v.ClampToInt16() );
                return field;
            }
            else if( propType == typeof(Boolean) )
            {
                var  field = new Toggle( cheatName );
                field.AddToClassList( "CheatLine" );        //Special case
                field.AddToClassList( "CheatToggle" );
                _refreshFieldUI = GetRefreshUI( field );
                PrepareUpdateCheatProperty( field );
                return field;
            }
            else if( propType == typeof(String) )
            {
                var  field = new TextField( cheatName );
                StyleField( field );
                _refreshFieldUI = GetRefreshUI( field );
                PrepareUpdateCheatProperty( field );
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

        private static void StyleField<TField>( TextInputBaseField<TField> field )
        {
            field.AddToClassList( "CheatLine" );
            field.AddToClassList( "CheatTextBox" );
            field.isDelayed = true;
        }

        private Action GetRefreshUI<TField>( TextInputBaseField<TField> field )
        {
            if( _cheatProperty.CanRead )
            {
                var getter = (Func<TField>)Delegate.CreateDelegate( typeof(Func<TField>), _cheatObject, _cheatProperty.GetGetMethod() );
                return ( ) =>
                {
                    field.SetValueWithoutNotify( getter() );
                };
            }

            return null;
        }

        private Action GetRefreshUI<TField, TProp>( TextInputBaseField<TField> field, Func<TProp, TField> propToFieldConversion )
        {
            if( _cheatProperty.CanRead )
            {
                var getter = (Func<TProp>)Delegate.CreateDelegate( typeof(Func<TProp>), _cheatObject, _cheatProperty.GetGetMethod() );
                return ( ) =>
                {
                    field.SetValueWithoutNotify( propToFieldConversion( getter() ) );
                };
            }

            return null;
        }

        private Action GetRefreshUI( Toggle field )
        {
            if( _cheatProperty.CanRead )
            {
                var getter = (Func<Boolean>)Delegate.CreateDelegate( typeof(Func<Boolean>), _cheatObject, _cheatProperty.GetGetMethod() );
                return ( ) =>
                {
                    field.SetValueWithoutNotify( getter() );
                };
            }

            return null;
        }

        private void PrepareUpdateCheatProperty<TField>( TextInputBaseField<TField> field )
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

        private void PrepareUpdateCheatProperty<TField, TProp>( TextInputBaseField<TField> field, Func<TField, TProp> fieldToPropConversion )
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

        private void PrepareUpdateCheatProperty( Toggle field )
        {
            if( _cheatProperty.CanWrite )
            {
                var setter = (Action<Boolean>)Delegate.CreateDelegate( typeof(Action<Boolean>), _cheatObject, _cheatProperty.GetSetMethod() );
                field.RegisterValueChangedCallback( evt => setter( evt.newValue ) );
            }
            else
            {
                field.SetEnabled( false );
            }
        }
    }
}