using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeletePanel : MonoBehaviour
{
    [SerializeField] private GameObject deletePanel;
    [SerializeField] private GameObject confirmPanel;
    public bool loggedIn = false;
    public bool paused = false;
    // Update is called once per frame
   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && loggedIn)
        {
            if (paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        else
        {
            return;
        }
    }

    void PauseGame()
    {
        paused = true;
        deletePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        paused = false;
        deletePanel.SetActive(false);
        confirmPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState= CursorLockMode.Locked;
    }

    public void BackToLogin()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
