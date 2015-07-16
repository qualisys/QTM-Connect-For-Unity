# [Qualisys](http://www.qualisys.com) [Unity](http://www.unity3d.com) SDK

This is a package with Unity scripts that lets Qualisys users stream marker, 6dof and user bones to Unity from [Qualisys Track Manager](http://www.qualisys.com/products/software/qtm).

Files in this package

Streaming
These are scripts that can be added to game objects for handling of streaming data from Qualisys Track Manager using the RTClientSDK.Net.
* RTMarkerStream.cs - Add this script to a Game object to visualize marker positions (using spheres) streamed from Qualisys Track Manager.
* RTBones.cs - Add this script to a Game object to visualize user defined bones (using line gizmos).
* RTObject.cs - Add this script to a Game object to get 6DOF object positions and rotations streamed from Qualisys Track Manager.

Helpers
Scripts in this folder are Unity helper scripts used to commuicate with Qualisys Track Manager and handle Unity user interface.
* RTClient.cs - Main script that handles the communication between Qualisys Track Manager and Unity.
* RTGUI.cs - This script is responsible for handling the Window->Qualisys->RTClient option that shows the QTM Streaming window where the user can select which Qualisys Track Manager to connect to.
* RTGUIInGame.cs - This script can be used to show a QTM connection user interface when in Game mode.

RTClientSDK.Net
These is a packaged version of the [Realtime client SDK for .Net](https://github.com/qualisys/RTClientSDK.Net).
