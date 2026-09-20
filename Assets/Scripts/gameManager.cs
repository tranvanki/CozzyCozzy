using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelData_SO currentLevel;

    public int MovesLeft { get; private set; }
    public int Score { get; private set; }
    public Dictionary<int, int> TargetRemaining { get; private set; } = new Dictionary<int, int>();
    public bool IsGameOver { get; private set; }

    public event Action OnMovesChanged; // sự kiện được gọi khi số lượt còn lại thay đổi
    public event Action OnScoreChanged; // sự kiện được gọi khi điểm số thay đổi
    public event Action<int, int> OnTargetChanged; // sự kiện được gọi khi số lượng mục tiêu còn lại thay đổi, tham số: colorIndex, số còn lại
    public event Action OnWin; // sự kiện được gọi khi người chơi thắng
    public event Action OnLose; // sự kiện được gọi khi người chơi thua

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (currentLevel == null)
        {
            Debug.LogError("[GameManager] Missing current level.");
            enabled = false;
            return;
        }

        MovesLeft = currentLevel.moveLimit;
        TargetRemaining.Clear();

        foreach (var goal in currentLevel.targetGoals ?? Array.Empty<TargetGoal>())
        {
            if (goal == null || goal.amount <= 0 || TargetRemaining.ContainsKey(goal.colorIndex))
                continue;

            TargetRemaining.Add(goal.colorIndex, goal.amount);
        }

        if (TargetRemaining.Count == 0)
            Debug.LogWarning("[GameManager] This level has no valid target goals.");
    }

    // Gọi từ SwapHandler khi swap tạo match hợp lệ
    public void OnValidMove()
    {
        if (IsGameOver)
            return;

        MovesLeft = Mathf.Max(0, MovesLeft - 1);
        OnMovesChanged?.Invoke();
    }

    // Gọi từ BoardManager.ResolveMatches sau khi đã xác định các tile bị blast
    public void OnTilesCleared(List<Tile> clearedTiles, int cascadeLevel)
    {
        if (IsGameOver || clearedTiles == null || clearedTiles.Count == 0)
            return;

        int basePoints = cascadeLevel switch
        {
            1 => 10,
            2 => 20,
            3 => 30,
            _ => 50
        };

        foreach (var tile in clearedTiles)
        {
            Score += basePoints;

            if (TargetRemaining.ContainsKey(tile.colorIndex) && TargetRemaining[tile.colorIndex] > 0)
            {
                TargetRemaining[tile.colorIndex]--;
                OnTargetChanged?.Invoke(tile.colorIndex, TargetRemaining[tile.colorIndex]);
            }
        }
        OnScoreChanged?.Invoke();
    }

    public void CheckWinLose()
    {
        if (IsGameOver)
            return;

        bool allTargetsDone = TargetRemaining.Count > 0;
        foreach (var kv in TargetRemaining)
            if (kv.Value > 0) { allTargetsDone = false; break; }

        if (allTargetsDone)
        {
            IsGameOver = true;
            OnWin?.Invoke();
        }
        else if (MovesLeft <= 0)
        {
            IsGameOver = true;
            OnLose?.Invoke();
        }
    }
}