using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Scene Settings")]
    [SerializeField] private string firstLevelSceneName = "Dungeon";

    public void StartRun()
    {
        // Reset any static run data or persistent managers here before loading
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void ToggleOptions(bool isOpen)
    {
        mainMenuPanel.SetActive(!isOpen);
        optionsPanel.SetActive(isOpen);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}