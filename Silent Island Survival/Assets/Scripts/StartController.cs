using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Functions

    public void ClickNewGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ClickLoadGame()
    {
        Debug.Log("Load game.");
    }

    public void ClickSettings()
    {
        Debug.Log("Open Settings.");
    }

    public void ClickQuitGame()
    {
        Debug.Log("Quit game.");
    }

    #endregion
}
