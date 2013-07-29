using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WhiteRainNS;
using WhiteRainNS.Objects;

namespace WhiteFish
{
    class CheckInventory
    {
        internal static bool NeedToRun
        {
            get
            {
                if (FishbotAction.GetInventorySlotsLeft <= 2)
                    return true;
                return false;
            }
        }

        internal static void Pulse()
        {
            Debug.Log("Error: Inventory is full!");
            Engine.Exit();
            return;
        }
    }
}
