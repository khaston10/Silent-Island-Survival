using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using utils;

public class StartController : MonoBehaviour
{

    #region - Variables - New World Creation Panel

    public GameObject newWorldCreationPanel;
    public Text worldSizeText;
    private int worldSize = 5;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        // Set text objects to values.
        worldSizeText.text = worldSize.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Functions

    public void ClickNewGame()
    {
        Utils.GenerateNewWorldMap();
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

    #region New World Creation Functions

    public void OpenCloseNewWorldCreationPanel()
    {
        if (newWorldCreationPanel.activeInHierarchy) newWorldCreationPanel.SetActive(false);
        else newWorldCreationPanel.SetActive(true);
    }

    public void IncreaseDecreaseWorldSize(bool increase)
    {
        if (increase)
        {
            // Check to see if the world size is already maxed. 
            if (worldSize >= 10) print("World size is already maxed!");
            else
            {
                worldSize += 1;
                worldSizeText.text = worldSize.ToString();
            }
        }

        else
        {
            // Check to see if the world size is already min. 
            if (worldSize <= 1) print("World size is already min!");
            else
            {
                worldSize -= 1;
                worldSizeText.text = worldSize.ToString();
            }
        }
    }

    public void GenerateNewWorldMap()
    {
        print("Create Map With World Size.");
    }

    #endregion

    #endregion
}
