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
    class FishbotAction
    {
        internal static bool IsFishing
        {
            get
            {
                WoW.Lua.DoString("spellData = UnitChannelInfo('player')");
                string spell = WoW.Lua.GetLocalizedText("spellData");

                if (spell.Length == 0)
                    return false;
                return true;
            }
        }

        internal static void CastSpellByName(string name)
        {
            WoW.Lua.DoString(string.Format("CastSpellByName(\"{0}\")", name));
        }

        internal static void CastItemByItemId(int itemId)
        {
            //UseMacroText("/use Name") will also work. Change it!
            WoW.Lua.DoString(string.Format(@"for b=1,4 do
    for i=1,36 do 
        itemId = GetContainerItemID(b, i);
        if itemId == {0} then
            UseContainerItem(b, i);
        end
    end
end", itemId));
        }

        internal static int GetInventorySlotsLeft
        {
            get {
                int totalSlotsLeft = 0;

                for(int i = 0; i <= 4; i++) {
                    WoW.Lua.DoString(string.Format("numberOfFreeSlots = GetContainerNumFreeSlots({0})", i));
                    int numberOfFreeSlots = Convert.ToInt32(WoW.Lua.GetLocalizedText("numberOfFreeSlots"));
                    totalSlotsLeft += numberOfFreeSlots;
                }

                return totalSlotsLeft;
            }
        }
    }
}
