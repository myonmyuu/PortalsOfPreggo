using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalsOfPreggoMain.Content
{
    [Serializable]
    public class PreggoOvum
    {
        public OvumState State;
        public Egg Embryo;
        public CharacterSaveData Father;
        public int MinutesPassed;

        public float GetGrowthPercent()
        {
            return (float)MinutesPassed / (float)PortalsOfPreggoPlugin.Instance.Settings.LengthPregnancy.Value * 100f;
        }

        public static PreggoOvum Create()
        {
            var res = new PreggoOvum();
            res.State = OvumState.Alive;
            return res;
        }
    }

    public enum PreggoPhase
    {
        Invalid,

        Menstrual,
        Follicular,
        Ovulation,
        Luteal,

        Fertilized,
        Pregnant,

        Menopause
    }

    [Serializable]
    public class PreggoData
    {
        public PreggoData() { }
        public PreggoData(Stats stats)
        {
            Character = stats;
        }

        [NonSerialized]
        public Dictionary<Stats, float> CumInVagina = new Dictionary<Stats, float>();
        [NonSerialized]
        public Dictionary<Stats, float> CumInUterus = new Dictionary<Stats, float>();

        public Dictionary<CharacterSaveData, float> CumInVaginaSaved = new Dictionary<CharacterSaveData, float>();
        public Dictionary<CharacterSaveData, float> CumInUterusSaved = new Dictionary<CharacterSaveData, float>();

        public int PhaseTime;
        public List<PreggoOvum> Ovums = new List<PreggoOvum>();
        public UniqueCharacter UniqueCharacter;
        public int? CharacterID;
        //public int? GeneticID;
        public PreggoType Type;
        public bool Silenced;
        public bool BirthControl;

        [NonSerialized]
        private Stats Character;

        public float VaginaCum => CumInVagina.GetSum();
        public float UterusCum => CumInUterus.GetSum();
        public float TotalCum => VaginaCum + UterusCum;

        public void ResetCharacter(int newId)
        {
            CharacterID = newId;
            Character = null;
        }
        public Stats GetCharacter()
        {
            if (Character == null)
            {
                switch (Type)
                {
                    case PreggoType.MC:
                        Character = SaveController.instance.mainCharacter.combatForm;
                        break;
                    case PreggoType.Player:
                        try
                        {
                            Character = SaveController.instance.playerChars[CharacterID.Value];
                        }
                        catch (Exception e)
                        {
                            PortalsOfPreggoPlugin.Instance.Log.LogError($"attempted to get invalid id for character: '{CharacterID.Value}', count: '{SaveController.instance.playerChars.Count}'");
                            PortalsOfPreggoPlugin.Instance.Log.LogError(e.StackTrace);
                            PortalsOfPreggoPlugin.Instance.Log.LogError("PREGGO DATA WILL BE REMOVED!!!");
                        }
                        
                        break;
                    case PreggoType.NPC:
                        Character = NPCManager.instance.getNpc(UniqueCharacter).stats;
                        break;
                    case PreggoType.Invalid:
                    case PreggoType.Temp:
                        PortalsOfPreggoPlugin.Instance.Log.LogError($"attempted to get character of non-permanent PreggoData");
                        break;
                }
            }
            return Character;
        }

        public void ClearSemen()
        {
            CumInVagina.Clear();
            CumInUterus.Clear();
        }

        public void PrepareForSave()
        {
            CumInVaginaSaved = CumInVagina.ToDictionary(x => new CharacterSaveData(x.Key), y => y.Value);
            CumInUterusSaved = CumInUterus.ToDictionary(x => new CharacterSaveData(x.Key), y => y.Value);
        }

        public void ReadData()
        {
            CumInVagina = CumInVaginaSaved.ToDictionary(x => x.Key.getStats(), y => y.Value);
            CumInUterus = CumInUterusSaved.ToDictionary(x => x.Key.getStats(), y => y.Value);
        }
    }
}
