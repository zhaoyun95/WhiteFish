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
using WhiteRainNS;
using WhiteRainNS.Objects;

namespace WhiteFish
{
    class Looting
    {
        internal static bool startLoggedInLogBox { get; set; }
        internal static bool lootLoggedInLogBox { get; set; }
        internal static int totalLoots { get; set; }

        internal static bool NeedToRun
        {
            get
            {
                if (FishbotAction.IsFishing)
                    return true;
                return false;
            }
        }

        internal static void Pulse()
        {
            if (!startLoggedInLogBox)
            {
                Debug.Log("Waiting for fish to bait");
                startLoggedInLogBox = true;
            }

            lock (WhiteRain.WoWGameObjectList)
            {
                while (NeedToRun)
                {
                    Debug.MainGUI.statusBarText.Text = "Status: Waiting for fish";

                    WoWGameObject Bobber = WhiteRain.WoWGameObjectList.Where(obj => obj.CreatedBy == WhiteRain.Me.Guid).FirstOrDefault();
                    uint bobberState = WhiteRain.WoW.Read<byte>(Bobber.BaseAddress + (int)Offsets.Fishbot.BobberHasMoved);

                    if (bobberState == 1)
                    {
                        Debug.MainGUI.statusBarText.Text = "Status: Looting";

                        if (!lootLoggedInLogBox)
                        {
                            Debug.Log("Clicking on bobber!");
                            totalLoots++;
                            lootLoggedInLogBox = true;
                        }

                        WhiteRain.WoW.Write<ulong>(WhiteRain.WoW.ImageBase + (int)Offsets.Fishbot.MouseOverGUID, Bobber.Guid);
                        WoW.Lua.DoString(string.Format("InteractUnit('mouseover')"));
                        Debug.MainGUI.totalLootsText.Text = string.Format("Total Loots: {0}", totalLoots);
                    }
                }
            }
        }
    }
}
