using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Patches.ItemPatches;

namespace Patches
{
    [Serializable]
    public class CumItem : Item
    {
        public Stats giver;

        public static CumItem Create(Stats giver)
        {
            var amt     = PreggoManager.Instance.GetCumAmount(giver);
            var uses    = Mathf.CeilToInt(amt / 10f);
            return new CumItem
            {
                giver = giver,
                icon = PrefabManager.Instance.Sprites["11"],
                name = $"{giver.CharName}'s Cum",
                useableInTown = true,
                useableInbattle = false,
                Rarity = Rarity.common,
                value = 1,
                uses = uses,
                CurrentUses = uses,
                type = Item.Type.consumable,
                passiveEffects = new Passives("", Passives.Type.Always),
                description = $"Cum of {giver.CharName} ({giver.species}).",
            };
        }
    }

    public class ItemPatches
    {
        public static class ItemNames
        {
            public const string PregAccel_1     = "Weak Pregnancy Accelerator";
            public const string PregAccel_2     = "Strong Pregnancy Accelerator";
            public const string PregAccel_3     = "Pregnancy Finalizer";

            public const string Aborter         = "Egg Transmitter";

            public const string Ovu_1           = "Ovulatory Stimulant";
            public const string Ovu_2           = "Ovulation Potion";

            public const string Fert            = "Fertility Cure";
            public const string BirthControl    = "Birth Control Potion";
        }

        public class PreggoItems
        {
            public Item PregAccel_1;
            public Item PregAccel_2;
            public Item PregAccel_3;

            public Item Aborter;

            public Item Ovu_1;
            public Item Ovu_2;

            public Item Fert;
            public Item BirthControl;
        }

        public static PreggoItems Items;
        public static List<Item> ItemList;

        public static void AddRecipes()
        {
            // just for reference
            SaveController.instance.addRecipe(
                new CraftingRecipe(
                    Item.Type.consumable,
                    "ManaCrystal",
                    Rarity.special,
                    20,
                    new CraftingMaterial[2]
                    {
                        new CraftingMaterial(MaterialType.CondensedCrystal, 1),
                        new CraftingMaterial(MaterialType.S_Light, 5)
                    },
                    5,
                    0,
                    0,
                    0
                )
            );
            SaveController.instance.addCraftingMaterial(MaterialType.CondensedCrystal, 1);
        }

        [HarmonyPatch(typeof(ItemController), "Awake")]
        [HarmonyPostfix]
        private static void ItemController_Awake()
        {
            CreateItems();
            InjectItems();
        }

        private static void CreateItems()
        {
            Items = new PreggoItems();
            ItemList = new List<Item>();

            var sprites = PrefabManager.Instance.Sprites;
            Items.PregAccel_1 = new Item()
            {
                icon = sprites["1"],
                name = ItemNames.PregAccel_1,
                useableInTown = true,
                useableInbattle = false,
                Rarity = Rarity.common,
                value = 15,
                uses = 2,
                CurrentUses = 2,
                type = Item.Type.consumable,
                passiveEffects = new Passives("", Passives.Type.Always),
                description = "Accelerates pregnancies by 30%."
            };
            ItemList.Add(Items.PregAccel_1);

            Items.PregAccel_2 = new Item()
            {
                icon = sprites["8"],
                name = ItemNames.PregAccel_2,
                useableInTown = true,
                useableInbattle = false,
                Rarity = Rarity.rare,
                value = 30,
                uses = 2,
                CurrentUses = 2,
                type = Item.Type.consumable,
                passiveEffects = new Passives("", Passives.Type.Always),
                description = "Accelerates pregnancies by 70%."
            };
            ItemList.Add(Items.PregAccel_2);

            Items.PregAccel_3 = new Item()
            {
                icon = sprites["9"],
                name = ItemNames.PregAccel_3,
                useableInTown = true,
                useableInbattle = false,
                Rarity = Rarity.legendary,
                value = 120,
                uses = 2,
                CurrentUses = 2,
                type = Item.Type.consumable,
                passiveEffects = new Passives("", Passives.Type.Always),
                description = "Instantly completes pregnancies."
            };
            ItemList.Add(Items.PregAccel_3);

            Items.Aborter = new Item()
            {
                icon = sprites["6"],
                name = ItemNames.Aborter,
                useableInTown = true,
                useableInbattle = false,
                Rarity = Rarity.rare,
                value = 50,
                uses = 2,
                CurrentUses = 2,
                type = Item.Type.consumable,
                passiveEffects = new Passives("", Passives.Type.Always),
                description = "Moves fertilized/implanted eggs to a nest in a different world."
            };
            ItemList.Add(Items.Aborter);

            Items.Ovu_1 = new Item()
            {
                icon = sprites["2"],
                name = ItemNames.Ovu_1,
                useableInTown = true,
                useableInbattle = false,
                Rarity = Rarity.common,
                value = 25,
                uses = 3,
                CurrentUses = 3,
                type = Item.Type.consumable,
                passiveEffects = new Passives("", Passives.Type.Always),
                description = "Progresses the menstrual cycle (but not past ovulation)."
            };
            ItemList.Add(Items.Ovu_1);

            Items.Ovu_2 = new Item()
            {
                icon = sprites["5"],
                name = ItemNames.Ovu_2,
                useableInTown = true,
                useableInbattle = false,
                Rarity = Rarity.legendary,
                value = 150,
                uses = 1,
                CurrentUses = 1,
                type = Item.Type.consumable,
                passiveEffects = new Passives("", Passives.Type.Always),
                description = "Immediately causes a uterus to ovulate. Phase must be Luteal to work."
            };
            ItemList.Add(Items.Ovu_2);

            Items.Fert = new Item()
            {
                icon = sprites["10"],
                name = ItemNames.Fert,
                useableInTown = true,
                useableInbattle = false,
                Rarity = Rarity.legendary,
                value = 250,
                uses = 1,
                CurrentUses = 1,
                type = Item.Type.consumable,
                passiveEffects = new Passives("", Passives.Type.Always),
                description = "Permanently cures infertility."
            };
            ItemList.Add(Items.Fert);

            Items.BirthControl = new Item()
            {
                icon = sprites["3"],
                name = ItemNames.BirthControl,
                useableInTown = true,
                useableInbattle = false,
                Rarity = Rarity.rare,
                value = 5,
                uses = 20,
                CurrentUses = 20,
                type = Item.Type.consumable,
                passiveEffects = new Passives("", Passives.Type.Always),
                description = "Enables/Disables ovulation."
            };
            ItemList.Add(Items.BirthControl);
        }

        private static ItemController _last_injected = null;
        private static void InjectItems()
        {
            if (!PortalsOfPreggoPlugin.Instance.Settings.Items.Value)
            {
                PortalsOfPreggoPlugin.Instance.Log.LogInfo("Item injection skipped, config is disabled.");
                return;
            }
            if (_last_injected == ItemController.instance)
            {
                PortalsOfPreggoPlugin.Instance.Log.LogInfo("Item injection skipped, already done.");
                return;
            }
            PortalsOfPreggoPlugin.Instance.Log.LogInfo("injecting items...");

            var ic = ItemController.instance;
            var pc = ic.commonItems.Length;
            var pr = ic.rareItems.Length;
            var pl = ic.legendaryItems.Length;
            ic.commonItems = ic.commonItems
                .Append(Items.PregAccel_1)
                .Append(Items.Ovu_1)
                .ToArray();
            ic.rareItems = ic.rareItems
                .Append(Items.PregAccel_2)
                .Append(Items.Aborter)
                .Append(Items.BirthControl)
                .ToArray();
            ic.legendaryItems = ic.legendaryItems
                .Append(Items.PregAccel_3)
                .Append(Items.Ovu_2)
                .Append(Items.Fert)
                .ToArray();

            PortalsOfPreggoPlugin.Instance.Log.LogInfo("items injected!");
            PortalsOfPreggoPlugin.Instance.Log.LogInfo($"common: {pc}->{ic.commonItems.Length}");
            PortalsOfPreggoPlugin.Instance.Log.LogInfo($"rare: {pr}->{ic.rareItems.Length}");
            PortalsOfPreggoPlugin.Instance.Log.LogInfo($"legendary: {pl}->{ic.legendaryItems.Length}");
            _last_injected = ic;
        }

        [HarmonyPatch(typeof(ItemController), nameof(ItemController.useConsumable))]
        [HarmonyPrefix]
        private static bool ItemController_useConsumable(Stats s, Item item, ref bool __result, ref ItemController __instance)
        {
            var uses = item.CurrentUses;
            if (uses <= 0)
                return true;

            bool preggoeli = PreggoManager.Instance.TryGetData(s, out var data);
            PreggoPhase phase = PreggoPhase.Invalid;

            if (preggoeli)
                phase = PreggoManager.Instance.GetPhase(data);

            bool _ispregnant()
            {
                return phase == PreggoPhase.Pregnant;
            }

            var preg_len = PreggoManager.Instance.LengthPregnancy;
            void _accel_preg(float percent)
            {
                foreach (var o in data.Ovums)
                {
                    if (o.State != OvumState.Implanted)
                        continue;

                    o.MinutesPassed = (int)Mathf.Min(preg_len, o.MinutesPassed + ((float)preg_len * percent));
                }
            }

            void _message(string message) => LogController.instance.addMessage(message);
            void _continue() => PreggoManager.Instance.PassTime(data, 1);
            void _mes_cont(string mes)
            {
                _message(mes);
                _continue();
            }

            bool _cancel(string reason)
            {
                _message(new FText($"item not usable on {s.CharName}: {reason}.").Clr("red"));
                return false;
            }

            bool? valid = null;

            switch(item)
            {
                case CumItem cum:
                    if (!preggoeli)
                        _cancel("Target has no womb.");
                    
                    data.CumInUterus.ChangeCum(cum.giver, 10);
                    _mes_cont("Inserted sperm.");
                    valid = true;
                    break;
            }

            switch (item.name)
            {
                case ItemNames.PregAccel_1:
                    if (!_ispregnant())
                        return _cancel("Target is not pregnant");
                    _accel_preg(.3f);
                    _mes_cont(new FText("Pregnancy accelerated").Clr("yellow"));
                    break;
                case ItemNames.PregAccel_2:
                    if (!_ispregnant())
                        return _cancel("Target is not pregnant");
                    _accel_preg(.7f);
                    _mes_cont(new FText("Pregnancy accelerated").Clr("yellow"));
                    break;
                case ItemNames.PregAccel_3:
                    if (!_ispregnant())
                        return _cancel("Target is not pregnant");
                    _accel_preg(1);
                    _mes_cont(new FText("Pregnancy completed").Clr("yellow"));
                    break;
                case ItemNames.Aborter:
                    if (!_ispregnant())
                        return _cancel("Target is not pregnant");
                    data.Ovums.Clear();
                    _mes_cont(new FText("Eggs moved to a distant, safe nest").Clr("yellow"));
                    break;
                case ItemNames.Ovu_1:
                    if (!preggoeli)
                        return _cancel("Target does not have a menstrual cycle");
                    if (phase == PreggoPhase.Luteal
                        || phase == PreggoPhase.Fertilized
                        || phase == PreggoPhase.Pregnant
                        || phase == PreggoPhase.Ovulation)
                        return _cancel("Target is Ovulating/Luteal");
                    var total       = PreggoManager.Instance.LengthTotal;
                    int extra       = (int)(total * .2f);
                    data.PhaseTime += extra;
                    int overshoot   = data.PhaseTime - total;
                    if (overshoot > 0)
                        data.PhaseTime = overshoot;
                    data.PhaseTime = Mathf.Min(PreggoManager.Instance.LengthMenstrual + PreggoManager.Instance.LengthFollicular, data.PhaseTime);
                    _mes_cont(new FText("Menstrual cycle progressed").Clr("yellow"));
                    break;
                case ItemNames.Ovu_2:
                    if (!preggoeli)
                        return _cancel("Target does not have a menstrual cycle");
                    if (phase != PreggoPhase.Luteal && phase != PreggoPhase.Fertilized && phase != PreggoPhase.Pregnant)
                        return _cancel("Target is not Luteal");
                    _mes_cont(new FText("Ovulation imminent").Clr("yellow"));
                    PreggoManager.Instance.Ovulate(data);
                    break;
                case ItemNames.Fert:
                    if (s.hasTrait(SpecialTraits.Infertile))
                    {
                        if (!s.removeTrait(SpecialTraits.Infertile))
                            return _cancel("Unable to remove Infertile");
                        _message(new FText($"{s.CharName} is no longer infertile!").Clr("green"));
                    }
                    else if (!s.hasTrait(GeneticTraitType.Fertile))
                    {
                        if (!s.gainTrait(GeneticTraitType.Fertile))
                            return _cancel("Unable to add Fertile");
                        _message(new FText($"{s.CharName} is now extra fertile.").Clr("green"));
                    }
                    else
                    {
                        return _cancel("Target is neither Infertile or not Fertile");
                    }
                    break;
                case ItemNames.BirthControl:
                    if (!preggoeli)
                        return _cancel("Target does not have a menstrual cycle");
                    data.BirthControl = !data.BirthControl;
                    if (data.BirthControl)
                        _message(new FText("Ovulation will be halted."));
                    else
                        _message(new FText("Ovulation will occur again."));
                    break;
                default:
                    if (valid ?? true)
                        return true;
                    break;
            }
            item.CurrentUses--;
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(Item), nameof(Item.getTooltipInfo))]
        [HarmonyPostfix]
        private static void Item_getTooltipInfo(bool showPrice, Item __instance, ref string __result)
        {
            if (!(__instance is CumItem cum))
                return;
            var _nl = Environment.NewLine;
            __result += _nl;
            var genetics = cum.giver.genetics;
            __result += string.Concat(
                "\n<b>Genetics</b>: \nMaxLevel: ",  Colors.getColor("level"),       genetics.MaxLevel,
                "</color>\nHp: ",                   Colors.getColor("health"),      genetics.hp,
                "</color>  Mana: ",                 Colors.getColor("mana"),        genetics.mana,
                "</color>\nStrength: ",             Colors.getColor("strength"),    genetics.strength,
                "</color>  Magic: ",                Colors.getColor("magic"),       genetics.magic,
                "</color>\nLust: ",                 Colors.getColor("lust"),        genetics.lust,
                "</color>\nFertility: ",            Colors.getColor("fertility"),   genetics.fertility,
                "</color>  Virility: ",             Colors.getColor("virility"),    genetics.virility,
                "</color>\nSize: ",                 genetics.size, " ",             genetics.gender,
                "\nTraits: <color=orange>"
            );

            var array = cum.giver.GeneticTraits;
            foreach (var geneticTraitType in array)
            {
                __result = string.Concat(__result, geneticTraitType.getName(), " ");
            }
        }

        [HarmonyPatch(typeof(InventoryData), nameof(InventoryData.getInventory))]
        [HarmonyPostfix]
        private static void InventoryData_getInventory(ref Inventory __result)
        {
            foreach (var item in __result.items.ToArray())
            {
                if (item.name == "Empty")
                {
                    __result.items.Remove(item);
                }
            }
        }
    }
}
