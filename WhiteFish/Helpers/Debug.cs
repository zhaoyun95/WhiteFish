/*
 * This file is part of the WhiteRain project (C) 2013 Finn Grimpe
 * Copyright 2013 Finn Grimpe, All Rights Reserved
 * 
 * Github:  https://github.com/finndev/WhiteFish/
 * Website: https://finn.lu/whitefish/
 * License: https://finn.lu/license/
 *
 */

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
