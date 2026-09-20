using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour
{   
    private Button playButton;
    private Button continueButton;

    void OnEnable() // sửa từ "Enable" thành "OnEnable"
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playButton = root.Q<Button>("Play");
        continueButton = root.Q<Button>("Continue");

        playButton.clicked += OnPlayClicked;
        // continueButton.clicked += OnContinueClicked;

        // bool hasSave = PlayerPrefs.HasKey("SavedLevel");
        // continueButton.style.display = hasSave ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void OnDisable()
    {
        playButton.clicked -= OnPlayClicked;
        // continueButton.clicked -= OnContinueClicked;
    }

    void OnPlayClicked()
    {
       
        PlayerPrefs.SetInt("SavedLevel", 1);
        SceneManager.LoadScene("World1");
    }
}