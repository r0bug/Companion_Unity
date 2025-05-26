using System;
using UnityEngine;

namespace CompanionUnity.Models
{
    [Serializable]
    public class Queue
    {
        public long id;
        public string name;
        public DateTime createdAt;
        public DateTime updatedAt;
        public bool isSynced;
        public DateTime? lastSyncedAt;

        public Queue()
        {
            id = 0;
            name = "";
            createdAt = DateTime.Now;
            updatedAt = DateTime.Now;
            isSynced = false;
            lastSyncedAt = null;
        }

        public Queue(string name)
        {
            this.id = 0;
            this.name = name;
            this.createdAt = DateTime.Now;
            this.updatedAt = DateTime.Now;
            this.isSynced = false;
            this.lastSyncedAt = null;
        }
    }
}