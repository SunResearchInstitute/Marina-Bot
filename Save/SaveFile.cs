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
            if (JsonConvert.DeserializeObject(System.IO.File.ReadAllText(File.FullName), typeof(List<ulong>)) is List<ulong> FileData) Data = FileData;
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public UlongSaveFile(FileInfo file) : base(file) { }
    }

    public class UlongStringSaveFile : SaveFile<List<UlongString>>
    {
        public override void Read()
        {
            Data = new List<UlongString>();
            if (JsonConvert.DeserializeObject(System.IO.File.ReadAllText(File.FullName), typeof(List<UlongString>)) is List<UlongString> FileData) Data = FileData;
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public UlongStringSaveFile(FileInfo file) : base(file) { }
    }

    public class FilterSaveFile : SaveFile<Dictionary<ulong, FilterData>>
    {
        public override void Read()
        {
            Data = new Dictionary<ulong, FilterData>();
            if (JsonConvert.DeserializeObject(System.IO.File.ReadAllText(File.FullName), typeof(Dictionary<ulong, FilterData>)) is Dictionary<ulong, FilterData> FileData) Data = FileData;
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public FilterSaveFile(FileInfo file) : base(file) { }
    }

    public class TrackerSaveFile : SaveFile<Dictionary<ulong, TrackerData>>
    {
        public override void Read()
        {
            Data = new Dictionary<ulong, TrackerData>();
            if (JsonConvert.DeserializeObject(System.IO.File.ReadAllText(File.FullName), typeof(Dictionary<ulong, TrackerData>)) is Dictionary<ulong, TrackerData> FileData) Data = FileData;
            //manually update time
            foreach (TrackerData data in Data.Values)
                data.dt = DateTime.Now;
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public TrackerSaveFile(FileInfo file) : base(file) { }
    }

    public class WarnSaveFile : SaveFile<Dictionary<ulong, Dictionary<ulong, WarnData>>>
    {
        public override void Read()
        {
            Data = new Dictionary<ulong, Dictionary<ulong, WarnData>>();
            if (JsonConvert.DeserializeObject(System.IO.File.ReadAllText(File.FullName), typeof(Dictionary<ulong, Dictionary<ulong, WarnData>>)) is Dictionary<ulong, Dictionary<ulong, WarnData>> FileData) Data = FileData;
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public WarnSaveFile(FileInfo file) : base(file) { }
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

    public class FilterData
    {
        public List<string> Words;
        public bool IsEnabled = true;

        public FilterData(List<string> list) => Words = list;
    }

    public class TrackerData
    {
        public DateTime dt;
        public TimeSpan ts;
        public string DmReason = null;
        public bool IsTrackerEnabled;
        public bool IsAlertEnabled;

        public TrackerData(DateTime date, TimeSpan time, bool tracker)
        {
            dt = date;
            ts = time;
            IsTrackerEnabled = tracker;
            IsAlertEnabled = false;
        }
    }

    public class WarnData
    {
        public DateTime time;
        public string Reason;

        public WarnData( string reasoning)
        {
            time = DateTime.Now;
            Reason = reasoning;
        }
    }
}