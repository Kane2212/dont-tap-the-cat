\# Don't Tap the Cat



\## Goal



A very small portrait Android reaction game designed to be

completed and released within one week.



\## Core Rule



Tap the target.

Never tap the cat.



\## Core Loop



Start

→ Target appears

→ Player taps target

→ Score increases

→ New target appears

→ Cat moves/interferes

→ Player eventually taps cat

→ Game Over

→ Retry



\## Day 1 Scope



Must have:



\- Target spawning

\- Target tap detection

\- Score

\- Cat

\- Basic cat movement

\- Cat tap detection

\- Game over

\- Retry



\## Day 1 Exclusions



Do NOT implement:



\- Ads

\- Analytics

\- IAP

\- Leaderboards

\- Online systems

\- Save system

\- Multiple cats

\- Missions

\- Shop

\- Skins

\- Complex architecture

\- Event bus

\- Dependency injection

\- Service locator

\- ECS



\## Technical Direction



Prefer simple MonoBehaviour-based Unity implementation.



Use direct references where practical.



Avoid premature abstraction.



\## Definition of Done



The game can be played from start to game over and restarted

without manual intervention.

