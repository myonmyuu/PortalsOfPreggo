using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace PortalsOfPreggoMain.Content
{
    public static class PreggoSaveManager
    {
        public static byte[] ByteSerialize(this object o)
        {
            byte[] bytes;
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, o);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        public static T ByteDeserialize<T>(this byte[] bytes)
        {
            T result;
            using (var stream = new MemoryStream(bytes))
            {
                result = (T)new BinaryFormatter().Deserialize(stream);
            }
            return result;
        }
    }

    [Serializable]
    public class PreggoSave
    {
        public PreggoData MC;
        public Dictionary<string, PreggoData> PlayerChars;
        public Dictionary<UniqueCharacter, PreggoData> NPCs;
        public Dictionary<string, (float, float)> WindowOffsets;

        public static PreggoSave Create()
        {
            var save = new PreggoSave
            {
                PlayerChars = new Dictionary<string, PreggoData>(),
                NPCs = new Dictionary<UniqueCharacter, PreggoData>(),
                WindowOffsets = new Dictionary<string, (float, float)>()
            };
            return save;
        }

        public PreggoSave PrepareForSave()
        {
            MC?.PrepareForSave();
            foreach (var (_, d) in PlayerChars)
                d?.PrepareForSave();
            foreach (var (_, d) in NPCs)
                d?.PrepareForSave();
            if (WindowOffsets == null)
                WindowOffsets = new Dictionary<string, (float, float)>();
            return this;
        }

        public PreggoSave ReadSaveData()
        {
            MC?.ReadData();
            foreach (var (_, d) in PlayerChars)
                d?.ReadData();
            foreach (var (_, d) in NPCs)
                d?.ReadData();
            if (WindowOffsets == null)
                WindowOffsets = new Dictionary<string, (float, float)>();
            return this;
        }
    }
}
