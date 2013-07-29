using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteFish
{
    class Idle
    {
        internal static bool NeedToRun { get { return false; } }

        internal static void Pulse()
        {
            Debug.MainGUI.statusBarText.Text = "Status: Idle";
        }
    }
}
