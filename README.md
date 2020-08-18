# Unity_Data_Snapshot_Injection
A Unity assembly injection thats allows the user to extract the property variable data on every game object loaded in the scene by the press of a button.
---
## How do I use this?
- Pull (or download) and compile the project.
- Get yourself a mono injector, I've been using <a href="https://github.com/warbler/SharpMonoInjector">SharpMonoInjector</a>, but any will do.
- Inject the .dll file you got from compiling the project into the assembly
    - Namespace is "Unity_Data_Snapshot_Tool_Injection"
    - Loader class name is "Loader"
    - Loader method name is "Init"
  > Note that if you are using SharpMonoInjector you will have to start the game, scan for the process and then inject.
- In the game, you should now see a "Save Snapshot" button in the top left corner.
- Uppon pressing said button, the game will freeze for a bit. A snapshot of all loaded game objects data will be saved into a snapshot folder in the \_Data folder, found in the games directory.
- Examine the snapshot at your leisure.
