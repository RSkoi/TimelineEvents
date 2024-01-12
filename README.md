# TimelineEvents
Plugin that extends the Timeline plugin with scriptable events, similar to events from Unity3D animations. The events can be fed scripts that will be executed when the Timeline cursor passes them. In other words, the player can interact with the Unity engine through timeline (and RuntimeUnityEditor). This can be useful for cases Timeline does not provide support for, e.g. specific settings of a Unity Particle System.

## Dependencies
- latest BepInEx
- latest KKAPI
- latest RuntimeUnityEditor
- latest Timeline

## Notes
- When an event fires it is marked as executed and will not fire again during the loop, unless Timeline is reset with its "Stop" button.
- The changes within the engine that are caused by each event are permanent until you load a new scene or restart studio entirely. If you want for these effects to be reset, you will need to insert an event at the start of the loop that does this manually.
- Event keyframes can not be copied, cut or pasted. This applies to all selected keyframes if at least one of those is an event keyframe.

## Known quirks
- Disabling the "Events" interpolable with the checkbox next to the name does not actually disable the events. If you want to disable the interpolable, use the "Disable" and "Enable" options from the right-click menu of the interpolable.

## Installation
Extract the RSkoi\_TimelineEvents\_<version>.zip archive into your main Koikatsu folder.
