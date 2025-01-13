namespace XGuard
{
    public class ProgramData : SaveableData
    {
        public override string Filename => "data.json";

        public string TerminationTokenHash { get; set; }
        public string BotToken { get; set; } = string.Empty;
        public List<long> ModeratorUserIds { get; set; } = new List<long>();
        public List<long> ChatIds { get; private set; } = new List<long>();
    }
}
