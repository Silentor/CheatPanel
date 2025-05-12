using System;
using UnityEngine.UIElements;

namespace Silentor.CheatPanel.UI
{
    public class DoubleClickManipulator : Clickable
    {
        public DoubleClickManipulator(Action handler, Int64 delay, Int64 interval ) : base( handler, delay, interval )
        {
            var activator = activators[0];
            activator.clickCount = 2;
            activators[0] = activator;
        }

        public DoubleClickManipulator(Action<EventBase> handler ) : base( handler )
        {
            var activator = activators[0];
            activator.clickCount = 2;
            activators[0]        = activator;
        }

        public DoubleClickManipulator(Action handler ) : base( handler )
        {
            var activator = activators[0];
            activator.clickCount = 2;
            activators[0]        = activator;
        }
    }
}