# CheatPanel
Unity ingame cheat console

## Design ideas
- The console should be a single prefab that can be dropped into any scene.
- The console should be able to be opened and closed with a key press (on PC), some touch gesture like draw some figure (on mobile) or just some UI button for every platform.
- The console has minimized mode with maximize button and user selectable game variables to observe (fps, memory usage or any user selectable variable).
- The console has maximized mode with a tab view with tabs for different categories of cheats. There is a mandatory System Tab with system-wide cheats and variables (device model, OS version, CPU/GPU/Memory stats, controls to change time scale, target fps/vsync etc). Also user can add his own cheats to custom tabs.
- User cheats its a class inherited from some base class/interface. Cheat is a public method with some optional parameters or a public property. Cheats are drawn in cheat tab like button, slider of some field according to a type of member of cheat class (or attributes to customize gui of cheat). User directly implement cheat logic in this method/property.
- There is a support for network cheats, with user provided RPC-like system (via interface?). Client side cheat realization send cheat call and optionally wait for result to display to cheat panel. The mirror server-side realization receives this call, executes cheat logic and optionally send result back to client.
- There is a tab for text based console commands. Its a same logic as a gui cheats, but for console lovers. Autocompletion for cheat names must be implemented.
- Also should be Unity console tab.
