using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchResolver : MonoBehaviour

{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject popEffectPrefab;
    [SerializeField] private float popDelay = 0.2f;

    private bool isResolving = false;
    public bool IsResolving => isResolving;
    

    // proccess match 3 tiles
    public void ProcessMatches(List<List<Tile>> matches)
    {
        if (isResolving || boardManager == null || gameManager == null || gameManager.IsGameOver || matches == null || matches.Count == 0) return;
        StartCoroutine(ResolveSequence(matches, true));
    }

        public void ProcessBombActivation(Tile bombTile, bool moveAlreadyConsumed = false)
        {
        if (isResolving || boardManager == null || gameManager == null || gameManager.IsGameOver || bombTile == null || bombTile.type != TileType.Bomb)
                return;

            var affectedTiles = boardManager.GetAreaTiles(bombTile.x, bombTile.y);
            if (affectedTiles.Count == 0)
                return;

        if (!moveAlreadyConsumed)
            gameManager.OnValidMove();

        StartCoroutine(ResolveSequence(new List<List<Tile>> { affectedTiles }, false));
        }

    private IEnumerator ResolveSequence(List<List<Tile>> matches, bool createBombsForCurrentStep)
    {
        isResolving = true;
        int cascadeLevel = 0;

        while (matches != null && matches.Count > 0)
        {
            cascadeLevel++;

            if (createBombsForCurrentStep)
                AudioManager.Instance.PlayMatchSfx();

            var tilesToSpareAsBomb = new HashSet<Tile>();
            if (createBombsForCurrentStep)
            {
                foreach (var group in matches)
                {
                    if (group.Count >= 4 && group[0].type != TileType.Bomb)
                    {
                        Tile bombCandidate = group[group.Count / 2];
                        boardManager.ConvertToBomb(bombCandidate);
                        tilesToSpareAsBomb.Add(bombCandidate);
                    }
                }
            }

            var uniqueTiles = new HashSet<Tile>();
            foreach (var group in matches)
                foreach (var tile in group)
                    if (tile != null && !tilesToSpareAsBomb.Contains(tile))
                        uniqueTiles.Add(tile);

            var chainReactionTiles = new List<Tile>();
            foreach (var tile in uniqueTiles)
            {
                if (tile.type == TileType.Bomb)
                {
                    AudioManager.Instance.PlayBombSfx();
                    chainReactionTiles.AddRange(boardManager.GetAreaTiles(tile.x, tile.y));
                }
            }
            foreach (var tile in chainReactionTiles)
                uniqueTiles.Add(tile);

            if (gameManager != null)
                gameManager.OnTilesCleared(new List<Tile>(uniqueTiles), cascadeLevel);

            foreach (var tile in uniqueTiles)
            {
                if (tile == null) continue;

                Vector3 pos = tile.transform.position;

                if (popEffectPrefab != null)
                    Instantiate(popEffectPrefab, pos, Quaternion.identity);

                boardManager.DestroyTileAt(tile.x, tile.y);
            }

            yield return new WaitForSeconds(popDelay);
            boardManager.CollapseAndRefill();
            yield return new WaitForSeconds(popDelay);

            matches = MatchChecker.FindAllMatches(boardManager.grid, boardManager.width, boardManager.height);
            createBombsForCurrentStep = true;
        }

        gameManager?.CheckWinLose();

        if (gameManager != null && !gameManager.IsGameOver && !boardManager.HasAvailableAction())
        {
            Debug.Log("[MatchResolver] Deadlock detected. Reshuffling board.");
            boardManager.ShuffleUntilPlayable();
        }

        isResolving = false;
    }
    
    private void ApplyGravity()
    {
        boardManager.CollapseAndRefill();
    }
}