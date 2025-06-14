using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private bool isPaused = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void StartGame()
    {
        SceneManager.LoadScene(3);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void PauseGame()
    {
        GameObject canvas = GameObject.Find("Canvas");
        Transform menuTransform = canvas.transform.Find("Menu");
        GameObject menu = menuTransform.gameObject;
        menu.SetActive(true);
        TogglePause();
    }

    public void Resume()
    {
        GameObject canvas = GameObject.Find("Canvas");
        Transform menuTransform = canvas.transform.Find("Menu");
        GameObject menu = menuTransform.gameObject;
        menu.SetActive(false);

        TogglePause();

    }

    public void Restart()
    {
        // Lấy tên của scene hiện tại đang hoạt động
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Tải lại scene với tên đã lấy được
        SceneManager.LoadScene(currentSceneName);
        TogglePause();
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log(isPaused ? "Game paused" : "Game resumed");
    }



}
