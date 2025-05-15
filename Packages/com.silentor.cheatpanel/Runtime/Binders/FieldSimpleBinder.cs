using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder cheat field <-> UI control. Field type is equal to field type, no conversion needed
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public class FieldSimpleBinder<TField> : CheatControlBinderBase
    {
        private readonly BaseField<TField>  _control;
        private readonly FieldInfo          _field;

        private readonly ICheats _cheatObject;

        public FieldSimpleBinder( BaseField<TField> control, FieldInfo field, ICheats cheatObject )
        {
            Assert.IsTrue( field.FieldType == typeof(TField) || (field.FieldType.IsEnum && typeof(TField) == typeof(Enum)) );

            _control       = control;
            _field         = field;
            _cheatObject = cheatObject;

            if ( !field.IsInitOnly && !field.IsLiteral )
                _control.RegisterValueChangedCallback( OnControlChanged );
            else
                _control.SetEnabled( false );
        }

        private void OnControlChanged( ChangeEvent<TField> evt )
        {
            _field.SetValue( _cheatObject, evt.newValue );
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
            var value = _field.GetValue( _cheatObject );
            _control.SetValueWithoutNotify( (TField)value );
        }
    }
    
}