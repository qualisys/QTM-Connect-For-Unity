# Qualisys Unity SDK

This repository houses Unity components for streaming marker, 6DOF and bone data to Unity from [Qualisys Track Manager (QTM)](http://www.qualisys.com/products/software/qtm).

These components are also available as a single, complete package that can be imported into Unity - [download here](http://www.qualisys.com/download/Qualisys-Real-Time-Streaming.unitypackage).

## Usage Example

1. Start QTM and start streaming real-time, from camera system or from a measurement file.
2. In your Unity project, select *Import Package > Custom Package*.
3. Show the QTM Streaming window using the menu option *Window > Qualisys  > RTClient*.
4. Attach the *RTMarkerStream.cs* script to a GameObject, or attach the *RTCharacterStream.cs* script to a character GameObject. (To get started, you can use an empty GameObject, and a character can be imported from *Window > Asset Store > 3D Models > Characters*.)
5. Enter "play" mode to verify.
6. All available QTM servers on the network will be automatically discovered and listed under *Server Settings* in the QTM Streaming window.
7. Press *Connect*. Unity will receive and display marker data.

### For character animation

After step 3 above:

1. Add a character to the scene (e.g. from *Window > Asset Store > 3D Models > Characters*, import a Character).
2. Attach the *RTCharacterStream.cs* script to the character GameObject.

Continue as above from step 5.

## Contents

### Streaming

The scripts below can be attached to GameObjects for streaming data from QTM. They are built using the [Qualisys Real-time Client SDK for .NET](https://github.com/qualisys/RTClientSDK.Net).

* *RTMarker.cs* - Add this script to a Game object to set the position of game object from a specific labeled marker name.
* *RTUnlabeledMarker.cs* - Add this script to a Game object to set the position of game object from a specific unlabeled marker id.
* *RTMarkerStream.cs* - Add this script to a Game object to visualize marker positions (using spheres) streamed from Qualisys Track Manager.
* *RTBones.cs* - Add this script to a Game object to visualize user defined bones (using line gizmos).
* *RTObject.cs* - Add this script to a Game object to get 6DOF object positions and rotations streamed from Qualisys Track Manager.
* *RTObjectMarkers.cs* - Add this script to Game objects of a 6DOF object body markers.
* *RTCharacterStream.cs* - Add this script to a Unity character to animate the character from marker streamed from Qualisys Track Manager.
* *RTGazeStream.cs* - Add this script to visualize gaze vectors as lines.
* *RTAnalog.cs* - Example script to display how to get data from a specific analog channel (using name).
* *RTAnalogIMU.cs* - Add this script and set analog channel names for IMU data (X, Y, Z, W).

### Helpers

These are helper scripts used to communicate with QTM and add functionality to the Unity user interface.

* *RTClient.cs* - Main script that handles the communication between Qualisys Track Manager and Unity.
* *RTGUI.cs* - This script is responsible for handling the Window->Qualisys->RTClient option that shows the QTM Streaming window where the user can select which Qualisys Track Manager to connect to.
* *RTGUIInGame.cs* - This script can be used to show a QTM connection user interface when in Game mode.

### RTClientSDK.Net

The Unity SDK includes a packaged version of the [Qualisys Real-time client SDK for .Net](https://github.com/qualisys/RTClientSDK.Net).

### Character Animation

Components for real-time character animation.
