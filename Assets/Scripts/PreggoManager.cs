using HarmonyLib;
using Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace PortalsOfPreggoMain.Content
{
    public static class PreggoMainExtensions
    {
        public static string GetStatName(this int stat)
        {
            switch (stat)
            {
                case 0: return "max level";
                case 1: return "strength";
                case 2: return "hp";
                case 3: return "mana";
                case 4: return "fertility";
                case 5: return "virility";
                case 6: return "lust";
                case 7: return "magic";
            }
            return "invalid???";
        }
        public static int GetStat(this Genetics genetics, int stat)
        {
            switch (stat)
            {
                case 0:
                    return genetics.MaxLevel;
                case 1:
                    return genetics.strength;
                case 2:
                    return genetics.hp;
                case 3:
                    return genetics.mana;
                case 4:
                    return genetics.fertility;
                case 5:
                    return genetics.virility;
                case 6:
                    return genetics.lust;
                case 7:
                    return genetics.magic;
            }
            PortalsOfPreggoPlugin.Instance.Log.LogWarning($"tried to get invalid stat at number '{stat}'");
            return 0;
        }
        public static Genetics GrowStat(this Genetics genetics, int stat, int amt)
        {
            switch (stat)
            {
                case 0:
                    genetics.MaxLevel += amt;
                    break;
                case 1:
                    genetics.strength += amt;
                    break;
                case 2:
                    genetics.hp += amt;
                    break;
                case 3:
                    genetics.mana += amt;
                    break;
                case 4:
                    genetics.fertility += amt;
                    break;
                case 5:
                    genetics.virility += amt;
                    break;
                case 6:
                    genetics.lust += amt;
                    break;
                case 7:
                    genetics.magic += amt;
                    break;
            }
            return genetics;
        }
        public static Genetics GrowRandom(this Genetics genetics)
        {
            return genetics.GrowStat(UnityEngine.Random.Range(0, 8), 1);
        }
        public static string ToColor(this Stats.Gender gender)
        {
            switch (gender)
            {
                case Stats.Gender.Female:
                    return "FD65FF";
                case Stats.Gender.Male:
                    return "3ACEFF";
                case Stats.Gender.Futa:
                    return "C481FF";
                default:
                    return "FFFFFF";
            }
        }
        public static V Ensure<K, V>(this IDictionary<K, V> col, K key)
            where V : new()
        {
            if (!col.TryGetValue(key, out V v))
            {
                v = new V();
                col.Add(key, v);
            }
            return v;
        }

        public static void GetParents(this Genetics g, out Stats mother, out Stats father)
        {
            void _find(int id, out Stats s)
            {
                bool _trycache(Stats _s)
                {
                    _s.EnsureGeneticsID();
                    if (_s.genetics.id == id)
                    {
                        PreggoManager.Instance.GeneticCache[id] = _s;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (!PreggoManager.Instance.GeneticCache.TryGetValue(id, out s))
                {
                    if (_trycache(SaveController.instance.mainCharacter.combatForm))
                    {
                        s = SaveController.instance.mainCharacter.combatForm;
                        return;
                    }
                    foreach (var ch in SaveController.instance.playerChars)
                    {
                        if (_trycache(ch))
                        {
                            s = ch; return;
                        }
                    }
                    foreach (var npc in SaveController.instance.npcs)
                    {
                        if (_trycache(npc.stats))
                        {
                            s = npc.stats; return;
                        }
                    }
                }
            }

            _find(g.motherId, out mother);
            _find(g.fatherId, out father);

            PortalsOfPreggoPlugin.Instance.Log.LogInfo($"getting parents for id {g.id}, fatherId: {g.fatherId}, motherId: {g.motherId}");

        }

        public static void EnsureGeneticsID(this Stats stats, int? min = null, int? max = null)
        {
            if (stats.genetics.id != 0)
            {
                return;
            }

            if (!min.HasValue)
            {
                min = int.MinValue + (int.MaxValue / 2);
            }
            if (!max.HasValue)
            {
                max = int.MaxValue;
            }
            var gene = stats.genetics;
            var idset = new HashSet<int>()
            {
                SaveController.instance.mainCharacter.combatForm.genetics.id
            };
            foreach (var s in SaveController.instance.playerChars)
            {
                idset.Add(s.genetics.id);
            }
            foreach (var n in SaveController.instance.npcs)
            {
                idset.Add(n.stats.genetics.id);
            }
            var newid = 0;
            while (newid == 0 || idset.Contains(newid))
            {
                newid = UnityEngine.Random.Range(min.Value, max.Value);
            }
            stats.SetGeneticsID(newid);
            PreggoManager.Instance.GeneticCache[gene.id] = stats;
        }

        public static void SetGeneticsID(this Stats stats, int id)
        {
            var gene = stats.genetics;
            gene.id = id;
            stats.genetics = gene;
        }

        public static Stats GetKeyFor(this IDictionary<Stats, float> col, Stats key)
        {
            foreach (var (stat, _) in col)
                if (key.genetics.id == stat.genetics.id)
                    return stat;
            return key;
        }

        public static float MoveCum(this IDictionary<Stats, float> from, IDictionary<Stats, float> to, float volume,
            Func<Stats, float> bonusPercent = null)
        {
            volume = Mathf.Abs(volume);
            float moved = 0;
            if (!from.Any())
                return moved;
            var basePercent = 1 / from.Count;
            var totalAvailable = from.GetSum();

            if (volume <= 0 || totalAvailable <= 0)
                return moved;
            var baseAmount = volume / totalAvailable;
            
            foreach (var (owner, cumAmt) in from.ToArray())
            {
                var min = cumAmt * baseAmount * 0.8f;
                var max = cumAmt * baseAmount * 1.2f;
                var amount = UnityEngine.Random.Range(min, max);
                if (bonusPercent != null)
                    amount *= bonusPercent(owner);

                amount = Math.Min(amount, cumAmt);
                from.ChangeCum(owner, -amount);
                to.ChangeCum(owner, amount);
                moved += amount;
            }
            return moved;
        }

        public static void ChangeCum(this IDictionary<Stats, float> col, Stats giver, float amount)
        {
            var key = GetKeyFor(col, giver);
            var newVal = col.GetValueOr(key, 0) + amount;
            col[key] = newVal;
            if (newVal <= 0)
                col.Remove(key);
        }

        public static V GetValueOr<K,V>(this IDictionary<K, V> col, K val, V def)
        {
            if (col.TryGetValue(val, out V v))
                return v;
            return def;
        }

        public static float GetSum<K>(this IDictionary<K, float> col) => col.Sum(x => x.Value);

        public static string GetUID(this Stats stats)
        {
            stats.EnsureGeneticsID();
            switch (stats.GetPreggoType(out var index))
            {
                default:
                case PreggoType.Invalid:
                    //PreggoPlugin.Instance.Log.LogWarning($"tried to get uid of invalid stats '{stats.CharName}'");
                    return "invalid";
                case PreggoType.MC:
                    return "player";
                case PreggoType.Player:
                    return $"plr_{stats.genetics.id}";
                case PreggoType.NPC:
                    return stats.uniqueCharacter.ToString();
                case PreggoType.Temp:
                    return $"TEMP {stats.genetics.id}";
            }
        }

        public static PreggoType GetPreggoType(this Stats stats, out int index)
        {
            index = SaveController.instance.playerChars.IndexOf(stats);
            if (!stats.IsPreggoEligible())
                return PreggoType.Invalid;
            else if (stats == SaveController.instance.mainCharacter.combatForm)
                return PreggoType.MC;
            else if (stats.uniqueCharacter.IsNPC())
                return PreggoType.NPC;
            else
                return index >= 0
                    ? PreggoType.Player
                    : PreggoType.Temp;
        }

        public static bool IsNPC(this UniqueCharacter c)
        {
            switch (c)
            {
                case UniqueCharacter.Sylvie:
                case UniqueCharacter.Castalia:
                case UniqueCharacter.Flora:
                case UniqueCharacter.Lumira:
                case UniqueCharacter.Mya:
                case UniqueCharacter.Aila:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPreggoEligible(this SexAction2 a) =>
                (a.giver == BodyPart.Cock && a.receiver == BodyPart.Pussy)
                || (a.giver == BodyPart.Pussy && a.receiver == BodyPart.Cock);

        public static Stats GetReceiver(this SexAction2 a, Stats initiator, Stats sub)
        {
            if (!a.IsPreggoEligible())
                return null;
            if (a.giver == BodyPart.Cock)
                return sub;
            else
                return initiator;
        }

        public static Stats GetGiver(this SexAction2 a, Stats initiator, Stats sub)
        {
            if (!a.IsPreggoEligible())
                return null;
            if (a.giver == BodyPart.Cock)
                return initiator;
            else
                return sub;
        }

        public static bool IsPreggoEligible(this Stats stats)
        {
            return stats.hasPussy();
        }

        public static void Deconstruct<K, V>(this KeyValuePair<K, V> pair, out K key, out V val)
        {
            key = pair.Key;
            val = pair.Value;
        }

        public static string ToHTMLColor(this PreggoPhase phase)
        {
            switch (phase)
            {
                case PreggoPhase.Menstrual:
                    return "9c606f";
                case PreggoPhase.Follicular:
                    return "9c608b";
                case PreggoPhase.Ovulation:
                    return "9c6099";
                case PreggoPhase.Luteal:
                    return "9c6099";
                case PreggoPhase.Fertilized:
                    return "e391e2";
                case PreggoPhase.Pregnant:
                    return "84609c";
                case PreggoPhase.Menopause:
                    return "555555";
            }
            PortalsOfPreggoPlugin.Instance.Log.LogWarning($"invalid phase for color: '{phase}'");
            return "555555";

//            throw new NotImplementedException();
        }

        public static string GetName(this PreggoPhase phase )
        {
            switch (phase)
            {
                case PreggoPhase.Menopause:
                    return "Ovarian Insufficiency";
                default:
                    return phase.ToString();
            }
        }
        public static Stats GetCurrent(this CharacterManager man)
        {
            if (man == null)
                return null;
            return Traverse.Create(man).Field("currentCharacter").GetValue() as Stats;
        }
    }

    public enum PreggoType
    {
        Invalid,
        MC,
        Player,
        NPC,
        Temp
    }

    [Flags]
    public enum BroadcastTarget
    {
        None    = 0,
        Log     = 1 << 0,
        Date    = 1 << 1,


        All     = Log | Date
    }

    public class SeenPreggoData
    {
        public float Cum;
        public bool Ovulation;
    }

    public class PreggoManager
    {
        private const float CUM_MOVE_VAGINA_MULT = 1f;
        private const float CUM_MOVE_UTERUS_MULT = 1f;
        private static Lazy<PreggoManager> _Instance = new Lazy<PreggoManager>(() => new PreggoManager());
        public static PreggoManager Instance => _Instance.Value;
        private IDictionary<Stats, float> CumOnFloor = new Dictionary<Stats, float>();
        private IDictionary<int, PreggoData> PreggoDataCache = new Dictionary<int, PreggoData>();
        public IDictionary<int, Stats> GeneticCache = new Dictionary<int, Stats>();

        public Dictionary<PreggoData, SeenPreggoData> SeenData = new Dictionary<PreggoData, SeenPreggoData>();

        private readonly Weighted_Randomizer.StaticWeightedRandomizer<int> OvumAmts = new Weighted_Randomizer.StaticWeightedRandomizer<int>()
        {
            [1] = 800,
            [2] = 10,
            [3] = 1,
        };
        private readonly Weighted_Randomizer.StaticWeightedRandomizer<Func<Species>> SpeciesRand = new Weighted_Randomizer.StaticWeightedRandomizer<Func<Species>>()
        {
            [() => CharacterControllerScript.instance.getRandomSpecies()] = 200,
            [() => CharacterControllerScript.instance.getRandomBaseSpecies()] = 400,
			[() => CharacterControllerScript.instance.getRandomSpecialHybrid()] = 5
        };
        private readonly Weighted_Randomizer.StaticWeightedRandomizer<Stats.Gender> GenderRand = new Weighted_Randomizer.StaticWeightedRandomizer<Stats.Gender>()
        { 
            [Stats.Gender.Male]     = 5,
            [Stats.Gender.Female]   = 7,
            [Stats.Gender.Futa]     = 9,
        };

        public delegate void PreggoPhaseChangedDelegate(PreggoData data, PreggoPhase prev, PreggoPhase next, bool last);
        public event PreggoPhaseChangedDelegate OnPhaseChanged;

        public delegate void PreggoDataUpdatedDelegate(PreggoData data);
        public event PreggoDataUpdatedDelegate OnDataUpdated;

        private PreggoSave _Save { get; set; }
        public PreggoSave Save
        {
            set
            {
                _Save = value;
            }
            get
            {
                if (_Save == null)
                {
                    PortalsOfPreggoPlugin.Instance.Log.LogInfo("Creating a new preggo save");
                    _Save = PreggoSave.Create();
                }
                return _Save;
            }
        }

        private float CumLeakPerMinute;
        private float CumMovePerMinute;

        public int LengthMenstrual;
        public int LengthFollicular;
        public int LengthOvulation;
        public int LengthLuteal;
        public int LengthTotal;
        public int LengthPregnancy;

        public IEnumerable<PreggoData> AllData => Enumerable.Empty<PreggoData>()
            .Append(Save.MC)
            .Concat(Save.PlayerChars.Select(x => x.Value))
            .Concat(Save.NPCs.Select(x => x.Value))
            .Where(x => x != null)
            .ToArray();

        public void ForceRemoveData(PreggoData data)
        {
            string fid = null;
            foreach (var (uid, pd) in Save.PlayerChars)
            {
                if (pd != data)
                    continue;

                fid = uid;
            }

            if (fid != null)
            {
                PortalsOfPreggoPlugin.Instance.Log.LogWarning($"force-removing preggo data: '{fid}'");
                Save.PlayerChars.Remove(fid);
            }
        }

        public PreggoManager()
        {
            OnPhaseChanged += PreggoManager_OnPhaseChanged;
        }

        private void PreggoManager_OnPhaseChanged(PreggoData data, PreggoPhase prev, PreggoPhase next, bool last)
        {
            if ((prev == PreggoPhase.Ovulation || next == PreggoPhase.Luteal) && !data.Ovums.Any())
            {
                Ovulate(data);
            }
        }

        private int GetPhaseFluctuation(PreggoPhase phase)
        {
            switch (phase)
            {
                case PreggoPhase.Menstrual:
                    return 4;
                case PreggoPhase.Follicular:
                    return 3;
                case PreggoPhase.Ovulation:
                    return 5;
                case PreggoPhase.Luteal:
                case PreggoPhase.Fertilized:
                case PreggoPhase.Pregnant:
                case PreggoPhase.Invalid:
                default:
                    return 0;
            }
        }

        public Stats MakeRandomCharacter(Stats.Gender? gender = null, Species? species = null)
        {
            if (!species.HasValue)
                species = SpeciesRand.NextWithReplacement()(); // hehe funny brackets

            var stats = CharacterControllerScript.instance.generateStats(species.Value, addId: true);
            for (var i = 0; i < SaveController.instance.newGamePlusInfo.hostilityMod * 7; i++)
                stats.genetics = stats.genetics.GrowRandom();

            stats.changeGender(gender ?? GenderRand.NextWithReplacement());
            return stats;
        }

        public void InitSave()
        {
            Reset();
            var hmap = new HashSet<PreggoData>();
            if (TryGetData(SaveController.instance?.mainCharacter?.combatForm, out var plrd))
            {
                hmap.Add(plrd);
            }

            if (SaveController.instance.playerChars == null)
            {
                PortalsOfPreggoPlugin.Instance.Log.LogWarning("InitSave() player chars are null??");
                return;
            }

            foreach (var s in SaveController.instance.playerChars)
            {
                if (s == null)
                {
                    PortalsOfPreggoPlugin.Instance.Log.LogWarning("InitSave() char is null?");
                    continue;
                }
                if (s.uniqueCharacter.IsNPC())
                    continue;

                s.EnsureGeneticsID();
                if (!TryGetData(s, out var pregd) && s.CurrGender != Stats.Gender.Male)
                {
                    PortalsOfPreggoPlugin.Instance.Log.LogWarning($"unable to get preggo data for '{s.CharName}'");
                    continue;
                }

                if (s.CurrGender == Stats.Gender.Male)
                {
                    continue;
                }

                if (hmap.Contains(pregd))
                {
                    var ch = pregd?.GetCharacter();
                    PortalsOfPreggoPlugin.Instance.Log.LogWarning($"duplicate preggo data?? {ch?.CharName}({ch?.genetics.id}) and {s?.CharName}({s?.genetics.id})");
                }
                else
                {
                    hmap.Add(pregd);
                }

            }

            if (SaveController.instance?.npcs == null)
            {
                PortalsOfPreggoPlugin.Instance.Log.LogWarning("InitSave() npcs null?");
            }

            foreach (var npc in SaveController.instance.npcs)
            {
                npc.stats.EnsureGeneticsID();
                _ = TryGetData(npc.stats, out _);
            }
        }

        public void CacheCycleLength()
        {
            var set = PortalsOfPreggoPlugin.Instance.Settings;

            LengthMenstrual = set.LengthMenstrual.Value;
            LengthFollicular = set.LengthFollicular.Value;
            LengthOvulation = set.LengthOvulation.Value;
            LengthLuteal = set.LengthLuteal.Value;
            LengthPregnancy = set.LengthPregnancy.Value;
            LengthTotal = LengthMenstrual + LengthFollicular + LengthOvulation + LengthLuteal;
        }

        public void CacheCumSettings()
        {
            CumLeakPerMinute = PortalsOfPreggoPlugin.Instance.Settings.CumLeakPerMinute.Value;
            CumMovePerMinute = PortalsOfPreggoPlugin.Instance.Settings.CumMovePerMinute.Value;
        }

        public void BroadcastMessage(string message, BroadcastTarget target = BroadcastTarget.All, bool silenced = false)
        {
            if (!NPCManager.instance) // prevent errors when main chara ovulates at game start
                return;
            if (silenced)
                return;

            if ((target & BroadcastTarget.Log) != BroadcastTarget.None)
            {
                try
                {
                    LogController.instance.addMessage(message);
                }
                catch { /* Should probably never be a problem */ }
            }

            if ((target & BroadcastTarget.Date) != BroadcastTarget.None)
            {
                try
                {
                    DateManager.instance.addMessage(message);
                }
                catch { /* If no date is valid, don't want to check if it'd be valid, yes I'm lazy */ }
            }
        }
        
        public void SpreadCum(PreggoData data, int minutes)
        {
            var vaginaAmt = data.CumInVagina.GetSum();
            var uterusAmt = data.CumInUterus.GetSum();

            var absDiff = Mathf.Abs(vaginaAmt - uterusAmt);

            var uterusPressure = 1f;
            var vaginaPressure = 1f;

            if (uterusAmt > vaginaAmt)
                uterusPressure += Mathf.Sqrt(absDiff);
            else
                vaginaPressure += Mathf.Sqrt(absDiff) * 2;

            var vagLimit = PreggoSettings.Instance.VaginaLimit.Value;
            if (vaginaAmt > vagLimit)
                vaginaPressure += Mathf.Sqrt(vaginaAmt - vagLimit);

            var utLimit = PreggoSettings.Instance.UterusLimit.Value;
            if (uterusAmt > utLimit)
                uterusPressure += Mathf.Sqrt(uterusAmt - utLimit);

            var vagOut = CumMovePerMinute * minutes * CUM_MOVE_UTERUS_MULT * vaginaPressure;
            var v_u = data.CumInVagina.MoveCum(
                data.CumInUterus,
                vagOut,
                s => 1f + (s.getModifiedVirility() / 300)
            );

            var uvOut = CumMovePerMinute * minutes * CUM_MOVE_VAGINA_MULT * uterusPressure;
            var u_v = data.CumInUterus.MoveCum(
                data.CumInVagina,
                uvOut
            );

            var leakage = minutes * CumLeakPerMinute * vaginaPressure;
            
            var v_g = data.CumInVagina.MoveCum(
                CumOnFloor,
                leakage
            );

            OnDataUpdated?.Invoke(data);
        }

        public void CumIn(Stats reciever, Stats giver, bool quiet = false, float? volume = null)
        {
            if (!TryGetData(reciever, out var data))
            {
                PortalsOfPreggoPlugin.Instance.Log.LogWarning($"tried to cum in invalid character '{reciever.CharName}'");
                return;
            }

            CumIn(data, giver, quiet, volume);
        }

        public float GetCumAmount(Stats giver)
        {
            float volume = 0;
            var virBonus = UnityEngine.Mathf.Log(giver.getModifiedVirility());
            var virMult = 1f + virBonus;
            volume = giver.bodyFeatures.ballsSize;

            virMult *= 1f + ((int)giver.size / 5f);
            var multFinal = UnityEngine.Mathf.LerpUnclamped(0.8f, virMult, UnityEngine.Random.value);
            volume *= multFinal;
            return volume;
        }

        public void CumIn(PreggoData data, Stats giver, bool quiet = false, float? volume = null)
        {
            var stats = data.GetCharacter();
            giver.EnsureGeneticsID();

            var depth = (int)stats.size * 20;
            var penSize = (int)giver.size * giver.bodyFeatures.cockSize;

            if (!volume.HasValue)
            {
                volume = GetCumAmount(giver);
            }
            volume *= PreggoSettings.Instance.CumMult.Value;

            var diff = UnityEngine.Mathf.Abs(depth - penSize);
            var diffPer = diff / depth;

            var amtInUterus = UnityEngine.Mathf.Lerp(0, volume.Value * 0.9f, diffPer);
            var amtInVagina = volume.Value - amtInUterus;

            data.CumInUterus.ChangeCum(giver, amtInUterus);
            data.CumInVagina.ChangeCum(giver, amtInVagina);

            if (!quiet)
            {
                BroadcastMessage(
                    PreggoLua.Instance.RunFileAsString("message_cum", giver, stats, volume, MoonSharp.Interpreter.UserData.CreateStatic<UnityEngine.Color>()),
                    silenced: data.Silenced
                );
            }
            SpreadCum(data, 60);
        }

        public void PassTimeForAll(int minutes)
        {
            foreach (var data in AllData)
            {
                PassTime(data, minutes);
            }
        }

        public void PassTime(PreggoData data, int minutes)
        {
            var chara = data.GetCharacter();
            if (chara == null)
            {
                ForceRemoveData(data);
                return;
            }
            List<(PreggoPhase, PreggoPhase)> queued = new List<(PreggoPhase,PreggoPhase)>();
            var startPhase = GetPhase(data);

            int cycleMinutes = minutes;
            cycleMinutes += UnityEngine.Random.Range(0, GetPhaseFluctuation(startPhase));

            int startTime = data.PhaseTime;
            int nextTime;

            while (cycleMinutes > 0)
            {
                var curPhase = GetPhase(data);
                if (startTime < LengthMenstrual)
                {
                    nextTime = LengthMenstrual;
                }
                else if (startTime < LengthMenstrual + LengthFollicular)
                {
                    nextTime = LengthMenstrual + LengthFollicular;
                }
                else if (startTime < LengthMenstrual + LengthFollicular + LengthOvulation)
                {
                    nextTime = LengthMenstrual + LengthFollicular + LengthOvulation;
                }
                else
                {
                    nextTime = LengthTotal;
                }

                var diff = Math.Max(1, nextTime - startTime);
                var step = Math.Min(diff, cycleMinutes);
                cycleMinutes -= step;
                data.PhaseTime += step;

                if (data.PhaseTime > LengthTotal && !data.Ovums.Any())
                {
                    data.PhaseTime -= LengthTotal;
                }

                var nPhase = GetPhase(data);
                if (curPhase != nPhase)
                {
                    queued.Add((curPhase, nPhase));
                }
            }

            if (data.Ovums.Any())
            {
                foreach (var ovum in data.Ovums.ToArray())
                {
                    ovum.MinutesPassed += minutes + UnityEngine.Random.Range(-1, 2);

                    switch (ovum.State)
                    {
                        case OvumState.Implanted:
                            if (ovum.MinutesPassed >= PortalsOfPreggoPlugin.Instance.Settings.LengthPregnancy.Value)
                            {
                                Birth(data, ovum);
                            }
                            break;
                        default:
                            if (ovum.MinutesPassed >= PortalsOfPreggoPlugin.Instance.Settings.LengthLuteal.Value)
                            {
                                data.Ovums.Remove(ovum);
                            }
                            break;
                    }
                }

                TryToImpregnate(data, minutes);
            }

            for (int i = 0; i < queued.Count; i++)
            {
                var c = queued[i];
                OnPhaseChanged?.Invoke(data, c.Item1, c.Item2, i == queued.Count - 1);
            }

            SpreadCum(data, minutes);
        }

        public PreggoPhase GetPhase(PreggoData data)
        {
            var chara = data.GetCharacter();
            if (chara.hasTrait(SpecialTraits.Infertile) || chara.getModifiedVirility() == 0)
            {
                data.PhaseTime = 0;
                return PreggoPhase.Menopause;
            }

            if (data.Ovums.Any(x => x.State == OvumState.Implanted))
                return PreggoPhase.Pregnant;
            if (data.Ovums.Any(x => x.State == OvumState.Fertilized))
                return PreggoPhase.Fertilized;
            if (data.Ovums.Any())
                return PreggoPhase.Luteal;

            if (data.PhaseTime <= LengthMenstrual)
            {
                return PreggoPhase.Menstrual;
            }
            else if (data.PhaseTime <= LengthMenstrual + LengthFollicular)
            {
                return PreggoPhase.Follicular;
            }
            else if (data.PhaseTime <= LengthMenstrual + LengthFollicular + LengthOvulation)
            {
                return PreggoPhase.Ovulation;
            }
            else if (data.PhaseTime <= LengthMenstrual + LengthFollicular + LengthOvulation + LengthLuteal)
            {
                return PreggoPhase.Luteal;
            }

            PortalsOfPreggoPlugin.Instance.Log.LogWarning($"{chara.CharName} is in invalid phase?");
            return PreggoPhase.Invalid;
        }

        public void Ovulate(PreggoData data)
        {
            var stats = data.GetCharacter();
            if (stats.getModifiedFertility() <= 0)
                return;
            if (data.BirthControl)
            {
                BroadcastMessage(
                    PreggoLua.Instance.RunFileAsString("message_birthcontrol", stats, "bea2fc"),
                    silenced: data.Silenced
                );
            }
            var amt = OvumAmts.NextWithReplacement();
            data.Ovums.AddRange(Enumerable.Range(0, amt).Select(x => PreggoOvum.Create()));
            
            BroadcastMessage(
                PreggoLua.Instance.RunFileAsString("message_ovulation", stats, amt, "bea2fc"),
                silenced: data.Silenced
            );
        }

        private const float MAX_FERT = 400;
        private readonly Stats EmptyStats = new Stats();
        private void TryToImpregnate(PreggoData data, int minutes)
        {
            var charStats = data.GetCharacter();
            if (charStats == null)
            {
                Debug.Log("Character no longer exists");
                return;
            }

            var fertility = charStats.getModifiedFertility();
            var rand = new Weighted_Randomizer.StaticWeightedRandomizer<Stats>(new System.Random().Next());
            foreach (var (ch, amt) in data.CumInUterus)
            {
                var viri = ch.getModifiedVirility();
                var weight = Mathf.Max(Mathf.Min(1, viri), minutes * (int)(amt * (1 + Mathf.Log(viri))));
                rand.Add(ch, weight);
            }

            var bigNone = Mathf.Lerp(1500, 4000, UnityEngine.Random.value);
            var smallNone = Mathf.Lerp(0, 10, UnityEngine.Random.value);
            var noneWeight = Mathf.Max(1, (int)Mathf.LerpUnclamped(bigNone, smallNone, charStats.getModifiedFertility() / MAX_FERT));
            rand.Add(EmptyStats, noneWeight);

            foreach (var egg in data.Ovums)
            {
                if (egg.State == OvumState.Fertilized
                    && UnityEngine.Random.value > 1.1f - (Mathf.Log(charStats.getModifiedFertility()) / 10f))
                    ImpregnateWith(data, egg);

                if (egg.State != OvumState.Alive)
                    continue;

                var fertResult = rand.NextWithReplacement();
                if (fertResult == EmptyStats)
                    continue;

                Fertilize(egg, data, fertResult);
            }

            return;
        }

        public PreggoData MakeDataForStats(Stats stats, PreggoType type)
        {
            var time = UnityEngine.Random.Range(0, LengthTotal);
            var data = new PreggoData(stats)
            {
                UniqueCharacter = stats.uniqueCharacter,
                PhaseTime = time,
                Type = type
            };

            if (time > LengthMenstrual + LengthFollicular + LengthOvulation)
            {
                Ovulate(data);
            }
            return data;
        }

        public Stats GetFather(PreggoOvum ovum, PreggoData data)
        {
            if (ovum.Embryo == null)
            {
                Debug.LogError("tried to get father without embryo, resetting ovum");

                var father = ovum.Father?.getStats();
                if (father != null)
                {
                    Debug.LogError("valid father is set, re-fertilizing");
                    Fertilize(ovum, data, father);
                }
                else
                {
                    ovum.MinutesPassed = 0;
                    ovum.State = OvumState.Alive;
                    return null;
                }
            }
            return ovum.Father?.getStats();
        }

        public string GetFatherNameColored(PreggoOvum ovum, PreggoData data)
        {
            var father = GetFather(ovum, data);
            if (father == null)
                return string.Empty;
            var name = 
                new FText(father.CharName)
                    .Clr(father.CurrGender.ToColor())
                + $" ({father.species})";
            return name;
        }

        private void Fertilize(PreggoOvum ovum, PreggoData mother, Stats father)
        {
            var motherStats     = mother.GetCharacter();
            ovum.Father         = new CharacterSaveData(father);
            ovum.State          = OvumState.Fertilized;
            ovum.MinutesPassed  = 0;
            ovum.Embryo         = new FarmPairing(father, motherStats).createEgg();

            BroadcastMessage(
                PreggoLua.Instance.RunFileAsString("message_fertilize", motherStats, father, "f0b1fc"),
                silenced: mother.Silenced
            );
            OnDataUpdated?.Invoke(mother);
        }

        private void ImpregnateWith(PreggoData data, PreggoOvum egg)
        {
            var cA = data.GetCharacter();
            if (cA == null)
            {
                BroadcastMessage(new FText("unable to get character stats for impregnate").Clr(UnityEngine.Color.red));
                return;
            }

            var father = egg.Father.getStats();
            BroadcastMessage(
                PreggoLua.Instance.RunFileAsString("message_impregnate", cA, father, "ff7dcd"),
                silenced: data.Silenced);

            egg.State = OvumState.Implanted;
            egg.MinutesPassed = 0;
            OnDataUpdated?.Invoke(data);
        }

        private const int EXTRA_STATS = 10;
        private void ModifyEmbryo(Egg embryo, Stats mother, Stats father)
        {
            var incEff = (mother.getModifiedFertility() + father.getModifiedVirility()) / MAX_FERT * .6f;
            var incDays = PortalsOfPreggoPlugin.Instance.Settings.LengthPregnancy.Value / 60f / 24f;
            var oldprog = embryo.hatchingProgress;
            embryo.hatchingProgress += Mathf.FloorToInt(embryo.baseProgress * incEff * incDays * 2f);

            var oldstab = embryo.stability;
            var extrastab = Mathf.FloorToInt(embryo.stability * incEff * 10f);
            embryo.stability += extrastab; 
            Debug.LogWarning($"egg mod: egg progress: {oldprog} -> {embryo.hatchingProgress}, stab: {oldstab} -> {embryo.stability}");
            for (int i = 0; i < incEff * EXTRA_STATS; i++)
            {
                embryo.genetics = embryo.genetics.GrowRandom();
            }
        }

        public void Birth(PreggoData data, PreggoOvum egg)
        {
            var stats = data.GetCharacter();
            if (egg.Embryo == null || stats == null)
            {
                LogController.instance.addMessage(new FText("error birthing").Clr("red"));
                return;
            }
            var father = egg.Father.getStats();
            BroadcastMessage(
                PreggoLua.Instance.RunFileAsString("message_layegg", stats, father, "ff91ad"),
                silenced: data.Silenced
            );

            ModifyEmbryo(egg.Embryo, data.GetCharacter(), father);
            SaveController.instance.storedEggs.Add(egg.Embryo);
            
            if (data.Type == PreggoType.MC && PortalsOfPreggoPlugin.Instance.Settings.MainCharScaling.Value)
            {
                var increased = new Dictionary<int, int>();
                for (int i = 0; i < 5; i++)
                {
                    var stat = UnityEngine.Random.Range(0, 8);
                    if (egg.Embryo.genetics.GetStat(stat) > stats.genetics.GetStat(stat))
                    {
                        stats.genetics = stats.genetics.GrowStat(stat, 1);
                        var cur = increased.Ensure(stat);
                        increased[stat] = cur + 1;
                    }
                }

                if (increased.Any())
                {
                    BroadcastMessage(new FText("Your genetic potential increases with the birth of a more powerful egg:"));
                    BroadcastMessage(new FText(
                        string.Join(", ", increased.Select(x => $"{x.Key.GetStatName()}: {x.Value}")))
                    );
                }
            }
            
            data.Ovums.Remove(egg);
            if (!data.Ovums.Any())
                data.PhaseTime = 0;
            data.ClearSemen();
            OnDataUpdated?.Invoke(data);
        }

        public bool TryGetData(Stats stats, out PreggoData data)
        {
            data = null;
            var type = stats.GetPreggoType(out var index);
            stats.EnsureGeneticsID();
            if (GeneticCache.TryGetValue(stats.genetics.id, out var estats))
                stats = estats;
            //Debug.LogWarning($"chara {stats.CharName} type: {type}, index: {index}");
            var uid = stats.GetUID();

            if (PreggoDataCache.TryGetValue(stats.genetics.id, out data))
            {
                if (type == PreggoType.Player && data.Type != type) // if enemy (temp) became ally
                {
                    PortalsOfPreggoPlugin.Instance.Log.LogWarning($"making temporary PreggoData for {stats.CharName} into permanent");
                    data.Type = PreggoType.Player;
                    data.CharacterID = index;
                    Save.PlayerChars[uid] = data;
                }
                return true;
            }

            switch (type)
            {
                case PreggoType.Invalid:
                    return false;
                case PreggoType.MC:
                    if (Save.MC == null)
                    {
                        data = MakeDataForStats(stats, type);
                        Save.MC = data;
                    }
                    data = Save.MC;
                    break;
                case PreggoType.Player:
                    if (!Save.PlayerChars.TryGetValue(uid, out data))
                    {
                        data = MakeDataForStats(stats, type);
                        data.CharacterID = index;
                        Save.PlayerChars[uid] = data;
                    }
                    if (index != data.CharacterID) // player character index seems to shift sometimes
                    {
                        data.ResetCharacter(index);
                    }
                    break;
                case PreggoType.NPC:
                    if (!Save.NPCs.TryGetValue(stats.uniqueCharacter, out data))
                    {
                        data = MakeDataForStats(stats, type);
                        Save.NPCs[stats.uniqueCharacter] = data;
                    }
                    break;
                case PreggoType.Temp:
                    PortalsOfPreggoPlugin.Instance.Log.LogWarning($"making temporary PreggoData for {stats.CharName}");
                    PortalsOfPreggoPlugin.Instance.Log.LogWarning(new System.Diagnostics.StackTrace().ToString());
                    data = MakeDataForStats(stats, type);
                    PreggoDataCache[stats.genetics.id] = data;
                    break;
            }
            PreggoDataCache[stats.genetics.id] = data;
            return data != null;
        }

        public void Reset()
        {
            GeneticCache.Clear();
            PreggoDataCache.Clear();
            CumOnFloor.Clear();
            SeenData.Clear();
        }
    }
}
