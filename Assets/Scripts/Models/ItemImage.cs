using System;
using UnityEngine;

namespace CompanionUnity.Models
{
    [Serializable]
    public class ItemImage
    {
        public long id;
        public long itemId;
        public string imagePath;
        public int orderIndex;
        public DateTime createdAt;
        
        // For Unity, we'll also store the texture
        [NonSerialized]
        public Texture2D texture;

        public ItemImage()
        {
            id = 0;
            itemId = 0;
            imagePath = "";
            orderIndex = 0;
            createdAt = DateTime.Now;
            texture = null;
        }

        public ItemImage(long itemId, string imagePath, int orderIndex = 0)
        {
            this.id = 0;
            this.itemId = itemId;
            this.imagePath = imagePath;
            this.orderIndex = orderIndex;
            this.createdAt = DateTime.Now;
            this.texture = null;
        }
    }
}