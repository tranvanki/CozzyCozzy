using UnityEngine;
using TMPro;

public class Move : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private GameManager gameManager;
    void OnEnable() => gameManager.OnMovesChanged += UpdateText;
    void OnDisable() => gameManager.OnMovesChanged -= UpdateText;
    void Start() => UpdateText();
    private void UpdateText() => movesText.text = $"Moves: {gameManager.MovesLeft}";
}
