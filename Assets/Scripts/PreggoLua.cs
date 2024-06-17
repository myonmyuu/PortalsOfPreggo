using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

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

            GlobalTable["log"]      = new Action<string>(PortalsOfPreggoPlugin.Instance.Log.LogInfo);
            GlobalTable["lerror"]   = new Action<string>(PortalsOfPreggoPlugin.Instance.Log.LogError);
            GlobalTable["require"]  = new Func<string, object>((file) =>
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

            GlobalTable["ftext"] = new Func<FText>(() => new FText());
            TypeTable   = new Table(GlobalScript);
            StaticTable = new Table(GlobalScript);
            NewTable    = new Table(GlobalScript);
            

            RegisterAssembly(typeof(MainCharacter).Assembly, "PoP");
            RegisterAssembly(typeof(PortalsOfPreggoPlugin).Assembly, "Preggo");
            RegisterAssembly(typeof(Delegate).Assembly, "System");
            RegisterAssembly(typeof(UnityEngine.Vector3).Assembly, "UnityEngine");
            RegisterAssembly(typeof(TMP_Text).Assembly, "UnityEngine");
            RegisterAssembly(typeof(UnityEngine.UI.Button).Assembly, "UnityEngine");

            GlobalTable["_types"]       = TypeTable;
            GlobalTable["_static"]      = StaticTable;
            GlobalTable["new"]          = NewTable;
            GlobalTable["_construct"]   = MakeCallback((ConstructDelegate)Construct);
            GlobalTable["_cb"]          = new Func<Closure, Action>(cl => () => { cl.Call(); });

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
                DataType.Function,
                typeof(UnityEngine.Events.UnityAction),
                dv => (UnityAction)(() => dv.Function.Call())
            );

            RunFile("setup");
        }

        public delegate object ConstructDelegate(Type t, params object[] args);
        public object Construct(Type t, params object[] args)
        {
            return Activator.CreateInstance(t, args);
        }

        public void RegisterAssembly(Assembly assembly, string prefix = "")
        {
            foreach (var t in assembly.GetTypes())
            {
                TypeTable[$"{prefix}.{t.Name}"]     = t;
                StaticTable[$"{prefix}.{t.Name}"]   = UserData.CreateStatic(t);
                NewTable[$"{prefix}.{t.Name}"]      = new Func<object>(() => Activator.CreateInstance(t));
            }
        }

        public void AddLuaPath(string path)
        {
            ExtraPaths.Add(path);
        }

        private string LuaPath = System.IO.Path.Combine(PortalsOfPreggoPlugin.Instance.Path, "scripts");
        private List<string> ExtraPaths = new List<string>();
        private MoonSharp.Interpreter.Script GlobalScript;
        private MoonSharp.Interpreter.Table GlobalTable;

        private MoonSharp.Interpreter.Table TypeTable;
        private MoonSharp.Interpreter.Table StaticTable;
        private MoonSharp.Interpreter.Table NewTable;

        public DynValue MakeCallback(Delegate d)
        {
            return DynValue.NewCallback(CallbackFunction.FromDelegate(GlobalScript, d));
        }

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
            foreach (var path in ExtraPaths.Append(LuaPath))
            {
                var finalpath = System.IO.Path.Combine(path, file + ".lua");

                if (!System.IO.File.Exists(finalpath))
                {
                    continue;
                }

                return Run(System.IO.File.ReadAllText(finalpath));
            }
            UnityEngine.Debug.LogWarning($"unable to find lua file {file}");
            return MoonSharp.Interpreter.DynValue.NewNil();
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
