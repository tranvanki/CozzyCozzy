using UnityEngine;
using System.Collections.Generic;

public enum PowerUpType { None, Striped, Wrapped, ColorBomb }

public class PowerUpSpawner : MonoBehaviour 
{
    public PowerUpType DetermineSpawn(List<Tile> group, bool isLShapeOrTShape)
    {
        // match 5 dạng L/T → bom vùng
        if (isLShapeOrTShape) return PowerUpType.Wrapped;      
        
        // match 5 thẳng hàng → phá toàn màu (Đưa lên trước match 4 để an toàn)
        if (group.Count >= 5) return PowerUpType.ColorBomb;     
        
        // match 4 thẳng hàng → phá hàng/cột
        if (group.Count == 4) return PowerUpType.Striped;       
        
        // match 3 → không sinh power-up
        return PowerUpType.None;                                 
    }
}