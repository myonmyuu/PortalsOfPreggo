using HarmonyLib;
using PortalsOfPreggoMain.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patches
{
    public class IOPatches
    {
        private static string GetSavePath(string saveName)
        {
            return System.IO.Path.Combine(PortalsOfPreggoPlugin.Instance.Path, "Save", $"{saveName}.preg");
        }

        [HarmonyPatch(typeof(SaveLoadManager), nameof(SaveLoadManager.saveGame))]
        [HarmonyPostfix]
        private static void SaveData(string s)
        {
            var save = PreggoManager.Instance.Save;
            if (save == null)
            {
                PortalsOfPreggoPlugin.Instance.Log.LogWarning($"Preggo: SaveData is null, can't save");
                return;
            }

            var path = GetSavePath(s);
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllBytes(path, save.PrepareForSave().ByteSerialize());
        }

        [HarmonyPatch(typeof(SaveLoadManager), nameof(SaveLoadManager.loadGame))]
        [HarmonyPostfix]
        private static void LoadData(string s)
        {
            var path = GetSavePath(s);
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            if (!System.IO.File.Exists(path))
            {
                PortalsOfPreggoPlugin.Instance.Log.LogInfo("Creating new preggo save...");
                PreggoManager.Instance.Save = PreggoSave.Create();
            }
            else
            {
                PortalsOfPreggoPlugin.Instance.Log.LogInfo("Loading save from file...");
                PreggoManager.Instance.Save = System.IO.File.ReadAllBytes(path)
                    .ByteDeserialize<PreggoSave>()
                    .ReadSaveData();
            }
            PreggoManager.Instance.InitSave();
        }

    }
}
