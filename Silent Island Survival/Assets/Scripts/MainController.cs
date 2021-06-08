using Packages.Rider.Editor.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditorInternal;
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
    int wood = 0;
    int stone = 0;
    bool playersTurn = true; // This will help to take the inputs away when it is not the player's turn.

    #region Variables - Units

    #region Unit Prefabs
    public GameObject[] basicUnitPrefabs;
    // When more types of prefabs get implemented we can create multiple arrays.
    // That way we can have multiple types of farmers, and soldiers, ect.

    public GameObject movementOptionPrefab;
    public GameObject movementSelectedPrefab;
    List<GameObject> movementOptionTiles = new List<GameObject>(); // Temporaly holds these objects as they exist.
    List<GameObject> movementSelectedTiles = new List<GameObject>(); // Temporaly holds these objects as they exist.
    bool movementSelectionCanContinue = true; // When a player ends the selection on an object that is not passable then the selection abilty must be disabled.
    GameObject selectedUnit; // When a unit is selected they will be tracked by this object.
    #endregion

    List<GameObject> unitsInPlay = new List<GameObject>();

    #endregion

    #region Variables - Camera - Light - Raycasting

    public Camera mainCam;
    public Light mainLightSourceSun;
    Ray ray;
    RaycastHit hit;
    string[] acceptableTags = new string[] {"Abandoned House", "Abandoned Factory", "Abandoned Vehicle", "Loot Box", "Tree", "Rock", "Trash"};
    string[] acceptableGroundTilesTags = new string[] { "GroundTile"};
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
    public GameObject InventoryInformationPanel;
    public Text currentDayText;
    public Text skillPointsAvailableText;
    public Text foodText;
    public Text woodText;
    public Text stoneText;
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
        ["Loot Box"] = "Any unit can scavenge loot from here.",
        ["Trash"] = "This item is of no use to you."
    };

    #endregion

    #region Panel - Individual Unit Panel

    public GameObject IndividualUnitPanel;
    public Text unitNameText;
    public Text unitHealthText;
    public Text unitActionPointsText;
    public Text unitAttackText;
    public Text unitAttackRangeText;
    public Text unitDefenseText;

    #endregion

    #region Panel - InteractWithStructurePanel

    public GameObject InteractWithStructurePanel;
    public GameObject InteractWithStructureFeedBackPanel;
    public Text InteractWithStructureFeedbackText;
    public Button scavangeButton;
    public Button plantCropsButton;
    public Button harvestButton;
    public Button repairButton;
    public Button upgradeButton;
    public Button draftOrdinanceButton;

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

    int selectedMapIndex = 1; // By default we select 0;
    public int WorldSize = 0; // The world size will be set at the start of the game and depends on what map is selected.
    public GameObject[] groundTiles; // This is where all of the active ground tiles will be stored.
    public GameObject TerrainBase; // This will be used so that the entire set of ground tiles can be set to be children of an object in the hierarchy.


    #endregion

    void Start()
    {

        #region Terrain Generation
        LoadGroundTitlesFromMap();
        #endregion

        #region Text Object Updates

        UpdateBasicInformationText();

        #endregion

        SpawnUnitsAtStartOfGame();

        // Start Player's Turn.
        PlayerTurn();
    }


    void Update()
    {
        // Each round will consist of 3 turns.
        // 1 Player Turn
        // 2 Zombie's Turn
        // Update Turn
        
        if (playersTurn)
        {
            UpdateCamPosition();

            // This section covers Ray Casting.
            // If the player clicks on any object other than a unit with the Individual Unit Panel active.
            if (Input.GetMouseButtonDown(0) && !IndividualUnitPanel.gameObject.activeSelf)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    UpdateGameWithMouseClickOnObject();

                }
            }

            // Closes the Individual Unit Panel when the mouse is released.
            else if (Input.GetMouseButtonUp(0) && IndividualUnitPanel.gameObject.activeSelf)
            {
                MoveSelectedUnit(selectedUnit);
                CloseIndividualUnitPanel();

                // Reset the player's abilty to select movements. 
                movementSelectionCanContinue = true;
            }


            // To map the unit path when player is selecting where to move.
            else if (Input.GetMouseButton(0) && IndividualUnitPanel)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    MapUnitMovementPath();
                }
            }
        }
        
       
    }

    #region Functions

    #region Camera Raycasting and Movement Fuctions

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

            // If the player clicks on a unit.
            else if (hit.collider.tag.ToString() == "Unit")
            {
                selectedUnit = hit.collider.transform.gameObject;
                OpenIndividualUnitPanel(hit.transform.gameObject);
                PlaceMovementOptionTiles(hit.transform.gameObject);
            }

            else
            {
                // If the player has selected a ground tile we need to check if it has any children.
                // If it does we need to open the appropriate menu.

                // Case 1 - The ground tile has a Tree, Rock, Zombie or Structures on it.
                // In this case we want to open the structure panel and highlight the ground tile.

                var childCount = hit.transform.childCount;
                for (var i = 0; i < childCount; i++)
                {
                    var child = hit.transform.GetChild(i);
                    var childTag = child.tag;

                    // Do something based on tag
                    if (acceptableTags.Contains(childTag))
                    {
                        OpenStructurePanel(childTag);
                    }

                    else Debug.Log("Tag Not Found");
                }

                // Case 2 - The ground title has a unit on it.
                
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
        // Set camera up for player's turn.
        updateCameraForPlayerTurn();

        // Need to enable the End Of Turn button so player can chose when to end the turn.
        EndOfDayTurnButton.gameObject.SetActive(true);
    }

    public void SpawnUnitsAtStartOfGame()
    {
        // Find a ground tile to spawn player on, it should be a clear, randomly selected tile and spawn the starting unit.
        var groundTileFound = false;

        while (!groundTileFound)
        {
            var randomIndex = Random.Range(0, groundTiles.Length);

            if (groundTiles[randomIndex] != null && groundTiles[randomIndex].tag == "GroundTile")
            {

                // Check to see if the ground tile is empty.
                if (groundTiles[randomIndex].GetComponent<GroundTileController>().terrainIsPassable)
                {
                    groundTileFound = true;
                    createUnitAtLocation(groundTiles[randomIndex].transform.position);

                    // Set the starting unit to the selected unit.
                    selectedUnit = unitsInPlay[0];
                }
                
            }
        }

    }

    public void createUnitAtLocation(Vector3 spawnLoc)
    {
        // Create game object.
        var temp = Instantiate(basicUnitPrefabs[0], spawnLoc, Quaternion.identity);

        // Make the unit a child of the ground tile.
        var row = Mathf.RoundToInt(spawnLoc.x);
        var col = Mathf.RoundToInt(spawnLoc.z);
        temp.transform.SetParent(groundTiles[LocateIndexOfGroundTile(row, col)].transform);

        // Set the ground tiles attribute so that the terrain is no longer passable.
        groundTiles[LocateIndexOfGroundTile(row, col)].GetComponent<GroundTileController>().terrainIsPassable = false;

        // Add the unit to the list of units in play.
        unitsInPlay.Add(temp);
    }

    public void updateCameraForPlayerTurn()
    {
        mainCam.transform.position = new Vector3(selectedUnit.transform.position.x, 5f, selectedUnit.transform.position.z - 4);
        mainCam.transform.rotation = Quaternion.Euler(45f, 0f, 0);
    }

    #endregion


    #region Zombie Turn Function

    public void ZombieTurn()
    {
        // Set the players turn to false to disable player inputs.
        playersTurn = false;

        // Set camera for zombie turn.
        updateCameraForZombieTurn();

        // Currently we are just waiting 2 seconds.
        StartCoroutine("WaitForZombieTurn", 2);
    }

    public void updateCameraForZombieTurn()
    {
        mainCam.transform.position = new Vector3(WorldSize / 2, WorldSize, WorldSize / 2);
        mainCam.transform.rotation = Quaternion.Euler(90f, 0f, 0);
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

        // Update Unit attributes.
        UpdateUnitsAtEndOfRound();

        // End Update and Start Player Turn, we also need to set the player's turn bool to true to allow player input.
        playersTurn = true;
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

    public void UpdateUnitsAtEndOfRound() 
    {
        // Cycle through the list of units and award points.
        for (int count = 0; count < unitsInPlay.Count; count++)
        {
            if (unitsInPlay[count].GetComponent<UnitController>().actionPoints < unitsInPlay[count].GetComponent<UnitController>().actionPointsLimit)
                unitsInPlay[count].GetComponent<UnitController>().actionPoints += 2;
        }
    }


    #endregion


    #region Terrain Generation Functions

    public void LoadGroundTitlesFromMap()
    {
        // 1. Check to see if the world map is square. If it is not we need to throw an error.
        // This strips all characters that are not 0, ., |, -, └, ┘, ┐, ┴, ┬, ├, ┤, ^, *, &, 1, 2, 3, 4, x and ┌
        string allCharsInString = System.Text.RegularExpressions.Regex.Replace(TerrainMaps[selectedMapIndex].text, @"[^.0|┐└┌┘┴┬├┤^&*1234x-]", ""); 

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

                // If the charecter depicts a X road we need to create a road block tile.
                else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == 'x')
                {
                    // Road Block.
                    // Pick a random road block tile.

                    groundTiles[LocateIndexOfGroundTile(line, letter)] = Instantiate(GroundTilePrefabs[4], new Vector3(line, 0, letter), Quaternion.Euler(0, -90, 0));
                    groundTiles[LocateIndexOfGroundTile(line, letter)].transform.SetParent(TerrainBase.transform);
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

    public void CloseAllOpenPanels()
    {
        BasicInformationPanel.SetActive(false);
        StructurePanel.SetActive(false);
        IndividualUnitPanel.SetActive(false);
    }

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

    public void ClickInventoryInformationButton() 
    {
        // If the panel is closed we will open it.
        if (InventoryInformationPanel.activeSelf)
        {
            InventoryInformationPanel.SetActive(false);
        }

        else
        {
            // Update the text.
            foodText.text = food.ToString();
            woodText.text = wood.ToString();
            stoneText.text = stone.ToString();
            InventoryInformationPanel.SetActive(true);
        }
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

    #region Individual Unit Panel Functions

    public void OpenIndividualUnitPanel(GameObject unit)
    {
        // Set text to match unit's attributes.
        unitNameText.text = unit.GetComponent<UnitController>().unitName;
        unitHealthText.text = unit.GetComponent<UnitController>().hitPoints.ToString();
        unitActionPointsText.text = unit.GetComponent<UnitController>().actionPoints.ToString();
        unitAttackText.text = unit.GetComponent<UnitController>().attack.ToString();
        unitAttackRangeText.text = unit.GetComponent<UnitController>().attackRange.ToString();
        unitDefenseText.text = unit.GetComponent<UnitController>().defense.ToString();

        IndividualUnitPanel.gameObject.SetActive(true);
    }

    public void CloseIndividualUnitPanel()
    {
        // Delete the old movement option titles if they exist.
        for (int listIndex = 0; listIndex < movementOptionTiles.Count; listIndex++)
        {
            GameObject.Destroy(movementOptionTiles[listIndex]);
        }
        movementOptionTiles.Clear();

        // Delete the old movement selected titles if they exist.
        for (int listIndex = 0; listIndex < movementSelectedTiles.Count; listIndex++)
        {
            GameObject.Destroy(movementSelectedTiles[listIndex]);
        }
        movementSelectedTiles.Clear();

        // Close the panel.
        IndividualUnitPanel.gameObject.SetActive(false);
    }

    public void PlaceMovementOptionTiles(GameObject unit)
    {
        // Delete the old movement titles if they exist.
        for (int listIndex = 0; listIndex < movementOptionTiles.Count; listIndex++)
        {
            GameObject.Destroy(movementOptionTiles[listIndex]);
        }
        movementOptionTiles.Clear();

        // We need to check all the tiles in the distance of the unit's action points. These variables help us get there.
        var moveDistance = unit.GetComponent<UnitController>().actionPoints;

        var unitRow = Mathf.RoundToInt(unit.transform.position.x);
        var unitCol = Mathf.RoundToInt(unit.transform.position.z);


        // Iterate through all the possible tiles and check to see if they are passable.
        for (int row = (unitRow - moveDistance); row <= (unitRow + moveDistance); row++)
        {
            for (int col = (unitCol - moveDistance); col <= (unitCol + moveDistance); col++)
            {
                // Check to see if the tile is close enough to be considered in movement distance.
                if ((Mathf.Abs(unitRow - row) + Mathf.Abs(unitCol - col)) <= moveDistance)
                {
                    // Check to see if the row, col is going to be in bounds of the array.
                    if ((row >= 0) && (col >= 0) && (LocateIndexOfGroundTile(row, col) < groundTiles.Length) && (LocateIndexOfGroundTile(row, col) >= 0))
                    {
                        // Check to see if the ground title exists in array.
                        if (groundTiles[LocateIndexOfGroundTile(row, col)] != null)
                        {
                            // Create a movement option tile.
                            var tempMovementOptionTile = Instantiate(movementOptionPrefab, new Vector3(row, .05f, col), Quaternion.identity);

                            // Add it to the list so we can easily delete them later.
                            movementOptionTiles.Add(tempMovementOptionTile);

                            // Make it a child of the ground title.
                            tempMovementOptionTile.transform.SetParent(groundTiles[LocateIndexOfGroundTile(row, col)].transform);
                        }
                    }
                }
                
            }
        }
    }

    public void MapUnitMovementPath()
    {
        // If the list of movemtent tiles is not empty we need to check if the player has the cursor back over the selector.
        // If the mouse is back over the starting tile then we will want to clear the list.
        if (movementSelectedTiles.Count > 0 && hit.transform.position.x == selector.transform.position.x && hit.transform.position.z == selector.transform.position.z)
        {
            // Give back the units action points.
            selectedUnit.GetComponent<UnitController>().actionPoints += movementSelectedTiles.Count;

            // Delete the old movement option titles if they exist.
            for (int listIndex = 0; listIndex < movementSelectedTiles.Count; listIndex++)
            {
                GameObject.Destroy(movementSelectedTiles[listIndex]);
            }
            movementSelectedTiles.Clear();

            //Reset movement selection.
            movementSelectionCanContinue = true;
        }


        // This function is called when the Individual Unit Panel is open and the player is holding the mouse down.
        // 1. Check to see if the object the mouse is over is a ground tile and the selection option is enabled.
        if (acceptableGroundTilesTags.Contains(hit.collider.tag.ToString()) && movementSelectionCanContinue)
        {
            // 2. If the ground title is the same tile that the player is on, we will do nothing.
            if (selectedUnit.transform.IsChildOf(hit.transform)) return;

            // 3. Check to see if the ground tile has the same location as a tile in the list movementSlectedTiles, if it is already listed we will ignore.
            for (int count = 0; count < movementSelectedTiles.Count; count++)
            {
                if (hit.transform.position.x == movementSelectedTiles[count].transform.position.x && hit.transform.position.z == movementSelectedTiles[count].transform.position.z)
                {
                    return;
                }
            }

            // 4. Check to see if the ground tile is adjacent to the last tile in the list, if it is not then we will ignore.
            if (movementSelectedTiles.Count > 0) 
            {
                if ((Mathf.Abs(hit.transform.position.x - movementSelectedTiles[movementSelectedTiles.Count - 1].transform.position.x) <= 0 &&
                    Mathf.Abs(hit.transform.position.z - movementSelectedTiles[movementSelectedTiles.Count - 1].transform.position.z) <= 1) ||
                    (Mathf.Abs(hit.transform.position.x - movementSelectedTiles[movementSelectedTiles.Count - 1].transform.position.x) <= 1 &&
                    Mathf.Abs(hit.transform.position.z - movementSelectedTiles[movementSelectedTiles.Count - 1].transform.position.z) <= 0))
                {

                }

                else return;
            }

            else
            {
                if ((Mathf.Abs(hit.transform.position.x - selectedUnit.transform.position.x) <= 0 &&
                    Mathf.Abs(hit.transform.position.z - selectedUnit.transform.position.z) <= 1) ||
                    (Mathf.Abs(hit.transform.position.x - selectedUnit.transform.position.x) <= 1 &&
                    Mathf.Abs(hit.transform.position.z - selectedUnit.transform.position.z) <= 0))
                {

                }

                else return;
            }
            

            // 5.  Check to see if the player has the action points and update them.
            if (hit.transform.gameObject.GetComponent<GroundTileController>().terrainIsPassable)
            {
                if (selectedUnit.GetComponent<UnitController>().actionPoints >= 1)
                {
                    selectedUnit.GetComponent<UnitController>().actionPoints -= 1;
                    unitActionPointsText.text = selectedUnit.GetComponent<UnitController>().actionPoints.ToString();
                }

                else return;
            }

            // 6. Create a movement Selected Tile at the location of the ground tile and add it to the list if the ground tile also has a movement option tile.
            // If the ground tile is set to not passable, then the list must end here.
            for (int count = 0; count < movementOptionTiles.Count; count++)
            {
                if (hit.transform.position.x == movementOptionTiles[count].transform.position.x && hit.transform.position.z == movementOptionTiles[count].transform.position.z)
                {
                    var tempTile = Instantiate(movementSelectedPrefab, new Vector3(hit.transform.position.x, .1f, hit.transform.position.z), Quaternion.identity);
                    tempTile.transform.SetParent(hit.transform);
                    movementSelectedTiles.Add(tempTile);
                }

                if (!hit.transform.gameObject.GetComponent<GroundTileController>().terrainIsPassable) movementSelectionCanContinue = false;
            }

            



        } 
    }

    public void MoveSelectedUnit(GameObject unit)
    {
        // If the movement selected list is not empty that means we need to move the unit.
        for (int count = 0; count < movementSelectedTiles.Count; count++)
        {
            // If the ground title is passable then we can move the unit.
            if (movementSelectedTiles[count].transform.parent.GetComponent<GroundTileController>().terrainIsPassable)
            {
                // Set the current ground tile back to passable.
                unit.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;

                unit.transform.position = new Vector3(movementSelectedTiles[count].transform.position.x, 0f, movementSelectedTiles[count].transform.position.z);
                unit.transform.SetParent(movementSelectedTiles[count].transform.parent);

                // Set the new ground tile to not passable.
                unit.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = false;
            }

            
            else
            {
                // We need to consider if a player has selected a game object that is not a ground tile.
                if (hit.transform.tag.ToString() == "Tree" || hit.transform.tag.ToString() == "Rock") Harvest(hit.transform.gameObject);

                else if (hit.transform.tag.ToString() == "Abandoned House" || hit.transform.tag.ToString() == "Abandoned Factory"
                            || hit.transform.tag.ToString() == "Abandoned Vehicle" || hit.transform.tag.ToString() == "Loot Box") OpenInteractWithStructurePanel(hit.transform.gameObject);

                else
                {
                    var childCount = hit.transform.childCount;
                    for (var i = 0; i < childCount; i++)
                    {
                        var child = hit.transform.GetChild(i);
                        var childTag = child.tag;

                        // If the ground tile contains a tree or rock we will harvest.
                        if (childTag == "Tree" || childTag == "Rock") Harvest(child.transform.gameObject);


                        // If the ground title is a Abandoned Structure or Structure we will bring up Interact with Structure Menu.
                        else if (childTag == "Abandoned House" || childTag == "Abandoned Factory" 
                            || childTag == "Abandoned Vehicle" || childTag == "Loot Box") OpenInteractWithStructurePanel(child.transform.gameObject);


                        // If the ground title contains a zombie we will have the player attack.

                        // If the ground title contains another player then nothing will happen.
                        // Do something based on tag
                    }
                }
            }



        }
    }

    public void Harvest(GameObject itemToHarvest)
    {
        // If the item is a tree.
        if (itemToHarvest.transform.tag.ToString() == "Tree")
        {
            if (selectedUnit.GetComponent<UnitController>().actionPoints >= 1)
            {
                selectedUnit.GetComponent<UnitController>().actionPoints -= 1;
                wood += 1;
                itemToHarvest.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;
                Destroy(itemToHarvest);
            }
        }

        // If the item is a rock.
        else if (itemToHarvest.transform.tag.ToString() == "Rock")
        {
            if (selectedUnit.GetComponent<UnitController>().actionPoints >= 1)
            {
                selectedUnit.GetComponent<UnitController>().actionPoints -= 1;
                stone += 1;
                itemToHarvest.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;
                Destroy(itemToHarvest);
            }
        }

        // If this item is not harvestable.
        else Debug.Log("Can not harvest this item.");
    }
    #endregion

    #region Interact With Structure Panel Function


    public void OpenInteractWithStructurePanel(GameObject structureObject)
    {
        // Set all buttons to inactive.
        SetButtonToSeeThrough(true, scavangeButton);
        SetButtonToSeeThrough(true, scavangeButton);
        SetButtonToSeeThrough(true, plantCropsButton);
        SetButtonToSeeThrough(true, harvestButton);
        SetButtonToSeeThrough(true, repairButton);
        SetButtonToSeeThrough(true, upgradeButton);
        SetButtonToSeeThrough(true, draftOrdinanceButton);
        
        
        // Open the panel in game.
        InteractWithStructurePanel.SetActive(true);

        // Use if statments to open the panel with the correct options displayed.
        if (structureObject.transform.tag.ToString() == "Abandoned House" || structureObject.transform.tag.ToString() == "Abandoned Factory"
            || structureObject.transform.tag.ToString() == "Abandoned Vehicle" || structureObject.transform.tag.ToString() == "Loot Box")
        {
            SetButtonToSeeThrough(false, scavangeButton);
        }

        else plantCropsButton.gameObject.SetActive(true); 
    }

    public void SetButtonToSeeThrough(bool seeThrough, Button button)
    {
        // This function can be called to make a button semi transparent.
        // The bool seeThrough allows transparent and opaque values to be set.

        Color color1 = button.image.color;
        Color color2 = button.GetComponentInChildren<Text>().color;

        if (seeThrough)
        {
            color1.a = .2f;
            color2.a = .2f;
            button.interactable = false;
        }
        
        else
        {
            color1.a = 1f;
            color2.a = 1f;
            button.interactable = true;
        }
        button.image.color = color1;
        button.GetComponentInChildren<Text>().color = color2;
    }

    public void ClickExitInteractWithStructurePanel()
    {
        InteractWithStructurePanel.SetActive(false);
    }

    public void ClickScavange()
    {
        // This can get more complicated but for now we will just randomly award food.
        var amtOfFoodToSavanged = Random.Range(0, 5);
        food += amtOfFoodToSavanged;
        InteractWithStructureFeedbackText.text = "+ " + amtOfFoodToSavanged.ToString() + " Food";
        InteractWithStructureFeedBackPanel.SetActive(true);
        SetButtonToSeeThrough(true, scavangeButton);
        foodText.text = food.ToString();
    }

    #endregion

    #endregion

    #endregion
}
