using UnityEngine;
public enum TileType { Normal, Crate, Frozen, RowClear, Bomb, ColorBomb }

public class Tile : MonoBehaviour
{   
    public int x,y;
    public TileType type;
    public int durability = 1; // crate can be destroyed after many hits
    public int spriteID; // ID for sprite
    public int colorIndex; // matches use this to compare tile colors
    

    public void Setup(int gridX, int gridY, TileType tileType, int id, Sprite sprite)
    {
        x = gridX;
        y = gridY;
        type = tileType;
        spriteID = id;
        colorIndex = id;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
