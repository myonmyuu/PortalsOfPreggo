using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Date;

namespace Patches
{
    public static class DateExtensions
    {
        public static Date GetOngoingDate(this DateManager man)
        {
            return Traverse.Create(man).Field("date").GetValue<Date>();
        }
    }

    public class DatePatches
    {
        private static readonly List<string> CumCards = new List<string>()
        {
            "breed",
            "fuckhard",
            "creampie"
        };

        [HarmonyPatch(typeof(Date), nameof(Date.didSexAction), typeof(SexAction2), typeof(bool))]
        [HarmonyPostfix]
        public static void Date_didSex(SexAction2 a, bool mcGiver, Date __instance)
        {
            var v = a.receiver == BodyPart.Pussy && a.giver == BodyPart.Cock;
            var invert = a.receiver == BodyPart.Cock && a.giver == BodyPart.Pussy;

            var reciever = mcGiver ^ invert
                ? __instance.partner
                : __instance.Player;
            var giver = mcGiver ^ invert
                ? __instance.Player
                : __instance.partner;

            var preggovalid = v || invert;
            if (!preggovalid)
                return;

            var checkVirg = PreggoSettings.Instance.DateVirginity.Value;
            if (checkVirg && reciever == __instance.partner && __instance.partner.hasTrait(SpecialTraits.Virgin))
            {
                PreggoManager.Instance.BroadcastMessage($"{__instance.PartnerName} has lost her virginity to you!");
                __instance.partner.removeTrait(SpecialTraits.Virgin);
            }
            else if (checkVirg && reciever != __instance.partner && __instance.Player.hasTrait(SpecialTraits.Virgin))
            {
                PreggoManager.Instance.BroadcastMessage($"You have given your virginity to {__instance.PartnerName}!");
                __instance.Player.removeTrait(SpecialTraits.Virgin);
            }

            // TODO: check if this was actually uneccessary or not
            //if (reciever == __instance.Player)
                //PortalsOfPreggoMain.Content.PreggoManager.Instance.CumIn(reciever, giver);
        }

        [HarmonyPatch(typeof(DateManager), nameof(DateManager.clickedOnCard))]
        [HarmonyPostfix]
        public static void DateManager_clickedOnCard(DateCard card, bool isPlayerCard, DateManager __instance)
        {
            var date = __instance.GetOngoingDate();

            var a = card.sexAction.receiver == BodyPart.Pussy && card.sexAction.giver == BodyPart.Cock;
            var invert = card.sexAction.receiver == BodyPart.Cock && card.sexAction.giver == BodyPart.Pussy;

            //      T   F
            //  T   x   o
            //  F   o   x
            var reciever = isPlayerCard ^ invert
                ? date.partner
                : date.Player;
            var giver = isPlayerCard ^ invert
                ? date.Player
                : date.partner;

            var preggovalid = a || invert;

            PreggoLua.Instance["isplayer"] = isPlayerCard;
            PreggoLua.Instance["datemanager"] = __instance;
            PreggoLua.Instance["preggovalid"] = preggovalid;
            PreggoLua.Instance["dateCard"] = card;
            PreggoLua.Instance["date"] = date;
            PreggoLua.Instance["giver"] = giver;
            PreggoLua.Instance["reciever"] = reciever;
            PreggoLua.Instance["giverPart"] = card.sexAction.giver.ToString();
            PreggoLua.Instance["recieverPart"] = card.sexAction.receiver.ToString();
            PreggoLua.Instance["keywords"] = card.keyWords.Select(x => x.ToString()).ToList();


            PreggoLua.Instance.RunFile("date_clickedcard");
        }

        [HarmonyPatch(typeof(DateManager), nameof(DateManager.clickedOnAction))]
        [HarmonyPrefix]
        public static void DateManager_clickedOnAction(int column, int index, DateManager __instance)
        {
            var date = __instance.GetOngoingDate();
            var dateActivity = Traverse.Create(__instance).Method("getActionList", column).GetValue<List<Date.DateActivity>>()[index];

            PreggoLua.Instance["dateactivity"] = dateActivity;
            PreggoLua.Instance["date"] = date;
            PreggoLua.Instance.RunFile("date_clickedaction");
        }

        [HarmonyPatch(typeof(DateManager), nameof(DateManager.clickedOnEventAction))]
        [HarmonyPrefix]
        public static void DateManager_clickedOnEventAction(int index, DateManager __instance)
        {
            var date = __instance.GetOngoingDate();
            if (__instance.inputMode != DateManager.InputMode.Basic)
            {
                PortalsOfPreggoPlugin.Instance.Log.LogWarning("input not basic");
                return;
            }

            Date.DateActivity dateActivity = date.eventChoices[index];
            var canplay = date.currentLewdity >= dateActivity.lewdityReq;

            PreggoLua.Instance["canplay"] = canplay;
            PreggoLua.Instance["dateactivity"] = dateActivity;
            PreggoLua.Instance.RunFile("date_clickedeventaction");
        }

        private static bool _playerIsGiver(DateCard c)
        {
            PreggoLua.Instance["dateCard"] = c;
            var fRet = PreggoLua.Instance.RunFile("date_playerIsGiver");
            if (fRet?.IsNil() == false)
            {
                return fRet.Type == MoonSharp.Interpreter.DataType.Boolean && fRet.Boolean;
            }
            return true;
        }

        [HarmonyPatch(typeof(Date), nameof(Date.getActualLewdReq))]
        [HarmonyPostfix]
        private static void checkVirginity(DateCard c, Date __instance, ref int __result)
        {
            if (c.sexAction.giver == BodyPart.Cock && c.sexAction.receiver == BodyPart.Pussy && (_playerIsGiver(c) && __instance.partner.hasTrait(SpecialTraits.Virgin)) && !__instance.partner.hasTrait(Personality.Nymphomanic))
                ++__result;
            else if (c.sexAction.receiver == BodyPart.Cock && c.sexAction.giver == BodyPart.Pussy && (!_playerIsGiver(c) && __instance.partner.hasTrait(SpecialTraits.Virgin)) && !__instance.partner.hasTrait(Personality.Nymphomanic))
                ++__result;
            __result = Mathf.Min(10, __result);
        }

        [HarmonyPatch(typeof(Date), nameof(Date.spentTime))]
        [HarmonyPostfix]
        private static void Date_spentTime(int v, bool affect)
        {
            PreggoManager.Instance.PassTimeForAll(v);
        }
    }
}
