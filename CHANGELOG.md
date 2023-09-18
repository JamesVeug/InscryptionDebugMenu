# Changelog

## 1.1.3
### Act 1
- Fixed Turn Number being on the same line as Difficulty
- Improved readability of CurrentNode information

### General
- Acts 2, 3, Grimora, and Magnificus now display the current turn number during combat

## 1.1.2
### General
- Fixed last update's 'Auto-Win' and 'Auto-Lose' fix only applying to Act 1
- 'Remove Max Energy' now uses the API extension method (definitely didn't add it to the API specifically for this update, no sirree)
- 'Add Energy' now also increases max energy if possible
- Draw card buttons will now be disabled if there are no remaining cards in the deck
- 'Deal Damage' and 'Take Damage' now work even if Player/Opponent Damage is disabled

### Act 3
- Fixed 'Auto-Win' and 'Auto-Lose' buttons causing an error (non-fatal)

## 1.1.1
### General
- Fixed 'Auto-Win' and 'Auto-Lose' buttons not working when Opponent or Player damage is disabled, respectively
- Fixed visual menu error when selecting 'Totem Battle' or 'Boss Battle' from the 'Trigger Sequence' menu
- Fixed '()' appearing on disabled buttons

## 1.1.0
### General
- Added config option to display item buttons vertically rather than horizontally
- Minor spelling and capitalisation fixes
- Game Info window now shows both the starting seed and the current seed, if possible
- Current node information now shows the id and node instance separately
- Changed card battle difficulty to show the combined total along with the modifier and base difficulties
- Changed starting console logs to LogLevel 'Debug'

### Act 1
- 'Replenish Candles' now relights your candles

### All Acts
- Added Game Board popup for manipulating cards and slots on the board
- Added +1/-1 buttons for currency, changed currency display

## 1.0.2
### General
- Added options to force card emissions, switch to alternate portraits
- Deck Viewer now shows API alternate pixel portraits, if they exist
- Fixed deck editor not completely working in Kaycee's Mod
- Removed BepInEx as a dependency (redundant)

## 1.0.1
### All Acts
- Added toggles for disabling direct damage to the player or opponent (persistent settings)

### Act 1
- Fixed Draw Tutor locking camera controls

### Act 2
- Fixed cursor interaction being disabled when opening a card pack with dialogue disabled = true

## 1.0.0
### General
- Added `F2` hotkey to draw from deck
- Added `LeftShift + F2` hotkey to draw from side deck
- Fixed having to release all buttons before activating the next hotkey
- Fixed (hopefully) error on boot when loading hotkeys
- API requirement bumped to 2.x.x
- Minor UI touch ups

### All Acts
- Added Dialogue Window (Thanks WhistleWind!)
- Added Draw Tutor button to select card from your deck to be added to your hand (Thanks WhistleWind!)

### Deck Editor
- Added support for Grimora Mod
- Added GUID filter when modifying sigils
- Added name and GUID filter when modifying special abilities
- Fixed not all Special abilities showing up 
- Cards in deck show using their display now instead of .name

### Act2
- Fixed null Error spam when showing debug menu in act 2 (Thanks WhistleWind!)

### Magnificus Act / Mod
- Added light support

## 0.9.0
### All Acts
- Ported and extended Deck editor from the Deck Editor mod
  - Works for all acts (only tested on Act 1 and 2)
  - Added support for adding gem costs
  - Added support to gemify a card
  - Added support for custom sigils and specials
  - Shows all portraits

## Act 2
- Fixed disable dialogue button not working

### General
- Windows now keep their position when closed and reopened (not saved)
- Minor adjustments to the UI


## 0.8.0
### General
- Added changeable Hotkeys (Saves through configs)
- Collapse buttons to save space

### All Acts
- Added buttons to change Energy during battles

## 0.7.0
### All Acts
- Added disable dialogue button (Saves through configs)

### Act 2
- Basic support to skip through card battles

## 0.6.1
### General
- Fixed act 1 totem fights always triggering Bird+Fecundity

## 0.6.0
### All Acts
- Added button to trigger any sequence (Card battles, gain totem... etc)

## 0.5.0
### Act 1
- Fixed Lag and Crash when loading expanded menu
- Button to draw card during battles
- Button to draw side deck card during battles
- Buttons to add/remove bones during battles

### Act 3 (PO3)
- Added basic support (Currency, Items... etc)
- Button to draw card during battles
- Button to draw side deck card during battles

### Grimora Act
- Added Item debug support
- Button to draw card during battles
- Button to draw side deck card during battles
- Buttons to add/remove bones during battles

### Misc
- Added FPS counter in Game info popup
- Item list shows Rulebook name and raw name of Item
- Resource Bank window redesigned with a filter and entrys are buttons now to copy them to clipboard.
- Moved Region override option to act 1


## 0.4.0
- Changed toggle button in header to off, minimal and maxed.
- Added options to change players items
- Added difficulty value to card battle
- Changed some windows to have a close button instead of toggle
- Bumped API requirement to 2.11