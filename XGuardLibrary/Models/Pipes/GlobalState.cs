namespace XGuardLibrary.Models.Pipes
{
    [Serializable]
    public class GlobalState
    {
        public int LockScreenTimer { get; set; } = -1;
        public bool Locked => LockScreenTimer > 0;
    }
}
