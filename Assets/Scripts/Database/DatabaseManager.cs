using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using CompanionUnity.Models;

namespace CompanionUnity.Database
{
    public class DatabaseManager : MonoBehaviour
    {
        private static DatabaseManager _instance;
        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DatabaseManager");
                    _instance = go.AddComponent<DatabaseManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private string databasePath;
        
        // In-memory storage for now (will be replaced with SQLite later)
        private List<Queue> queues = new List<Queue>();
        private List<Item> items = new List<Item>();
        private List<ItemImage> itemImages = new List<ItemImage>();
        
        private long nextQueueId = 1;
        private long nextItemId = 1;
        private long nextImageId = 1;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            databasePath = Path.Combine(Application.persistentDataPath, "companion.db");
            Debug.Log($"Database path: {databasePath}");
            
            // Load data from PlayerPrefs for now
            LoadData();
        }

        #region Queue Operations

        public Queue CreateQueue(string name)
        {
            var queue = new Queue(name) { id = nextQueueId++ };
            queues.Add(queue);
            SaveData();
            return queue;
        }

        public List<Queue> GetAllQueues()
        {
            return new List<Queue>(queues);
        }

        public Queue GetQueueById(long id)
        {
            return queues.FirstOrDefault(q => q.id == id);
        }

        public void UpdateQueue(Queue queue)
        {
            queue.updatedAt = DateTime.Now;
            var index = queues.FindIndex(q => q.id == queue.id);
            if (index >= 0)
            {
                queues[index] = queue;
                SaveData();
            }
        }

        public void DeleteQueue(long id)
        {
            queues.RemoveAll(q => q.id == id);
            items.RemoveAll(i => i.queueId == id);
            // Also remove images for items in this queue
            var itemIds = items.Where(i => i.queueId == id).Select(i => i.id).ToList();
            itemImages.RemoveAll(img => itemIds.Contains(img.itemId));
            SaveData();
        }

        public int GetQueueItemCount(long queueId)
        {
            return items.Count(i => i.queueId == queueId);
        }

        public int GetQueueImageCount(long queueId)
        {
            var itemIds = items.Where(i => i.queueId == queueId).Select(i => i.id).ToList();
            return itemImages.Count(img => itemIds.Contains(img.itemId));
        }

        #endregion

        #region Item Operations

        public Item CreateItem(long queueId, string name, string description = null)
        {
            var item = new Item(queueId, name, description) { id = nextItemId++ };
            items.Add(item);
            SaveData();
            return item;
        }

        public List<Item> GetItemsByQueueId(long queueId)
        {
            return items.Where(i => i.queueId == queueId).ToList();
        }

        public Item GetItemById(long id)
        {
            return items.FirstOrDefault(i => i.id == id);
        }

        public void UpdateItem(Item item)
        {
            item.updatedAt = DateTime.Now;
            var index = items.FindIndex(i => i.id == item.id);
            if (index >= 0)
            {
                items[index] = item;
                SaveData();
            }
        }

        public void DeleteItem(long id)
        {
            items.RemoveAll(i => i.id == id);
            itemImages.RemoveAll(img => img.itemId == id);
            SaveData();
        }

        #endregion

        #region ItemImage Operations

        public ItemImage AddImageToItem(long itemId, string imagePath, int orderIndex = 0)
        {
            var image = new ItemImage(itemId, imagePath, orderIndex) { id = nextImageId++ };
            itemImages.Add(image);
            SaveData();
            return image;
        }

        public List<ItemImage> GetImagesByItemId(long itemId)
        {
            return itemImages.Where(img => img.itemId == itemId)
                           .OrderBy(img => img.orderIndex)
                           .ToList();
        }

        public void DeleteImage(long id)
        {
            itemImages.RemoveAll(img => img.id == id);
            SaveData();
        }

        #endregion

        #region Data Persistence

        private void SaveData()
        {
            // Save to PlayerPrefs as JSON for now
            PlayerPrefs.SetString("queues", JsonUtility.ToJson(new SerializableList<Queue>(queues)));
            PlayerPrefs.SetString("items", JsonUtility.ToJson(new SerializableList<Item>(items)));
            PlayerPrefs.SetString("itemImages", JsonUtility.ToJson(new SerializableList<ItemImage>(itemImages)));
            
            PlayerPrefs.SetString("nextQueueId", nextQueueId.ToString());
            PlayerPrefs.SetString("nextItemId", nextItemId.ToString());
            PlayerPrefs.SetString("nextImageId", nextImageId.ToString());
            
            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            if (PlayerPrefs.HasKey("queues"))
            {
                var queueData = PlayerPrefs.GetString("queues");
                queues = JsonUtility.FromJson<SerializableList<Queue>>(queueData).items ?? new List<Queue>();
            }

            if (PlayerPrefs.HasKey("items"))
            {
                var itemData = PlayerPrefs.GetString("items");
                items = JsonUtility.FromJson<SerializableList<Item>>(itemData).items ?? new List<Item>();
            }

            if (PlayerPrefs.HasKey("itemImages"))
            {
                var imageData = PlayerPrefs.GetString("itemImages");
                itemImages = JsonUtility.FromJson<SerializableList<ItemImage>>(imageData).items ?? new List<ItemImage>();
            }

            if (PlayerPrefs.HasKey("nextQueueId"))
                long.TryParse(PlayerPrefs.GetString("nextQueueId"), out nextQueueId);
                
            if (PlayerPrefs.HasKey("nextItemId"))
                long.TryParse(PlayerPrefs.GetString("nextItemId"), out nextItemId);
                
            if (PlayerPrefs.HasKey("nextImageId"))
                long.TryParse(PlayerPrefs.GetString("nextImageId"), out nextImageId);
        }

        #endregion

        [Serializable]
        private class SerializableList<T>
        {
            public List<T> items;

            public SerializableList(List<T> list)
            {
                items = list;
            }
        }
    }
}