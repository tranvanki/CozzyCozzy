using UnityEngine;

[CreateAssetMenu(fileName = "TileColorDatabase", menuName = "Puzzle/TileColorDatabase")]
public class TileColorDatabase_SO : ScriptableObject
{
    [System.Serializable]
    public class ColorEntry
    {
        public int id;
        public string colorName;
        public Sprite sprite;
    }

    public ColorEntry[] entries = new ColorEntry[0];

    public Sprite GetSprite(int id)
    {
        if (entries == null) return null;

        foreach (var entry in entries)
        {
            if (entry != null && entry.id == id)
                return entry.sprite;
        }

        Debug.LogError($"[TileColorDatabase] Không tìm thấy sprite cho colorIndex = {id}");
        return null;
    }

    public bool HasColor(int id)
    {
        if (entries == null) return false;

        foreach (var entry in entries)
        {
            if (entry != null && entry.id == id)
                return true;
        }

        return false;
    }
}
