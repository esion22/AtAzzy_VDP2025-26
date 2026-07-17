# At Azzy's - Video Game Design and Programming

This repository contains the source code, game builds, and design documents for **At Azzy's**, a 2D top-down farming and cooking/simulation game. The project was developed for the **Video Game Design and Programming** (VDP) course at **Politecnico di Milano** (Academic Year 2025-2026), taught by **Prof. Pier Luca Lanzi**. 

The game received a grade of **29/30**, rated by both the course professors and fellow students.

* **Playable WebGL Build**: [At Azzy's on itch.io](https://polimi-game-collective.itch.io/at-azzys)

## Team Members
* Gabriel Leonardi (Team Leader, Developer)
* Sidar Cicek (Developer)
* Lorenzo Meroi (Developer, Sound & Music)
* Francesco Poloni (Art, Environment Design)
* Jiaxin Yang (Developer)

---

## Game Overview & Vision

*At Azzy's* is a 2D top-down pixel-art management and role-playing game. The player takes the role of Luke, an apprentice mage who must keep the village tavern running after its beloved owner, Azzy, has been suddenly transformed into a tiny dragon by a mysterious hooded figure.

Under the guidance of Azzy, the player manages the tavern and searches for a way to return Azzy back to his human self. To run the tavern successfully, the player uses magical spellcasting to tend to the garden, grow crops, and cook food for the townsfolk.

### The Two Gameplay Loops
In-game days are divided into two distinct, alternating phases:
* **Morning / Afternoon (Cozy Phase)**: Focuses on garden care and market square visits. The player can harvest crops planted earlier, choose which seeds to plant, visit the market, and browse the recipe book to decide which ingredients are needed. There is no time limit; the tavern only opens when the player decides.
* **Evening (Fast-Paced Cooking Phase)**: Initiated when the player opens the tavern doors. This is a time-management cooking phase where the player must prepare food for customers before they lose patience and leave. Orders only include recipes that can be prepared with the player's current resources. Multitasking is crucial, as spells can be cast from a distance to operate cooking stations while serving customers.

---

## Deliverables and Repository Structure
* **[AtAzzySource](./AtAzzySource)**: The complete Unity project folder (Assets, Packages, ProjectSettings).
* **[Documents/GDD](./Documents/GDD)**: Game Design Documents.
  * **[final_GDD.pdf](./Documents/GDD/final_GDD.pdf)** / **[final_GDD.md](./Documents/GDD/final_GDD.md)**: Full design specifications detailing world architecture, mechanics, UI, audio systems, and UML diagrams.
* **[Executables](./Executables)**: Local game builds (Prototype WebGL, Beta WebGL, Final WebGL, and Windows Standalone).
  * *Note: These builds are excluded from Git version control due to file size limits, but the playable web version can be accessed directly on itch.io.*
* **[References](./References)**: Asset citations.

---

## Asset Credits and Redistribution Policy

### Credits
A full list of asset credits is documented in **[References/references.txt](./References/references.txt)**.
* **Graphics**: Pixel art tilesets and sprites derived from *Tiny Farm RPG* (Emanuelledev) and *Pixel Crawler* (Anokolisa) asset packs.
* **Audio**: Sound effects (SFX) and background music tracks sourced from *OpenGameArt.org*.

### Non-Distribution Policy
To comply with the licensing terms of the third-party assets (particularly the commercial *Tiny Farm RPG* asset pack), **no copyrighted graphical textures (spritesheets) or audio files are distributed in this repository**. 

During development, all raw asset files were stored in a separate local directory that is excluded from this repository. If you open this project in Unity, you will encounter missing references (e.g. missing sprites or silent audio). To run the project locally in Unity, you must download/purchase the respective asset packs and place them in the correct `Assets/` subfolders.

---

## License
The source code written by the team is licensed under the MIT License. Embedded commercial asset packs remain subject to their original licenses.
