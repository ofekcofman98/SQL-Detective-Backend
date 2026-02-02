***SQL Detective***

SQL Detective is a cross-platform detective investigation game where players solve cases using SQL queries.
The game is built in Unity (PC + Mobile) and communicates with a custom .NET backend and Supabase PostgreSQL database.

**🎮 Game Overview**

* PC: the main investigation platform (exploration, dialogue, story progression).

* Mobile: becomes an interactive SQL controller using a visual drag-and-drop Query Builder.

* Players progress through missions, uncover clues, and unlock new database tables as they solve each step of the case.


**🔍 Key Features:**

  ✔ Visual SQL Query Builder (mobile)
  * Drag & drop SELECT, FROM, WHERE, AND, JOIN, value inputs, and table/column selections. 
    * Multi-condition WHERE with AND logic
    * Dynamic Clause rendering system
    * Condition-indexed button logic
    * Fully modular UI rendering with Populator classes
  
  ✔ Dynamic Story & Mission System
  * Driven entirely by ScriptableObjects:
    * SQL missions (query-based progression)
    * Non-SQL missions (interact, explore, unlock)
    * Popup tutorial missions
    * Each mission unlocks new tables, clues, or sequences
  
  ✔ Supabase Cloud Database
  
  ✔ Custom Backend (C#, .NET)
  * A dedicated backend handles:
    * Game state saving/loading
    * PC & Mobile communication
    * Query relay & validation
    * Unique session keys
    * Server polling and event routing
  
  ✔ Cross-Device Sync
  * PC and mobile communicate through:
    * Backend session APIs
    * Unique key pairing
    * Realtime polling channels
    * Full game progress synchronization
  
  ✔ 3D Interactive World
  * Players explore scenes, interact with objects, access clues, and uncover the criminal step-by-step.
 
**🛠️ Tech Stack**
* Unity (C#):
  * PC + Mobile implementations
  * Drag-and-drop UI framework
  * Modular QueryBuilder architecture
  * DataGridDisplayer (Excel-like dynamic grid)
  * Mission system
  * Schema Displayer with FK arrows
  * Event-driven managers

* Backend (.NET 8 Web API):
  * REST APIs for:
  * Game progress
  * Query relay
  * Session pairing
  * Clean modular service structure
  * Deployed via Docker

* Database (Supabase PostgreSQL):
  * RPC functions
  * Dynamic schema queries
  * Cloud-hosted tables for gameplay
  * Uses Supabase Auth + Row Level Security (RLS disabled in dev)


**🚀 Play the Game**
🎥 Gameplay Demo: https://youtu.be/9zejBFtsaI8

🕹️ Play on Itch.io (PC + Mobile) : https://ofekcofman98.itch.io/sql-detective-real

Mobile play requires pairing with a PC via unique session key.
