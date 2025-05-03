using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class ParameterFieldWrapper<TField, TParameter> : CheatFieldWrapperBase
    {
        private readonly BaseField<TField>        _field;
        private readonly Func<TField, TParameter> _fieldToParam;

        public ParameterFieldWrapper( BaseField<TField> field, TField defaultValue, Func<TField, TParameter> fieldToParam )
        {
            _field             = field;
            _fieldToParam = fieldToParam;
            _field.SetValueWithoutNotify( defaultValue );
        }

        public override VisualElement GetField( )
        {
            return _field;
        }

        public override Object GetBoxedFieldValue( )
        {
            return _fieldToParam ( _field.value );
        }

        public override void RefreshFieldUI( )
        {
            //Not applicable
            throw new NotImplementedException();
        }
    }
}