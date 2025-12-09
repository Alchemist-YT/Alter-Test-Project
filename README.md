# Alter Test Project

Project Overview
1. Engine         : Unity 2022.3.62.f3 (LTS)
2. Networking     : Netcode for GameObjects (NGO)
3. External Assets: DOTween (Animations), UniTask (Async Logic)

Scene: Assets/Scenes/Game.unity.

<H1> Initiative Logic </H1>
Initiative is recalculated at the start of every Reveal.

The player with the Highest Score goes first or random player if tie.

<H1> Reveal Sequence </H1>

For the Reveal Sequence, a Queue system was used:

Two Queues store both players' cards, which are iterated through to reveal them one by one. 

UniTask is used for awaiting.

Dequeue Initiative Player's Card --> Resolve Ability --> Update Score. 
--> Opponent reveals their card --> Resolve Ability --> Update Score.

Iterate until every card is revealed.
