using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder enum cheat property <-> UI control. Works with boxing.
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public class PropertyEnumBinder : CheatFieldBinderBase
    {
        private readonly EnumField _field;
        private readonly PropertyInfo      _property;
        private readonly ICheats _cheatObject;

        public PropertyEnumBinder( EnumField field, PropertyInfo property, ICheats cheatObject )
        {
            Assert.IsTrue( property.CanRead || property.CanWrite );

            _field            = field;
            _property         = property;
            _cheatObject = cheatObject;

            if ( property.CanWrite )
                _field.RegisterValueChangedCallback( OnFieldChanged );
            else
                _field.SetEnabled( false );
        }

        private void OnFieldChanged( ChangeEvent<Enum> changeEvent )
        {
            _property.SetValue( _cheatObject, changeEvent.newValue );
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
            if ( _property.CanRead )
            {
                var boxedValue = _property.GetValue( _cheatObject );
                _field.SetValueWithoutNotify( (Enum)boxedValue );
            }
        }
    }
    
}