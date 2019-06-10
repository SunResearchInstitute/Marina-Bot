using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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
            List<ulong> FileData = JsonConvert.DeserializeObject<List<ulong>>(System.IO.File.ReadAllText(File.FullName));
            if (FileData != null) Data = FileData;
            else Data = new List<ulong>();
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public UlongSaveFile(FileInfo file) : base(file) { }
    }

    public class UlongStringSaveFile : SaveFile<List<UlongString>>
    {
        public override void Read()
        {
            List<UlongString> FileData = JsonConvert.DeserializeObject<List<UlongString>>(System.IO.File.ReadAllText(File.FullName));
            if (FileData != null) Data = FileData;
            else Data = new List<UlongString>();
        }
        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public UlongStringSaveFile(FileInfo file) : base(file) { }
    }

    public class FilterSaveFile : SaveFile<Dictionary<ulong, FilterData>>
    {
        public override void Read()
        {
            Dictionary<ulong, FilterData> FileData = JsonConvert.DeserializeObject<Dictionary<ulong, FilterData>>(System.IO.File.ReadAllText(File.FullName));
            if (FileData != null) Data = FileData;
            else Data = new Dictionary<ulong, FilterData>();
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public FilterSaveFile(FileInfo file) : base(file) { }
    }

    public class TrackerSaveFile : SaveFile<Dictionary<ulong, TrackerData>>
    {
        public override void Read()
        {
            Dictionary<ulong, TrackerData> FileData = JsonConvert.DeserializeObject<Dictionary<ulong, TrackerData>>(System.IO.File.ReadAllText(File.FullName));
            if (FileData != null) Data = FileData;
            else Data = new Dictionary<ulong, TrackerData>();
            //manually update time
            foreach (TrackerData data in Data.Values)
                data.dt = DateTime.Now;
        }

        public override void Write() => System.IO.File.WriteAllText(File.FullName, JsonConvert.SerializeObject(Data));

        public TrackerSaveFile(FileInfo file) : base(file) { }
    }

    //Server: User to Warns
    public class WarnSaveFile : SaveFile<Dictionary<ulong, Dictionary<ulong, List<WarnData>>>>
    {
        public override void Read()
        {
            Dictionary<ulong, Dictionary<ulong, List<WarnData>>> FileData = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<ulong, List<WarnData>>>>(System.IO.File.ReadAllText(File.FullName));
            if (FileData != null) Data = FileData;
            else Data = new Dictionary<ulong, Dictionary<ulong, List<WarnData>>>();
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
        public ulong Issuer;
        public DateTime Time;
        public string Reason;

        public WarnData(DateTime dt, string reasoning, ulong user)
        {
            Time = dt;
            Reason = reasoning;
            Issuer = user;
        }
    }
}
