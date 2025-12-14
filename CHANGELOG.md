# Changelog
## 1.5.3
### General
- Fixed being unable to add items (TVFLabs)
### Act 1
- Added Totem selector (TVFLabs)
### Act 2
- Fixed Draw Card buttons now working during battles

## 1.5.2
### General
- Fixed item overriding affecting the wrong slot
### Act 1
- Clarified what 'Toggle Map Override' actually does
- Added 'Override Current Region' button

## 1.5.1
### General
- Fixed Skip Next Node not working

## 1.5.0
### General
- Fixed consumable menu being broken
- Fixed ProgressionData buttons not correctly learning/unlearning entries
- Fixed AbilityInfo info list having broken formatting
- Fixed 'Add Card to Hand' discarding temporary mods
- Added option to reroll rare card choice
- Added Gems manager popup for card battles
- Rearranged some elements in the card battle menu

### Magnificus Act
- Added debug support for MagnificusMod

### Part 3
- Fixed consumables menu displaying and modifying the wrong items
- Added Fast Travel option to Map menu
- Removed 'Toggle All Nodes' option due to unintended behaviours and lack of utility

## 1.4.1
### General
- Fixed filter in AbilityInfo window being case sensitive when it shouldn't
- Added additional hotkey options
- Updated Change Hotkeys menu UI

### Game Info
- Added new subwindow 'Show Progression Data'
	- Allows you to learn and unlearn cards, abilities, items, mechanics

### Deck Editor
- Fixed 'Add Card' scroll menu overflow
- Fixed 'Edit Abilities' menu not showing multiple entries for stacking sigils
- Moved sigil modification toggles below management tabs
- Modified text of sigil and special ability management tabs to clarify purpose and distinguish them from each other

## 1.4.0
### General
- Fixed GUIClip errors when exiting to main menu during a battle
- Fixed Show AbilityInfos menu not working correctly
- Fixed Show Dialogue Events menu not working correctly
- Relocated size toggles for the main window to the drag bar
- Updated information shown in the Show AbilityInfos menu
- Internal refactoring of sequence helper classes and BaseAct classes

### Act 2
- Fixed Currency menu not referencing GBC's save data
- Added 'Reload' and 'Restart' functionality

### Act 3
- Fixed Currency menu referencing Runstate.Run instead of Part3SaveData.Data
- Fixed deck review view breaking when editing cards after obtaining the Mox Module
- Added 'Reload and 'Restart' functionality
- P03 KCM: Enabled Bones submenu

### Grimora
- Disabled 'Force Emission' button
- Items submenu is now disabled in the vanilla Grimora Act
- GrimoraMod: Enabled Currency submenu
- GrimoraMod: Added support for several mod sequencers
- GrimoraMod: Added debug functionality for Hammer
- GrimoraMod: Removed 'Reload' and 'Restart' buttons due to broken functionality

### Trigger Sequence Menu
- Added toggle to show only working sequences for the current act
- Added toggle to hide the mod GUID on the buttons
- Triggering an unhandled node sequence now displays the 'Trigger Sequence' button in the menu
- 'SpecialCardSequence' menu now displays the sequence's name as the header instead

### Card Battles
- Fixed the 'Draw Tutor' button being clickable while Tutor sequence is still active
- Added new function to create a new card in your hand
- Added new function to clear current Bones
- Added new function to reset the scale balance
- Renamed 'Auto-Win' and 'Auto-Lose' buttons to 'WIN Battle' and 'LOSE Battle'

### Game Board
- Fixed unnecessary horizontal scroll bar in the 'Replace/Create Card in Slot' list
- Added new function to modify the selected slot's SlotModificationType
- Added new function to reset the selected card's taken damage
- Added new function to reset the selected card's shields
- Rearranged menu elements
- 'Modify card' popup window now updates when you select a new slot
- Renamed 'No triggers' button to 'Ignore Triggers'

### Deck Editor
- Fixed forced portraits not updating the emission sprite
- Fixed forced portraits not working in Act 2
- Fixed 'Force X Portrait' buttons changing size when toggling
- Fixed additional Force Portrait buttons appearing when they shouldn't
- Added support for card name replacements (thank you JustinBustin5)
- Added 'Duplicate Selected Card' button
- Added toggle to search for cards by their sigils
- Added support for cards that cost multiple of the same colour Mox
- Added buttons to increase Attack and Health by 2
- Adjusted cost button spacings
- Menu now shows a card's display name alongside their internal name
- Ability search field will now compare against an ability's rulebook and either its AbilityBehaviour.Name or enum
- Act 2: Added toggle to the Card Finder for adding a card to the collection as well as the deck
- Act 2: Adding cards to your deck while the collection is open will now update the panel contents
- Act 2: Removing a card from your deck will remove it from the collection as well

## 1.3.0
### General
- Fixed 'Reload Act' and 'Restart Act' buttons' functionality being reversed
- Fixed Boss Battle opponent selection menu altering OpponentManager.AllOpponents
- Added config option (and buttons) to rescale menu window sizes
- Added config option to make the 'Take/Deal Damage' and 'Auto-win/lose' buttons deal scale damage all at once instead of individually
- Boss Battle opponent selection menu now shows the name of custom bosses instead of the enum number
- Improved UI
- Game elements behind the debug window can no longer be interacted with

### Deck Editor
- Fixed searching sigils by name not considering the enum name
- Added ability to add sigils marked as fromMerge, fromTotem, and/or fromLatch
- Added ability to mark a card's current sigils as fromMerge, fromTotem, fromLatch
- Sigil buttons now display their rulebook name next to their icon
- Sigils displayed in the 'Edit' abilities section now indicate if they're from a card mod or not (and if so, what kind of mod)
- Buttons for changing pages in the sigils section now wrap around to the beginning and end
- Can now display the custom API alternate portraits and force them to display
- Can now nullify gem costs and remove them individually
- Can now display custom costs added through the API

### Game Board
- Fixed error spam when the menu is open outside of a battle
- Fixed error when using the menu to replace a card on the board
- Fixed the Modify Card button not updating when changing the current selection
- Added buttons to clear the board and queue to the menu

### Act 2
- Fixed error spam during intro animation and during various sections like opening card packs or entering card battles

## 1.2.2
### General
- Using the deck editor while viewing your deck will now properly update the displayed card(s)
- Fixed the deck editor ability search not returning sigils with matching internal names

## 1.2.1
### General
- Fixed 'Deal Damage' options in Game Board menu not working

### Grimora
- Slightly condensed card battle information

## 1.2.0
### General
- Game Board menu:
	- Added ability to modify cards
	- Added buttons for damaging/healing cards
	- Minor visual improvements
- GameInfo menu:
	- Added 'Debug Tools' button for manipulating built-in debug options (debug keys and disable KCM oil painting)
	- Minor tweaks to Scenes list and count to improve readability

### Act 1
- Fixed minor error spam when moving to a new region

## 1.1.3
### General
- Acts 2, 3, Grimora, and Magnificus now display the current turn number during combat
- Fixed DrawCard and DrawSideCard hotkey functions not being properly bound during Awake()
- Fixed hotkey function error message not correctly displaying what functionId is erroring
- Added 'Show AbilityInfos' submenu under Game Info menu

### Act 1
- Fixed Turn Number being on the same line as Difficulty
- Improved readability of CurrentNode information

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