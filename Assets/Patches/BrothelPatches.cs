using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Patches
{
    public class BrothelPatches
    {

		[HarmonyPatch(typeof(Brothel), nameof(Brothel.resolveBrothelDay))]
		[HarmonyPostfix]
		private static void resolveBrothelDay(Brothel __instance)
		{
			PopulationGroup prevDay = __instance.prevDay;
			int futaCount	= prevDay.getCount(Stats.Gender.Futa);
			int maleCount	= prevDay.getCount(Stats.Gender.Male);
			int femCount	= prevDay.getCount(Stats.Gender.Female);

			float futaPercent = futaCount / (float)(futaCount + maleCount + femCount);
			float malePercent = maleCount / (float)(futaCount + maleCount + femCount);

			var validRooms = __instance.availableRooms.Where(x => x.type != Brothel.RoomType.Show && (x.menAllowed || x.futasAllowed));

			foreach (var room in validRooms)
			{
				int futasInRoom = Mathf.FloorToInt(room.guestsVisited * futaPercent);
				int maleInRoom  = Mathf.CeilToInt(room.guestsVisited * malePercent);

				for (int i = 0; i < futasInRoom + maleInRoom; i++)
                {
					var harlot = room.brothelCharacters.GetRandom();

					if (PreggoManager.Instance.TryGetData(harlot.stats, out var data))
                    {
						var guest = PreggoManager.Instance.MakeRandomCharacter(i > futasInRoom ? Stats.Gender.Male : Stats.Gender.Futa);
						PreggoManager.Instance.CumIn(data, guest, quiet: true);
					}
				}
			}
		}

		private const BrothelShow.ShowAction LAST = BrothelShow.ShowAction.Dismiss;
		private const BrothelShow.ShowAction _Ride = (BrothelShow.ShowAction)(LAST + 1);

		[HarmonyPatch(typeof(BrothelShow), nameof(BrothelShow.getAvailableActions))]
		[HarmonyPostfix]
		private static void getAvailableActions_postfix(
		  BrothelShow __instance,
		  ref List<BrothelShow.ShowActionInfo> __result)
		{
			BrothelShow.Guest selectedViewer = __instance.getSelectedViewer();
			if (selectedViewer == null || selectedViewer.gender == Stats.Gender.Female)
				return;
			__result.Add(new BrothelShow.ShowActionInfo(_Ride, "Ride", 10, 5, 15, "attempt to finish quest by riding them.\nChance equal to satifaction.\nOn success, get a big tip and a creampie."));
		}

		[HarmonyPatch(typeof(BrothelShowInterfaceController), nameof(BrothelShowInterfaceController.clickedAction))]
		[HarmonyPrefix]
		private static bool clickedAction_prefix(BrothelShowInterfaceController __instance, BrothelShow.ShowActionInfo a, ref string __state)
		{
			__state = null;
			if (a == null || __instance.currentShow.minutesLeft < 1 || (TownManager.instance.playerTooExhaustedFor(a.exhaustion) || __instance.summaryWindow.activeSelf))
				return true;

			switch (a.action)
			{
				default:
					return true;
				case _Ride:
					_RideAction(__instance, out __state);
					break;

			}
			return true;
		}

		private static void _RideAction(BrothelShowInterfaceController __instance, out string __state)
		{
			BrothelShow.Guest selectedViewer = __instance.currentShow.getSelectedViewer();
			if (selectedViewer.demand == BrothelShow.Demand.Finish || UnityEngine.Random.Range(0, 100) < selectedViewer.satisfaction)
			{
				__state = new FText($"You hop on {new FText(selectedViewer.name).Clr("orange")}'s penis, and she quickly finishes inside you.");
				__instance.addMessage(selectedViewer.name + new FText(" creampies you, tips <color=yellow>" + (object)selectedViewer.getFinishingTip() + "</color> and leaves").Clr(UnityEngine.Color.green));
				Stats stats = CharacterControllerScript.instance.generateStats(selectedViewer.species);
				stats.CurrGender = selectedViewer.gender;
				PreggoManager.Instance.CumIn(SaveController.instance.mainCharacter.combatForm, stats);

				__instance.currentShow.tips += selectedViewer.getFinishingTip();
				++__instance.currentShow.satisfiedViewers;
			}
			else
			{
				__state = new FText($"You hop on {new FText(selectedViewer.name).Clr("orange")}'s penis, but, embarassingly, you aren't able to make her cum.");
				__instance.addMessage("failing to finish " + selectedViewer.name + "(" + (object)selectedViewer.satisfaction + "% success), she leaves the show");
				++__instance.currentShow.LeftViewers;
			}
			for (int index = 0; index < __instance.currentShow.viewers.Count; ++index)
			{
				if (__instance.currentShow.viewers[index].id == __instance.currentShow.selectedViewerId)
				{
					__instance.currentShow.viewers.RemoveAt(index);
					break;
				}
			}
			__instance.currentShow.selectedViewerId = -1;
		}

		[HarmonyPatch(typeof(BrothelShowInterfaceController), nameof(BrothelShowInterfaceController.clickedAction))]
		[HarmonyPostfix]
		private static void clickedAction_postfix(BrothelShowInterfaceController __instance, BrothelShow.ShowActionInfo a, ref string __state)
		{
			if (__state != null)
				__instance.actionDescription.text = __state;
		}
	}
}
