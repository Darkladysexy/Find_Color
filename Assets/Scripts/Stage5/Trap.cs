using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Trap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Clone")
        {
            ReloadScene();
        }

    }
    public void ReloadScene()
    {
        // Lấy tên của scene hiện tại đang hoạt động
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Tải lại scene với tên đã lấy được
        SceneManager.LoadScene(currentSceneName);
    }
}
