using System;

namespace ElectionApp.Core
{
    public abstract class Entity<T> : EntityBase<T>
    {
        public DateTime CreatedAt { get; set; }
    }

    public abstract class EntityBase<T>
    {
        public T Id { get; set; }
    }
}