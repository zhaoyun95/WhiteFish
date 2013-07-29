using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WhiteFish
{
    class Lur
    {
        internal static int Tries { get; set; }

        internal static bool NeedToRun
        {
            get
            {
                if (Debug.MainGUI.lurId.Text.Length == 0) //No need to run, when the user doesn't want to use Lurs.
                    return false;

                WoW.Lua.DoString("hasMainHandEnchant, timeLeft = GetWeaponEnchantInfo();");
                int timeLeft;
                Int32.TryParse(WoW.Lua.GetLocalizedText("timeLeft"), out timeLeft);

                if (timeLeft == 0)
                    return true;
                return false;
            }
        }

        internal static void Pulse()
        {
            while (NeedToRun)
            {
                Debug.MainGUI.statusBarText.Text = "Status: Putting lur";

                if (Tries >= 5)
                {
                    Debug.Log(string.Format("Error: Unable to put lur. Tries: {0}", Tries));
                    Debug.Log("Please make sure your character has that lur in your inventory.");
                    Engine.Exit();
                    return;
                }

                Debug.Log(string.Format("Putting lur. Trie(s): {0}", (Tries + 1)));
                FishbotAction.CastItemByItemId(Convert.ToInt32(Debug.MainGUI.lurId.Text));
                Tries++;
                Thread.Sleep(3500);

                if (!NeedToRun) //Success!
                    Tries = 0;
            }
        }
    }
}
