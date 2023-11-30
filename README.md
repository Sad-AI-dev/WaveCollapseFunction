# WaveCollapseFunction
Implementation of the Wave Collapse Function Algorithm in the Unity engine.  
The project contains a 2d and 3d iteration of the algorithm, both with 2 data sets.  
This project was created for an open-ended excercise during University.

## 2D Algorithm
![lines algorithm](Readme-Files/2D-Lines.gif?raw=true)  
![Nature algorithm](Readme-Files/2D-Nature.gif?raw=true)

## 3D Algorithm
**Add gifs of the 3d algorithm doing its thing here**

## Potential for improvement
This is my first experiment with the Wave Collapse Function Algorithm, so the results have some room for improvement.  
Found Problems:
- Setting up a dataset is very time-consuming
- Datasets have too litle control over how often any given tile is used
- The Algorithm often fails to generate a 'chunk' by working itself into a corner

Some Potential Solutions
- More powerfull and robust tools to build data sets
	- This could, and likely should, result in a completely different approach to how datasets are constructed and stored
- Adding support for weighted chance of individual tiles
- Currently the algorithm only perpetuates to the immediately neighboring tiles, 
increasing this to also be all neighbors connected to the immediately neighboring 
tiles would make the algorithm more efficient and less prone to failing.
- Entropy is currently calculated as simply the number of tiles the current uncollapsed
tile might still become. I think an improvement could be made by accounting for the
weighted chance values of the tiles in the current uncollapsed tile.

## Art Credits
Not all art in this project are of my creation, sources for these can be found below:  
- Nature Sprite Sheet: [itch.io page](https://cainos.itch.io/pixel-art-top-down-basic)