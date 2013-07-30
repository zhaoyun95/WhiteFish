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
    class Idle
    {
        internal static bool NeedToRun { get { return false; } }

        internal static void Pulse()
        {
            Debug.MainGUI.statusBarText.Text = "Status: Idle";
        }
    }
}
