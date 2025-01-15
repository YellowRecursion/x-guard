namespace XGuardLibrary
{
    public class ProgramData : SaveableData
    {
        public override string Filename => "data.json";

        public string TerminationTokenHash { get; set; }
        public string BotToken { get; set; } = string.Empty;
        public HashSet<long> ModeratorUserIds { get; set; } = new HashSet<long>();
        public HashSet<long> ChatIds { get; private set; } = new HashSet<long>();
    }
}
