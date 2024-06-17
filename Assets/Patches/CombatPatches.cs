using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patches
{
    public static class CombatPatches
    {
        private static Action makePregShield(Character chara)
        {
            return () =>
            {
                if (!PreggoSettings.Instance.PregnantShield.Value)
                    return;

                if (!PreggoManager.Instance.TryGetData(chara.stats, out var data))
                    return;

                chara.gainShield(data.Ovums.Sum(x => x.Embryo.genetics.hp));
            };
        }
        
        [HarmonyPatch(typeof(Character), nameof(Character.setPassives))]
        [HarmonyPostfix]
        private static void Character_addPregShield(Character __instance)
        {
            __instance.onTurnBegin = (Action)Delegate.Combine(__instance.onTurnBegin, makePregShield(__instance));
        }
    }
}
