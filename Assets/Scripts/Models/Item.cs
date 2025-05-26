using System;
using UnityEngine;

namespace CompanionUnity.Models
{
    [Serializable]
    public class Item
    {
        public long id;
        public long queueId;
        public string name;
        public string description;
        public DateTime createdAt;
        public DateTime updatedAt;

        public Item()
        {
            id = 0;
            queueId = 0;
            name = "";
            description = null;
            createdAt = DateTime.Now;
            updatedAt = DateTime.Now;
        }

        public Item(long queueId, string name, string description = null)
        {
            this.id = 0;
            this.queueId = queueId;
            this.name = name;
            this.description = description;
            this.createdAt = DateTime.Now;
            this.updatedAt = DateTime.Now;
        }
    }
}