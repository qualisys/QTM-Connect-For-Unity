# Qualisys Unity SDK

This repository houses Unity components for streaming data from [Qualisys Track Manager (QTM)](https://www.qualisys.com/software/qualisys-track-manager) to Unity.

## Download Link
Download the Asset package at [releases](https://github.com/qualisys/Qualisys-Unity-SDK/releases)

## Example Usage
Character animation using QTM Real-Time Output
1. In your Unity project, select **Import Package** > **Custom Package...** and import this package.
2. Double click the file **DemoSkeletonQtm** to launch QTM.
3. In QTM. Start streaming by pressing **Ctrl** + **Shift** + **Space** or select **Play** > **Play with Real-Time Output**
4. In Unity, open the scene **DemoSkeletonScene**.
5. Press **Play**.
6. Connect to localhost using the right side menu.
Note that you can also use the editor window to connect to QTM. It's located under **Window** > **Qualisys** > **RTClient**

## Contents

### Components

The scripts below can be attached to GameObjects for streaming data from QTM. They are built using the [Qualisys Real-time Client SDK for .NET](https://github.com/qualisys/RTClientSDK.Net).

* **RTGenericSkeleton.cs** - Control a generic Unity rig using custom skeleton streaming from QTM. 
* **RTSkeleton.cs** - Add this script to use qtm skeleton data to drive mecanim humanoid character in Unity.
* **RTMarker.cs** - Add this script to a Game object to set the position of game object from a specific labeled marker name.
* **RTUnlabeledMarker.cs** - Add this script to a Game object to set the position of game object from a specific unlabeled marker id.
* **RTMarkerStream.cs** - Add this script to a Game object to visualize marker positions (using spheres) streamed from Qualisys Track Manager.
* **RTBones.cs** - Add this script to a Game object to visualize user defined bones (using line gizmos).
* **RTObject.cs** - Add this script to a Game object to get 6DOF object positions and rotations streamed from Qualisys Track Manager.
* **RTObjectMarkers.cs** - Add this script to Game objects of a 6DOF object body markers.
* **RTGazeStream.cs** - Add this script to visualize gaze vectors as lines.
* **RTAnalog.cs** - Example script to display how to get data from a specific analog channel (using name).
* **RTAnalogIMU.cs** - Add this script and set analog channel names for IMU data (X, Y, Z, W).
* **RTForcePlate.cs** - Add this script to visualizie force plate data. Example scene: DemoForceDataScene

### Helpers

These are helper scripts used to communicate with QTM and add functionality to the Unity user interface.

* **RTClient.cs** - Main script that handles the communication between Qualisys Track Manager and Unity.
* **RTGUI.cs** - This script is responsible for handling the Window->Qualisys->RTClient option that shows the QTM Streaming window where the user can select which Qualisys Track Manager to connect to.
* **RTGUIInGame.cs** - This script can be used to show a QTM connection user interface when in Game mode.

### RTClientSDK.Net

The Unity SDK includes a packaged version of the [Qualisys Real-time client SDK for .Net](https://github.com/qualisys/RTClientSDK.Net).
