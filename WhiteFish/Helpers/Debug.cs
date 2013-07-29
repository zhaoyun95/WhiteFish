using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteFish
{
    class Debug
    {
        internal static GUI.Main MainGUI;

        internal static void Initialize(GUI.Main mainGUI)
        {
            MainGUI = mainGUI;
        }

        internal static void Log(string msg)
        {
             MainGUI.LogBox.AppendText(string.Format("[WhiteFish] D: {0} {1}", msg, Environment.NewLine));
        }
    }
}
