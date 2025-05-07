using System.Reflection;
using System.Threading;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel
{
    /// <summary>
    /// Simple sync cheat button without params
    /// </summary>
    public class CheatMethod : Cheat
    {
        private readonly MethodInfo _methodInfo;
        private readonly ICheats    _cheatObject;
        private readonly CheatPanel _cheatPanel;

        public CheatMethod( MethodInfo methodInfo, ICheats cheatObject, CheatPanel cheatPanel ) : base( methodInfo, cheatObject )
        {
            _methodInfo      = methodInfo;
            _cheatObject     = cheatObject;
            _cheatPanel = cheatPanel;
        }

        protected override VisualElement GenerateUI( )
        {
            var btn = new Button( );
            btn.text = Name;
            btn.AddToClassList( "CheatBtn" );
            btn.AddToClassList( "CheatLine" );
            btn.clicked += ( ) =>
            {
                var result = _methodInfo.Invoke( _cheatObject, null );
                if( result != null )
                {
                    _cheatPanel.ShowResult( result.ToString() );
                }
            };

            return btn;
        }

        protected override void RefreshUI( )
        {
            //No need
            throw new System.NotImplementedException();
        }

        protected override RefreshUITiming GetRefreshUITiming( )
        {
            return RefreshUITiming.Never;
        }
    }
}