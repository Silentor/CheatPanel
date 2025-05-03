using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class PropertyFieldWrapper<TField, TProperty> : CheatFieldWrapperBase
    {
        private readonly BaseField<TField>       _field;
        private readonly PropertyInfo            _property;
        private readonly Func<TProperty, TField> _propToField;
        private readonly Func<TField, TProperty> _fieldToProp;
        private readonly Func<TProperty>         _getter;
        private readonly Action<TProperty>       _setter;

        public PropertyFieldWrapper(  BaseField<TField> field, PropertyInfo  property, ICheats cheatObject,
                                         Func<TField, TProperty> fieldToProp, Func<TProperty, TField> propToField )
        {
            Assert.IsTrue( property.CanRead || property.CanWrite );
            Assert.IsTrue( property.PropertyType == typeof(TProperty) );

            _field            = field;
            _property         = property;
            _propToField      = propToField;
            _fieldToProp = fieldToProp;
            if( property.CanRead )
                _getter = (Func<TProperty>)Delegate.CreateDelegate(typeof(Func<TProperty>), cheatObject, _property.GetGetMethod());
            if( property.CanWrite )
            {
                _setter = (Action<TProperty>)Delegate.CreateDelegate(typeof(Action<TProperty>), cheatObject, _property.GetSetMethod());
                _field.RegisterValueChangedCallback( OnFieldChanged );
            }
            else
                _field.SetEnabled( false );
        }

        private void OnFieldChanged( ChangeEvent<TField> evt )
        {
            var propValue = _fieldToProp( evt.newValue );
            _setter( propValue );
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
            var fieldValue = _propToField( value );
            _field.SetValueWithoutNotify( fieldValue );
        }

        
    }
    
}