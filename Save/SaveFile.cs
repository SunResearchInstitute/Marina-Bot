using System.Collections.Generic;
using System.IO;
using System;

namespace RK800.Save
{

    public abstract class SaveFile<T> : ISaveFile
    {
        public T SaveData;
        public SaveFile(FileInfo file) : base(file) { }
    }

    public class UlongSaveFile : SaveFile<List<ulong>>
    {
        public override void Read()
        {
            SaveData = new List<ulong>();
            using (StreamReader reader = new StreamReader(Open()))
            {
                string s;
                while ((s = reader.ReadLine()) != null) SaveData.Add(ulong.Parse(s));
            }
        }

        public override void Write()
        {
            if (File.Length > 0)
            {
                File.Delete();
                File.Create().Close();
            }

            using (StreamWriter writer = new StreamWriter(Open()))
                foreach (ulong value in SaveData)
                    writer.WriteLine($"{value}");
        }

        public UlongSaveFile(FileInfo file) : base(file) { }
    }
    public class UlongStringSaveFile : SaveFile<List<KeyValuePair<ulong, string>>>
    {
        public override void Read()
        {
            SaveData = new List<KeyValuePair<ulong, string>>();
            using (StreamReader reader = new StreamReader(Open()))
            {
                string s;
                while ((s = reader.ReadLine()) != null)
                {
                    string[] split = s.Split(": ");
                    SaveData.Add(new KeyValuePair<ulong, string>(ulong.Parse(split[0]), split[1]));
                }
            }
        }

        public override void Write()
        {
            if (File.Length > 0)
            {
                File.Delete();
                File.Create().Close();
            }

            using (StreamWriter writer = new StreamWriter(Open()))
                foreach (KeyValuePair<ulong, string> pair in SaveData)
                    writer.WriteLine($"{pair.Key}: {pair.Value}");
        }
        public UlongStringSaveFile(FileInfo file) : base(file) { }
    }

    public class UlongTimeSpanSaveFile : SaveFile<Dictionary<ulong, TimeSpan>>
    {
        public override void Read()
        {
            SaveData = new Dictionary<ulong, TimeSpan>();
            using (StreamReader reader = new StreamReader(Open()))
            {
                string s;
                while ((s = reader.ReadLine()) != null)
                {
                    string[] split = s.Split(": ");
                    SaveData.Add(ulong.Parse(split[0]), TimeSpan.Parse(split[1]));
                }
            }
        }

        public override void Write()
        {
            if (File.Length > 0)
            {
                File.Delete();
                File.Create().Close();
            }

            using (StreamWriter writer = new StreamWriter(Open()))
                foreach (KeyValuePair<ulong, TimeSpan> pair in SaveData)
                    writer.WriteLine($"{pair.Key}: {pair.Value}");
        }
        public UlongTimeSpanSaveFile(FileInfo file) : base(file) { }
    }

    public class TrackerSaveFile : SaveFile<Dictionary<ulong, DateTime>>
    {
        public override void Read()
        {
            SaveData = new Dictionary<ulong, DateTime>();
            using (StreamReader reader = new StreamReader(Open()))
            {
                string s;
                while ((s = reader.ReadLine()) != null) SaveData.Add(ulong.Parse(s), DateTime.Now);
            }
        }

        public override void Write()
        {
            if (File.Length > 0)
            {
                File.Delete();
                File.Create().Close();
            }

            using (StreamWriter writer = new StreamWriter(Open()))
                foreach (ulong value in SaveData.Keys)
                    writer.WriteLine($"{value}");
        }

        public TrackerSaveFile(FileInfo file) : base(file) { }
    }

}