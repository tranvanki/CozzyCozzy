using UnityEngine;

public enum CellState { Empty, Normal, BoxObstacle, IceObstacle }

[System.Serializable]
public class CellData
{
    public CellState state = CellState.Normal;
    public int colorIndex = -1;
}

[System.Serializable]
public class TargetGoal
{
    public int colorIndex;
    public int amount = 1;
}

[CreateAssetMenu(fileName = "Level", menuName = "Puzzle/LevelData")]
public class LevelData_SO : ScriptableObject
{
    public int width = 6;
    public int height = 8;
    public int moveLimit = 24;
    public TargetGoal[] targetGoals = new TargetGoal[0];

    [TextArea(8, 8)]
    public string[] rows = new[]
    {
        ".NNNN.",
        "NNNNNN",
        "NNNNNN",
        "NNNNNN",
        "NNNNNN",
        "NNNNNN",
        "NNNNNN",
        ".NNNN."
    };

    public CellData[] cells = new CellData[48];

    private void OnValidate()
    {
        if (width <= 0) width = 6;
        if (height <= 0) height = 8;

        if (cells == null || cells.Length != width * height)
            cells = new CellData[width * height];

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] == null)
                cells[i] = new CellData();
        }

        if (targetGoals == null)
            targetGoals = new TargetGoal[0];
    }

    public CellData GetCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogError($"[LevelData_SO] Cell out of range: ({x}, {y}) for size {width}x{height}");
            return null;
        }

        return cells[y * width + x];
    }

    [ContextMenu("Generate Cells From Rows")]
    public void GenerateCellsFromRows()
    {
        if (rows == null || rows.Length == 0)
        {
            Debug.LogWarning("[LevelData_SO] Không có dữ liệu rows để generate");
            return;
        }

        int rowCount = rows.Length;
        int colCount = rows[0].Length;

        width = colCount;
        height = rowCount;
        cells = new CellData[width * height];

        for (int y = 0; y < rowCount; y++)
        {
            string row = rows[rowCount - 1 - y];
            if (row.Length != colCount)
            {
                Debug.LogWarning($"[LevelData_SO] Row {y} có độ dài {row.Length}, mong đợi {colCount}");
                continue;
            }

            for (int x = 0; x < colCount; x++)
            {
                var cell = new CellData();
                char symbol = row[x];

                if (symbol == '.')
                    cell.state = CellState.Empty;
                else if (symbol == 'C')
                    cell.state = CellState.BoxObstacle;
                else if (symbol == 'I')
                    cell.state = CellState.IceObstacle;
                else
                    cell.state = CellState.Normal;

                cell.colorIndex = -1;
                cells[y * width + x] = cell;
            }
        }
    }
}