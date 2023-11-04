using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Patches
{
    public class FarmPatches
    {
		[HarmonyPatch(typeof(FarmPairing), nameof(FarmPairing.breedCharacters))]
		[HarmonyPostfix]
		private static void FarmPairing_breedCharacters(FarmPairing __instance)
		{
			var a_b_valid = __instance.a.character.hasCock() && __instance.b.character.hasPussy();
			var b_a_valid = __instance.b.character.hasCock() && __instance.a.character.hasPussy();

			if (__instance.a.behaviour == FarmCharacter.Behaviour.Breed)
			{
                if (a_b_valid)
                    PreggoManager.Instance.CumIn(__instance.b.character, __instance.a.character, quiet: true);
                else if (b_a_valid)
                    PreggoManager.Instance.CumIn(__instance.a.character, __instance.b.character, quiet: true);
            }
            if (__instance.b.behaviour == FarmCharacter.Behaviour.Breed)
            {
                if (b_a_valid)
                    PreggoManager.Instance.CumIn(__instance.a.character, __instance.b.character, quiet: true);
                else if (a_b_valid)
                    PreggoManager.Instance.CumIn(__instance.b.character, __instance.a.character, quiet: true);
            }
		}

		[HarmonyPatch(typeof(FarmPairing), nameof(FarmPairing.getDailyEggProgress))]
		[HarmonyPostfix]
		private static void FarmPairing_getDailyEggProgress(FarmPairing __instance, ref int __result)
		{
			if (__instance.a.produceEgg || __instance.b.produceEgg)
				__result = 0;
		}

		[HarmonyPatch(typeof(FarmInterfaceController), nameof(FarmInterfaceController.changeLeftCharacterFarm))]
		[HarmonyPostfix]
		private static void FarmInterfaceController_changeLeft(Stats st, bool isPair, FarmInterfaceController __instance)
		{
			if (!Traverse.Create(__instance).Field("returnToQuickManage").GetValue<bool>())
				return;

			var tempC = QuickCharacterManager.instance.temporaryCharacters;
			if (tempC.Count < 2)
				return;

			__instance.changeRightCharacterFarm(tempC[1]);
        }

        [HarmonyPatch(typeof(Egg), nameof(Egg.getTooltip))]
        [HarmonyPostfix]
        private static void Egg_getTooltip(Egg __instance, ref string __result)
		{
			__instance.genetics.GetParents(out var mother, out var father);

			var res = Environment.NewLine;
			res += new FText($"Mother: ").Clr(Color.white);
            res += new FText(father?.CharName ?? "?").Clr((father?.CurrGender ?? Stats.Gender.None).ToColor());
            res += ",     ";
            res += new FText($"Father: ").Clr(Color.white);
            res += new FText(mother?.CharName ?? "?").Clr((mother?.CurrGender ?? Stats.Gender.None).ToColor());

            __result += res;
        }
	}
}
