using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patches
{
    public static class TownExtensions
    {
        public static (Stats giver, Stats receiver) GetTrainingCharacters(this TownInterfaceController t)
        {
            var tr = Traverse.Create(t);
            return (
                tr.Field("sexGiver").GetValue<Stats>(),
                tr.Field("sexReceiver").GetValue<Stats>()
            );
        }
    }

    public class TownPatches
    {
        [HarmonyPatch(typeof(TownManager), nameof(TownManager.addCharacter))]
        [HarmonyPostfix]
        public static void TownManager_addCharacter(Stats s)
        {
            if (PreggoManager.Instance.TryGetData(s, out var data))
            {
                PortalsOfPreggoPlugin.Instance.Log.LogDebug($"Added PreggoData for '{s.CharName}'");
            }
        }


        [HarmonyPatch(typeof(TownManager), nameof(TownManager.passTime), typeof(int))]
        [HarmonyPrefix]
        private static void TownManager_passTime(int minutes)
        {
            PreggoManager.Instance.CacheCycleLength();
            PreggoManager.Instance.CacheCumSettings();

            PreggoLua.Instance["minutes"] = minutes;
            PreggoLua.Instance.RunFile("event_timePassed");

            foreach (var data in PreggoManager.Instance.AllData)
            {
                PreggoManager.Instance.PassTime(data, minutes);
            }
        }


        [HarmonyPatch(typeof(TownManager), nameof(TownManager.passTime), typeof(GameTime), typeof(bool))]
        [HarmonyPrefix]
        private static void TownManager_passTime_GT(GameTime t, bool rest = false)
        {
            TownManager_passTime(t.getMinutes());
        }
    }
}
