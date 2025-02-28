namespace XGuardLibrary.Models.Pipes
{
    [Serializable]
    public class PipeMessage
    {
        public PipeMessage()
        {
            Id = Guid.NewGuid().ToString();
        }

        public PipeMessage(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }
        public string Name { get; set; }
        public object Data { get; set; }
    }
}
