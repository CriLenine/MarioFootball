# MarioFootball

The aim of this project was to reproduce the game Mario Smash Football with Unity.  
We worked on it for **6 weeks**, as a team of **5 students**.

## Content

The game holds 4 scenes :
- the main menu;
- the options menu;
- the character selection and game settings menu;
- the match scene.

## Features

The game can be played with a keyboard or with a controller, by one player against an AI.  
The options menu allow the player to change the volume.  
The player can choose the character which will be the captain of his team, then the one that each of his teammates will embody.  
Finally he can choose the game length, the number of goals required to win, and the AI difficulty.  
  
The match starts with a skippable animation which spawns the players and make them go to their kickoff position.  
There is then a countdown to the kickoff and the match can start with a first pass.  

When the player **has** the ball he is able to :
- make a directed pass to a teammate;
- make a lobbed pass in a specific direction;
- shoot to the enemy goal;
- dribble, which will make him unaffected by tackles.

When he **does not have** the ball, he can :
- change controlled player;
- tackle;
- perform a headbutt;

And in all situations the player can throw an item.

There is another skippable animation on each scored goal.

There is also a pause menu available at any time.

## My participation

I strongly participated in the global architecture of the project for the first few days of reflexion.

Besides the little and diverses parts of gameplay script i wrote (mainly on singletons and *Player.cs*),  
I coded :
 - the chrono class (*Chrono.cs*)
 - the cameras system (*CameraManager.cs*, *CameraController.cs*);
 - the complete functioning of the `CameraControlQueue` (*CameraManager.cs*) and the `PlayerActionsQueue` (*Player.cs*);
 - the item-related scripts (Scripts/Items).

I also organised the scripts (for instance with regions and local functions).
