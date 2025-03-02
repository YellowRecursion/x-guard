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

        public string Id { get; set; }
        public string Name { get; set; }
        public string? Data { get; set; }

        public override string ToString()
        {
            return $"PipeMessage '{Name}' [ID:{Id}]";
        }
    }
}
