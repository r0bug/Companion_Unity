using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace CompanionUnity.Camera
{
    public class CameraManager : MonoBehaviour
    {
        private static CameraManager _instance;
        public static CameraManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CameraManager>();
                }
                return _instance;
            }
        }

        [Header("Camera Settings")]
        [SerializeField] private RawImage cameraDisplay;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        
        private WebCamTexture webCamTexture;
        private bool isCameraActive = false;
        
        public bool IsCameraActive => isCameraActive;
        
        // Events
        public event Action<string> OnPhotoTaken;
        public event Action<string> OnCameraError;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public void StartCamera()
        {
            if (isCameraActive) return;

            StartCoroutine(InitializeCamera());
        }

        private IEnumerator InitializeCamera()
        {
            // Request camera permission
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                OnCameraError?.Invoke("Camera permission denied");
                yield break;
            }

            // Get available cameras
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                OnCameraError?.Invoke("No camera found");
                yield break;
            }

            // Use the first available camera (prefer back camera on mobile)
            string deviceName = devices[0].name;
            foreach (var device in devices)
            {
                if (device.isFrontFacing == false)
                {
                    deviceName = device.name;
                    break;
                }
            }

            // Create and start webcam texture
            webCamTexture = new WebCamTexture(deviceName, 1920, 1080, 30);
            webCamTexture.Play();

            // Wait for camera to start
            float timeout = 5f;
            while (!webCamTexture.didUpdateThisFrame && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (!webCamTexture.isPlaying)
            {
                OnCameraError?.Invoke("Failed to start camera");
                yield break;
            }

            // Set up display
            if (cameraDisplay != null)
            {
                cameraDisplay.texture = webCamTexture;
                
                // Adjust aspect ratio
                if (aspectRatioFitter != null)
                {
                    float videoRatio = (float)webCamTexture.width / (float)webCamTexture.height;
                    aspectRatioFitter.aspectRatio = videoRatio;
                }
                
                // Correct rotation for mobile devices
                cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle);
            }

            isCameraActive = true;
        }

        public void StopCamera()
        {
            if (!isCameraActive) return;

            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
                webCamTexture = null;
            }

            if (cameraDisplay != null)
            {
                cameraDisplay.texture = null;
            }

            isCameraActive = false;
        }

        public void TakePhoto()
        {
            if (!isCameraActive || webCamTexture == null) return;

            StartCoroutine(CapturePhoto());
        }

        private IEnumerator CapturePhoto()
        {
            // Wait for end of frame
            yield return new WaitForEndOfFrame();

            // Create texture from camera
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            // Rotate if needed
            if (webCamTexture.videoRotationAngle != 0)
            {
                photo = RotateTexture(photo, webCamTexture.videoRotationAngle);
            }

            // Save to persistent data path
            string fileName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string filePath = Path.Combine(Application.persistentDataPath, "Photos", fileName);
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Save as JPG
            byte[] bytes = photo.EncodeToJPG(85);
            File.WriteAllBytes(filePath, bytes);

            // Clean up
            Destroy(photo);

            // Notify listeners
            OnPhotoTaken?.Invoke(filePath);
        }

        private Texture2D RotateTexture(Texture2D originalTexture, float angle)
        {
            int width = originalTexture.width;
            int height = originalTexture.height;
            
            Texture2D rotated;
            
            if (Mathf.Abs(angle - 90) < 1 || Mathf.Abs(angle - 270) < 1)
            {
                rotated = new Texture2D(height, width);
            }
            else
            {
                rotated = new Texture2D(width, height);
            }

            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotatedPixels = new Color32[original.Length];

            if (Mathf.Abs(angle - 90) < 1)
            {
                // Rotate 90 degrees clockwise
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        rotatedPixels[x * height + (height - y - 1)] = original[y * width + x];
                    }
                }
            }
            else if (Mathf.Abs(angle - 180) < 1)
            {
                // Rotate 180 degrees
                for (int i = 0; i < original.Length; i++)
                {
                    rotatedPixels[original.Length - 1 - i] = original[i];
                }
            }
            else if (Mathf.Abs(angle - 270) < 1)
            {
                // Rotate 270 degrees clockwise (90 counter-clockwise)
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        rotatedPixels[(width - x - 1) * height + y] = original[y * width + x];
                    }
                }
            }

            rotated.SetPixels32(rotatedPixels);
            rotated.Apply();

            return rotated;
        }

        private void OnDestroy()
        {
            StopCamera();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                StopCamera();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                StopCamera();
            }
        }
    }
}