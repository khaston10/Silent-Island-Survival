using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{

    #region Variables - Terrain Generation

    #region Terrain Generation Pre-Fabs
    public TextAsset[] TerrainMaps;
    public GameObject[] GroundTilePrefabs;
    #endregion

    int selectedMapIndex = 1; // By default we select 0;
    public int WorldSize = 0; // The world size will be set at the start of the game and depends on what map is selected.
    GameObject[] groundTiles; // This is where all of the active ground tiles will be stored.
    public GameObject TerrainBase; // This will be used so that the entire set of ground tiles can be set to be children of an object in the hierarchy.


    #endregion

    void Start()
    {

        #region Terrain Generation
        
        LoadGroundTitlesFromMap();
        //Debug.Log(groundTiles.Length);
        #endregion
    }


    void Update()
    {
        // Each round will consist of 3 turns.

        // 1 Player Turn
        PlayerTurn();

        // 2 Zombie's Turn
        ZombieTurn();

        // Update Turn
        UpdateTurn();

    }

    #region Functions


    #region Player Turn Functions

    public void PlayerTurn()
    {

    }

    #endregion


    #region Zombie Turn Function

    public void ZombieTurn()
    {

    }

    #endregion


    #region Update Turn Function

    public void UpdateTurn()
    {

    }

    #endregion


    #region Terrain Generation Functions

    public void LoadGroundTitlesFromMap()
    {
        // 1. Check to see if the world map is square. If it is not we need to throw an error.
        string allCharsInString = System.Text.RegularExpressions.Regex.Replace(TerrainMaps[selectedMapIndex].text, @"[^.0]", ""); // This strips all characters that are not 0 or .

        if (allCharsInString.Length / (TerrainMaps[selectedMapIndex].text.IndexOf('\n') - 1) != (TerrainMaps[selectedMapIndex].text.IndexOf('\n') - 1))
        {
            Debug.Log("The Map Is Not Square!");
        }

        // 2. Save the WorldSize variable based on the map side length.
        else WorldSize = TerrainMaps[selectedMapIndex].text.IndexOf('\n') - 1;

        // 3. Use for loops it instantiate ground tiles, position them, and add them to the array groundTiles. 
        groundTiles = new GameObject[WorldSize * WorldSize];
        for (int line = WorldSize -1; line >= 0; line--)
        {

            for (int letter = 0; letter < WorldSize; letter ++)
            {
                // If the charecter depicts land we need to create it.
                if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '0')
                {

                    groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[0], new Vector3(line, 0, letter), Quaternion.identity);
                    groundTiles[LocateIndexOfGroundTile(line, letter)].transform.SetParent(TerrainBase.transform);

                    // We need to update the location attribute on the instance of the prefab.
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[0] = line;
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[1] = letter;

                }

                // In the future we can add other objects here, like fences and trees ...
            }
        }
    }

    public int LocateIndexOfGroundTile(int row, int column)
    {
        // Gound tiles are in a grid in the game but stored in a single array in code.
        // So this function will take in the row and coulmn and return the index of the ground tile
        // so that it can be found in the groundTiles array.
        // The formula to find the index given a row and column is: (Col * (World Size)) + Row
        return ((column * (WorldSize)) + row);
    }

    #endregion

    #endregion
}