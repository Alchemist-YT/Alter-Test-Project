# Alter Test Project

Project Overview
1. Engine         : Unity 2022.3 (LTS)
2. Networking     : Netcode for GameObjects (NGO)
3. External Assets: DOTween (Animations), UniTask (Async Logic)

Scene: Assets/Scenes/Game.unity.

<H1> Initiative Logic </H1>
Initiative is recalculated at the start of every Reveal.

The player with the Highest Score goes first or random player if tie.

<H1> Reveal Sequence </H1>

For Reveal Sequence used a Queue system:

Used 2 Queues to store both players cards and then iterate to it and reveal one by one. used unitask for awating.

Dequeue Initiative Player's Card --> Resolve Ability --> Update Score.
--> Opponent reveals their 1st card --> Score Updates.

Iterate until every card is revealed.
