using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class ParameterFieldSimpleWrapper<T> : CheatFieldWrapperBase
    {
        private readonly BaseField<T> _field;

        public ParameterFieldSimpleWrapper( BaseField<T> field )
        {
            _field = field;
        }

        public ParameterFieldSimpleWrapper( BaseField<T> field, T defaultValue )
        {
            _field = field;
            _field.SetValueWithoutNotify( defaultValue );
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
            //Not applicable
            throw new NotImplementedException();
        }
    }
}