using System;

namespace Silentor.CheatPanel.DevProject
{
    public class AddRemoveTestCheats : ICheats
    {
        private readonly CheatPanel _panel;
        private Test1Cheats _cheats1;
        private Test2Cheats _cheats2;

        public AddRemoveTestCheats( CheatPanel panel )
        {
            _panel = panel;
        }

        public void AddCheat1( )
        {
            _cheats1 = new Test1Cheats();
            _panel.AddCheats( _cheats1 );
        }

        public void AddCheat2( )
        {
            _cheats2 = new Test2Cheats();
            _panel.AddCheats( _cheats2 );
        }

        public void RemoveCheat1( )
        {
            _panel.RemoveCheats( _cheats1 );
        }

        public void RemoveCheat2( )
        {
            _panel.RemoveCheats( _cheats2 );
        }


    }
}