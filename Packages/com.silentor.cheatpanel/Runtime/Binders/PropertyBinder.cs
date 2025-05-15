using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder cheat property <-> UI control
    /// </summary>
    public class PropertyBinder<TControl, TProperty> : CheatControlBinderBase
    {
        private readonly BaseField<TControl>       _control;
        private readonly PropertyInfo            _property;
        private readonly Func<TProperty, TControl> _propToControl;
        private readonly Func<TControl, TProperty> _controlToProp;
        private readonly Func<TProperty>         _getter;
        private readonly Action<TProperty>       _setter;

        public PropertyBinder(  BaseField<TControl> control, PropertyInfo  property, ICheats cheatObject,
                                         Func<TControl, TProperty> controlToProp, Func<TProperty, TControl> propToControl )
        {
            Assert.IsTrue( property.CanRead || property.CanWrite );
            Assert.IsTrue( property.PropertyType == typeof(TProperty) );

            _control            = control;
            _property         = property;
            _propToControl      = propToControl;
            _controlToProp = controlToProp;
            if( property.CanRead )
                _getter = (Func<TProperty>)Delegate.CreateDelegate(typeof(Func<TProperty>), cheatObject, _property.GetGetMethod());
            if( property.CanWrite )
            {
                _setter = (Action<TProperty>)Delegate.CreateDelegate(typeof(Action<TProperty>), cheatObject, _property.GetSetMethod());
                _control.RegisterValueChangedCallback( OnFieldChanged );
            }
            else
                _control.SetEnabled( false );
        }

        private void OnFieldChanged( ChangeEvent<TControl> evt )
        {
            var propValue = _controlToProp( evt.newValue );
            _setter( propValue );
        }

        public override VisualElement GetControl( )
        {
            return _control;
        }

        public override Object GetBoxedControlValue( )
        {
            return _control.value;
        }

        public override void RefreshControl( )
        {
            if( _getter == null )
                return;

            var value = _getter( );
            var fieldValue = _propToControl( value );
            _control.SetValueWithoutNotify( fieldValue );
        }

        
    }
    
}