using System;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder cheat field <-> UI control. Field type is equal to field type, no conversion needed
    /// </summary>
    /// <typeparam name="TControl"></typeparam>
    public class FieldBinder<TControl, TField> : CheatControlBinderBase
    {
        private readonly BaseField<TControl>  _control;
        private readonly FieldInfo          _field;
        private readonly Func<TField, TControl> _fieldToControl;
        private readonly Func<TControl, TField> _controlToField;
        private readonly ICheats _cheatObject;

        public FieldBinder( BaseField<TControl> control, FieldInfo field, ICheats cheatObject, 
                            Func<TControl, TField> controlToField, Func<TField, TControl> fieldToControl )
        {
            Assert.IsTrue( field.FieldType == typeof(TField) );

            _control       = control;
            _field         = field;
            _cheatObject = cheatObject;
            _fieldToControl = fieldToControl;
            _controlToField = controlToField;

            if ( !field.IsInitOnly && !field.IsLiteral )
                _control.RegisterValueChangedCallback( OnControlChanged );
            else
                _control.SetEnabled( false );
        }

        private void OnControlChanged( ChangeEvent<TControl> evt )
        {
            var fieldValue = _controlToField( evt.newValue );
            _field.SetValue( _cheatObject, fieldValue );
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
            var controlValue = _fieldToControl( (TField)value );
            _control.SetValueWithoutNotify( controlValue );
        }
    }
    
}