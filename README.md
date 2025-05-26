# Companion Unity

Unity-based mobile companion app for eBay Tools, providing photo capture and queue management functionality.

## Overview

This is a Unity port of the EbaytoolsCompanion Android app, designed to capture product photos and information on mobile devices for later processing with the eBay Tools desktop application.

## Features

- **Queue Management**: Create and manage collections of items
- **Photo Capture**: Take multiple photos per item using device camera
- **Item Organization**: Add items with names and descriptions
- **Data Export**: Export queues as JSON files for desktop import
- **Cross-Platform**: Works on Android, iOS, and other Unity-supported platforms

## Project Structure

```
Assets/
├── Scripts/
│   ├── Models/         # Data models (Queue, Item, ItemImage)
│   ├── Database/       # Database management and persistence
│   ├── Camera/         # Camera functionality
│   ├── UI/             # UI management and controllers
│   └── Utils/          # Utilities (export, etc.)
├── Prefabs/           # Reusable UI components
├── Scenes/            # Unity scenes
└── Materials/         # Materials and shaders
```

## Setup Instructions

1. Open the project in Unity 2021.3 LTS or later
2. Import required packages:
   - TextMeshPro (for better text rendering)
   - Unity UI package
3. Configure platform settings:
   - For Android: Set minimum API level to 21
   - For iOS: Configure camera usage description
4. Build and deploy to your target platform

## Key Components

### DatabaseManager
- Singleton that manages all data operations
- Currently uses PlayerPrefs for persistence (SQLite integration planned)
- Handles CRUD operations for queues, items, and images

### CameraManager
- Manages device camera access and photo capture
- Handles permissions and camera initialization
- Saves photos to persistent storage

### UIManager
- Controls navigation between screens
- Manages user interactions
- Updates UI based on data changes

### QueueExporter
- Exports queue data to JSON format
- Compatible with eBay Tools desktop import
- Includes device information and timestamps

## Building

### Android
1. File > Build Settings > Android
2. Configure Player Settings:
   - Package name: com.ebaytools.companion.unity
   - Minimum API Level: 21
   - Target API Level: Latest
3. Build APK or AAB

### iOS
1. File > Build Settings > iOS
2. Configure Player Settings:
   - Bundle Identifier: com.ebaytools.companion.unity
   - Camera Usage Description
3. Build Xcode project
4. Open in Xcode and deploy

## Data Format

The app exports data in JSON format compatible with eBay Tools desktop:

```json
{
  "version": "2.0",
  "exportDate": "2023-XX-XXTXX:XX:XX.XXXZ",
  "deviceInfo": {
    "manufacturer": "...",
    "model": "...",
    "platform": "..."
  },
  "queues": [
    {
      "id": 1,
      "name": "Queue Name",
      "items": [
        {
          "id": 1,
          "name": "Item Name",
          "images": [
            {
              "imagePath": "path/to/image.jpg",
              "orderIndex": 0
            }
          ]
        }
      ]
    }
  ]
}
```

## Future Enhancements

- SQLite database integration for better performance
- Voice input for item names
- Barcode scanning
- Cloud sync capabilities
- Real-time preview of captured photos
- Batch operations for multiple items

## Requirements

- Unity 2021.3 LTS or later
- Target device with camera
- Minimum 2GB RAM recommended