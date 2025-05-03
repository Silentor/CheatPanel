using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    public class FieldOnlyWrapper<T> : CheatFieldWrapperBase
    {
        private readonly BaseField<T> _field;

        public FieldOnlyWrapper( BaseField<T> field )
        {
            _field = field;
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