using HarmonyLib;
using MoonSharp.Interpreter;
using NPC;
using PortalsOfPreggoMain.Content;
using Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Patches
{
	public static class CharacterExtensions
	{
		public static Stats GetCurrent(this CharacterManager manager)
		{
			return Traverse.Create(manager).Field("currentCharacter").GetValue<Stats>();
		}
		public static Stats GetCharacterManagerChar()
		{
			return CharacterManager.instance?.GetCurrent();
        }
	}
	public class CharacterPatches
	{
		private static bool DEBUG  = false;
		private static bool Primed = true;
		static CharacterPatches()
        {
            PreggoManager.Instance.OnDataUpdated += Preggo_OnDataUpdated;
        }

        private static void Preggo_OnDataUpdated(PreggoData data)
        {
			if (data == Last)
				InitUterusForData(data, false, false);
        }

        [HarmonyPatch(typeof(Character), nameof(Character.fuck))]
		[HarmonyPostfix]
		public static void Character_fuck(Character target, Character __instance)
		{
			var a_b_valid = __instance.stats.hasCock() && target.stats.hasPussy();
			var b_a_valid = target.stats.hasCock() && __instance.stats.hasPussy();

			//if (a_b_valid && b_a_valid) //Check if this is necessary?
			//{
			//	if (UnityEngine.Random.value >= 0.5)
			//		PreggoManager.Instance.CumIn(target.stats, __instance.stats);
			//	else
			//		PreggoManager.Instance.CumIn(__instance.stats, target.stats);
			//}
			//else
			
			PortalsOfPreggoPlugin.Instance.Log.LogInfo($"{__instance.name} is fucking {target.name}: a->b valid: {a_b_valid},  b->a valid: {b_a_valid}");
			if (a_b_valid && PreggoManager.Instance.TryGetData(target.stats, out var prdata))
			{
				Last = prdata;
                PreggoManager.Instance.CumIn(prdata, __instance.stats);
			}
			else if (b_a_valid && PreggoManager.Instance.TryGetData(__instance.stats, out prdata))
			{
				Last = prdata;
				PreggoManager.Instance.CumIn(prdata, target.stats);
			}
		}

        [HarmonyPatch(typeof(MainCharacter), nameof(MainCharacter.getCharacterToSpawn))]
        [HarmonyPostfix]
        private static void MaincharacterScreenManager_getCharacterToSpawn(MainCharacter __instance, ref Stats __result)
		{
			__result.SetGeneticsID(0);
			__result.EnsureGeneticsID(int.MinValue, int.MinValue + (int.MaxValue / 2));
        }

		/// <summary>
		/// Workaround to avoid new games throwing errors on character creation
		/// getting stats is disabled before the save is started, and re-enabled after that
		/// </summary>
        [HarmonyPatch(typeof(CharacterCreationManager), nameof(CharacterCreationManager.confirm))]
        [HarmonyPrefix]
		private static void CharacterCreationManager_confirm()
		{
			Primed = false;
		}
        [HarmonyPatch(typeof(SceneController), nameof(SceneController.changeScene))]
        [HarmonyPostfix]
        private static void SceneController_changeScene(string name)
		{
			Primed = true;
		}

        private static Stats GetInvolvedCharacter(SexAction2 act, Species specifrandom, bool otherisgiver)
		{
            Debug.LogWarning("is this working???");
            if (!Primed)
			{
                Debug.LogWarning("NOT PRIMED");
                return null;
			}
            Date date;
            Stats other;
            if ((DateManager.instance?.mainWindow?.activeSelf == true)
				&& ((date = DateManager.instance?.GetOngoingDate()) != null))
            {
                Debug.LogWarning("is in date, not valid");
                //return;
                other = date.partner;
            }
            else if (CharacterManager.instance?.mainWindow?.activeSelf == true)
            {
                Debug.LogWarning("involved: character window");
                other = Traverse.Create(CharacterManager.instance).Field("currentCharacter").GetValue<Stats>();
            }
			else
			{
                Debug.LogWarning("involved: null");
                other = null;
			}
            
			if (other != null && other.species != specifrandom)
			{
				PortalsOfPreggoPlugin.Instance.Log.LogWarning($"GetInvolcedCharacter() species mismatch? other species: {other.species}, specirandom: {specifrandom} ");
			}
			if (other == null)
            {
                Debug.LogWarning("involved: new random");
                var rpart = otherisgiver
					? act.giver
					: act.receiver;
				var rgen = rpart == BodyPart.Cock
					? Stats.Gender.Male
					: Stats.Gender.Female;
                Debug.LogWarning($"making random stats");
                other = PreggoManager.Instance.MakeRandomCharacter(
                    UnityEngine.Random.value >= 0.4f
                        ? Stats.Gender.Futa
                        : rgen,
                    specifrandom
                );
            }
			return other;
        }

		[HarmonyPatch(typeof(MainCharacter), nameof(MainCharacter.receivedAction))]
		[HarmonyPostfix]
		public static void MainCharacter_receivedAction(SexAction2 a, Species s, MainCharacter __instance)
		{
			Debug.LogWarning($"MC recieved: r: {a.receiver}, g: {a.giver}, s: {s}");
			if (!a.IsPreggoEligible())
            {
				//Debug.LogWarning("not preggo eligible");
				return;
			}

			Stats other = GetInvolvedCharacter(a, s, true);
			if (other != null)
				PreggoManager.Instance.CumIn(a.GetReceiver(other, __instance.combatForm), a.GetGiver(other, __instance.combatForm));
		}

        [HarmonyPatch(typeof(MainCharacter), nameof(MainCharacter.gaveAction))]
        [HarmonyPostfix]
        private static void MainCharacter_didaction(SexAction2 a, Species s, MainCharacter __instance)
		{
            Debug.LogWarning($"MC did: r: {a.receiver}, g: {a.giver}, s: {s}");
            if (!a.IsPreggoEligible())
				return;

            Stats other = GetInvolvedCharacter(a, s, false);
			if (other != null)
				PreggoManager.Instance.CumIn(a.GetReceiver(__instance.combatForm, other), a.GetGiver(__instance.combatForm, other));
        }


        private static float OTHER_EGG_PROGRESS = 0;
		private static float MC_EGG_PROGRESS = 0;

		[HarmonyPatch(typeof(SexSceneController.SexScene), nameof(SexSceneController.SexScene.confirmAction))]
		[HarmonyPostfix]
		private static void SexScene_confirmAction_post(SexSceneController.SexScene __instance)
		{
			__instance.otherEggToggle = true;
			__instance.mcEggToggle = true;

			var otherPre = OTHER_EGG_PROGRESS;
			var mcPre = MC_EGG_PROGRESS;

			OTHER_EGG_PROGRESS = __instance.otherEggProgress;
			MC_EGG_PROGRESS = __instance.mcEggProgress;

			if (OTHER_EGG_PROGRESS > otherPre)
			{
				PreggoManager.Instance.CumIn(__instance.other, __instance.mc);
			}

			if (MC_EGG_PROGRESS > mcPre)
			{
				PreggoManager.Instance.CumIn(__instance.mc, __instance.other);
			}
		}

		[HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.getPossibleActions))]
		[HarmonyPostfix]
		private static void chara_getActions_post(Stats s, CharacterManager __instance, ref List<CharacterManager.ActionButton> __result)
		{
			var plr = SaveController.instance.mainCharacter.combatForm;
			bool _needStam(int amt)
			{
				var e = SaveController.instance.mainCharacter.combatForm.Stamina > amt;
				if (!e)
					LogController.instance.addMessage("Too exhausted", true);
				TownManager.instance.losePlayerStamina(amt);
				return e;

			}

			List<CharacterManager.ActionButton> luaRes = new List<CharacterManager.ActionButton>();
			_ = PreggoManager.Instance.TryGetData(s, out var pregData);
			var _add = new Action<string, string, int, string, MoonSharp.Interpreter.Closure>((string name, string tooltip, int stamina, string text, MoonSharp.Interpreter.Closure onPress) =>
			{
				luaRes.Add(new CharacterManager.ActionButton(name, delegate
				{
					if (!_needStam(stamina))
						return;

					__instance.descriptionText.text = text;
					onPress.Call();
				}, tooltip));
			});


            PreggoLua.Instance["current"] = s;
			PreggoLua.Instance["pregData"] = pregData;
			PreggoLua.Instance["addAct"] = PreggoLua.Instance.MakeCallback(_add);
			PreggoLua.Instance.RunFile("charOptions");

			/*if (s.CurrGender != Stats.Gender.Female && s.CurrentEnergy > 0)
			{
				__result.Add(new CharacterManager.ActionButton("Collect cum", delegate
				{
					if (!_needStam(10))
						return;

                    s.loseEnergy(1);
					var item = CumItem.Create(s);
                    SaveController.instance.stash.addItem(item);
					LogController.instance.addMessage($"Collected {item.uses} unit(s) of cum!");
                }, $"Collect some of {s.CharName}'s cum."));
			}*/

			foreach (var li in luaRes)
				__result.Add(li);
		}

		private static bool IsMCChild(Stats chara)
		{
			if (chara == null)
				return false;
			var plrId = SaveController.instance.mainCharacter.combatForm.genetics.id;
			return chara.genetics.fatherId == plrId
				|| chara.genetics.motherId == plrId
				|| chara.specialTraits.Any(x => x == SpecialTraits.SoulFragment);
        }

		[HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.setIntroDialogue))]
		[HarmonyPostfix]
		private static void dialog_post(CharacterManager __instance)
		{

			var current = __instance.GetCurrent();
            var plr = SaveController.instance.mainCharacter.combatForm;
			var plrId = plr.genetics.id;
			var tmp = __instance.dialogueObject.GetComponentInChildren<TMPro.TMP_Text>();

			var isSub =
				current.personalityTraits.Contains(Personality.Shy)
				|| current.personalityTraits.Contains(Personality.Submissive)
				|| current.personalityTraits.Contains(Personality.Coward)
				|| (current.size <= plr.size && current.RelationToMc > 50);

			if (current.genetics.fatherId == plrId || current.genetics.motherId == plrId)
				tmp.text = tmp.text.Replace(plr.CharName, plr.CurrGender == Stats.Gender.Male
					? isSub
						? "Daddy"
						: "Dad"
					: isSub
						? "Mommy"
						: "Mom");
			else if (current.genetics.id < -(int.MaxValue / 2))
				tmp.text = tmp.text.Replace(plr.CharName, plr.CurrGender == Stats.Gender.Male
					? "Master"
					: "Mistress");
        }

		private static PreggoData Last;
		private static Uterus CurrentUterus;
		private static bool IsPlayerChar;
		private static Uterus EnsureUterus(bool showIfHidden)
		{
			if (!CurrentUterus)
            {
				CurrentUterus = PrefabManager.Instance.GetUterus();
			}

            if (showIfHidden && !CurrentUterus.gameObject.activeSelf)
            {
                CurrentUterus.gameObject.SetActive(true);
            }
            return CurrentUterus;
		}

		private static TMPro.TMP_Text GetUterusText(Uterus u)
		{
			return u.TextRect.GetComponent<TMPro.TMP_Text>();
		}

		public static void ForceToggleUterus()
		{
			var ut = EnsureUterus(false);
			var active = !ut.gameObject.activeSelf;
			if (active)
			{
				InitUterusForData(Last, false, true);
			}
			ut.gameObject.SetActive(active);
		}

		private static void InitUterusForData(PreggoData mother, bool npc, bool showIfHidden)
		{
			var ut = EnsureUterus(showIfHidden);
			if (mother == null)
			{
				ut.gameObject.SetActive(false);
				return;
			}
			else if (mother.Type == PreggoType.Temp)
			{
                PortalsOfPreggoPlugin.Instance.Log.LogWarning($"Init uterus for temp?");
				//PreggoPlugin.Instance.Log.LogWarning(new System.Diagnostics.StackTrace().ToString());
            }
			Last = mother;
			float size = 2.1f;
            ut.gameObject.SetActive(true);
            ut.Toggle.onValueChanged.RemoveAllListeners();
			ut.Toggle.onValueChanged.AddListener(b =>
			{
				mother.Silenced = !b;
			});

			if (mother.UniqueCharacter.IsNPC() && npc)
			{
				ut.MoveRoot.anchoredPosition = new Vector2(-350, 0);
				size = 1.95f;
				IsPlayerChar = false;
			}
			else
			{
				ut.MoveRoot.anchoredPosition = new Vector2(-450, 0);
				IsPlayerChar = true;
			}
			var seen = PreggoManager.Instance.SeenData.Ensure(mother);
			ut.Init(new Uterus.UterusData(
				!mother.Silenced,
				mother.CumInUterus.GetSum(),
				seen.Ovulation,
				seen.Cum,
				size,
				mother.Ovums.Select(x => new OvumVisData() { 
					State = x.State,
					GrowthPercent = (float)x.MinutesPassed / (float)PreggoManager.Instance.LengthPregnancy
				}).ToArray()
			));
			seen.Ovulation = mother.Ovums.Any();
			seen.Cum = mother.CumInUterus.GetSum();

			var uText = GetUterusText(ut);
			uText.text = String.Empty;

			var phase = PreggoManager.Instance.GetPhase(mother);
			var phaseStr = new FText("Phase: ") + new FText(phase.ToString()).Clr(phase.ToHTMLColor());

			if (phase == PreggoPhase.Pregnant)
				phaseStr = phaseStr.Size(110);

			var uterusSperm = Mathf.FloorToInt(mother.CumInUterus.GetSum());
			var vagSperm = Mathf.FloorToInt(mother.CumInVagina.GetSum());
			uText.text += "<width=150%>";
			uText.text += phaseStr + "    ";
			uText.text += new FText($"Total sperm in vagina: {vagSperm + uterusSperm}ml ({uterusSperm}ml in uterus)")
				.Size(80)
				// .Tag("align", "\"right\"")
				//.Tag("cspace", "-0.07em")
				;
			var _nl = Environment.NewLine;
			var cumSize = 60;

			if (DEBUG)
			{
                uText.text
                    += _nl
                    + new FText($"debug: type:{mother.Type}, id:{mother.CharacterID}");
            }

            uText.text
					+= _nl
					+ new FText("In uterus:").Size(cumSize);

			if (mother.BirthControl)
			{
				uText.text += new FText(" (On birth control)").Size(cumSize);
			}
			if (mother.Ovums.Any())
			{
				uText.text += _nl
					+ new FText(string.Join(_nl, mother.Ovums.Select(x => {
						var res = new FText(" - ");

						if (x.State == OvumState.Implanted)
							res += $"Implanted ovum: ({Mathf.FloorToInt(x.GetGrowthPercent())}% grown) Father: {PreggoManager.Instance.GetFatherNameColored(x, mother)}";
						else if (x.State == OvumState.Fertilized)
							res += $"Fertilized ovum: Father: {PreggoManager.Instance.GetFatherNameColored(x, mother)}";
						else
							res += "Ovum ";

						return res.Size(cumSize);
					}))).Size(cumSize);
			}

			uText.text += _nl + new FText(string.Join(_nl, mother.CumInUterus.Select(x
				=> new FText(" - Sperm from ").Size(cumSize) + new FText(x.Key.CharName).Clr(x.Key.CurrGender.ToColor())
				.Size(cumSize) + new FText($" ({x.Key.species}): {Mathf.FloorToInt(x.Value)}ml").Size(cumSize)
			)));

			// workaround to avoid uterus window constantly popping up when moving in portals
			ut.gameObject.SetActive(SaveController.instance.gamePhase == SaveController.GamePhase.Town);
        }

		[HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.closeWindow))]
		[HarmonyPostfix]
		private static void CharacterManager_closeWindow(bool canReturnToQuickMenu = false)
		{
			InitUterusForData(null, false, false);
		}


		[HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.updateCharacter))]
		[HarmonyPostfix]
		private static void updateCharacter(CharacterManager __instance)
		{
			var cur = Traverse.Create((object)__instance).Field("currentCharacter").GetValue() as Stats;

			if (!PreggoManager.Instance.TryGetData(cur, out var mother))
			{
				InitUterusForData(null, false, false);
				return;
			}

			InitUterusForData(mother, false, false);
		}

		[HarmonyPatch(typeof(LocationManager), nameof(LocationManager.updateButtons))]
		[HarmonyPostfix]
		private static void getNPCInfo()
		{
			if (!TownInterfaceController.instance.locationCharacter?.activeSelf == true)
			{
				InitUterusForData(null, true, false);
				return;
			}

			var npc = SaveController.instance.npcs
				.FirstOrDefault(x => x.getCurrentLocation() == SaveController.instance.currLocation);

			if (npc == null || !PreggoManager.Instance.TryGetData(npc.stats, out var npcData))
			{
				InitUterusForData(null, true, false);
				return;
			}

			//Debug.LogWarning("show npc uterus");
			InitUterusForData(npcData, true, true);
		}

		[HarmonyPatch(typeof(TownInterfaceController), nameof(TownInterfaceController.hideAll))]
		[HarmonyPostfix]
		private static void LocationManager_hide(bool firstCall = false)
		{
			if (!IsPlayerChar)
				InitUterusForData(null, false, false);
			
		}

        [HarmonyPatch(typeof(QuickCharacterManager), nameof(QuickCharacterManager.updateTemporaryCharacterTexts))]
        [HarmonyPostfix]
        public static void QuickCharacterManager_updateTemporaryCharacterTexts(QuickCharacterManager __instance)
		{
			if (__instance.temporaryCharacters.Count < 2)
				return;

			if (__instance.temporaryCharacters[0].CurrGender == __instance.temporaryCharacters[1].CurrGender && __instance.temporaryCharacters[0].CurrGender != Stats.Gender.Futa)
                return;

            var farmPairing = new FarmPairing(__instance.temporaryCharacters[0], __instance.temporaryCharacters[1]);
			__instance.breedButtonText.text = "Farm\n" + farmPairing.getDailyEggProgress(firstIsFemale: true) + "% / " + farmPairing.getDailyEggProgress(firstIsFemale: false) + "%";
            __instance.breedButtonText.color = new Color(1f, 0.85f, 0f);
        }



        private static Stats _LastGiver;
		private static Stats _LastReciever;

		[HarmonyPatch(typeof(Stats), nameof(Stats.gaveAction))]
		[HarmonyPostfix]
		private static void gaveAction(Stats __instance, SexAction2 a, bool training = false, bool withMaincharacter = false, bool inBattle = false)
        {
			_LastGiver = __instance;
        }

		[HarmonyPatch(typeof(Stats), nameof(Stats.receivedAction))]
		[HarmonyPostfix]
		private static void Stats_receivedAction(Stats __instance, SexAction2 a, bool training = false, bool withMaincharacter = false, bool inBattle = false)
		{
			if (!a.IsPreggoEligible())
				return;

			if (training)
            {
				var (giver, receiver) = TownInterfaceController.instance.GetTrainingCharacters();
				if (giver == null || receiver == null)
					return;

				var reversed = a.giver == BodyPart.Pussy;
				if (reversed)
					PreggoManager.Instance.CumIn(giver, receiver);
				else
					PreggoManager.Instance.CumIn(receiver, giver);

			}
		}
	}
}
