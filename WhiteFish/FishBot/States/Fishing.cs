using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WhiteFish
{
    class Fishing
    {
        internal static int Tries { get ; set; }

        internal static bool NeedToRun
        {
            get
            {
                if (!FishbotAction.IsFishing)
                    return true;
                return false;
            }
        }

        internal static void Pulse()
        {
            while (NeedToRun)
            {
                if (Looting.lootLoggedInLogBox || Looting.startLoggedInLogBox)
                {
                    Looting.lootLoggedInLogBox = false;
                    Looting.startLoggedInLogBox = false;
                }

                Debug.MainGUI.statusBarText.Text = "Status: Casting Fishing";

                if (Tries >= 6)
                {
                    Debug.Log(string.Format("Error: Unable to fish. Tries: {0}", Tries));
                    Debug.Log("Please make sure your character is able to fish.");
                    Engine.Exit();
                    return;
                }

                Debug.Log(string.Format("Cast Fishing. Trie(s): {0}", (Tries + 1)));

                FishbotAction.CastSpellByName(Debug.MainGUI.fishingSpell.Text);
                Tries++;
                Thread.Sleep(350);

                if (FishbotAction.IsFishing)
                    Tries = 0;
            }
        }
    }
}
