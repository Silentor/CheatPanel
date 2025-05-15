using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.Binders
{
    /// <summary>
    /// Binder for some value and UI control. Used for cheat method parameters
    /// </summary>
    public class ParameterSimpleBinder<T> : CheatControlBinderBase
    {
        private readonly BaseField<T> _field;

        public ParameterSimpleBinder( BaseField<T> field )
        {
            _field = field;
        }

        public ParameterSimpleBinder( BaseField<T> field, T defaultValue )
        {
            _field = field;
            _field.SetValueWithoutNotify( defaultValue );
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
            //Not applicable
            throw new NotImplementedException();
        }
    }
}