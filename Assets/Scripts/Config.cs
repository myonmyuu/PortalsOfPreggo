using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalsOfPreggoMain.Content
{
    internal struct PreggoConfig
    {
        public string Section;
        public string Name;
        public string Description;

        public PreggoConfig(string section, string name, string description)
        {
            Section = section;
            Name = name;
            Description = description;
        }

        public ConfigEntry<T> Bind<T>(BaseUnityPlugin plugin, T? defaultVal)
            where T : struct
        {
            return plugin.Config.Bind(
                new ConfigDefinition(Section, Name),
                defaultVal ?? default,
                new ConfigDescription(Description)
            );
        }
    }
    internal struct PreggoConfigSection
    {
        public PreggoConfigSection(string section)
        {
            Section = section;
        }
        public string Section;
        public PreggoConfigSection SubSection(string id) => new PreggoConfigSection($"{Section}.{id}");
        public PreggoConfig GetNamedDefinition(string key, string desc) => new PreggoConfig(Section, key, desc);
    }

    public class PreggoSettings
    {
        private const int MINUTES_PER_DAY = 60 * 24;
        private static class Definitions
        {
            private const string PREGGO_MAIN        = "Preggo";
            private const string PREGGO_CUM         = "Cum";
            private const string PREGGO_INCEST      = "Incest";
            private const string PREGGO_CYCLE       = "Cycle";
            private const string PREGGO_ANIMALS     = "NonHuman";
            private const string PREGGO_GAMEPLAY    = "Gameplay";
            private const string PREGGO_OTHER       = "Other";

            private static PreggoConfigSection MainSection = new PreggoConfigSection(PREGGO_MAIN);

            private static PreggoConfigSection CumSection = MainSection.SubSection(PREGGO_CUM);
            public static PreggoConfig CumLeakPerMinute = CumSection.GetNamedDefinition("Cum leakage",  "How much cum characters should leak per minute in ml");
            public static PreggoConfig CumMovePerMinute = CumSection.GetNamedDefinition("Cum movement", "Approximately how much cum should shift around in characters in ml");
            public static PreggoConfig CumMultiplier    = CumSection.GetNamedDefinition("Cum multiplier", "Modifies the amount of cum for cumshots. 1 = 100%");
            public static PreggoConfig VaginaLimit      = CumSection.GetNamedDefinition("Vagina cum limit", "Cum limit in ml. Limits are 'soft', when passed pressure outward increases, causing them to empty quicker");
            public static PreggoConfig UterusLimit      = CumSection.GetNamedDefinition("Uterus cum limit", "Cum limit in ml, see above");

            private static PreggoConfigSection IncestSection = MainSection.SubSection(PREGGO_INCEST);
            public static PreggoConfig IncestAdditions  = IncestSection.GetNamedDefinition("Incest additions", "The player's offspring refer to the player as Mum/Mommy/Dad/Daddy");

            private static PreggoConfigSection CycleSection = MainSection.SubSection(PREGGO_CYCLE);
            public static PreggoConfig LengthMenstrual  = CycleSection.GetNamedDefinition("Period length", "How many minutes character should spend menstruating");
            public static PreggoConfig LengthFollicular = CycleSection.GetNamedDefinition("Follicular length", "How many minutes character should be in the follicular phase");
            public static PreggoConfig LengthOvulation  = CycleSection.GetNamedDefinition("Ovulation length", "How many minutes an ovulation should take");
            public static PreggoConfig LengthLuteal     = CycleSection.GetNamedDefinition("Luteal length", "How many minutes an ovum should live for on average");
            public static PreggoConfig LengthPregnancy  = CycleSection.GetNamedDefinition("Pregnancy length", "How many minutes a pregnancy should take on average");

            private static PreggoConfigSection GameplaySection = MainSection.SubSection(PREGGO_GAMEPLAY);
            public static PreggoConfig PregnantShield   = GameplaySection.GetNamedDefinition("Pregnant Shield", "Pregnant characters get a shield each turn dependant on the child's hp gene");

            private static PreggoConfigSection Other    = MainSection.SubSection(PREGGO_OTHER);
            public static PreggoConfig DateVirginity    = Other.GetNamedDefinition("Date virginity", "Vagina-penetrating actions for virgins cost 1 lewdity more and deflower the character");
            public static PreggoConfig Items            = Other.GetNamedDefinition("Items", "Add pregnancy-related items");
            public static PreggoConfig MainCharScaling  = Other.GetNamedDefinition("Main character genetic scaling", "If the main character's genetics should improve as they birth offspring with better genetics");
        }

        public static PreggoSettings Instance => PortalsOfPreggoPlugin.Instance.Settings;
        public PreggoSettings(PortalsOfPreggoPlugin instance)
        {
            CumLeakPerMinute    = Definitions.CumLeakPerMinute.Bind<float>(instance, 0.0208334f);
            CumMovePerMinute    = Definitions.CumMovePerMinute.Bind<float>(instance, 0.0069f);
            CumMult             = Definitions.CumMultiplier.Bind<float>(instance, 1);
            VaginaLimit         = Definitions.VaginaLimit.Bind<float>(instance, 150);
            UterusLimit         = Definitions.UterusLimit.Bind<float>(instance, 100);

            PregnantShield      = Definitions.PregnantShield.Bind<bool>(instance, true);

            IncestAdditions     = Definitions.IncestAdditions.Bind<bool>(instance, true);

            DateVirginity       = Definitions.DateVirginity.Bind<bool>(instance, true);
            Items               = Definitions.Items.Bind<bool>(instance, true);
            MainCharScaling     = Definitions.MainCharScaling.Bind<bool>(instance, true);

            LengthMenstrual     = Definitions.LengthMenstrual.Bind<int>(instance, MINUTES_PER_DAY * 2);
            LengthFollicular    = Definitions.LengthFollicular.Bind<int>(instance, MINUTES_PER_DAY * 2);
            LengthOvulation     = Definitions.LengthOvulation.Bind<int>(instance, 60);
            LengthLuteal        = Definitions.LengthLuteal.Bind<int>(instance, MINUTES_PER_DAY * 2);
            LengthPregnancy     = Definitions.LengthPregnancy.Bind<int>(instance, MINUTES_PER_DAY * 4);
        }

        public ConfigEntry<float> CumLeakPerMinute;
        public ConfigEntry<float> CumMovePerMinute;
        public ConfigEntry<float> CumMult;
        public ConfigEntry<float> VaginaLimit;
        public ConfigEntry<float> UterusLimit;

        public ConfigEntry<bool> PregnantShield;

        public ConfigEntry<bool> IncestAdditions;

        public ConfigEntry<bool> DateVirginity;
        public ConfigEntry<bool> Items;
        public ConfigEntry<bool> MainCharScaling;

        public ConfigEntry<int> LengthMenstrual;
        public ConfigEntry<int> LengthFollicular;
        public ConfigEntry<int> LengthOvulation;
        public ConfigEntry<int> LengthLuteal;
        public ConfigEntry<int> LengthPregnancy;
    }
}
