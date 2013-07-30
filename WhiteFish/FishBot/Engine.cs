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
using System.Threading;

namespace WhiteFish
{
    class Engine
    {
        internal static Thread FishThread { get; set; }

        internal static void Run()
        {
            FishThread = new Thread(delegate()
            {
                while (true)
                {
                    //Go through all states, ordered by "priority", check if they need to be run and if so, run them.

                    if (CheckInventory.NeedToRun) //Is our inventory full?
                        CheckInventory.Pulse();

                    if (Looting.NeedToRun) //Are we already casting fishing?
                        Looting.Pulse();

                    if (Lure.NeedToRun) //Do we have to renew our lur?
                        Lure.Pulse();

                    if (Fishing.NeedToRun) //Do we have to fish?
                        Fishing.Pulse();

                    if (Idle.NeedToRun) //zZzZz
                        Idle.Pulse();
                }
            });
            FishThread.Start();
        }

        internal static void Exit()
        {
            Debug.MainGUI.statusBarText.Text = "Status: Idle";
            Debug.MainGUI.StartStopBtn.Text = "Start";
            if(FishThread != null)
                FishThread.Abort();
        }
    }
}
