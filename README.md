# Qualisys Unity SDK

This is a repository with Unity scripts that lets Qualisys users stream marker, 6dof and user bones to Unity from [Qualisys Track Manager](http://www.qualisys.com/products/software/qtm).
A complete custom package that can be imported into Unity is available [here](http://www.qualisys.com/download/Qualisys-Real-Time-Streaming.unitypackage).

### Example on how to use
1. Start QTM and start streaming real-time (either real-time from camera system or from measurement file).
2. Create new project.
3. Import Package -> Custom Package...
4. Show the QTM Streaming window using the menu option Window->Qualisys->RTClient.
5. Create an empty GameObject and the RTMarkerStream.cs script to it.
6. Press "play" mode
7. The QTM servers available on the network will be automatically discovered and displayed in the Server settings in the QTM Streaming window.
8. Press Connect and then Unity will receive markers and display them.

#### For character animation, instead of 5:
1. Go to Window -> Asset Store -> 3D Models -> Characters and download and import Character
2. Add character to the scene
3. Drag and drop RTCharacterStream.cs to the character in the Hierarchy.
Then continue as above at 6.

## Files in this package

#### Streaming
These are scripts that can be added to game objects for handling of streaming data from Qualisys Track Manager using the RTClientSDK.Net.
* RTMarker.cs - Add this script to a Game object to set the position of game object from a specific labeled marker name.
* RTUnlabeledMarker.cs - Add this script to a Game object to set the position of game object from a specific unlabeled marker id.
* RTMarkerStream.cs - Add this script to a Game object to visualize marker positions (using spheres) streamed from Qualisys Track Manager.
* RTBones.cs - Add this script to a Game object to visualize user defined bones (using line gizmos).
* RTObject.cs - Add this script to a Game object to get 6DOF object positions and rotations streamed from Qualisys Track Manager.
* RTObjectMarkers.cs - Add this script to Game objects of a 6DOF object body markers.
* RTCharacterStream.cs - Add this script to a Unity character to animate the character from marker streamed from Qualisys Track Manager.
* RTGazeStream.cs - Add this script to visualize gaze vectors as lines.
* RTAnalog.cs - Example script to display how to get data from a specific analog channel (using name).
* RTAnalogIMU.cs - Add this script and set analog channel names for IMU data (X, Y, Z, W).

#### Helpers
Scripts in this folder are Unity helper scripts used to commuicate with Qualisys Track Manager and handle Unity user interface.
* RTClient.cs - Main script that handles the communication between Qualisys Track Manager and Unity.
* RTGUI.cs - This script is responsible for handling the Window->Qualisys->RTClient option that shows the QTM Streaming window where the user can select which Qualisys Track Manager to connect to.
* RTGUIInGame.cs - This script can be used to show a QTM connection user interface when in Game mode.

#### RTClientSDK.Net
The Unity SDK includes a packaged version of the [Real-time client SDK for .Net](https://github.com/qualisys/RTClientSDK.Net).

#### Character Animation
Files for real-time animation of a character.

## Links
* [Qualisys](http://www.qualisys.com)
* [Unity](http://madewith.unity.com)
