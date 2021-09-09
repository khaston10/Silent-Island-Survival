using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using utils;
using System.IO;

public class StartController : MonoBehaviour
{

    #region - Variables - New World Creation Panel

    public GameObject newWorldCreationPanel;
    public Text worldSizeText;
    private int worldSize = 10;
    public List<string> map = new List<string>();
    public string ActiveMapName;

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
        GenerateNewWorldMap();
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
            if (worldSize >= 100) print("World size is already maxed!");
            else
            {
                worldSize += 10;
                worldSizeText.text = worldSize.ToString();
            }
        }

        else
        {
            // Check to see if the world size is already min. 
            if (worldSize <= 50) print("World size is already min!");
            else
            {
                worldSize -= 10;
                worldSizeText.text = worldSize.ToString();
            }
        }
    }

    public void GenerateNewWorldMap()
    {
        string[] Map = new string[worldSize];
        Map = Utils.GenerateNewWorldMap(worldSize);
        WriteMapToTxtFile("Map01.txt", Map);

    }

    private void WriteMapToTxtFile(string fileName, string[] inMap)
    {
        // Path to file.
        string path = Application.dataPath + "\\" + fileName;

        File.WriteAllText(path, "");

        // Add the map.
        for (int numOfLines = 0; numOfLines < worldSize; numOfLines++)
        {
            File.AppendAllText(path, inMap[numOfLines] + "\n");
        }

        // Save the file.

        // Save map name in global control script so the main scene can open it.
        SaveActiveMapName(fileName);
        
    }

    public void SaveActiveMapName (string inMapName)
    {
        GlobalControl.Instance.ActiveMapName = inMapName;
    }

    #endregion

    #endregion
}
