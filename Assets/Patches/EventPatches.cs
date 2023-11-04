using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patches
{
    public class EventPatches
    {
		[HarmonyPatch(typeof(SaveController), nameof(SaveController.progressEvent), typeof(TownEventController.TownEvent), typeof(int))]
		[HarmonyPostfix]
		private static void eventPostTown(SaveController __instance, TownEventController.TownEvent townEvent, int amt = 1)
		{
			var prog = __instance.townEventProgress.First(x => x.townEvent == townEvent);

			var man = NPCManager.instance;
			var syl = man.getNpc(UniqueCharacter.Sylvie).stats;
			var cas = man.getNpc(UniqueCharacter.Castalia).stats;
			var flo = man.getNpc(UniqueCharacter.Flora).stats;
			var lum = man.getNpc(UniqueCharacter.Lumira).stats;
			var mya = man.getNpc(UniqueCharacter.Mya).stats;
			var ail = man.getNpc(UniqueCharacter.Aila).stats;
			var plr = SaveController.instance.mainCharacter.combatForm;


			//if (prog.progress <= 1)
			//	return;
			//switch (townEvent)
			//{
			//	case TownEventController.TownEvent.SylviePortal:
			//		PreggersSaveController.instance.addStatsLoad(cas, syl);
			//		break;
			//}

			PreggoLua.Instance["event"] = (int)townEvent;
			PreggoLua.Instance["eventName"] = townEvent.ToString();
			PreggoLua.Instance["prog"] = prog.progress;
			PreggoLua.Instance["saveController"] = __instance;
			PreggoLua.Instance.RunFile("townEvent");

		}

		[HarmonyPatch(typeof(SaveController), nameof(SaveController.progressEvent), typeof(OWEventController.OWEvent), typeof(int))]
		[HarmonyPostfix]
		private static void eventPostOW(SaveController __instance, OWEventController.OWEvent owEvent, int amt = 1)
		{
			var prog = __instance.owEventProgress.First(x => x.owEvent == owEvent);
			PreggoLua.Instance["event"] = (int)owEvent;
			PreggoLua.Instance["eventName"] = owEvent.ToString();
			PreggoLua.Instance["prog"] = prog.progress;
			PreggoLua.Instance["saveController"] = __instance;
			PreggoLua.Instance.RunFile("event_ow");
		}
	}
}
