\# Don't Tap the Cat — AI Development Rules



\## Project



Small Unity Android portrait reaction game.



The goal is to ship a playable commercial game quickly.



\## Development Philosophy



Prioritize:



1\. Working gameplay

2\. Simple implementation

3\. Clear code

4\. Fast iteration

5\. Stability



Do not optimize for architectural sophistication.



\## Architecture



Prefer simple MonoBehaviour components.



Prefer direct references.



Avoid introducing systems unless they are required by the current task.



Do NOT introduce:



\- Event Bus

\- Dependency Injection

\- Service Locator

\- ECS

\- Networking

\- Generic gameplay frameworks

\- Unnecessary interfaces

\- Generic managers

\- Premature abstractions



\## Scope Control



Follow GAME\_SPEC.md.



Do not add features that are not required for the current milestone.



If you think an additional feature is useful, mention it in the final report instead of implementing it.



\## Implementation



Before changing code:



1\. Inspect relevant existing files.

2\. Explain the intended change briefly.

3\. Implement the smallest viable solution.

4\. Check for compiler errors.

5\. Report changed files.



\## Unity



Do not modify unrelated scenes or assets.



Prefer simple Unity components and serialized references.



Avoid runtime Find calls when a serialized reference is practical.



\## Completion



A task is complete only when:



\- Code compiles.

\- No obvious null-reference problems remain.

\- The requested gameplay works.

\- No unrelated files were modified.

