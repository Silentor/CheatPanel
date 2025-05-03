using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    /// <summary>
    /// Property type is equal to field type, no conversion needed
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public class PropertyFieldSimpleWrapper<TField> : CheatFieldWrapperBase
    {
        private readonly BaseField<TField> _field;
        private readonly PropertyInfo      _property;
        private readonly Func<TField>      _getter;
        private readonly Action<TField>    _setter;

        public PropertyFieldSimpleWrapper( BaseField<TField> field, PropertyInfo property, ICheats cheatObject )
        {
            Assert.IsTrue( property.CanRead || property.CanWrite );
            Assert.IsTrue( property.PropertyType == typeof(TField) );

            _field            = field;
            _property         = property;
            if( property.CanRead )
                _getter = (Func<TField>)Delegate.CreateDelegate(typeof(Func<TField>), cheatObject, _property.GetGetMethod());
            if( property.CanWrite )
            {
                _setter = (Action<TField>)Delegate.CreateDelegate(typeof(Action<TField>), cheatObject, _property.GetSetMethod());
                _field.RegisterValueChangedCallback( OnFieldChanged );
            }
            else
                _field.SetEnabled( false );
        }

        private void OnFieldChanged( ChangeEvent<TField> evt )
        {
            _setter( evt.newValue );
        }

        public override VisualElement GetField( )
        {
            return _field;
        }

        public override Object GetBoxedFieldValue( )
        {
            return _field.value;
        }

        public override void RefreshFieldUI( )
        {
            if( _getter == null )
                return;

            var value = _getter( );
            _field.SetValueWithoutNotify( value );
        }
    }
    
}