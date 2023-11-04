using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalsOfPreggoMain.Content
{
    public class PreggoLua
    {
        private static Lazy<PreggoLua> _Instance = new Lazy<PreggoLua>(() => PortalsOfPreggoPlugin.Instance.Lua);
        public static PreggoLua Instance => _Instance.Value;

        public PreggoLua()
        {
            GlobalScript = new MoonSharp.Interpreter.Script();
            GlobalTable = GlobalScript.Globals;
            //GlobalTable = new MoonSharp.Interpreter.Table(GlobalScript);

            GlobalTable["log"] = new Action<string>(e => PortalsOfPreggoPlugin.Instance.Log.LogInfo(e));
            GlobalTable["get"] = new Func<string, object>((file) =>
            {
                switch (file)
                {
                    case nameof(CharaInfo):
                        return new CharaInfo(CharacterManager.instance?.GetCurrent());
                    case "PreggoPlugin":
                        return PortalsOfPreggoPlugin.Instance;
                    case "Preggo":
                        return PreggoManager.Instance;
                    case "CharacterManager":
                        return CharacterManager.instance;
                    case "Town":
                        return TownManager.instance;
                    case "Save":
                        return SaveController.instance;
                }
                return RunFile(file);
            });
            GlobalTable["require"] = GlobalTable["get"];
            GlobalTable["ftext"] = new Func<FText>(() => new FText());
        }

        private string LuaPath = System.IO.Path.Combine(PortalsOfPreggoPlugin.Instance.Path, "scripts");
        private MoonSharp.Interpreter.Script GlobalScript;
        private MoonSharp.Interpreter.Table GlobalTable;
        public DynValue Run(string lua)
        {
            try
            {
                return GlobalScript.DoString(lua, GlobalTable);
            }
            catch (MoonSharp.Interpreter.SyntaxErrorException e)
            {
                UnityEngine.Debug.LogException(e);
                UnityEngine.Debug.LogError($"call stack: {e.CallStack}");
                UnityEngine.Debug.LogError($"lua file content:\n{lua}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            return MoonSharp.Interpreter.DynValue.NewNil();
        }

        public string RunFileAsString(string file, params object[] args)
        {
            if (args != null)
            {
                var table = DynValue.NewTable(GlobalScript);
                for (int i = 0; i < args.Length; i++)
                {
                    table.Table[i + 1] = args[i];
                }
                this["args"] = table;
            }
            var res = RunFile(file);
            return res.ToObject<string>();
        }

        public object this[object key]
        {
            set => GlobalTable[key] = value;
        }

        public DynValue RunFile(string file)
        {
            var path = System.IO.Path.Combine(LuaPath, file + ".lua");

            if (!System.IO.File.Exists(path))
            {
                UnityEngine.Debug.LogWarning($"unable to find lua file {path}");
                return MoonSharp.Interpreter.DynValue.NewNil();
            }

            return Run(System.IO.File.ReadAllText(path));
        }
    }


    public struct CharaInfo
    {
        public CharaInfo(Stats cust)
        {
            var man = NPCManager.instance;
            Sylvie = man?.getNpc(UniqueCharacter.Sylvie)?.stats;
            Castalia = man?.getNpc(UniqueCharacter.Castalia)?.stats;
            Flora = man?.getNpc(UniqueCharacter.Flora)?.stats;
            Lumira = man?.getNpc(UniqueCharacter.Lumira)?.stats;
            Mya = man?.getNpc(UniqueCharacter.Mya)?.stats;
            Aila = man?.getNpc(UniqueCharacter.Aila)?.stats;
            Player = SaveController.instance.mainCharacter.combatForm;
            Other = cust;
        }

        public Stats Sylvie;
        public Stats Castalia;
        public Stats Flora;
        public Stats Lumira;
        public Stats Mya;
        public Stats Aila;

        public Stats Player;
        public Stats Other;
    }
}
