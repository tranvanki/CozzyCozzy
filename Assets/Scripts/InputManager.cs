using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Tile selectedTile;
    private Vector2 startTouchPos;

    [SerializeField] private BoardManager boardManager;
    [SerializeField] private SwapHandler swapHandler;
    [SerializeField] private MatchResolver matchResolver;

    private void Update()
    {
        if (boardManager == null || boardManager.grid == null ||
            (matchResolver != null && matchResolver.IsResolving))
            return;

        bool pressedThisFrame = false;
        bool releasedThisFrame = false;
        Vector2 currentPos = default;

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            // Read press and release states unconditionally
            pressedThisFrame = touch.press.wasPressedThisFrame;
            releasedThisFrame = touch.press.wasReleasedThisFrame;
            if (touch.press.isPressed || releasedThisFrame)
            {
                // Read position only if the touch is active or just released
                currentPos = touch.position.ReadValue();
            }
           
        }
    
        if (!pressedThisFrame && !releasedThisFrame && Mouse.current != null)
        {
            pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
            currentPos = Mouse.current.position.ReadValue();
        }

        if (pressedThisFrame)
        {
            Vector2 worldPos = Camera.main ? Camera.main.ScreenToWorldPoint(currentPos) : currentPos;
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
                Debug.Log($"[TOUCH] pressed pos={currentPos} world={worldPos} camera={(Camera.main != null)} hit={(hit != null ? hit.gameObject.name : "NULL")}");

            Tile tile = hit != null ? hit.GetComponentInParent<Tile>() : null;
                Debug.Log($"[TOUCH] tileFound={(tile != null)}");
    
            if (tile != null)
            {
                selectedTile = tile;
                startTouchPos = currentPos;
            }
        }

        if (releasedThisFrame && selectedTile != null)
        {
            Vector2 delta = currentPos - startTouchPos;
            Debug.Log($"[RELEASE] delta={delta} magnitude={delta.magnitude}");

            if (delta.magnitude <= 30f && selectedTile.type == TileType.Bomb)
            {
                if (matchResolver != null)
                    matchResolver.ProcessBombActivation(selectedTile);

                selectedTile = null;
                return;
            }

            if (delta.magnitude > 30f)
            {
                Vector2Int dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
                    : new Vector2Int(0, (int)Mathf.Sign(delta.y));

                int nx = selectedTile.x + dir.x;
                int ny = selectedTile.y + dir.y;
                bool validCell = boardManager != null && boardManager.IsValidCell(nx, ny);
                Tile targetTile = validCell ? boardManager.grid[nx, ny] : null;
                Debug.Log($"[RELEASE] dir={dir} target=({nx},{ny}) validCell={validCell} targetNull={targetTile == null} swapHandlerNull={swapHandler == null}");

                if (validCell && targetTile != null && swapHandler != null)
                {
                    swapHandler.TrySwap(selectedTile, targetTile);
                    Debug.Log("[RELEASE] TrySwap called");
                }
            }

            selectedTile = null;
        }
    }
}
