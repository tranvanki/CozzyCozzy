using UnityEngine;
using UnityEngine.SceneManagement;
public class UIMenu : MonoBehaviour
{
//Play button on mobile android
    public void PlayGame()
    {
        SceneManager.LoadScene("World1");
    }
}
