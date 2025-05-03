using System;

namespace Silentor.CheatPanel
{
    public abstract class CheatFieldWrapperBase
    {
        public abstract Object GetBoxedFieldValue( );
        public abstract void RefreshFieldUI( );
    }
}