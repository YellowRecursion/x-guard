using Newtonsoft.Json;

namespace XGuardLibrary
{
    public abstract class SaveableData
    {
        [JsonIgnore]
        public abstract string Filename { get; }

        [JsonIgnore]
        public string FilePath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Filename);
            }
        }

        public bool IsFileExists() => File.Exists(FilePath);

        public void Load()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                JsonConvert.PopulateObject(json, this);
            }
            else
            {
                Save();
            }
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
    }
}
