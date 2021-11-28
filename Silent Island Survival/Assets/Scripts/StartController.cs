using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using utils;
using System.IO;
using UnityEngine.Audio;

public class StartController : MonoBehaviour
{

    #region - Variables - New World Creation Panel

    public GameObject newWorldCreationPanel;
    public GameObject loadPanel;
    public GameObject feedbackPanel;
    public GameObject settingsPanel;
    public Text feedbackText;
    public Text worldSizeText;
    private int worldSize = 50;
    public List<string> map = new List<string>();
    public string ActiveMapName;


    private string[] savePath = new string[4];

    #endregion

    #region Variables - Sound

    public AudioMixerGroup MusicMixer;
    public AudioMixerGroup SFXMixer;
    public AudioSource SFXSource;

    #region Variables UISounds
    public AudioClip Clack;
    public AudioClip mouseClick01;
    public AudioClip mouseClick02;
    public AudioClip mouseClick03;
    public AudioClip mouseClick04;
    public AudioClip mouseClickCoarse;
    public AudioClip mouseClickFunny;
    public AudioClip Ping;
    public AudioClip mouseClickSpace;
    public AudioClip mouseClickTiny;
    #endregion

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // Set text objects to values.
        worldSizeText.text = worldSize.ToString();

        // Set the save paths.
        // There are 3 save files that will hold maps and the paths to the files are stored in a string array.
        savePath[0] = Application.dataPath + "/Saves/Save00";
        savePath[1] = Application.dataPath + "/Saves/Save01";
        savePath[2] = Application.dataPath + "/Saves/Save02";
        savePath[3] = Application.dataPath + "/Saves/Save03";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Functions

    public void ClickOpenCloseLoadGamePanel()
    {
        // Play Mouse Click
        SFXSource.PlayOneShot(mouseClick01);

        if (loadPanel.activeInHierarchy)
        {
            loadPanel.SetActive(false);
            feedbackPanel.SetActive(false);
        }

        else loadPanel.SetActive(true);
    }

    public void ClickQuitGame()
    {
        // Play Mouse Click
        SFXSource.PlayOneShot(mouseClick01);

        Debug.Log("Quit game.");
    }

    public void OpenCloseSettingsPanel()
    {
        // Play Mouse Click
        SFXSource.PlayOneShot(mouseClick01);

        if (settingsPanel.activeInHierarchy) settingsPanel.SetActive(false);
        else settingsPanel.SetActive(true);

    }

    #region New World Creation Functions

    public void OpenCloseNewWorldCreationPanel()
    {
        // Play Mouse Click
        SFXSource.PlayOneShot(mouseClick01);

        if (newWorldCreationPanel.activeInHierarchy) newWorldCreationPanel.SetActive(false);
        else newWorldCreationPanel.SetActive(true);
    }

    public void LoadExistingMap(int savePathIndex)
    {
        // Check to see if the map file exists.
        if (System.IO.File.Exists(savePath[savePathIndex] + "/Map.txt"))
        {
            SaveActiveMapName(savePath[savePathIndex]);
            SceneManager.LoadScene(1);
        }

        else
        {
            feedbackText.text = "This slot is empty.";
            if (feedbackPanel.activeInHierarchy == false)
            {
                feedbackPanel.SetActive(true);
            }
        }
    }

    public void IncreaseDecreaseWorldSize(bool increase)
    {
        // Play Mouse Click
        SFXSource.PlayOneShot(mouseClick01);

        if (increase)
        {
            // Check to see if the world size is already maxed. 
            if (worldSize >= 200) print("World size is already maxed!");
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
        WriteMapToTxtFile(savePath[0], Map);
        SceneManager.LoadScene(1);

    }

    private void WriteMapToTxtFile(string path, string[] inMap)
    {

        File.WriteAllText(path + "/Map.txt", "");

        // Add the map.
        for (int numOfLines = 0; numOfLines < worldSize; numOfLines++)
        {
            File.AppendAllText(path + "/Map.txt", inMap[numOfLines] + "\n");
        }

        // Save the file.

        // Save map name in global control script so the main scene can open it.
        SaveActiveMapName(path);
        
    }

    public void SaveActiveMapName (string path)
    {
        GlobalControl.Instance.ActiveSavePath = path;
    }

    #region Sound Functions

    public void AdjustMusicVol(float volume)
    {
        MusicMixer.audioMixer.SetFloat("MusicVol", volume);
    }

    public void AdjustSFXVol(float volume)
    {
        SFXMixer.audioMixer.SetFloat("SFXVol", volume);
    }

    public void PlaySFXClip(AudioClip audioClip)
    {
        SFXSource.PlayOneShot(audioClip);
    }

    #endregion

    #endregion

    #endregion
}
