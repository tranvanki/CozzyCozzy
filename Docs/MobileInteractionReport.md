# Mobile Puzzle Interaction Report

## Incident

On mobile, puzzle pieces render but a touch cannot select or swap them.

## Root Cause

`InputManager.Update()` finds a `Collider2D` using `Physics2D.OverlapPoint()`. In `Assets/Prefab/Tile.prefab`, the `CircleCollider2D` is on the `PuzzlePiece` child, while the `Tile` component is on the sibling `CellTile` object. Both are children of the prefab root.

The old lookup, `hit.TryGetComponent<Tile>(...)`, only inspected `PuzzlePiece`; it therefore failed to set `selectedTile`. Since swaps require a selected tile when touch is released, no drag could cause a swap.

## Implemented Fix

`InputManager.Update()` now resolves the tile with:

```csharp
Tile tile = hit != null ? hit.GetComponentInParent<Tile>() : null;
```

`GetComponentInParent<Tile>()` starts at the collider object and walks to the shared prefab root, where it can find the `Tile` component through its child hierarchy. It works for the current prefab layout without changing the tile visual or collider setup.

## Feature Workflow

1. `BoardManager.Start()` calls `InitializeBoard()`.
2. `BoardManager.InitializeBoard()` instantiates `tilePrefab` once per valid grid cell and calls `Tile.Setup()` to assign coordinates and sprite data.
3. A mobile finger-down is read from `Touchscreen.current.primaryTouch` in `InputManager.Update()`.
4. `InputManager` converts the screen position with `Camera.main.ScreenToWorldPoint()` and calls `Physics2D.OverlapPoint()` to find the touched 2D collider.
5. The collider is resolved to its logical `Tile`; `selectedTile` and `startTouchPos` are stored.
6. On finger-up, `InputManager` calculates `delta`. A drag longer than 30 pixels becomes a cardinal `Vector2Int dir`.
7. `BoardManager.IsValidCell()` verifies the neighbor. `SwapHandler.TrySwap()` receives the selected and destination tiles.
8. `SwapHandler.SwapInGrid()` exchanges their grid positions and coordinate fields.
9. `MatchChecker.FindAllMatches()` validates the result. Invalid swaps are reversed; valid swaps call `GameManager.OnValidMove()` to decrement moves and publish `OnMovesChanged`.

## Code Map

| File | Function or variable | Responsibility |
| --- | --- | --- |
| `Assets/Scripts/InputManager.cs` | `Update()` | Polls touch/mouse input and drives selection and drag swaps. |
| `Assets/Scripts/InputManager.cs` | `selectedTile` | Tile captured when a press hits a puzzle-piece collider. |
| `Assets/Scripts/InputManager.cs` | `startTouchPos` | Screen-space press origin used to calculate drag direction. |
| `Assets/Scripts/InputManager.cs` | `pressedThisFrame`, `releasedThisFrame`, `currentPos` | Per-frame input state normalized for touch and mouse. |
| `Assets/Scripts/InputManager.cs` | `boardManager`, `swapHandler` | Inspector references used to validate cells and request swaps. |
| `Assets/Scripts/BoardManager.cs` | `InitializeBoard()` | Instantiates the prefab and initializes `grid`. |
| `Assets/Scripts/BoardManager.cs` | `IsValidCell(int x, int y)` | Rejects out-of-bounds and empty destination cells. |
| `Assets/Scripts/BoardManager.cs` | `grid`, `width`, `height` | Authoritative tile positions and board dimensions. |
| `Assets/Scripts/Tile.cs` | `Setup(...)` | Sets `x`, `y`, `type`, `spriteID`, `colorIndex`, and displayed sprite. |
| `Assets/Scripts/SwapHandler.cs` | `TrySwap(Tile a, Tile b)` | Applies a tentative swap and validates matches. |
| `Assets/Scripts/SwapHandler.cs` | `SwapInGrid(Tile a, Tile b)` | Exchanges grid entries and each tile's `x`/`y` fields. |
| `Assets/Scripts/MatchChecker.cs` | `FindAllMatches(...)` | Returns matching tile groups after a tentative swap. |
| `Assets/Scripts/gameManager.cs` | `OnValidMove()` | Decrements `MovesLeft` and raises `OnMovesChanged`. |

## Verification

- `dotnet build Assembly-CSharp.csproj` completes with 0 warnings and 0 errors.
- On an Android/iOS device, press and drag a tile farther than 30 pixels to an adjacent valid cell.
- Confirm a swap that produces no match returns both pieces to their initial cells.
- Confirm a matching swap decrements the move counter once.

## Follow-up Risks

- `SwapHandler.TrySwap()` currently identifies a valid match but does not call `MatchResolver.ProcessMatches()`. Valid matches decrement moves but are not yet removed or collapsed by this path.
- Test the same gesture with device safe-area layouts and UI overlays; a full-screen UI raycast target can still intercept touch before game logic receives it.