using UnityEngine;
using System.Collections.Generic;

public static class MatchChecker
{
    public static List<List<Tile>> FindAllMatches(Tile[,] grid, int width, int height)
    {
        var result = new List<List<Tile>>();

        // Quét ngang
        for (int y = 0; y < height; y++)
        {
            int runStart = 0;
            for (int x = 1; x <= width; x++)
            {
                bool sameAsPrev = x < width && grid[x, y] != null && grid[runStart, y] != null
                                   && grid[x, y].colorIndex == grid[runStart, y].colorIndex;
                if (!sameAsPrev)
                {
                    int runLength = x - runStart;
                    if (runLength >= 3)
                    {
                        var group = new List<Tile>();
                        for (int k = runStart; k < x; k++) group.Add(grid[k, y]);
                        result.Add(group);
                    }
                    runStart = x;
                }
            }
        }

        // Quét dọc — tương tự, đổi trục x/y
        for (int x = 0; x < width; x++)
        {
            int runStart = 0;
            for (int y = 1; y <= height; y++)
            {
                bool sameAsPrev = y < height && grid[x, y] != null && grid[x, runStart] != null
                                   && grid[x, y].colorIndex == grid[x, runStart].colorIndex;
                if (!sameAsPrev)
                {
                    int runLength = y - runStart;
                    if (runLength >= 3)
                    {
                        var group = new List<Tile>();
                        for (int k = runStart; k < y; k++) group.Add(grid[x, k]);
                        result.Add(group);
                    }
                    runStart = y;
                }
            }
        }

        return result;
        // Lưu ý: 2 group ngang+dọc giao nhau tại 1 điểm (hình chữ L/T) cần merge lại
        // ở bước ResolveMatches để xác định đúng shape cho PowerUpSpawner (mục 3.4)
    }


}
