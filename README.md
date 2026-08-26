# 2.5D Rugby Simulation

**A rugby simulation developed from scratch in Unity and C#**

`Unity` `C#` `Object-Oriented Programming` `Finite State Machines` `AI` `Algorithms`

---

## Overview

This project is a 2.5D rugby simulation that I designed and developed from scratch in **Unity using C#** as my A-Level Computer Science project.

The aim was to create a rugby game that was simple to control while still modelling the structure and decision-making of real rugby.

Rather than scripting individual sequences, I developed interconnected gameplay systems controlling player behaviour, possession, passing, defensive positioning, tackles, rucks, lineouts, scoring, conversions and match flow.

The development process was documented in a **~250-page technical report**, covering requirements analysis, stakeholder research, system design, algorithms, implementation, testing and evaluation.

---

## 🎮 Gameplay

The simulation includes:

- Player movement and sprinting
- Passing and pass selection
- Tackling
- Rucks
- Lineouts
- Tries and scoring
- Conversions
- Kick-offs and restarts
- Attacking formations
- Defensive AI
- Player attributes
- Difficulty settings
- Rebindable controls
- Match-state management

> **Gameplay demo coming soon**

<!-- Add a gameplay GIF here:
![Gameplay Demo](media/gameplay.gif)
-->

---

# 🧠 Technical Overview

The project uses a modular, object-oriented architecture with **finite state machines, event-driven communication and custom gameplay algorithms**.

Major systems were separated into dedicated components rather than implementing the game as a single controller.

Examples include:

- Player state management
- Defensive AI
- Match management
- Score management
- Possession management
- Ruck management
- Lineout management
- Passing
- Ball movement
- Input management

This allowed individual systems to be developed and tested independently while still communicating during gameplay.

---

# 🔄 Finite State Machines

Finite State Machines were used extensively to manage complex behaviour without creating large collections of nested conditional statements.

## Player FSM

Player behaviour is separated into states representing different actions and situations.

Examples include:

- Idle
- Moving
- Sprinting
- Passing
- Tackling
- Ruck involvement

Each state controls its own behaviour and transitions to other states when the required conditions are met.

## Defensive AI FSM

AI-controlled defenders transition between behaviours including:

**Maintain Defensive Line → Chase Ball Carrier → Tackle**

This allows defensive players to react dynamically to changes in possession and player positioning.

## Match States

Match flow is also controlled through state transitions between situations such as:

**Open Play → Tackle → Ruck → Open Play**
<img width="667" height="271" alt="image" src="https://github.com/user-attachments/assets/310d718a-aa86-4a1e-b688-7f5cf328a226" />

as well as:

**Lineout**
<img width="339" height="669" alt="image" src="https://github.com/user-attachments/assets/c1816156-f5ae-4ee7-ab08-31492006ab22" />

**Try → Conversion → Kick-off**

This helped keep match logic modular and predictable.

---

# 🎯 Probability-Based Passing Algorithm

One of the main algorithms developed for the project determines whether a pass is successfully completed.

A fixed probability would have made all passes equally difficult, so pass success instead considers several variables:

- Distance between players
- Player handling ability
- Game difficulty
- Maximum effective passing distance

Conceptually:

```text
Pass Distance
      +
Player Handling
      +
Game Difficulty
      ↓
Pass Probability
      ↓
Success / Miss
```

The distance component uses **non-linear fall-off**, making increasingly long passes progressively more difficult.

The final probability is constrained between approximately **5% and 98%** so that neither success nor failure becomes completely guaranteed.

If the pass fails, the amount by which the ball misses its target is also affected by pass distance and the calculated probability.

This produces more realistic variation than simply choosing between a successful or unsuccessful animation.

---

# 🏃 Dynamic Attacking Formation Algorithm

A key gameplay problem was preventing supporting players from simply clustering around the ball carrier.

I therefore developed a dynamic attacking formation system.

The algorithm calculates player positions relative to the current ball carrier using:

- An **attack direction vector**
- A perpendicular **lateral vector**
- Player lane assignments
- Width offsets
- Depth offsets

Conceptually:

```text
                 Direction of Attack
                         ↑

                Ball Carrier
                     ●

             ●               ●
        ●                         ●
    ●                                 ●
```

Each supporting player receives a stable formation position rather than constantly competing for the same location.

A dictionary maps individual players to their assigned formation positions, helping maintain formation stability as the ball and players move around the pitch.

---

# 🛡️ Defensive Pairing Algorithm

The defensive system assigns defenders to attacking threats rather than having every AI player chase the ball.

Attackers and defenders are ordered based on their position across the pitch and then paired.

```text
ATTACK

 A1       A2       A3       A4
 ↓        ↓        ↓        ↓
 D1       D2       D3       D4

DEFENCE
```

Once paired, the defensive finite state machine determines whether a defender should:

- Maintain the defensive line
- Chase the ball carrier
- Attempt a tackle

This creates more structured defensive behaviour.

---

# 📦 Data Structures

Several data structures were selected for specific gameplay requirements.

| Data Structure | Example Use |
|---|---|
| **Stack** | Managing players involved in rucks |
| **Queue** | Ordering players for lineouts |
| **Dictionary / Hash Table** | Player-to-position and defensive assignments |
| **Lists** | Managing collections of players and gameplay objects |
| **JSON** | Persistent storage of user control bindings |

Choosing structures based on the behaviour required by each system helped simplify the implementation.

---

# ⚡ Event-Driven Architecture

Some systems communicate using events rather than continuously checking one another.

For example, when possession changes, a possession-change event can notify dependent systems.

<img width="672" height="232" alt="image" src="https://github.com/user-attachments/assets/ca00515c-a522-4db3-89fd-9db85b57ebda" />


This reduces unnecessary dependencies between components and makes systems easier to modify independently.

---

# 🎮 Rebindable Controls & Persistence

The game includes customisable controls.

Player key bindings can be changed through the interface and are stored using **JSON**, allowing them to persist between sessions.

The system includes checks for invalid or duplicate bindings.

---

# 🧱 Object-Oriented Design

The project uses object-oriented programming throughout its architecture.

Concepts applied include:

- Encapsulation
- Composition
- Aggregation
- Interfaces
- Polymorphism
- Separation of responsibilities

Different gameplay systems are split into specialised classes rather than placing all functionality within individual player objects.

This helped keep the project maintainable as its complexity increased.

---

# 🧪 Testing & Robustness

I created a structured **66-test test plan** covering both normal gameplay and edge cases.

Testing included:

### Functional Testing

- Passing
- Movement
- Tackling
- Rucks
- Lineouts
- Scoring
- Conversions
- Control rebinding

### State Testing

Testing transitions between states such as:

```text
Open Play → Tackle → Ruck → Open Play
```

and:

```text
Try → Conversion → Kick-off
```

### Edge Cases

Examples included:

- Rapid repeated user inputs
- Attempting to pass with no valid receiver
- Pausing during gameplay events
- Invalid state transitions
- Duplicate manager objects
- Missing/null references

Defensive checks and validation were added where necessary to prevent invalid game states.

---

# 📋 Development Process

The project followed a structured software-development process:

```text
Requirements
     ↓
Stakeholder Research
     ↓
System Design
     ↓
Algorithm Design
     ↓
Implementation
     ↓
Iterative Testing
     ↓
Evaluation
```

Feedback was collected from potential users and used to influence gameplay and interface decisions.

The complete process was documented in a **~250-page development report**.

---

# 🛠️ Technologies

### Programming

- C#
- Object-Oriented Programming
- Algorithm design
- Event-driven programming
- Finite State Machines

### Development

- Unity
- Visual Studio
- Git / GitHub

### Concepts

- Game AI
- State management
- Vector mathematics
- Probability modelling
- Data structures
- Software architecture
- Testing and debugging
- JSON persistence

---

# 📂 Repository Structure

```text
rugby-simulation-unity/
│
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   ├── AI/
│   │   ├── Match/
│   │   ├── Passing/
│   │   ├── Rucks/
│   │   └── Lineouts/
│   │
│   ├── Scenes/
│   ├── Prefabs/
│   └── UI/
│
├── Documentation/
│   └── Technical Report.pdf
│
├── media/
│   ├── gameplay.gif
│   └── screenshots/
│
└── README.md
```

> The exact repository structure may differ from the structure above depending on the final cleaned Unity project.

---

# 📖 Technical Documentation

The project is supported by a **~250-page technical development report** documenting:

- Requirements analysis
- Stakeholder research
- System architecture
- Class design
- Sequence diagrams
- Finite state machines
- Algorithm design
- Pseudocode
- Implementation
- Testing
- Evaluation

A shortened technical overview is provided in this README so the project can be understood without reading the complete report.

<!-- Add report link once uploaded:
[📄 View Full Technical Report](Documentation/Technical-Report.pdf)
-->

---

# 🚀 What I Learned

This project gave me experience taking a large software project from an initial idea through requirements analysis, architecture and algorithm design to implementation and testing.

The biggest challenge was coordinating many interacting gameplay systems while keeping the architecture manageable.

Developing the project strengthened my understanding of:

- Designing software before implementing it
- Breaking complex systems into smaller components
- Selecting appropriate algorithms and data structures
- Designing finite state machines
- Debugging interactions between independent systems
- Testing complex state-based behaviour
- Iteratively improving a system using user feedback

---

## Author

**William Jephcott**

BEng Robotics Engineering with Artificial Intelligence  
University of Warwick

[LinkedIn](YOUR_LINKEDIN_URL)
