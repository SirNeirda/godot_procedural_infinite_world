# godot_procedural_world

Full Godot C# project for procedural infinite landmass generation, with random items or obstacles spreading throughout the map, all with physics enabled. The base mesh generation logic is based on Sebastian Lague's Procedural Landmass Generation, optimized and customized to work in Godot 4.4 and lower.
This project also includes a simple playable demo with a character, showcasing the features. Also includes some helper functions not present in Godot's default engine, such as in-world gizmos and text for easy debugging.

Compiled demo downloadable here or here.
Demo Project
In the demo, you can play a simple character in TP view. Obstacles are generated and randomized throughout the infinite map. Mobs spawn randomly around the player and move toward him.
This is a simple starter demo to showcase the project, but can be extended to support any actual gameplay.
How to Play
- You start on an elevated platform.
- Move using WASD
- Run using Shift
- Throw crates with Left Click
- Jump with Space

Mobs are constantly spawned around the player, but if you're overrun nothing really happens.
Mobs are cleaned-up when they get too far.
Doing any sort of action will consume stamina, shown in the upper right bar. Once you're out of stamina, you can only walk.
Key Features
- Terrain generation, by default using Godot's Simplex Perlin Noise, but fully extensible and allowing any type of blend or crazy stuff.
- Terrain LOD. Adjustable draw distance.
- The mesh is generated around the player and cleaned depending on distance.
- Objects can be spawned throughout the map, customizable and randomizable.
- Physics enabled playable character and items.
Incomplete features
- There are a selection of shaders to go along the terrain, which are largely incomplete. With the mesh coordinates, it is possible to blend textures according to elevation/orientation, or any other variable supported in Godot.
- Water spawns at elevation 0. Although terrain objects will not spawn under water, the character and mobs will not drown or swim. There is no water logic.
- For now there isn't any kind of variable exposed to customize generation, all must be done through code.
- The project is somewhat over-engineered with some overkill functions and classes.

If you like this project, need help or have cool ideas, join our Discord!
