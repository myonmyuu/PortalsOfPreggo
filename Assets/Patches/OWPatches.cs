using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patches
{
    internal class OWPatches
    {
        [HarmonyPatch(typeof(OWGameController), nameof(OWGameController.enemyMove))]
        [HarmonyPostfix]
        private static void OWGameController_enemyMove(int timePassed, bool canEngageInCombat)
        {
            PreggoManager.Instance.PassTimeForAll(timePassed);
        }
    }
}
