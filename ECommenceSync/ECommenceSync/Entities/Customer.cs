using ECommenceSync.Interfaces;
using System;

namespace ECommenceSync.Entities
{
    public class Customer<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public string Name { get; set; }
        public bool Updated { get; set; }
        public int RetryCount { get; set; }
        public bool Deleted { get; set; }
        public string LastName { get; set; }
        public string Firstname { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public TKey IdGender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Newsletter { get; set; }
        public int MaxPaymentDays { get; set; }
        public bool Active { get; set; }
        public bool IsGuest { get; set; }

    }
}
