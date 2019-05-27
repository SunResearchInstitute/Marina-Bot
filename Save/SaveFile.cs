using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;

namespace RK800.Save
{

    public abstract class SaveFile<T> : ISaveFile
    {
        public T Data;
        public SaveFile(FileInfo file) : base(file) { }
    }

    public class UlongSaveFile : SaveFile<List<ulong>>
    {
        public override void Read()
        {
            Data = new List<ulong>();
            List<ulong> FileData = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(File.FullName)) as List<ulong>;
            if (FileData != null) Data = FileData; 
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public UlongSaveFile(FileInfo file) : base(file) { }
    }
    public class UlongStringSaveFile : SaveFile<List<UlongString>>
    {
        public override void Read()
        {
            Data = new List<UlongString>();
            List<UlongString> FileData = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(File.FullName)) as List<UlongString>;
            if (FileData != null) Data = FileData; 
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public UlongStringSaveFile(FileInfo file) : base(file) { }
    }

    public class TrackerSaveFile : SaveFile<Dictionary<ulong, TrackerData>>
    {
        public override void Read()
        {
            Data = new Dictionary<ulong, TrackerData>();
            Dictionary<ulong, TrackerData> FileData = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(File.FullName)) as Dictionary<ulong, TrackerData>;
            if (FileData != null) Data = FileData; 
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public TrackerSaveFile(FileInfo file) : base(file) { }
    }

    public class UlongString
    {
        public ulong ul;
        public string str;


        public UlongString(ulong u64, string s)
        {
            ul = u64;
            str = s;
        }
    }

    public class TrackerData
    {
        public DateTime dt;
        public TimeSpan ts;
        public string str;
        public bool IsTrackerEnabled;
        public bool IsAlertEnabled;

        public TrackerData(DateTime date, TimeSpan time, string s = null, bool tracker = false, bool alert = false)
        {
            dt = date;
            ts = time;
            str = s;
            IsTrackerEnabled = tracker;
            IsAlertEnabled = alert;
        }
    }
}