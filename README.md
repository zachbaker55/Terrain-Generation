
# Zachary Baker - Terrain Generation
Created in 2025.

## Project Overview

A procedural terrain generation system built in Unity capable of generating both world maps and fully explorable three-dimensional isometric worlds. The generator is highly configurable, allowing nearly every aspect of generation—including terrain, biomes, vegetation, and world size to be customized through editable settings. Biomes and world features are entirely data-driven, making it easy to extend the generator without modifying generation logic.

The project also includes visualization tools for inspecting intermediate generation stages and an image stitching system capable of capturing large, high-resolution maps of entire generated worlds.


## Development

The world generation pipeline combines several procedural generation techniques to create natural-looking terrain while remaining highly configurable. Terrain is generated from multiple layered Perlin noise maps controlling elevation, flatness, and terrain variation. Island generation combines Voronoi regions, configurable falloff curves, and noise blending to create single or multi-island worlds with adjustable shapes and distributions.

Biome placement is determined by evaluating environmental factors such as temperature, precipitation, elevation, inlandness, and terrain flatness against data-driven biome definitions. Vegetation placement utilizes both Poisson Disc Sampling and grid-based sampling algorithms to produce natural feature distribution while allowing biome-specific density control. The generator ultimately produces a three-dimensional indexed tile map that can be rendered as an isometric world using configurable tile definitions.

## Technical Challenges

One of the primary goals of this project was balancing procedural variety with deterministic and configurable generation. Rather than relying on a single noise map, the terrain is generated through a multi-stage pipeline where independent algorithms contribute specific environmental characteristics before being combined into the final world.

Several custom algorithms were implemented throughout development, including Voronoi-based island generation, gradient and falloff blending, biome selection using weighted environmental sampling, Chamfer Distance Transform for inlandness calculations, variable-radius Poisson Disc Sampling for natural vegetation placement, and grid-based sampling for additional world features. Particular attention was given to separating generation logic from content through data-driven biome and feature definitions, allowing new environments and vegetation types to be introduced with minimal code changes while keeping the generation system reusable and scalable.

## Screenshots
<img width="250" height="500" alt="image" src="https://github.com/user-attachments/assets/037cb473-9f3e-4339-82d2-897641646581" />

<img width="250" height="250" alt="image" src="https://github.com/user-attachments/assets/8e74d544-9527-4054-8a69-1907392e43d1" />
<img width="250" height="250" alt="image" src="https://github.com/user-attachments/assets/ac433095-530c-47d9-be68-b1224dfabcb6" />
<img width="250" height="250" alt="image" src="https://github.com/user-attachments/assets/85cec838-0a27-4b69-ad0e-9308fcb87fd5" />
<img width="250" height="250" alt="image" src="https://github.com/user-attachments/assets/2d7a8a7f-5247-4697-bbcc-7f835b61cfbe" />
<img width="250" height="250" alt="image" src="https://github.com/user-attachments/assets/50614ae2-c447-4b9b-bb80-6d710d29891b" />
<img width="250" height="250" alt="image" src="https://github.com/user-attachments/assets/1e80ae4f-499d-4a48-9e0b-c998a0c4d38e" />
<img width="250" height="250" alt="image" src="https://github.com/user-attachments/assets/eddcb2df-cf5a-49c3-8cde-349c35c14094" />

<img width="8192" height="4096" alt="World_2026-08-03_14-14-22" src="https://github.com/user-attachments/assets/cf19e016-328a-4ff8-b7f8-7a77753fa691" />
<img width="8192" height="4096" alt="World_2026-08-03_14-36-19" src="https://github.com/user-attachments/assets/4396d4a6-d765-4d8c-9386-fcc587a7f0d5" />
