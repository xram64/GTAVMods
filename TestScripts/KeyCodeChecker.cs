using System;
using System.Windows.Forms;
using GTA;

namespace TestScripts
{
    // A tool to find the correct key codes for each keyboard key by displaying the name of each key when pressed.
    class KeyCodeChecker : Script
    {
        public KeyCodeChecker()
        {
            //KeyUp += OnKeyUp;    // ***Enable/disable script functionality here***
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            GTA.UI.Screen.ShowSubtitle("Key pressed: " + e.KeyCode.ToString(), 3000);
        }
    }
}
