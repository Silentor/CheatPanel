using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder cheat property <-> UI control. Property type is equal to field type, no conversion needed
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public class PropertySimpleBinder<TField> : CheatControlBinderBase
    {
        private readonly BaseField<TField> _field;
        private readonly PropertyInfo      _property;
        private readonly Func<TField>      _getter;
        private readonly Action<TField>    _setter;

        public PropertySimpleBinder( BaseField<TField> field, PropertyInfo property, ICheats cheatObject )
        {
            Assert.IsTrue( property.CanRead || property.CanWrite );
            Assert.IsTrue( property.PropertyType == typeof(TField) || (property.PropertyType.IsEnum && typeof(TField) == typeof(Enum)) );

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

        public override VisualElement GetControl( )
        {
            return _field;
        }

        public override Object GetBoxedControlValue( )
        {
            return _field.value;
        }

        public override void RefreshControl( )
        {
            if( _getter == null )
                return;

            var value = _getter( );
            _field.SetValueWithoutNotify( value );
        }
    }
    
}