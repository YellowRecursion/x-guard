namespace XGuard
{
    public class UserConfig : SaveableData
    {
        public override string Filename => "user-config.json";

        public string TerminationToken { get; set; } = TerminationModule.TERMINATION_TOKEN;
        public string BotToken { get; set; } = string.Empty;
        public HashSet<long> ModeratorUserIds { get; private set; } = new HashSet<long>() { 0 };
    }
}
