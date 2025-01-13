using Newtonsoft.Json;

namespace XGuard
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
                return Path.Combine(Program.CurrentDirectory, Filename);
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
        }
    }
}
