using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patches
{
    public class PartyPatches
    {
        // kinda ugly, though unsure how else to do it
        private static void _InitPostChange() => PreggoManager.Instance.InitSave();

        [HarmonyPatch(typeof(Party), nameof(Party.addCharacter))]
        [HarmonyPostfix]
        private static void Party_add(Stats s)
        {
            _InitPostChange();
        }

        [HarmonyPatch(typeof(Party), nameof(Party.removeCharacter))]
        [HarmonyPostfix]
        private static void Party_remove(Stats s)
        {
            _InitPostChange();
        }
    }
}
