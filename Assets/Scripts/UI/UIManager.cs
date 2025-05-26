using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CompanionUnity.Models;
using CompanionUnity.Database;
using CompanionUnity.Camera;
using CompanionUnity.Utils;

namespace CompanionUnity.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();
                }
                return _instance;
            }
        }

        [Header("Panels")]
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject queueSelectionPanel;
        [SerializeField] private GameObject queueListPanel;
        [SerializeField] private GameObject itemCapturePanel;

        [Header("Queue Selection UI")]
        [SerializeField] private InputField queueNameInput;
        [SerializeField] private Button createQueueButton;
        [SerializeField] private Button selectExistingButton;
        [SerializeField] private Text feedbackText;

        [Header("Queue List UI")]
        [SerializeField] private Transform queueListContent;
        [SerializeField] private GameObject queueItemPrefab;
        [SerializeField] private Button backToSelectionButton;

        [Header("Item Capture UI")]
        [SerializeField] private Text currentQueueNameText;
        [SerializeField] private InputField itemNameInput;
        [SerializeField] private Button capturePhotoButton;
        [SerializeField] private Button saveItemButton;
        [SerializeField] private Button exportQueueButton;
        [SerializeField] private Button backToQueuesButton;
        [SerializeField] private Text itemCountText;
        [SerializeField] private Text photoCountText;
        [SerializeField] private Transform photoThumbnailContainer;
        [SerializeField] private GameObject photoThumbnailPrefab;

        private Queue currentQueue;
        private Item currentItem;
        private List<string> currentPhotoPaths = new List<string>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Set up button listeners
            createQueueButton.onClick.AddListener(OnCreateQueueClicked);
            selectExistingButton.onClick.AddListener(OnSelectExistingClicked);
            backToSelectionButton.onClick.AddListener(OnBackToSelectionClicked);
            capturePhotoButton.onClick.AddListener(OnCapturePhotoClicked);
            saveItemButton.onClick.AddListener(OnSaveItemClicked);
            exportQueueButton.onClick.AddListener(OnExportQueueClicked);
            backToQueuesButton.onClick.AddListener(OnBackToQueuesClicked);

            // Subscribe to camera events
            CameraManager.Instance.OnPhotoTaken += OnPhotoTaken;
            CameraManager.Instance.OnCameraError += OnCameraError;

            // Start with home panel
            ShowPanel(PanelType.Home);
            
            // Auto-navigate to queue selection after a short delay
            Invoke(nameof(NavigateToQueueSelection), 0.5f);
        }

        private void NavigateToQueueSelection()
        {
            ShowPanel(PanelType.QueueSelection);
        }

        public enum PanelType
        {
            Home,
            QueueSelection,
            QueueList,
            ItemCapture
        }

        public void ShowPanel(PanelType panelType)
        {
            // Hide all panels
            homePanel.SetActive(false);
            queueSelectionPanel.SetActive(false);
            queueListPanel.SetActive(false);
            itemCapturePanel.SetActive(false);

            // Show selected panel
            switch (panelType)
            {
                case PanelType.Home:
                    homePanel.SetActive(true);
                    break;
                case PanelType.QueueSelection:
                    queueSelectionPanel.SetActive(true);
                    queueNameInput.text = "";
                    ShowFeedback("");
                    break;
                case PanelType.QueueList:
                    queueListPanel.SetActive(true);
                    RefreshQueueList();
                    break;
                case PanelType.ItemCapture:
                    itemCapturePanel.SetActive(true);
                    if (currentQueue != null)
                    {
                        currentQueueNameText.text = currentQueue.name;
                        UpdateItemCaptureStats();
                    }
                    CameraManager.Instance.StartCamera();
                    break;
            }
        }

        #region Queue Selection

        private void OnCreateQueueClicked()
        {
            string queueName = queueNameInput.text.Trim();
            
            if (string.IsNullOrEmpty(queueName))
            {
                ShowFeedback("Please enter a queue name", true);
                return;
            }

            // Create new queue
            currentQueue = DatabaseManager.Instance.CreateQueue(queueName);
            ShowFeedback($"Created queue: {queueName}", false);
            
            // Navigate to item capture
            ShowPanel(PanelType.ItemCapture);
        }

        private void OnSelectExistingClicked()
        {
            ShowPanel(PanelType.QueueList);
        }

        #endregion

        #region Queue List

        private void RefreshQueueList()
        {
            // Clear existing items
            foreach (Transform child in queueListContent)
            {
                Destroy(child.gameObject);
            }

            // Get all queues
            var queues = DatabaseManager.Instance.GetAllQueues();

            // Create UI items
            foreach (var queue in queues)
            {
                GameObject queueItem = Instantiate(queueItemPrefab, queueListContent);
                
                // Set queue info
                Text nameText = queueItem.transform.Find("NameText").GetComponent<Text>();
                Text statsText = queueItem.transform.Find("StatsText").GetComponent<Text>();
                Button selectButton = queueItem.GetComponent<Button>();

                nameText.text = queue.name;
                
                int itemCount = DatabaseManager.Instance.GetQueueItemCount(queue.id);
                int photoCount = DatabaseManager.Instance.GetQueueImageCount(queue.id);
                statsText.text = $"{itemCount} items • {photoCount} photos";

                // Set up button
                Queue capturedQueue = queue; // Capture for lambda
                selectButton.onClick.AddListener(() => OnQueueSelected(capturedQueue));
            }
        }

        private void OnQueueSelected(Queue queue)
        {
            currentQueue = queue;
            ShowPanel(PanelType.ItemCapture);
        }

        private void OnBackToSelectionClicked()
        {
            ShowPanel(PanelType.QueueSelection);
        }

        #endregion

        #region Item Capture

        private void OnCapturePhotoClicked()
        {
            if (string.IsNullOrEmpty(itemNameInput.text.Trim()))
            {
                ShowFeedback("Please enter item name first", true);
                return;
            }

            CameraManager.Instance.TakePhoto();
        }

        private void OnPhotoTaken(string photoPath)
        {
            currentPhotoPaths.Add(photoPath);
            
            // Create thumbnail
            GameObject thumbnail = Instantiate(photoThumbnailPrefab, photoThumbnailContainer);
            Text indexText = thumbnail.GetComponentInChildren<Text>();
            indexText.text = currentPhotoPaths.Count.ToString();
            
            UpdatePhotoCount();
        }

        private void OnCameraError(string error)
        {
            ShowFeedback($"Camera error: {error}", true);
        }

        private void OnSaveItemClicked()
        {
            string itemName = itemNameInput.text.Trim();
            
            if (string.IsNullOrEmpty(itemName))
            {
                ShowFeedback("Please enter item name", true);
                return;
            }

            if (currentPhotoPaths.Count == 0)
            {
                ShowFeedback("Please capture at least one photo", true);
                return;
            }

            // Create item
            currentItem = DatabaseManager.Instance.CreateItem(currentQueue.id, itemName);

            // Add photos
            for (int i = 0; i < currentPhotoPaths.Count; i++)
            {
                DatabaseManager.Instance.AddImageToItem(currentItem.id, currentPhotoPaths[i], i);
            }

            // Clear for next item
            itemNameInput.text = "";
            currentPhotoPaths.Clear();
            
            // Clear photo thumbnails
            foreach (Transform child in photoThumbnailContainer)
            {
                Destroy(child.gameObject);
            }

            UpdateItemCaptureStats();
            ShowFeedback($"Saved item: {itemName}", false);
        }

        private void OnExportQueueClicked()
        {
            string exportPath = QueueExporter.ExportQueue(currentQueue.id);
            
            if (!string.IsNullOrEmpty(exportPath))
            {
                ShowFeedback($"Queue exported to: {exportPath}", false);
            }
            else
            {
                ShowFeedback("Failed to export queue", true);
            }
        }

        private void OnBackToQueuesClicked()
        {
            CameraManager.Instance.StopCamera();
            ShowPanel(PanelType.QueueList);
        }

        private void UpdateItemCaptureStats()
        {
            if (currentQueue == null) return;

            int itemCount = DatabaseManager.Instance.GetQueueItemCount(currentQueue.id);
            int photoCount = DatabaseManager.Instance.GetQueueImageCount(currentQueue.id);

            itemCountText.text = $"Items: {itemCount}";
            photoCountText.text = $"Total Photos: {photoCount}";
        }

        private void UpdatePhotoCount()
        {
            photoCountText.text = $"Current Photos: {currentPhotoPaths.Count}";
        }

        #endregion

        private void ShowFeedback(string message, bool isError = false)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = isError ? Color.red : Color.green;
            }
        }

        private void OnDestroy()
        {
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.OnPhotoTaken -= OnPhotoTaken;
                CameraManager.Instance.OnCameraError -= OnCameraError;
            }
        }
    }
}