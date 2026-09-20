using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{   
     [SerializeField] private TileColorDatabase_SO tileColorDatabase;
    [SerializeField] private GameObject cellBackgroundPrefab;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Sprite[] tileSprites;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SpawnCellBackground(Vector2 position, Transform parent, int x, int y)
    {
        if(cellBackgroundPrefab == null)
         return;

         GameObject background = Instantiate(
            cellBackgroundPrefab, 
            position, 
            Quaternion.identity, 
            parent);
         background.name = $"CellBackground_{x}_{y}";
    }
    public Tile SpawnTile(
        Vector2 position,
        Transform parent,
        int x,
        int y,
        TileType tileType,
        int colorIndex)
    {
        Sprite sprite = ResolveSprite(colorIndex);
        if (tilePrefab == null || sprite == null)
            return null;

         GameObject tileObject = Instantiate(
            tilePrefab,
            position,
            Quaternion.identity,
            parent
         );

         tileObject.name = $"Tile_{x}_{y}";
         Tile tile = tileObject.GetComponent<Tile>();
         if(tile == null)
        {
            Destroy(tileObject);
            Debug.LogError("[TileSpawner] Tile prefab requires a Tile component.");
            return null;
        }
         tile.Setup(x,y,tileType,colorIndex,sprite);
         return tile;
    }
    public int GetRandomSafeColorIndex(Tile[,] grid, int x, int y)
    {
        List<int> validIndices = GetAvailableColorIndices();

        if (validIndices.Count == 0)
            return 0;

        RemoveHorizontalMatchColor(grid, x, y, validIndices);
        RemoveVerticalMatchColor(grid, x, y, validIndices);

        if (validIndices.Count == 0)
            return 0;

        return validIndices[Random.Range(0, validIndices.Count)];
    }
    private Sprite ResolveSprite(int colorIndex)
    {
        if(tileColorDatabase != null)
        {
            Sprite databaseSprite = tileColorDatabase.GetSprite(colorIndex);
               if (databaseSprite != null)
                return databaseSprite;
        }
        if (tileSprites == null || tileSprites.Length == 0)
            return null;

        int index = Mathf.Clamp(colorIndex, 0, tileSprites.Length - 1);
        return tileSprites[index];
    }
    private List<int> GetAvailableColorIndices()
    {
        var indices = new List<int>();
        if (tileColorDatabase != null)
        {
            foreach (var entry in tileColorDatabase.entries)
            {
                if (entry != null)
                    indices.Add(entry.id);
            }

            return indices;
        }

         if (tileSprites != null)
        {
            for (int index = 0; index < tileSprites.Length; index++)
                indices.Add(index);
        }

        return indices;

    }
    private static void RemoveHorizontalMatchColor(
        Tile[,] grid,
        int x,
        int y,
        List<int> validIndices)
    {
        if (x < 2 || grid[x - 1, y] == null || grid[x - 2, y] == null)
            return;

        int previousColor = grid[x - 1, y].spriteID;
        if (previousColor == grid[x - 2, y].spriteID)
            validIndices.Remove(previousColor);
    }
    private static void RemoveVerticalMatchColor(
        Tile[,] grid,
        int x,
        int y,
        List<int> validIndices)
    {
        if (y < 2 || grid[x, y - 1] == null || grid[x, y - 2] == null)
            return;

        int previousColor = grid[x, y - 1].spriteID;
        if (previousColor == grid[x, y - 2].spriteID)
            validIndices.Remove(previousColor);
    }
    
}
