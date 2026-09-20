using UnityEngine;
using System.Collections.Generic;
[RequireComponent(typeof(BombHandler))]
public class BoardManager : MonoBehaviour
{
    [SerializeField] private LevelData_SO levelData;
    [SerializeField] private TileColorDatabase_SO tileColorDatabase;
    [SerializeField] private GameObject cellBackgroundPrefab;
    private BombHandler bombHandler;
    public int width = 6;
    public int height = 8;
    public GameObject tilePrefab;
    public Sprite[] tileSprites;
    public Tile[,] grid;
    private GameObject[,] allTiles;

    private void Awake()
    {
        bombHandler = GetComponent<BombHandler>();

        if (levelData != null)
        {
            width = levelData.width;
            height = levelData.height;
        }
    }
    public void ConvertToBomb(Tile tile)
{   bombHandler.ConvertToBomb(tile);
   
}
/// Lấy toàn bộ Tile trong vùng (2*radius+1) x (2*radius+1) quanh tâm — mặc định radius=1 → vùng 3x3
public List<Tile> GetAreaTiles(int centerX, int centerY, int radius = 1)
    {
        return bombHandler.GetAreaTiles(
        grid,
        width,
        height,
        centerX,
        centerY,
        radius);
    }
    /// Xóa đúng 1 Tile tại (x,y) khỏi cả grid lẫn Scene — không tự collapse, để chỗ gọi tự quyết định thời điểm
public void DestroyTileAt(int x, int y)
    {
         if (grid[x, y] != null)
        {
            Destroy(grid[x,y].gameObject);
            grid[x, y] = null;
        }

    }
    private void Start()
    {
        InitializeBoard();
    }
    public bool IsValidCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return false;

        if (levelData != null)
        {
            var cell = levelData.GetCell(x, y);
            return cell == null || cell.state != CellState.Empty;
        }

        return true;
    }

    public bool HasAvailableAction()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = grid[x, y];
                if (tile == null)
                    continue;

                if (tile.type == TileType.Bomb)
                    return true;

                if (CreatesMatchAfterSwap(x, y, x + 1, y) || CreatesMatchAfterSwap(x, y, x, y + 1))
                    return true;
            }
        }

        return false;
    }

    public bool ShuffleUntilPlayable(int maxAttempts = 100)
    {
        var normalTiles = new List<Tile>();
        var positions = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = grid[x, y];
                if (tile != null && tile.type == TileType.Normal)
                {
                    normalTiles.Add(tile);
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }

        if (normalTiles.Count < 2)
            return false;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Shuffle(normalTiles);

            for (int index = 0; index < positions.Count; index++)
            {
                Vector2Int position = positions[index];
                Tile tile = normalTiles[index];
                grid[position.x, position.y] = tile;
                allTiles[position.x, position.y] = tile.gameObject;
                tile.x = position.x;
                tile.y = position.y;
                tile.transform.position = GetWorldPosition(position.x, position.y);
            }

            if (MatchChecker.FindAllMatches(grid, width, height).Count == 0 && HasAvailableAction())
                return true;
        }

        Debug.LogError("[BoardManager] Could not create a playable board after reshuffling.");
        return false;
    }

    private bool CreatesMatchAfterSwap(int firstX, int firstY, int secondX, int secondY)
    {
        if (!IsValidCell(secondX, secondY))
            return false;

        Tile firstTile = grid[firstX, firstY];
        Tile secondTile = grid[secondX, secondY];
        if (firstTile == null || secondTile == null)
            return false;

        grid[firstX, firstY] = secondTile;
        grid[secondX, secondY] = firstTile;
        bool createsMatch = MatchChecker.FindAllMatches(grid, width, height).Count > 0;
        grid[firstX, firstY] = firstTile;
        grid[secondX, secondY] = secondTile;

        return createsMatch;
    }

    private static void Shuffle(List<Tile> tiles)
    {
        for (int index = tiles.Count - 1; index > 0; index--)
        {
            int swapIndex = Random.Range(0, index + 1);
            (tiles[index], tiles[swapIndex]) = (tiles[swapIndex], tiles[index]);
        }
    }

    public void CollapseAndRefill()
    {
        for (int x = 0; x < width; x++)
        {
            int destinationY = 0;
            for (int y = 0; y < height; y++)
            {
                Tile tile = grid[x, y];
                if (tile == null)
                    continue;

                if (destinationY != y)
                {
                    grid[x, destinationY] = tile;
                    grid[x, y] = null;
                    allTiles[x, destinationY] = tile.gameObject;
                    allTiles[x, y] = null;
                    tile.y = destinationY;
                    tile.transform.position = GetWorldPosition(x, destinationY);
                }

                destinationY++;
            }

            for (int y = destinationY; y < height; y++)
            {
                if (!IsValidCell(x, y))
                    continue;

                int colorIndex = GetRandomSafeColorIndex(x, y);
                Sprite sprite = ResolveSprite(colorIndex);
                if (sprite == null)
                    continue;

                GameObject newTile = Instantiate(tilePrefab, GetWorldPosition(x, y), Quaternion.identity, transform);
                newTile.name = $"Tile_{x}_{y}";

                Tile tile = newTile.GetComponent<Tile>();
                tile.Setup(x, y, TileType.Normal, colorIndex, sprite);
                grid[x, y] = tile;
                allTiles[x, y] = newTile;
            }
        }
    }

    private Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(x - (width - 1) / 2f, y - (height - 1) / 2f);
    }
    

    public void InitializeBoard()
{
    allTiles = new GameObject[width, height];
    grid = new Tile[width, height];

    if (levelData != null)
    {
        if (levelData.cells == null || levelData.cells.Length != width * height)
            levelData.GenerateCellsFromRows();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = levelData.GetCell(x, y);
                if (cell == null || cell.state == CellState.Empty)
                    continue;

                var tileType = cell.state switch
                {
                    CellState.BoxObstacle => TileType.Crate,
                    CellState.IceObstacle => TileType.Frozen,
                    _ => TileType.Normal
                };

                int colorIndex = cell.colorIndex >= 0 ? cell.colorIndex : GetRandomSafeColorIndex(x, y);
                Sprite sprite = ResolveSprite(colorIndex);
                if (sprite == null)
                    continue;

                Vector2 tempPosition = new Vector2(x - (width - 1) / 2f, y - (height - 1) / 2f);

                
                if (cellBackgroundPrefab != null)
                {
                    var bg = Instantiate(cellBackgroundPrefab, tempPosition, Quaternion.identity, transform);
                    bg.name = $"CellBG_{x}_{y}";
                }
                // --------------------------------------------

                GameObject newTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity, transform);
                newTile.name = $"Tile_{x}_{y}";

                Tile tileScript = newTile.GetComponent<Tile>();
                tileScript.Setup(x, y, tileType, colorIndex, sprite);
                grid[x, y] = tileScript;
                allTiles[x, y] = newTile;
            }
        }

        SpawnInitialBomb();
        EnsurePlayableBoard();
        return;
    }

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            if (!IsValidCell(x, y)) continue;

            Vector2 tempPosition = new Vector2(x - (width - 1) / 2f, y - (height - 1) / 2f);

            if (cellBackgroundPrefab != null)
            {
                var bg = Instantiate(cellBackgroundPrefab, tempPosition, Quaternion.identity, transform);
                bg.name = $"CellBG_{x}_{y}";
            }

            GameObject newTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity, transform);
            newTile.name = $"Tile_{x}_{y}";

            int randomIndex = GetRandomSafeColorIndex(x, y);
            Sprite sprite = ResolveSprite(randomIndex);
            if (sprite == null)
                continue;

            Tile tileScript = newTile.GetComponent<Tile>();
            tileScript.Setup(x, y, TileType.Normal, randomIndex, sprite);
            grid[x, y] = tileScript;
            allTiles[x, y] = newTile;
        }
    }

    SpawnInitialBomb();
    EnsurePlayableBoard();
}

    private void EnsurePlayableBoard()
    {
        if (!HasAvailableAction())
            ShuffleUntilPlayable();
    }

    private void SpawnInitialBomb()
    {
        Tile closestTile = null;
        float closestDistance = float.MaxValue;
        Vector2 boardCenter = new Vector2((width - 1) / 2f, (height - 1) / 2f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = grid[x, y];
                if (tile == null || tile.type != TileType.Normal)
                    continue;

                float distance = Vector2.SqrMagnitude(new Vector2(x, y) - boardCenter);
                if (distance < closestDistance)
                {
                    closestTile = tile;
                    closestDistance = distance;
                }
            }
        }

        if (closestTile != null)
            ConvertToBomb(closestTile);
    }

    private Sprite ResolveSprite(int colorIndex)
    {
        if (tileColorDatabase != null)
        {
            Sprite sprite = tileColorDatabase.GetSprite(colorIndex);
            if (sprite != null)
                return sprite;
        }

        if (tileSprites != null && tileSprites.Length > 0)
        {
            int clampedIndex = Mathf.Clamp(colorIndex, 0, tileSprites.Length - 1);
            return tileSprites[clampedIndex];
        }

        return null;
    }

    private int GetRandomSafeColorIndex(int x, int y)
    {
        List<int> validIndices = new List<int>();

        if (tileColorDatabase != null)
        {
            foreach (var entry in tileColorDatabase.entries)
            {
                if (entry != null)
                    validIndices.Add(entry.id);
            }
        }
        else if (tileSprites != null)
        {
            for (int i = 0; i < tileSprites.Length; i++)
                validIndices.Add(i);
        }

        if (validIndices.Count == 0)
            return 0;

        if (x >= 2 && grid[x - 1, y] != null && grid[x - 2, y] != null)
        {
            int id1 = grid[x - 1, y].spriteID;
            int id2 = grid[x - 2, y].spriteID;
            if (id1 == id2 && validIndices.Contains(id1))
                validIndices.Remove(id1);
        }

        if (y >= 2 && grid[x, y - 1] != null && grid[x, y - 2] != null)
        {
            int id1 = grid[x, y - 1].spriteID;
            int id2 = grid[x, y - 2].spriteID;
            if (id1 == id2 && validIndices.Contains(id1))
                validIndices.Remove(id1);
        }

        if (validIndices.Count == 0)
            return 0;

        return validIndices[Random.Range(0, validIndices.Count)];
    }
}
