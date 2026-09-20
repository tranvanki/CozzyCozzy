using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InputManager inputManager;

    private VisualElement overlay;
    private Label titleLabel;
    private Label subtitleLabel;
    private Button retryButton;
    private Button menuButton;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        overlay = root.Q<VisualElement>("Overlay");
        titleLabel = root.Q<Label>("TitleLabel");
        subtitleLabel = root.Q<Label>("SubtitleLabel");
        retryButton = root.Q<Button>("RetryButton");
        menuButton = root.Q<Button>("MenuButton");

        retryButton.clicked += OnRetryClicked;
        menuButton.clicked += OnMenuClicked;

        gameManager.OnWin += HandleWin;
        gameManager.OnLose += HandleLose;
    }

    void OnDisable()
    {
        retryButton.clicked -= OnRetryClicked;
        menuButton.clicked -= OnMenuClicked;

        gameManager.OnWin -= HandleWin;
        gameManager.OnLose -= HandleLose;
    }

    void HandleWin()
    {
        titleLabel.text = "You Win!";
        subtitleLabel.text = $"Score: {gameManager.Score}";
        overlay.style.display = DisplayStyle.Flex;
        inputManager.enabled = false;
    }

    void HandleLose()
    {
        titleLabel.text = "Game Over";
        subtitleLabel.text = "Out of moves";
        overlay.style.display = DisplayStyle.Flex;
        inputManager.enabled = false;
    }

    void OnRetryClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnMenuClicked()
    {
        SceneManager.LoadScene("MainMenu"); // đổi đúng tên scene Menu bạn đặt
    }
}