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

            PreggoManager.Instance.PassTimeForAll(minutes);
        }


        [HarmonyPatch(typeof(TownManager), nameof(TownManager.passTime), typeof(GameTime), typeof(bool))]
        [HarmonyPrefix]
        private static void TownManager_passTime_GT(GameTime t, bool rest = false)
        {
            TownManager_passTime(t.getMinutes());
        }

        [HarmonyPatch(typeof(LocationManager), nameof(LocationManager.updateButtons))]
        [HarmonyPostfix]
        private static void LocationManager_updateButtons(LocationManager __instance)
        {
            switch (SaveController.instance.currLocation)
            {
                case Location.Name.Market:
                    int _getItemCost(Item it)
                    {
                        var cost = it.getRealValue() * 7;
                        if (SaveController.instance.Ascension >= 7)
                            cost *= 2;
                        if (SaveController.instance.hasModifier(NewGamePlusInfo.NewGameModifier.Expensive))
                            cost *= 2;
                        return cost;
                    }

                    var marketEntries = new Item[]
                    {
                        ItemPatches.Items.PregAccel_1,
                        ItemPatches.Items.PregAccel_2,
                        ItemPatches.Items.Ovu_1,
                        ItemPatches.Items.Aborter,
                        ItemPatches.Items.BirthControl,
                    }.Select(x => new TownInterfaceController.GenericSelectionEntry(x.getNameWithRarity(),
                    delegate
                    {
                        var cost = _getItemCost(x);
                        if (TownManager.instance.hasRessource(Ressource.Money, cost))
                        {
                            SaveController.instance.stash.addItem(Item.Copy(x));
                            TownManager.instance.loseMoney(cost);
                            LogController.instance.addMessage("Bought " + x.getNameWithRarity());
                        }
                        else
                        {
                            LogController.instance.addMessage("not enough gold.");
                        }
                    }, $"({new FText(_getItemCost(x).ToString()).Clr(UnityEngine.Color.yellow)}g) {x.getTooltipInfo()}", x.icon, false));
                    var actbtn = new LocationManager.ActionButton(new FText("Pregnancy Items").Clr(UnityEngine.Color.magenta),
                        delegate
                        {
                            TownInterfaceController.instance.startGenericSelection("Buy", marketEntries.ToList());
                            return true;
                        }, "Buy from a selection of pregnancy products for a very high cost");

                    var btn = UnityEngine.Object.Instantiate(__instance.buttonPrefab, __instance.actionRoster.transform);
                    btn.transform.localScale = Vector3.one;
                    ToolTipManager.instance.addTooltipTo(btn, actbtn.tooltip);
                    btn.GetComponentInChildren<TMPro.TMP_Text>().text = actbtn.name;
                    btn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => actbtn.action());
                    break;
            }
        }
    }
}
