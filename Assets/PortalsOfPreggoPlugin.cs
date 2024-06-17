using BepInEx;
using PortalsOfPreggoMain.Content;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BepInPlugin(PortalsOfPreggoPlugin.ID, "Portals of Preggo", "0.1.5")]
public class PortalsOfPreggoPlugin : BaseUnityPlugin
{
    public const string ID = "PortalsOfPreggo";
    
    public const int SCREEN_WIDTH  = 1440;
    public const int SCREEN_HEIGHT = 2560;

    public static PortalsOfPreggoPlugin Instance;
    public BepInEx.Logging.ManualLogSource Log => Logger;
    public string Path => System.IO.Path.GetDirectoryName(Info.Location);
    public PreggoSettings Settings;
    public PreggoLua Lua;

    private HarmonyLib.Harmony PreggoHarmony = new HarmonyLib.Harmony("PortalsOfPreggo");

    private void Awake()
    {
        Instance = this;
        MoonSharp.Interpreter.UserData.RegistrationPolicy = MoonSharp.Interpreter.Interop.InteropRegistrationPolicy.Automatic;
        Settings = new PreggoSettings(this);
        Lua = new PreggoLua();
        gameObject.AddComponent<PrefabManager>();

        PreggoManager.Instance.CacheCumSettings();
        PreggoManager.Instance.CacheCycleLength();

        Patch();
    }

    private void OnDestroy()
    {
        Unpatch();
    }

    private void Patch()
    {
        Log.LogInfo("Patching...");

        PreggoHarmony.PatchAll(typeof(Patches.BrothelPatches));
        PreggoHarmony.PatchAll(typeof(Patches.CharacterPatches));
        PreggoHarmony.PatchAll(typeof(Patches.DatePatches));
        PreggoHarmony.PatchAll(typeof(Patches.EventPatches));
        PreggoHarmony.PatchAll(typeof(Patches.FarmPatches));
        PreggoHarmony.PatchAll(typeof(Patches.IOPatches));
        PreggoHarmony.PatchAll(typeof(Patches.TownPatches));
        PreggoHarmony.PatchAll(typeof(Patches.PartyPatches));
        PreggoHarmony.PatchAll(typeof(Patches.ItemPatches));
        PreggoHarmony.PatchAll(typeof(Patches.OWPatches));
        PreggoHarmony.PatchAll(typeof(Patches.CombatPatches));

        Log.LogInfo("Patching complete.");
    }

    private void Unpatch()
    {
        Log.LogInfo($"Portals Preggo unpatching...");
        PreggoHarmony.UnpatchSelf();
        Log.LogInfo($"unpatching complete");
    }

    private void Update()
    {
        if (UnityInput.Current.GetKeyDown(UnityEngine.KeyCode.L))
        {
            Lua.RunFile("debug");
        }
        // showing/hiding the uterus window is still pretty inconsistent, so users may need to
        // disable it manually
        else if (UnityInput.Current.GetKeyDown(UnityEngine.KeyCode.P))
        {
            Patches.CharacterPatches.ForceToggleUterus();
        }
    }
}
