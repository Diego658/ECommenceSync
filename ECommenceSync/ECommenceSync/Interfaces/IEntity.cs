namespace ECommenceSync.Interfaces
{
    public interface IEntity<TKey> where TKey : struct
    {
        public TKey Id { get; set; }
        public string Name { get; set; }
        public bool Updated { get; set; }
        public int RetryCount { get; set; }
    }
}
