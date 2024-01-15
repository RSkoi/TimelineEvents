# TimelineEvents
Plugin that extends the Timeline plugin with scriptable events, similar to events from Unity3D animations. The events can be fed scripts that will be executed when the Timeline cursor passes them. In other words, the player can interact with the Unity engine through timeline (and RuntimeUnityEditor). This can be useful for cases Timeline does not provide support for, e.g. specific settings of a Unity Particle System.

As the plugin feeds each script to the RuntimeUnityEditor as code which is then executed, notice that certain limitations apply. When writing a script, always first debug it with the REPL console from the RuntimeUnityEditor.

## DISCLAIMER ABOUT ARBITRARY CODE EXECUTION
TimelineEvents allows for arbitrary code execution after a scene is loaded (and Timeline is playing) inside the Koikatsu studio. Arbitrary code execution can be used by bad faith actors to gain access to the player's PC and private data. When loading a scene, an overlay will pop up, blocking execution of the scripts within the scene by default and prompting the player to review the scripts, or discard them.
Do **NOT** run scripts from scenes if you do not know the creator or trust the source. The maker of this plugin holds no responsibility for damages that result from its use.

## Config
- **Force script warning on all scenes:** Force the script warning on all scenes, regardless if the warning has been dismissed before. When unchecked/false, the script warning will only pop up once on each scene. (Default: true)
- **Carry over cache:** Whether TimelineEvents should carry over the cache across studio sessions. When unchecked/false, the script warnings will pop up independently of previous sessions. (Default: false)
- **Dump scripts to file:** Dump all scripts within the current scene to a text file. (Default: CTRL+D)

## Dependencies
- latest BepInEx
- latest KKAPI
- latest RuntimeUnityEditor
- latest Timeline

## Notes
- When an event fires it is marked as executed and will not fire again during the loop, unless Timeline is reset with its "Stop" button.
- The changes that are caused by each event are permanent until you load a new scene or restart studio entirely. If you want for these effects to be reset in the loop, you will need to insert an event at the start of the loop that does this manually.

## Known quirks
- Disabling the "Events" interpolable with the checkbox next to the name does not actually disable the events. If you want to disable the interpolable, use the "Disable" and "Enable" options from the right-click menu of the interpolable.
- Event keyframes can not be copied, cut or pasted. This applies to all selected keyframes if at least one of those is an event keyframe.

## Installation
Extract the contents of the RSkoi\_TimelineEvents\_\<version\>.zip archive into your main Koikatsu folder so that the RSkoi_TimelineEvents.dll file goes into the BepInEx/plugins folder.
