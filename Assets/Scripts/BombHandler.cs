using UnityEngine;
using System.Collections.Generic;
public class BombHandler : MonoBehaviour
{   
    private const string BombSpritePath = "PuzzleAssets/Boosters/Icons/Bomb";
    private Sprite bombSprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   //BombHandler trả về danh sách tile trong 3x3
   

   private void Awake()
    {
        bombSprite = Resources.Load<Sprite>(BombSpritePath);
        if(bombSprite == null)
        {
            Debug.LogError($"[BombHandler] Missing sprite at Resources/{BombSpritePath}");
        }
    }
    public void ConvertToBomb(Tile tile)
    {
        if(tile == null || bombSprite == null)
        return;
        tile.type = TileType.Bomb;
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null)
            spriteRenderer.sprite = bombSprite;

    }
     public List<Tile> GetAreaTiles(
        Tile[,] grid,
        int width,
        int height,
        int centerX,
        int centerY,
        int radius = 1)
    {
        var tiles = new List<Tile>();

        for(int offsetX = -radius; offsetX <= radius;offsetX++)
        {
            for(int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                 int x = centerX + offsetX;
                int y = centerY + offsetY;
                if(x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue;
                }
                Tile tile = grid[x, y];
                if (tile != null)
                    tiles.Add(tile);
                }
            }
            return tiles;
        
        
    }

}