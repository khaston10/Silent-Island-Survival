using Packages.Rider.Editor.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{

    public int currentDay = 1;
    int currentHour = 15; // This can have values of 0 - 23.
    int skillPointsAvailable = 0;
    int food = 10;
    int population = 1;
    int populationCap = 5;

    #region Variables - Camera - Light - Raycasting

    public Camera mainCam;
    public Light mainLightSourceSun;
    Ray ray;
    RaycastHit hit;
    string[] acceptableTags = new string[] {"Abandoned House", "Abandoned Factory", "Abandoned Vehicle", "Loot Box", "Tree", "Rock"};
    public GameObject selector;
    #region Variables - Movement 
    float camTranslateSpeed = 5f;
    float camZoomSpeed = 2f;
    float camBottomBounds = 2f;
    float camTopBound = 10f;

    #endregion



    #endregion

    #region Variables - Panels

    #region Panel - Basic Information Panel

    public GameObject BasicInformationPanel;
    public Button EndOfDayTurnButton;
    public Text currentDayText;
    public Text skillPointsAvailableText;
    public Text foodText;
    public Text populationText;
    public Text populationCapText;

    #endregion

    #region Panel - Structure Panel

    public GameObject StructurePanel;
    public Text StructureTitleText;
    public Text StructureDescriptionText;
    public Image StructureImage;
    public Sprite[] StructureSprites;
    Dictionary<string, string> structureDescriptionsDict = new Dictionary<string, string>()
    {
        ["Tree"] = "This is a tree. Any unit can harvest it for wood.",
        ["Rock"] = "This is a rock. Any unit can harvest it for stone.",
        ["Farm Plot"] = "Needs to be added",
        ["Living Quarters"] = "Needs to be added",
        ["Wood Fence"] = "Needs to be added",
        ["Medical Tent"] = "Needs to be added",
        ["Town Hall"] = "Needs to be added",
        ["Abandoned House"] = "Any unit can scavenge supplies from here. Beware of zombies!",
        ["Abandoned Factory"] = "Any unit can scavenge supplies from here. Beware of zombies!",
        ["Abandoned Vehicle"] = "Vehicle is too destroyed to functions, but supplies can still be scavenged.",
        ["Loot Box"] = "Any unit can scavenge loot from here."
    };

    #endregion

    #endregion

    #region Variables - Terrain Generation

    #region Terrain Generation Pre-Fabs
    public TextAsset[] TerrainMaps;
    public GameObject[] GroundTilePrefabs;
    public GameObject[] TreePrefabs;
    public GameObject[] RockPrefabs;
    public GameObject[] LootPrefabs;
    public GameObject[] AbandonedVehiclesPrefabs;
    public GameObject[] AbandonedHousePrefabs;
    public GameObject[] AbandonedFactoryPrefabs;
    #endregion

    int selectedMapIndex = 0; // By default we select 0;
    public int WorldSize = 0; // The world size will be set at the start of the game and depends on what map is selected.
    GameObject[] groundTiles; // This is where all of the active ground tiles will be stored.
    public GameObject TerrainBase; // This will be used so that the entire set of ground tiles can be set to be children of an object in the hierarchy.


    #endregion

    void Start()
    {

        #region Terrain Generation
        LoadGroundTitlesFromMap();
        PlaceCameraAtStart();
        #endregion

        #region Text Object Updates

        UpdateBasicInformationText();

        #endregion



        // Start Player's Turn.
        PlayerTurn();
    }


    void Update()
    {
        // Each round will consist of 3 turns.
        // 1 Player Turn
        // 2 Zombie's Turn
        // Update Turn
        UpdateCamPosition();

        // This section covers Ray Casting.
        if (Input.GetMouseButtonDown(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                UpdateGameWithMouseClickOnObject();
                
            }
        }
    }

    #region Functions

    #region Camera Raycasting and Movement Fuctions

    public void PlaceCameraAtStart()
    {
        mainCam.transform.position = new Vector3(WorldSize / 2, 5f, WorldSize / 2);
        mainCam.transform.Rotate(45f, 0f, 0f);
    }

    public void UpdateCamPosition()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) && mainCam.transform.position.z < WorldSize - 10)
        {
            mainCam.transform.Translate(Vector3.forward * camTranslateSpeed *Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) && mainCam.transform.position.z > 10)
        {
            mainCam.transform.Translate(-Vector3.forward * camTranslateSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) && mainCam.transform.position.x > 10)
        {
            mainCam.transform.Translate(-Vector3.right * camTranslateSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) && mainCam.transform.position.x < WorldSize - 10)
        {
            mainCam.transform.Translate(Vector3.right * camTranslateSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.Q) && mainCam.transform.position.y < camTopBound)
        {
            mainCam.transform.Translate(Vector3.up * camZoomSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.E) && mainCam.transform.position.y > camBottomBounds)
        {
            mainCam.transform.Translate(-Vector3.up * camZoomSpeed * Time.deltaTime, Space.World);
        }
    }

    public void UpdateGameWithMouseClickOnObject()
    {
        if (!IsPointerOverUIObject())
        {
            // If the player has selected an object that should open the structure panel.
            if (acceptableTags.Contains(hit.collider.tag.ToString()))
            {
                OpenStructurePanel(hit.collider.tag.ToString());
            }

            else
            {
                // If the player has selected a ground tile we need to check if it has any children.
                // If it does we need to open the appropriate menu.

                // Case 1 - The ground tile has a Tree, Rock, Zombie or Structure on it.
                // In this case we want to open the structure panel and highlight the ground tile.

                var childCount = hit.transform.childCount;
                for (var i = 0; i < childCount; ++i)
                {
                    var child = hit.transform.GetChild(i);
                    var childTag = child.tag;
                    Debug.Log(childTag);

                    // Do something based on tag
                    if (acceptableTags.Contains(childTag))
                    {
                        OpenStructurePanel(childTag);
                    }

                    else Debug.Log("Tag Not Found");
                }

                // Case 2 - The ground title has a unit on it.
                // To Do once I have units.
            }

            // Case 3 - The ground tile is empty.
            // We need to move the selector object to the tile's position.
            selector.transform.position = hit.transform.position;
            selector.transform.position += new Vector3(0f, .1f, 0f);
        }
    }
    
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    #endregion


    #region Player Turn Functions

    public void PlayerTurn()
    {
        // Need to enable the End Of Turn button so player can chose when to end the turn.
        EndOfDayTurnButton.gameObject.SetActive(true);
    }

    #endregion


    #region Zombie Turn Function

    public void ZombieTurn()
    {
        // Currently we are just waiting 2 seconds.
        StartCoroutine("WaitForZombieTurn", 2);
    }

    IEnumerator WaitForZombieTurn(float waitTime)
    {
        Debug.Log("Zombie Turn Start");
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Zombie Turn End");
        UpdateTurn();
    }

    #endregion


    #region Update Turn Function

    public void UpdateTurn()
    {
        // Update the time of day and sunlight.
        UpdateTimeOfDay();
        UpdateLightSource();

        // Update Text Objects.
        UpdateAllText();

        // End Update and Start Player Turn.
        PlayerTurn();
    }

    public void UpdateTimeOfDay()
    {
        if (currentHour < 23) currentHour++;
        else
        {
            currentHour = 0;
            currentDay++;
        }
    }

    public void UpdateAllText()
    {
        // Need to add update functions as panels get added to the code.
        UpdateBasicInformationText();
    }

    public void UpdateLightSource()
    {
        // Sun should increase in intensity from 4 am to 12 pm.
        if (currentHour >= 4 && currentHour < 12) mainLightSourceSun.intensity += .1f;

        // Then decrease in intensity from 4 pm until 12 am.
        else if (currentHour >= 16 && currentHour <= 23) mainLightSourceSun.intensity -= .1f;
    }

    #endregion


    #region Terrain Generation Functions

    public void LoadGroundTitlesFromMap()
    {
        // 1. Check to see if the world map is square. If it is not we need to throw an error.
        // This strips all characters that are not 0, ., |, -, └, ┘, ┐, ┴, ┬, ├, ┤, ^, *, &, 1, 2, 3, 4 and ┌
        string allCharsInString = System.Text.RegularExpressions.Regex.Replace(TerrainMaps[selectedMapIndex].text, @"[^.0|┐└┌┘┴┬├┤^&*1234-]", ""); 

        if (allCharsInString.Length / (TerrainMaps[selectedMapIndex].text.Replace(" ", "").IndexOf('\n') - 1) != (TerrainMaps[selectedMapIndex].text.Replace(" ", "").IndexOf('\n') - 1))
        {
            Debug.Log("The Map Is Not Square!");
        }

        // 2. Save the WorldSize variable based on the map side length.
        else WorldSize = TerrainMaps[selectedMapIndex].text.Replace(" ", "").IndexOf('\n') - 1;

        // 3. Use for loops it instantiate ground tiles, position them, and add them to the array groundTiles. 
        groundTiles = new GameObject[WorldSize * WorldSize];
        for (int line = WorldSize - 1; line >= 0; line--)
        {

            for (int letter = 0; letter < WorldSize; letter++)
            {
                // If the charecter depicts land or an object that should have land under it we need to create it.
                if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '0' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '^' 
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '&' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '*' 
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '1' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '2' 
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '3' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '4')
                {
                    groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[0], new Vector3(line, 0, letter), Quaternion.identity);
                    groundTiles[LocateIndexOfGroundTile(line, letter)].transform.SetParent(TerrainBase.transform);

                    // We need to update the location attribute on the instance of the prefab.
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[0] = line;
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[1] = letter;
                }

                // If the charecter depicts a vertical or horizontal road we need to create it.
                else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '|' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '-')
                {
                    // Vertical road.
                    if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '|')
                    {
                        groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[1], new Vector3(line, 0, letter), Quaternion.Euler(0, 90, 0));
                    }

                    // Horizontal road.
                    else groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[1], new Vector3(line, 0, letter), Quaternion.identity);

                    groundTiles[LocateIndexOfGroundTile(line, letter)].transform.SetParent(TerrainBase.transform);

                    // We need to update the location attribute on the instance of the prefab.
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[0] = line;
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[1] = letter;
                }

                // If the charecter depicts a curved road we need to create it.
                else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '└' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┘'
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┐' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┌')
                {
                    // Up - Right Road.
                    if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '└')
                    {
                        groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[2], new Vector3(line, 0, letter), Quaternion.identity);
                    }

                    // Up - Left Road.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┘')
                    {
                        groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[2], new Vector3(line, 0, letter), Quaternion.Euler(0, -90, 0));
                    }

                    // Down - Left Road.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┐')
                    {
                        groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[2], new Vector3(line, 0, letter), Quaternion.Euler(0, 180, 0));
                    }

                    // Down - Rightt Road.
                    else
                    {
                        groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[2], new Vector3(line, 0, letter), Quaternion.Euler(0, 90, 0));
                    }

                    groundTiles[LocateIndexOfGroundTile(line, letter)].transform.SetParent(TerrainBase.transform);

                    // We need to update the location attribute on the instance of the prefab.
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[0] = line;
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[1] = letter;
                }

                // If the charecter depicts a T road we need to create it.
                else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┴' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┬'
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '├' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┤')
                {
                    // T Up Road.
                    if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┴')
                    {
                        groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[3], new Vector3(line, 0, letter), Quaternion.Euler(0, -90, 0));
                    }

                    // T Down Road.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '┬')
                    {
                        groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[3], new Vector3(line, 0, letter), Quaternion.Euler(0, 90, 0));
                    }

                    // T Right Road.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '├')
                    {
                        groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[3], new Vector3(line, 0, letter), Quaternion.Euler(0, 0, 0));
                    }

                    // T Left Road.
                    else
                    {
                        groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[3], new Vector3(line, 0, letter), Quaternion.Euler(0, 180, 0));
                    }

                    groundTiles[LocateIndexOfGroundTile(line, letter)].transform.SetParent(TerrainBase.transform);

                    // We need to update the location attribute on the instance of the prefab.
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[0] = line;
                    groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().location[1] = letter;
                }
            } 

        }

        // 4. Use for loops it instantiate objects that will be children of these tiles, trees, rocks, abandoned structures, ect. 
        for (int line = WorldSize - 1; line >= 0; line--)
        {
            for (int letter = 0; letter < WorldSize; letter++)
            {
                // Now we need to add objects that will be children of these tiles, trees, rocks, abandoned structures, ect.
                if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '^' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '*'
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '&' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '1' 
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '2' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '3')
                {
                    // Trees.
                    if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '^')
                    {
                        Instantiate(TreePrefabs[0], new Vector3(line, 0, letter), Quaternion.Euler(0f, GetRandomRotation(), 0f)).transform.SetParent(groundTiles[LocateIndexOfGroundTile(line, letter)].transform);

                        // Set the ground tile's attribute terrainIsPassable to false.
                        groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().terrainIsPassable = false;
                    }

                    // Rocks.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '*')
                    {
                        Instantiate(RockPrefabs[0], new Vector3(line, 0, letter), Quaternion.Euler(0f, GetRandomRotation(), 0f)).transform.SetParent(groundTiles[LocateIndexOfGroundTile(line, letter)].transform);

                        // Set the ground tile's attribute terrainIsPassable to false.
                        groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().terrainIsPassable = false;
                    }

                    // Loot.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '&')
                    {
                        Instantiate(LootPrefabs[0], new Vector3(line, 0, letter), Quaternion.Euler(0f, GetRandomRotation(), 0f)).transform.SetParent(groundTiles[LocateIndexOfGroundTile(line, letter)].transform);

                        // Set the ground tile's attribute terrainIsPassable to false.
                        groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().terrainIsPassable = false;
                    }

                    // Abandoned Vehicle.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '3')
                    {

                        Instantiate(AbandonedVehiclesPrefabs[0], new Vector3(line, 0, letter), Quaternion.Euler(0f, GetRandomRotation(), 0f)).transform.SetParent(groundTiles[LocateIndexOfGroundTile(line, letter)].transform);

                        // Set the ground tile's attribute terrainIsPassable to false.
                        groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().terrainIsPassable = false;
                    }

                    // Abandoned House.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '1')
                    {

                        Instantiate(AbandonedHousePrefabs[0], new Vector3(line, 0, letter), Quaternion.Euler(0f, GetRandomOrthogonalRotation(), 0f)).transform.SetParent(groundTiles[LocateIndexOfGroundTile(line, letter)].transform);

                        // Set the ground tile's attribute terrainIsPassable to false.
                        groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().terrainIsPassable = false;
                    }

                    // Abandoned Factory.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '2')
                    {

                        Instantiate(AbandonedFactoryPrefabs[0], new Vector3(line, 0, letter), Quaternion.Euler(0f, GetRandomOrthogonalRotation(), 0f)).transform.SetParent(groundTiles[LocateIndexOfGroundTile(line, letter)].transform);

                        // Set the ground tile's attribute terrainIsPassable to false, this includes all adjacent tiles there are 9 tiles in all.

                        for (int row = -1; row < 2; row++)
                        {
                            for (int col = -1; col < 2; col++)
                            {
                                if (groundTiles[LocateIndexOfGroundTile(line + row, letter + col)] != null)
                                {
                                    groundTiles[LocateIndexOfGroundTile(line + row, letter + col)].GetComponent<GroundTileController>().terrainIsPassable = false;
                                }
                            }
                        }
                    }
                }
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

    public float GetRandomRotation()
    {
        float[] rotations = { 0f, 90f, -90f, 180f, 45f, -45f};
        return rotations[Random.Range(0, 6)];
    }

    public float GetRandomOrthogonalRotation()
    {
        float[] rotations = { 0f, 90f, -90f, 180f};
        return rotations[Random.Range(0, 4)];
    }

    #endregion


    #region Panel Fuctions

    #region Basic Information Panel Functions

    public void ClickEndTurn()
    {
        // Start the zombie's turn.
        ZombieTurn();

        // Make this button disappear.
        EndOfDayTurnButton.gameObject.SetActive(false);
    }

    public void UpdateBasicInformationText()
    {
        currentDayText.text = currentDay.ToString();
        skillPointsAvailableText.text = skillPointsAvailable.ToString();
        foodText.text = food.ToString();
        populationText.text = population.ToString();
        populationCapText.text = populationCap.ToString();
    }

    #endregion

    #region Structure Panel Functions

    public void OpenStructurePanel(string objectTag)
    {
        // This should be called with the raycast function with the tag as an input.
        StructureTitleText.text = objectTag;
        StructureDescriptionText.text = structureDescriptionsDict[objectTag];

        // Find the sprite.
        int spriteIndex = -1;

        for (int i = 0; i < StructureSprites.Length; i++)
        {
            if (StructureSprites[i].name == objectTag) spriteIndex = i;
        }

        if (spriteIndex != -1) StructureImage.sprite = StructureSprites[spriteIndex];
        else Debug.Log("StructureImage could not load Sprite.");


        StructurePanel.gameObject.SetActive(true);

    }

    public void ClickCloseStructurePanel()
    {
        StructurePanel.gameObject.SetActive(false);
    }

    #endregion

    #endregion

    #endregion
}
