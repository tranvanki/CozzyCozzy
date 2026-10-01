# File 01 - Game Design Document

**Project:** CozzyCozzy  
**Version:** 0.2  
**Status:** Living document  
**Engine / Platform:** Unity 6.3 / Android  
**Genre:** Casual Puzzle - Match-3 Swap

## Document Rules

This Markdown file is the repository source for the Google Docs document. Paste its content into Google Docs and apply the status colors below. Google Docs is the review and publishing surface; this file preserves the approved structure with the project source.

| Tag | Google Docs color | Meaning |
| --- | --- | --- |
| `[BLACK]` | Black | Implemented and verified in the current project. |
| `[RED]` | Red | New scope to read and implement in the next approved development phase. |
| `[GREEN]` | Green | Planned design, not implemented and not committed to the next phase. |

Rules:

- Every change updates the Changelog and changes the affected text color in Google Docs.
- Use `Unit` as the generic gameplay actor term. For this Match-3 project, playable Units are tiles; `Troop`, `Enemy`, and `Boss` are not in scope.
- Use `Blocker` for an object that restricts a board cell, `Goal` for a win requirement, `Power-up` for an in-board special tile, and `Booster` for an external player action.
- All data documents must use stable IDs. This document defines schemas and ID rules only; it contains no live level or balance data.

## Table Of Contents

1. Product Scope
2. Core Gameplay
3. Game Flow
4. Board, Camera, and Control
5. Board Units, Blockers, Goals, and Power-ups
6. Resolution, Combo, and Scoring Logic
7. Data Architecture and ID Rules
8. Progression, Map, and Level Types
9. Shop, Economy, and Live Features
10. UI, Audio, and Technical Systems
11. Formula Inputs for File 05 Flowchart
12. Out-of-Scope Unit Combat Model
13. Development Roadmap
14. Changelog

## 1. Product Scope

### 1.1 Product Definition

`[BLACK]` CozzyCozzy is a single-player Match-3 puzzle game. The player swaps adjacent tiles to form matches, clears color Goals before moves reach zero, and resolves cascades until the board stabilizes.

`[BLACK]` The current playable build contains Main Menu, one gameplay scene, color-collection Goals, move limit, score, Bomb Power-up, deadlock reshuffle, and Win/Lose UI.

`[GREEN]` The full product direction supports a map-based level sequence, multiple Blocker types, external Boosters, level rewards, and retry loops.

### 1.2 Non-Goals

`[BLACK]` Current scope has no combat map, Troop, Enemy, Boss, equipment, physical damage, magical damage, critical damage, or skill-stat progression.

`[GREEN]` Combat systems must be specified in a separate design document before becoming part of this project. They are not assumed by File 01.

## 2. Core Gameplay

### 2.1 Core Loop

`[BLACK]` Start level -> create board -> player swaps two adjacent tiles -> validate match -> revert invalid swap or resolve valid match -> clear tiles -> refill board -> resolve cascades -> update Goals, score, and moves -> evaluate Win/Lose -> retry, return to menu, or continue.

### 2.2 Start State

`[BLACK]` A level initializes from `LevelData_SO`: board width, height, move limit, color Goals, cell states, and optional predefined tile colors.

`[BLACK]` The board prevents immediate horizontal or vertical Match-3 during random generation and reshuffles when it has no available action.

`[BLACK]` The current build also converts the normal tile nearest board center into an initial Bomb.

`[RED]` Define whether initial Bomb creation remains a global rule, becomes a level property, or is removed. The implementation and data contract must agree.

### 2.3 Win Rule

`[BLACK]` Win when every configured color Goal has remaining count equal to zero.

`[RED]` Define behavior for a level with no configured Goals. Current code does not auto-win such a level.

### 2.4 Lose Rule

`[BLACK]` Lose when `MovesLeft <= 0` and at least one Goal remains.

`[BLACK]` A move is consumed only by a valid match swap or Bomb activation. An invalid swap returns to its original positions without consuming a move.

### 2.5 Board Resolution Loop

`[BLACK]` A valid match clears matching tiles, may create a Bomb, activates any Bomb included in the clear, applies gravity, refills valid cells, and repeats matching until no further match remains.

`[BLACK]` After resolution, the game checks Win/Lose, then reshuffles normal tiles if no action is available.

## 3. Game Flow

### 3.1 Main Flow

`[BLACK]` Main Menu -> Play -> `World1` -> gameplay -> Win/Lose popup.

`[BLACK]` Retry reloads the active gameplay scene. Back to Menu loads `MainMenu`.

`[RED]` Continue must load the latest unlocked level. The button exists visually but has no active handler.

### 3.2 State Model

| State ID | State | Entry | Exit |
| --- | --- | --- | --- |
| `GAME_STATE_MENU` | Main Menu | App launch, back to menu | Play / Continue |
| `GAME_STATE_INITIALIZING` | Build board and UI | Gameplay scene load | Board playable |
| `GAME_STATE_PLAYER_INPUT` | Accept tile touch or swipe | Board stable | Valid action |
| `GAME_STATE_RESOLVING` | Clear, cascade, refill | Valid match or Bomb | Board stable |
| `GAME_STATE_WIN` | Show win popup and lock input | All Goals complete | Retry / Menu / Continue |
| `GAME_STATE_LOSE` | Show lose popup and lock input | Moves exhausted | Retry / Menu |

`[BLACK]` `MatchResolver.IsResolving` blocks swaps during resolution. `GameOverUI` disables `InputManager` after Win or Lose.

## 4. Board, Camera, and Control

### 4.1 Board Structure

`[BLACK]` Board size is defined per level by `LevelData_SO.width` and `height`. The current configured level uses a 6 x 8 grid.

`[BLACK]` A cell has state `Empty`, `Normal`, `BoxObstacle`, or `IceObstacle`. `Empty` cells cannot hold or refill tiles.

`[RED]` Define the canonical source of level shape: `cells` or `rows`. Current runtime uses `cells` whenever its array length is valid, so `rows` is not reapplied to an already populated level.

### 4.2 Camera, Zoom, and Pan

`[BLACK]` Gameplay uses a fixed orthographic camera. There is no player zoom or pan implementation.

`[GREEN]` If boards exceed the fixed viewport, use bounded pinch zoom and pan. Camera bounds must always keep the full active board reachable and must not allow the board to leave the viewport entirely.

### 4.3 Input and Movement Logic

`[BLACK]` Android input reads `Touchscreen.current.primaryTouch`; Editor fallback reads mouse input.

`[BLACK]` On press, the game finds a tile through `Physics2D.OverlapPoint`. On release, a drag longer than 30 screen pixels chooses the dominant horizontal or vertical direction.

`[BLACK]` Diagonal swaps are not allowed. The neighbor must be within bounds, valid for the level cell, and occupied.

`[BLACK]` A tap on an in-board Bomb directly activates it.

`[RED]` Add and verify `EnhancedTouchSupport.Enable()` only if device testing confirms that the current primary-touch polling is unreliable. The current source does not enable Enhanced Touch.

### 4.4 Interaction Controls

| Control ID | Input | Current behavior | Status |
| --- | --- | --- | --- |
| `CONTROL_SWIPE_SWAP` | Swipe tile to cardinal neighbor | Tentative swap, match validation, revert or resolve | `[BLACK]` |
| `CONTROL_TAP_BOMB` | Tap in-board Bomb | Detonate 3 x 3 area and spend one move | `[BLACK]` |
| `CONTROL_BOOSTER_BOMB` | HUD Bomb button | Scene binding targets a missing `ActivateBombMode` method | `[RED]` |
| `CONTROL_BOOSTER_SHOVEL` | HUD Shovel button | No gameplay behavior | `[GREEN]` |
| `CONTROL_BOOSTER_PAINT_ROLLER` | HUD Paint Roller button | No gameplay behavior | `[GREEN]` |
| `CONTROL_SETTINGS` | Settings button | No settings UI or mute control in current source | `[GREEN]` |

## 5. Board Units, Blockers, Goals, and Power-ups

### 5.1 Board Unit Table

| ID | Object | Creation | Interaction | Clear Rule | Status |
| --- | --- | --- | --- | --- | --- |
| `TILE_NORMAL` | Colored tile | Initial board or refill | Swap and match | Clear in a Match or explosion | `[BLACK]` |
| `TILE_BOMB` | 3 x 3 Power-up | Initial center tile or Match-4+ | Tap, swap, or match inclusion | Clears its 3 x 3 area | `[BLACK]` |
| `BLOCKER_CRATE` | Crate cell | Level cell state | Intended adjacent-match damage | Currently behaves as colored tile and can be directly cleared | `[RED]` |
| `BLOCKER_FROZEN` | Frozen cell | Level cell state | Intended adjacent-match damage | Currently behaves as colored tile and can be directly cleared | `[RED]` |
| `POWERUP_ROW_CLEAR` | Row/column clear Power-up | Match-4 design | Not implemented | Not implemented | `[GREEN]` |
| `POWERUP_COLOR_BOMB` | Color clear Power-up | Match-5 design | Not implemented | Not implemented | `[GREEN]` |

### 5.2 Blocker Rules

`[RED]` Crate and Frozen must have explicit visual, collision, matchability, damage radius, durability, and drop/refill rules. The existing `durability` field is unused.

`[GREEN]` Recommended default rule: Blockers do not participate in color matches; an adjacent clear reduces durability by one; at zero durability the Blocker clears; a cleared Blocker counts toward its configured Goal only when the level data declares it.

### 5.3 Goals

`[BLACK]` Current Goal type is color collection. When a cleared tile's color matches a configured Goal ID, remaining count decreases by one.

`[GREEN]` Planned Goal types: clear Blocker, collect object, reach score, and deliver object. Each must define an ID, target value, and clear event source.

### 5.4 Power-up and Combo Rules

`[BLACK]` A matching group of four or more produces a Bomb at the middle element of the detected group. That Bomb is spared from the current clear.

`[BLACK]` A Bomb included in a later match triggers a 3 x 3 area clear. Chain-affected Bombs are also cleared, but the resolver does not recursively calculate a second explosion from those chain-added Bombs.

`[RED]` Define merged L/T groups before using `PowerUpSpawner`; current match groups are separate horizontal and vertical runs, so shape classification is not connected to resolution.

`[GREEN]` Planned combo matrix: Bomb + Bomb, Bomb + RowClear, Bomb + ColorBomb, RowClear + RowClear, and ColorBomb + ColorBomb. Each combination requires a deterministic area rule, effect order, and score event.

## 6. Resolution, Combo, and Scoring Logic

### 6.1 Match Rules

`[BLACK]` `MatchChecker` scans contiguous horizontal and vertical runs. Three or more tiles with equal `colorIndex` form a match.

`[BLACK]` The current checker does not merge intersecting horizontal and vertical groups before resolution.

### 6.2 Clear and Refill Rules

`[BLACK]` Cleared tiles are removed from the grid. In each column, remaining tiles shift downward; each valid empty cell is then refilled with a normal tile.

`[BLACK]` Refill color selection avoids creating an immediate horizontal or vertical Match-3 using the two previously generated neighbors.

### 6.3 Current Score Rule

`[BLACK]` Score is granted per cleared tile, based on cascade level: first cascade 10, second 20, third 30, fourth and later 50.

`[GREEN]` Future scoring must define: base score by match type, Power-up creation score, Power-up activation score, combo score, and a maximum cascade multiplier. Do not replace current scoring until a data-backed formula is approved.

### 6.4 Deadlock Rule

`[BLACK]` A board is playable if it contains a Bomb or a hypothetical swap with the right/up neighbor produces any match.

`[BLACK]` If no action exists, normal tiles are shuffled with Fisher-Yates for up to 100 attempts. A result is accepted only when it has no existing match and has an available action.

`[RED]` Validate deadlock reshuffle with configured Blockers, because only `Normal` tiles are shuffled.

## 7. Data Architecture and ID Rules

### 7.1 General Rules

`[BLACK]` Every data record used by future design/data files must have a unique string ID. IDs are immutable after release; display names are editable localization content.

`[BLACK]` ID format: uppercase prefix, underscore, identifier. Example: `LEVEL_WORLD_001`, `GOAL_COLOR_RED`, `BOOSTER_BOMB`.

`[BLACK]` IDs must not encode balance values, language, dates, or ordering that may change.

### 7.2 Required Data Contracts

| Data ID prefix | Record | Required fields | Consumer |
| --- | --- | --- | --- |
| `LEVEL_` | Level | `id`, `mapId`, `boardWidth`, `boardHeight`, `moveLimit`, `cellLayoutId`, `goalIds`, `initialRuleIds` | Level runtime |
| `CELL_LAYOUT_` | Board layout | `id`, `width`, `height`, `cells` | Board runtime |
| `GOAL_` | Goal | `id`, `type`, `targetId`, `targetAmount` | Game manager / HUD |
| `TILE_` | Tile definition | `id`, `colorId`, `spriteId`, `matchable` | Tile runtime |
| `BLOCKER_` | Blocker definition | `id`, `durability`, `clearTrigger`, `spriteId`, `goalCountRule` | Resolver |
| `POWERUP_` | In-board Power-up | `id`, `creationRule`, `activationRule`, `effectRuleId` | Resolver |
| `BOOSTER_` | External Booster | `id`, `charges`, `targetingRule`, `effectRuleId`, `priceId` | HUD / Shop |
| `MAP_` | World map | `id`, `order`, `levelIds`, `unlockRuleId` | Map UI |
| `REWARD_` | Reward | `id`, `type`, `amount`, `sourceRuleId` | Progression |
| `PRICE_` | Shop price | `id`, `currencyId`, `amount` | Shop |

### 7.3 Current Runtime Data

`[BLACK]` Current runtime uses `LevelData_SO` for size, move limit, color Goals, `rows`, and `CellData[]`; `TileColorDatabase_SO` maps color integer IDs to names and sprites.

`[RED]` Replace integer color references with named data IDs only through a migration plan; do not silently reorder existing `colorIndex` values.

## 8. Progression, Map, and Level Types

### 8.1 Maps

`[BLACK]` Main Menu uses a World1 background image. There is no selectable map progression runtime.

`[GREEN]` Map categories: `MAP_MAIN` for the current world sequence, `MAP_EVENT` for time-limited content, and `MAP_TUTORIAL` for mandatory onboarding. Each map has ordered level IDs and unlock rules.

### 8.2 Level Types

`[BLACK]` Current level type: move-limited color collection.

`[GREEN]` Future level types: Blocker clear, object collection, score target, and mixed Goal. A level type is a Goal composition, not a separate board engine.

### 8.3 Completion, Stars, Retry, and Unlock

`[BLACK]` Retry is available through Game Over UI. No Star progression or level unlock persistence is implemented.

`[GREEN]` Completion contract: award 1-3 Stars from explicit threshold rules, persist best Stars per `LEVEL_` ID, unlock the next level after minimum Star condition, and allow retry without consuming a life until a life system is defined.

## 9. Shop, Economy, and Live Features

`[BLACK]` No currency, purchase, inventory, shop, lives, daily reward, ad reward, or analytics system is implemented.

`[GREEN]` Shop scope: purchase external Booster charges and optional retry resources. Every offer needs `OFFER_`, `PRICE_`, reward IDs, availability rule, and receipt handling before implementation.

`[GREEN]` Live feature scope: daily reward, events, notifications, A/B tests, remote config, and analytics. These require separate technical design and privacy review; they are not included in core gameplay implementation.

## 10. UI, Audio, and Technical Systems

### 10.1 UI

`[BLACK]` Main Menu and Game Over use UI Toolkit. Gameplay HUD for Moves and color Goals uses UGUI/TextMeshPro.

`[BLACK]` Game Over popup displays Win score or Lose state, locks input, and exposes Retry and Main Menu.

`[RED]` Validate Game Over UI on Android builds. The implementation assumes all named UXML elements and assigned references are present.

### 10.2 Audio

`[BLACK]` `AudioManager` is a `DontDestroyOnLoad` singleton with separate music and SFX `AudioSource` instances. It loads background music plus swap, match, and Bomb clips from Resources.

`[BLACK]` Swap plays on every tentative swap; match plays when resolving match-generated groups; Bomb plays when a matched Bomb activates.

`[GREEN]` Add independent music/SFX mute toggles with persistent settings. Define `SETTING_AUDIO_MUSIC_MUTED` and `SETTING_AUDIO_SFX_MUTED` keys before implementation.

### 10.3 Technical Architecture

| System | Owner | Responsibility | Status |
| --- | --- | --- | --- |
| Board | `BoardManager` | Grid, spawn, refill, playable-board check | `[BLACK]` |
| Tile | `Tile` | Coordinates, type, color, visual sprite | `[BLACK]` |
| Matching | `MatchChecker` | Horizontal/vertical run detection | `[BLACK]` |
| Swap | `SwapHandler` | Tentative swap, revert, valid action | `[BLACK]` |
| Resolution | `MatchResolver` | Clear, Bomb creation, cascade, end check | `[BLACK]` |
| Bomb | `BombHandler` | Bomb conversion and 3 x 3 area | `[BLACK]` |
| Game state | `GameManager` | Moves, score, Goals, Win/Lose events | `[BLACK]` |
| Input | `InputManager` | Touch/mouse selection, swipe direction | `[BLACK]` |
| Level data | `LevelData_SO` | Level board and Goal configuration | `[BLACK]` |
| Color catalog | `TileColorDatabase_SO` | Color ID to sprite mapping | `[BLACK]` |
| Power-up rules | `PowerUpSpawner` | Proposed type selection only; not wired | `[GREEN]` |

## 11. Formula Inputs for File 05 Flowchart

`[BLACK]` File 05 must model the implemented resolution order: input -> tentative swap -> match validation -> revert or consume move -> resolve groups -> convert eligible tile to Bomb -> clear tiles and activated Bomb area -> score/Goals -> gravity/refill -> cascade test -> Win/Lose -> deadlock test -> player input.

`[BLACK]` Current score formula for tile $i$ at cascade $c$ is:

$$
Score_i(c) =
\begin{cases}
10 & c = 1 \\
20 & c = 2 \\
30 & c = 3 \\
50 & c \geq 4
\end{cases}
$$

`[RED]` File 05 must add the missing decision points before Blocker and external Booster implementation: target validity, Blocker durability update, effect area, affected-Bomb recursion, and goal-event emission.

`[GREEN]` Physical Damage, Magical Damage, Skill Damage, Critical, Unit Level, Troop, Enemy, and Boss formulas are not applicable to this project. Do not create formulas for them unless the product scope changes to add combat Units.

## 12. Out-of-Scope Unit Combat Model

| Unit rank | Current project behavior | Status |
| --- | --- | --- |
| Troop | Not used | `[BLACK]` |
| Enemy | Not used | `[BLACK]` |
| Boss | Not used | `[BLACK]` |

`[GREEN]` If a future game mode introduces combat, define Unit classes, stats, spawn logic, targeting, damage formulas, skill formulas, critical rules, and level-growth formulas in a combat design document before adding them to File 01.

## 13. Development Roadmap

### 13.1 Implemented Baseline

`[BLACK]` Board generation, swipe swap, match validation, invalid-swap revert, cascade/refill, color Goals, move limit, score, Bomb Power-up, deadlock reshuffle, audio playback, HUD, Main Menu, and Game Over flow.

### 13.2 Next Approved Scope

`[RED]` Repair the external Bomb button binding and define its targeting/charge rule.

`[RED]` Implement Crate and Frozen as actual Blockers with tested durability and clear-event rules.

`[RED]` Choose and implement one canonical board-layout source (`cells` or `rows`).

`[RED]` Define and implement Continue persistence, or remove the inactive button.

### 13.3 Planned Scope

`[GREEN]` RowClear, ColorBomb, power-up combos, Shovel, Paint Roller, settings mute UI, map unlocks, Stars, level progression, shop, economy, and live features.

## 14. Changelog

| Version | Date | Scope | Updated sections |
| --- | --- | --- | --- |
| `0.2` | 2026-09-22 | Rebuilt File 01 as project-wide GDD source. Documented actual implementation, statuses, ID contracts, non-goals, roadmap, and Google Docs color convention. | All sections |
| `0.1` | Before 2026-09-22 | Initial working gameplay description. | Legacy document |
