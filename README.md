**CLI Roguelike Tetris**

A command-line Tetris game built in F# featuring stage-based progression, random power-ups, and an endless mode.

**Setup and Installation**

Install the .NET SDK from official Microsoft website. 

Open a terminal in the project folder. 

Install the required JSON library: 

dotnet add package FSharp.SystemTextJson

**Run the game:**
start dotnet run

**Controls**

Move: Left and Right Arrow keys

Soft Drop: Down Arrow key

Hard Drop: Spacebar

Rotate: X (Clockwise) and Z (Counter-Clockwise)

Hold Slot 1: C key

Hold Slot 2: H key (Requires Power-up)

Quick Clear: Q key (Requires Power-up)

Save and Quit: S key (Only available between stages)

**Game Rules**

Normal Mode: Clear the required number of lines to complete a stage.

Power-ups: After each stage, choose one of three random power-ups to improve your abilities.

Modifiers: Random challenges like Fog of War (hidden next piece) or Garbage Rise will apply at the start of stages.

Power-up Limit: You can have a maximum of 3 active power-ups. You must replace one to add a fourth.

Endless Mode: Unlocked after Stage 5. Difficulty and modifiers stack indefinitely.
