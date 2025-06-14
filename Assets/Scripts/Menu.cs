using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public bool isPaused = false;
    public static Menu instant;
    void Awake()
    {
        instant = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        instant = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Resume()
    {
        GameObject canvas = GameObject.Find("Canvas");
        Transform menuTransform = canvas.transform.Find("Menu");
        GameObject menu = menuTransform.gameObject;
        menu.SetActive(false);
        // TogglePause();
        Time.timeScale = 1;
        isPaused = false;

    }

    public void PauseGame()
    {
        GameObject canvas = GameObject.Find("Canvas");
        Transform menuTransform = canvas.transform.Find("Menu");
        GameObject menu = menuTransform.gameObject;
        menu.SetActive(true);
        // TogglePause();
        Time.timeScale = 0;
        isPaused = true;
    }

    public void Restart()
    {
        // Lấy tên của scene hiện tại đang hoạt động
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Tải lại scene với tên đã lấy được
        SceneManager.LoadScene(currentSceneName);
        // TogglePause();
        Time.timeScale = 1;
        isPaused = false;
    }
    // void TogglePause()
    // {
    //     isPaused = !isPaused;
    //     Time.timeScale = isPaused ? 0f : 1f;
    //     Debug.Log(isPaused ? "Game paused" : "Game resumed");
    // }
    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
