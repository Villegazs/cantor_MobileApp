using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : AlternateStateObject
{
    public GameObject optionsMenu;
    public GameObject buttonsMenu;


    void OnDisable()
    {
        optionsMenu.SetActive(true);
    }
    void OnEnable()
    {
        buttonsMenu.SetActive(false);
        optionsMenu.SetActive(false);
    }
    public void TogglePauseMenu()
    {
        ToggleObjectState(buttonsMenu);
    }

    public void RestartActiveScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
