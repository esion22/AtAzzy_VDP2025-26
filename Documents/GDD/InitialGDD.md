# Overview and vision statement

## Introduction

*At Azzy's* is a 2D TopDown pixel-art management and role-playing game for PC. The player takes the role of an apprentice mage who must keep the village tavern running after its owner has been transformed into a tiny dragon.

## Description

In *At Azzy's* the player must manage the tavern and help the original owner, Azzy, return to human form. To run the tavern the player uses spells in many different ways: to grow ingredients in the tavern garden and to cook for the townsfolk. Not every ingredient can be grown in the garden, so the player will visit the village market to buy additional ingredients and unlock new recipes for the tavern.

In-game days are divided into two gameplay segments:

* Morning / Afternoon — This segment focuses on garden care and market visits and is intended to be cozy and relaxing. The player can harvest crops planted earlier, choose which seeds to plant, visit the market, and browse the recipe book to decide which ingredients are needed for specific dishes. New recipes are not all available at the start; the player unlocks them over time by progressing through the game and the recipe book. There is no time limit during this segment: the tavern only opens when the player decides to open it.

* Evening — The evening begins when the player opens the tavern. This segment is a fast-paced cooking phase in which the player must prepare food for customers before they become dissatisfied and leave. The player takes orders, prepares dishes, and serves townsfolk within limited time, earning gold for successful service. Orders include only recipes that can be prepared with the player’s current resources. A day ends when either the player runs out of ingredients or a fixed number of guests have entered the tavern. Cooking heavily involves creative use of spells, since they can be used from a distance, which makes multitasking a crucial part of the gameplay.

Spells are fundamental and are used for both farming and cooking. Example spells include:

* grow spell
* water spell
* fire spell
* cut spell

Their specific uses in gameplay are described in later sections of this document.

Azzy, in dragon form, accompanies the player throughout the adventure. Azzy serves as the player’s mentor, provides advice, assists during the tavern’s opening hours, and functions as the in-game tutorial during the first minutes of gameplay.


# Characters
Below is a list of all the characters and character groups that will be present in the game and what their role will be:

- ### Azkoonn (Azzy)
  Azkoonn has been the tavernkeeper of "At Azzy's" for decades, but he was now suddently turned into a tiny dragon by a mysterious hooded figure.
  Even if he may look small, his temper did not change one bit: he is still kind to every customer and ready to help Luke to the best of his abilities.
  His role will be the one of a mentor, teaching Luke how to tend to his garden and how to serve amazing meals to the customers.
- ### Luke (Main Character)
  They were an apprentice learning the art of spellcasting under the guidance of a wizard who lives in a tower in the nearby city, but once he discovered that the beloved town's tavern would close they stepped up and became the new tavernkeeper.
  Under the guidance of Azkoonn they will manage the tavern and find a way to turn Azkoonn back to his human self.
- ### Shopkeepers
  These characters reside in the village square, each one selling their hard earned wares:
	- Sophie, a pretty saleswoman who sells seeds that his husband produces.
	- Michael, a young and hopefull fisherman who sells the fish he catches. 
	- Hugo, a big middle-aged man who butchers animals for other people.
	- Rose, An old sweet farmer lady that can only manage to tend to their animals, selling eggs and dairy products.
  Their role is to provide otherwise unobtainable ingredients to Luke and immerse them in the village life.
- ### Customers
  These are the townfolk that will come in during the dinner shift at the tavern and order there.
  Their role is to immerse the player into the village life with short dialogue boxes which can be personalized for each customer, who could return after a couple of days.


# Story

Slyfall is a small village, located in a long and narrow valley, well known throughout for the high morale of his townfolk.
You are an apprendice mage, commuting everyday to study the tomes at the tower of the prestigious Thorin, a powerful mage who lives next town over.
While passing through Slyfall you are used to greet everyone, including Azkoonn, the tavernkeeper, who was usually tending to his crops in the back, but today the sight was different: a hooded figure burst out of the tavern and disappeared into the nearby forest, you rush into the tavern to find that Azkoonn has been turned into a tiny dragon!
Not even Azkoonn knows who did it, but not "At Azzy's" must close down since he cannot cook food or tend to the garden like this, which is where you step in!
Thorin always said that practice is what makes you learn the fastest, so which better opportunity to use your spells then cooking with them.
A lot of challenges await, will you manage to grow your own ingredients and keep the tavern going? Will the crops wither away or flourish once again? Who would ever want to inflict a curse on the beloved small town tavernkeeper?
Legends say that there exists a recipe capable of dispelling any curse, will you be able to discover it and cook it?

# World Design / Scenery

## Overview

This section describes all scenes of At Azzy's; the world is divided into three main scenes: Tavern, Garden, and Market.
Each scene has its own characteristics detailed in the table below:

- In the tavern the player is able to cook food with ingredients and to sell it to customers, by using the kitchen.
- The Garden supports the farming gameplay and depicts a field which is framed by fences.
- The Market provides the trading functionality with several shops.

A detailed description of the gameplay is available in the Gameplay section. Generally speaking, each scene should express a cozy and relaxed atmosphere. The color palette and lightning is warm and pastel to underline the cozy feeling of the world. On top on that, the sound design and visual details support the mood with relaxing music and vegetation.

## Scenes

| Scene      | Gameplay Role             | Objects                                                                    | Composition                    | Mood & Emotion        | Colors                    | Lighting      | Sound                  | Environment Details           |
|------------|---------------------------|----------------------------------------------------------------------------|--------------------------------|-----------------------|---------------------------|---------------|------------------------|-------------------------------|
| **Tavern** | Cooking & selling<br>food | Cooking station, chairs,<br>tables, cupboards, shelves,<br>pots, customers | Split: kitchen & customer area | Magical, rustic, cozy | Warm, pastel,<br>low sat. | Warm, relaxed | Music & cooking sounds | —                             |
| **Garden** | Farming                   | Crops, fences                                                              | Fences frame crops             | Cozy                  | Warm, pastel,<br>low sat. | Daylight      | Music & farm SFX       | Trees, grass,<br>fruits, pond |
| **Market** | Trading area              | Houses, Shops (seeds,<br>fish, meat, dairy), Library, Roads                | Aligned side by side           | Cozy                  | Warm, pastel,<br>low sat. | Daylight     | Music                  | Trees, grass                  |

## Tavern - scenic composition
![Tavern Scene](../images/tavern_composition.png)


# Gameplay

## General Gameplay Elements

**Movement**: The player will be able to move using the WASD keys. There is no need for additional movement (like jumping or running)

**Interactions**: The player can interact with objects by clicking them with the mouse. Sometimes the player might need to be on the tile next to the object to interact with it.

**Inventory**: A simple inventory system is necessary. This can be achieved with a UI element that opens when pressing the **B** key or by clicking on a small bag icon on the screen. (The game is too short to expect the player to remember all the keys; a small icon on the screen can help if they get confused.)

**UI Elements**: A spell UI element that tells the player which key to press for each spell, There will be a small hotbar containing the spells avaliable to the player.

**Selector UI** element indicates the current seed or ingredient selected. With the mouse scroll wheel, the player can change the selected item. This helps minimize the number of times the player opens the inventory.

---

## Spells

* The spells can be used with the number keys **1–4**, probably with a toggle system rather than a “hold to use” system. 
  All the spells can be directed with the mouse. 

---

## Farming Gameplay

During the morning, the player is able to collect all the ingredients for the meals. The player can travel between two different screens: **the Farm** and **the Market**.

**The Market** is structured as a market square where the player can interact with some NPCs and market stalls to buy seeds, meat, fish, and dairy products. Interacting with NPCs triggers dialogues and brings up a UI element with the list of purchasable items. The player can buy items only if they have enough money.

**The Farm** is structured as a garden with some farming tiles. By interacting with these tiles, the player can plant seeds. The plants need to be watered with the **water spell**, and their growth can be sped up with the **growth spell**. If these two spells are used, the plants will be fully grown by the next day (this allows the vertical slice to be completed in about 20 minutes). The crops can be harvested with the **cut spell**.

During the first day (the tutorial), the player will be taught how to plant, water, and grow the plants.
At the start of the second day, the market is unlocked.

**List of interactable elements** that need to be implemented:

* NPCs and market stalls
* UI elements
* Farming tiles
* Seeds and plants
* Unwanted bushes that can be burned (might not be implemented)

---

### Spells in Farming

* **Grow Spell**: Speeds up the growing process to one day. Maybe for more sophisticated plants, it takes longer.
* **Water Spell**: Waters the plants and allows them to grow.
* **Fire Spell**: Burns bushes to make space for new plants. If used on your own plants, it burns them.
* **Cut Spell**: Allows harvesting the crops.

## Cooking System - Gameplay

The cooking system serves as the core gameplay mechanic, integrating **movement control**, **spellcasting**, **order management**, and **time pressure** to create an immersive tavern management experience. Players must utilize spells to complete customer orders within time limits for generous rewards.

## II. Core Gameplay Mechanics

### 2. Order Management System
- **Order Acceptance Process**: Customers wait in the reception area, players take their orders before their wait time runs out
- **Order Interface**: Real-time display at the top of the screen:
  - Unique customer portrait
  - Specified dish requirements
  - Preparation countdown (customer leaves if timeout)
- **Multitasking**: Supports handling multiple orders simultaneously
- **Delivery**: The player will hand deliver each order to the correct customer

### 3. Spell System
- **Control Method**: Number keys (1/2/3/4) combined with the mouse position to control the range of the spells
- **Spell System**:
  - **Fire Spell**: Exclusive to stone stoves, used for boiling and roasting
  - **Water Spell**: Provides water filling, dough mixing, and fire extinguishing functions
  - **Cut Spell**: Processes ingredients, completing cutting and grinding tasks
  - **Growth Spell**: Creates ingredient enlargement effects


---

## Progressive Game Progression

### Phase One: Basic Tutorial (First Day)
**Teaching Objective**: Master the basic cooking operation cycle

**Guidance Content**:
- Dragon guides the opening of first day operations
- Step-by-step tutorial through "Baked Potatoes" recipe:
  - Fire spell to bake the potato
  - Cut spell to open up the potato
- Core mechanism learning:
  - Order acceptance and interface recognition
  - Safe stove usage standards
  - Understanding delivery mechanisms
- Enter settlement after serving fixed customer flow

### Phase Two: Advanced Challenges (Second Day)
**Progression Goal**: The player will learn the impact of his decisions from the previous day (amount of money earned and crops planted).


**New Content**:
- The market will open up, allowing the player to buy new recipes
- The recipe book will be given to the player, highlighting missing ingredients to unlock new recipes

**System Deepening**:
- Importance of variety in crop planting
- Multitasking coordination will be needed for the cooking part

---

## Interactive Environment Design

### 1. Kitchen Area Division
- **Stone Stove Area**: Only safe zone permitted for heating operations
- **Wood Preparation Area**: Dedicated to ingredient preprocessing, prohibits placing hot utensils
- **Operation Standards**: All ingredient processing must be completed on counters

### 2. Safety Management System
- **Heating Restrictions**: Fire spell only activates on stone stoves or fireplaces
- **Fire Warning**: Hot utensils or fire spells contacting wooden surfaces trigger fires
- **Emergency Handling**: Water spell can extinguish fires promptly

---

## Strategic Planning System

### Cooking Efficiency Optimization
- **Order Sequencing**: Determine priority based on urgency and reward value
- **Space Utilization**: Reasonably allocate counter resources for parallel operations
- **Pathway Design**: Optimize routes for ingredient retrieval, processing, and cooking

---

## Interface and Feedback System

### 1. Real-time Status Display
- **Task Status**: Real-time updates of player and dragon assistant task progress
- **Resource Occupancy**: Visually displays stove usage status

### 2. Contextual Operation Guidance
- **Skill Prompts**:
  - Highlight fire spell when approaching stoves
  - Display placement key when holding ingredients
  - Prompt cut spell when ingredients are on wooden counters
  - Indicate water spell during fire emergencies
- **Order Warnings**: Special highlighting for urgent orders
- **Safety Alerts**: Trigger warnings when hot utensils contact danger zones

---

## VIII. Recipe System

Example recipes: 
### 1. Baked Potatoes
* **Description**: Basic tutorial dish, mastering the usage of spells
* **Required Ingredients**: Potatoes
* **Cooking Steps**:
  1. Take potatoes from ingredient menu
  2. Place potato in the **stone oven**, use **Fire Spell** to bake it
  3. Use plate to retrieve the potato
  4. Place on **wooden counter** and use **Cut Spell** to pen up the potato
  5. Serve to customer

### 2. Pan-Seared Steak
* **Description**: Main course testing heat control mastery
* **Required Ingredients**: Steak
* **Cooking Steps**:
  1. Place steak on **stone stove** frying pan
  2. Use **Fire Spell** to pan-sear
  3. Control timing for desired doneness
  4. Plate and serve to customer

---

## Core Design Goals

### Experience Focus
- **Operation Precision**: Accurate coordination of movement and spellcasting
- **Spatial Awareness**: Efficient utilization of kitchen layout
- **Process Optimization**: Efficiency maximization under constraints

### Difficulty Balance
- **Operation Depth**: Enhanced technicality through movement control
- **Strategic Dimension**: Delivery limitations foster tactical choices
- **Safety Standards**: Dedicated stoves improve operational accuracy requirements 
- **Timing**: Timely orders reward extra money, waiting too much will lead to customers leaving



# Technical Specification

## Controls

At Azzy's is a PC game played with keyboard and mouse. The player moves using the **WASD** keys; the mouse and additional keyboard keys are used to interact with objects, cast spells, and cook. The game is developed specifically for keyboard and mouse because the mouse’s fast movements are critical for managing multitasking during the cooking segment. Adapting the controls for gamepads or other controllers can be considered later, but it is not a development priority.

### Rebindable controls

Priority for rebindable controls is optional and can be implemented later; it is not a development priority for the initial build. When implemented, players should be able to remap any keyboard key and mouse button used for gameplay, including movement, interact, hotbar slots, spells and menu actions. The system should support saving and loading control profiles and include a “Restore defaults” option. The in-game UI for this feature should be a simple keybinding screen that shows clear prompts and warns the player about conflicting bindings.

### Mouse settings

Mouse settings are also optional and can be added later. When exposed, settings should include a sensitivity slider that affects cursor and aim responsiveness and a way to change cursor size (either a slider or small/medium/large options). Because mouse precision is central to cooking and multitasking gameplay, these options are recommended for the polished build to give players more control over aiming and cursor behavior.

### Saving the game

The game should autosave at key points to minimize lost progress: at the start of each in-game day (after the morning setup is complete), at the end of each in-game day (after the evening service completes) and at regular time periods. A manual save option should be available from the pause menu and create a manual save slot. When the player quits the game it should resume from the most recent autosave or from the last manual save; if the player quits during an evening shift, the evening should resume from the last autosave so progress prior to the current evening is not lost. To improve robustness, keep a small number of backup autosave files (for example the last three autosaves), show clear save timestamps in the load menu, and prompt the player before overwriting manual save slots.

---

# Deadlines 

## 21/10/2025

Everyone has to finish their assigned part of the GDD
- Lorenzo: Technical Specification, Overview and vision statement
- Laya: Gameplay (cooking part)
- Francesco: Gameplay (farming part)
- Gabriel: Characters, Story
- Sidar: Scenery

## 31/10/2025

Everyone will experiment on unity projects similar to ours (2d top down)