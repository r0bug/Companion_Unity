using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using CompanionUnity.Models;
using CompanionUnity.Database;

namespace CompanionUnity.Utils
{
    [Serializable]
    public class ExportQueue
    {
        public long id;
        public string name;
        public string createdAt;
        public string updatedAt;
        public bool isSynced;
        public List<ExportItem> items;
    }

    [Serializable]
    public class ExportItem
    {
        public long id;
        public string name;
        public string description;
        public string createdAt;
        public string updatedAt;
        public List<ExportImage> images;
    }

    [Serializable]
    public class ExportImage
    {
        public long id;
        public string imagePath;
        public int orderIndex;
    }

    [Serializable]
    public class ExportData
    {
        public string version = "2.0";
        public string exportDate;
        public Dictionary<string, string> deviceInfo;
        public List<ExportQueue> queues;
    }

    public static class QueueExporter
    {
        private static string DateToString(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
        }

        public static string ExportAllQueues()
        {
            var db = DatabaseManager.Instance;
            var queues = db.GetAllQueues();

            var exportQueues = new List<ExportQueue>();

            foreach (var queue in queues)
            {
                var items = db.GetItemsByQueueId(queue.id);
                var exportItems = new List<ExportItem>();

                foreach (var item in items)
                {
                    var images = db.GetImagesByItemId(item.id);
                    var exportImages = images.Select(img => new ExportImage
                    {
                        id = img.id,
                        imagePath = img.imagePath,
                        orderIndex = img.orderIndex
                    }).ToList();

                    exportItems.Add(new ExportItem
                    {
                        id = item.id,
                        name = item.name,
                        description = item.description,
                        createdAt = DateToString(item.createdAt),
                        updatedAt = DateToString(item.updatedAt),
                        images = exportImages
                    });
                }

                exportQueues.Add(new ExportQueue
                {
                    id = queue.id,
                    name = queue.name,
                    createdAt = DateToString(queue.createdAt),
                    updatedAt = DateToString(queue.updatedAt),
                    isSynced = queue.isSynced,
                    items = exportItems
                });
            }

            var exportData = new ExportData
            {
                exportDate = DateToString(DateTime.Now),
                deviceInfo = GetDeviceInfo(),
                queues = exportQueues
            };

            return SaveExportData(exportData, "ebaytools_export");
        }

        public static string ExportQueue(long queueId)
        {
            var db = DatabaseManager.Instance;
            var queue = db.GetQueueById(queueId);
            
            if (queue == null)
            {
                Debug.LogError($"Queue with id {queueId} not found");
                return null;
            }

            var items = db.GetItemsByQueueId(queue.id);
            var exportItems = new List<ExportItem>();

            foreach (var item in items)
            {
                var images = db.GetImagesByItemId(item.id);
                var exportImages = images.Select(img => new ExportImage
                {
                    id = img.id,
                    imagePath = img.imagePath,
                    orderIndex = img.orderIndex
                }).ToList();

                exportItems.Add(new ExportItem
                {
                    id = item.id,
                    name = item.name,
                    description = item.description,
                    createdAt = DateToString(item.createdAt),
                    updatedAt = DateToString(item.updatedAt),
                    images = exportImages
                });
            }

            var exportQueue = new ExportQueue
            {
                id = queue.id,
                name = queue.name,
                createdAt = DateToString(queue.createdAt),
                updatedAt = DateToString(queue.updatedAt),
                isSynced = queue.isSynced,
                items = exportItems
            };

            var exportData = new ExportData
            {
                exportDate = DateToString(DateTime.Now),
                deviceInfo = GetDeviceInfo(),
                queues = new List<ExportQueue> { exportQueue }
            };

            return SaveExportData(exportData, $"ebaytools_queue_{queue.name}");
        }

        private static Dictionary<string, string> GetDeviceInfo()
        {
            return new Dictionary<string, string>
            {
                { "manufacturer", SystemInfo.deviceName },
                { "model", SystemInfo.deviceModel },
                { "platform", Application.platform.ToString() },
                { "unityVersion", Application.unityVersion },
                { "systemMemory", SystemInfo.systemMemorySize.ToString() + " MB" }
            };
        }

        private static string SaveExportData(ExportData data, string filePrefix)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                
                // Create export directory
                string exportDir = Path.Combine(Application.persistentDataPath, "exports");
                if (!Directory.Exists(exportDir))
                {
                    Directory.CreateDirectory(exportDir);
                }

                // Generate filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filename = $"{filePrefix}_{timestamp}.json";
                string filePath = Path.Combine(exportDir, filename);

                // Write to file
                File.WriteAllText(filePath, json);

                Debug.Log($"Export saved to: {filePath}");
                return filePath;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to export data: {e.Message}");
                return null;
            }
        }

        public static List<string> GetExportedFiles()
        {
            string exportDir = Path.Combine(Application.persistentDataPath, "exports");
            if (!Directory.Exists(exportDir))
            {
                return new List<string>();
            }

            return Directory.GetFiles(exportDir, "*.json")
                          .OrderByDescending(f => File.GetCreationTime(f))
                          .ToList();
        }

        public static void DeleteExportFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Deleted export file: {filePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete export file: {e.Message}");
            }
        }
    }
}