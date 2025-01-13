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
                FileSafetyManager.AddFileToIgnoreList(FilePath);
                var json = File.ReadAllText(FilePath);
                FileSafetyManager.RemoveFileFromIgnoreList(FilePath);
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
            FileSafetyManager.AddFileToIgnoreList(FilePath);
            File.WriteAllText(FilePath, json);
            FileSafetyManager.RemoveFileFromIgnoreList(FilePath);
        }
    }
}
