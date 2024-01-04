[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-24ddc0f5d75046c5622901739e7c5dd533143b0c8e959d652212380cedb1ea36.svg)](https://classroom.github.com/a/kQY_5cCT)
[![Open in Visual Studio Code](https://classroom.github.com/assets/open-in-vscode-718a45dd9cf7e7f842a935f5ebbe5719a5e09af4491e668f4dbf3b35d5cca122.svg)](https://classroom.github.com/online_ide?assignment_repo_id=11269193&assignment_repo_type=AssignmentRepo)
# BossFight - Toy Story: The M&M Attack
## Student Names
* Itamar Citrin (ID 209701853)
* Yonatan Vologdin (ID 323316828)

## Explain your design. What did you try to achieve?
Among the core elements of our design are:
- Tail movement. One of our goals was to utilize the boss's tail for battle purposes. In order to that, we designed a
  system that moves the tail from its 'idle' position that was implemented in previous exercises (which is simply moving from side to side).
  That system introduces tail movement from the position it was when it stopped swinging, to attack left/right, and then to return
  back to its original position.
- Giving a meaningful role for the flocking agents. The flocking agents, represented by the M&M candies, 
were integrated into the gameplay to let the player utilize it's tail against the enemies. We adjusted the flocking
vectors to make the agents follow the player, while staying a little separated from each other and staying aligned.
- Spawning the flocking agents. We designed a custom pool system to make the spawning of new agents more efficient on runtime.
- Engaging start and end screens. We designed a visually appealing start screen with a rotating camera and buttons,
  as well as an end screen displaying the player's score, created a more polished and complete experience.
- Pickups system. We designed a script that occasionally spawns a fruit that will heal the player. 
- Increasing difficulty. To make the game more interesting, we added increased difficulty every time the player successfully
  defeats a couple of peep groups, by increasing the spawn rate of the peeps and increasing the damage they deal.

## What would you improve given more time?
Given more time, we would improve the following:
- Diverse enemy types. We would introduce different types of enemies with unique behaviors and attack patterns in order 
  to increase the depth and challenge of the gameplay. Each enemy type could require different strategies to defeat, 
  adding variety and replayability.
- Stage based gameplay. If we had more time, we'd implement a stage based progression system that would provide a 
  sense of progression and accomplishment. Each stage could introduce new enemy types, culminating in a boss fight at the end of each round.
- Special attacks and power-ups to the player. Incorporating a power-up meter that fills over time would allow the player to unleash 
  special tail attacks for a limited duration. These attacks could have more significant impact and provide moments of excitement and reward.
- Adding more "juice": To make the game more visually appealing and immersive, we'd add more animations to the characters, environment, and attacks.
  In addition, we'd add shaders for the tail swing, particle systems and more.
- Complex level design with A*. Creating complex levels with walls and obstacles, and then implementing A* for the flocks,
  would introduce a new aspect of the game, where enemies can come from more places, and spawn in different locations.