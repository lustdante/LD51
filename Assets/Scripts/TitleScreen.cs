using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    private void Awake()
    {
        Screen.SetResolution(800, 600, false);
    }

    public void LoadTargetScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
