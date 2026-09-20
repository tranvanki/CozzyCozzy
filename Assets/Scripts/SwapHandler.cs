using UnityEngine;

public class SwapHandler : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MatchResolver matchResolver; 


    public void TrySwap(Tile a, Tile b)
    {
        if (boardManager == null || gameManager == null || matchResolver == null || a == null || b == null)
        {
            return;
        }

        if (matchResolver.IsResolving)
            return;

        AudioManager.Instance.PlaySwapSfx();

        bool swapIncludesBomb = a.type == TileType.Bomb || b.type == TileType.Bomb;
        SwapInGrid(a, b);

        if (swapIncludesBomb)
        {
            gameManager.OnValidMove();
            matchResolver.ProcessBombActivation(a.type == TileType.Bomb ? a : b, true);
            return;
        }

        var matches = MatchChecker.FindAllMatches(boardManager.grid, boardManager.width, boardManager.height);

        if (matches.Count == 0)
        {
            SwapInGrid(a, b);
        }
        else
        {
            gameManager.OnValidMove();
            matchResolver.ProcessMatches(matches);
        }
    }

    void SwapInGrid(Tile a, Tile b)
    {
        boardManager.grid[a.x, a.y] = b;
        boardManager.grid[b.x, b.y] = a;

        (a.x, b.x) = (b.x, a.x);
        (a.y, b.y) = (b.y, a.y);
        Vector3 posA = a.transform.position;
    a.transform.position = b.transform.position;
    b.transform.position = posA;
    }
    
}

