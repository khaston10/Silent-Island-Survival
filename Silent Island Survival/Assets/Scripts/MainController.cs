using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using utils;

public class MainController : MonoBehaviour
{

    public int currentDay = 1;
    int currentHour = 15; // This can have values of 0 - 23.
    float skillPointsAvailable = 2;
    int food = 10;
    int population = 1;
    int populationCap = 5;
    int wood = 10;
    int stone = 10;
    bool playersTurn = true; // This will help to take the inputs away when it is not the player's turn.

    #region Variables - Units

    #region Unit Prefabs
    public GameObject[] basicUnitPrefabs;
    // When more types of prefabs get implemented we can create multiple arrays.
    // That way we can have multiple types of farmers, and soldiers, ect.

    public GameObject movementOptionPrefab;
    public GameObject movementSelectedPrefab;
    List<GameObject> movementOptionTiles = new List<GameObject>(); // Temporaly holds these objects as they exist.
    public List<GameObject> movementSelectedTiles = new List<GameObject>(); // Temporaly holds these objects as they exist.
    bool movementSelectionCanContinue = true; // When a player ends the selection on an object that is not passable then the selection abilty must be disabled.
    public bool unitIsMoving = false; // When the unit is moving we do not want the player to input any controls.
    public bool unitIsAttacking = false; // When the unit is attacking, this should be set to true.
    public GameObject selectedUnit; // When a unit is selected they will be tracked by this object.
    #endregion

    float unitMovementSpeed = 3;
    public List<GameObject> unitsInPlay = new List<GameObject>();

    // Used during movement of unit.
    int nextPositionIndex = 0;
    Vector3 NextPosition = Vector3.zero;

    // Used during attacking mode.
    public GameObject attackingImage;
    int attackRange;
    int xPositionOfUnit;
    int zPositionOfUnit;
    public GameObject attackIndicatorPrefab;
    public List<GameObject> attackIndicators;
    public GameObject attackFeedbackPanel;
    public Text attackFeedbackText;

    #endregion

    #region Variables - Zombies
    public GameObject zombiePrefab;
    public List<GameObject> zombiesInPlay = new List<GameObject>();
    public GameObject selectedZombie;
    private int zombieCap = 5; // There can only be 3 zombies in game at one time.


    #endregion

    #region Variables - Camera - Light - Raycasting

    public Camera mainCam;
    public Light mainLightSourceSun;
    Ray ray;
    RaycastHit hit;
    string[] acceptableTags = new string[] {"Abandoned House", "Abandoned Factory", "Abandoned Vehicle", "Loot Box", "Tree", "Rock", "Trash"};
    string[] acceptableStructureTags = new string[] { "Farm Plot", "Living Quarters", "Medical Facility", "Wall", "Town Hall" };
    string[] acceptableGroundTilesTags = new string[] { "GroundTile", "Holding Factory"};
    string[] acceptableUnitTags = new string[] { "Unit" };
    public GameObject selector;
    #region Variables - Movement 
    int actionPointAvailable;
    float camTranslateSpeed = 5f;
    float camZoomSpeed = 2f;
    float camBottomBounds = 2f;
    float camTopBound = 10f;
    float camRotateSpeed = 20f;

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
    public Slider HealthUnitSlider;

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
    public Button ExitInteractWithStructurePanel;

    #endregion

    #region Panel - BuildPanel

    #region Structure Prefabs

    public GameObject farmPlotPrefab;
    public GameObject livingQuartersPrefab;
    public GameObject medicalFacilityPrefab;
    public GameObject wallPrefab;
    public GameObject townHallPrefab;

    #endregion

    public GameObject buildOptionPrefab;
    bool structureIsBeingBuilt = false;
    List<GameObject> buildOptionTiles = new List<GameObject>(); // Temporaly holds these objects as they exist.
    GameObject currentStructureSelectedToBuild;
    public Text structureTitleText;
    public Text requiredMatsText;
    public GameObject BuildFeedbackPanel;
    public GameObject StructureStatsPanel;
    public Text BuildFeedbackText;

    // The following text objects are for the StructureStatsPanel
    public Text SelectedStructureTitleText;
    public Text SelectedStructureCurrentLevelText;
    public Text SelectedStructureMaxLevelText;
    public Text RequiredWoodForUpgradeText;
    public Text RequiredStoneForUpgradeText;
    public Text RequiredFoodForUpgradeText;
    public Slider structureHitPointsSlider;

    // The following are selectors.
    public GameObject farmPlotSelector;
    public GameObject livingQuartersSelector;
    public GameObject medicalFacilitySelector;
    public GameObject wallSelector;
    public GameObject townHallSelector;

    // This variable was created to keep track of the gameObject the player is trying to call methods from.
    GameObject selectedStructureForUse;

    // To keep track of structures at update, we need a list to hold them.
    List<GameObject> currentStructuresInGame = new List<GameObject>();

    public Button ToggleBuildPanel;
    public GameObject BuildPanel;
    public Button createFarmPlotButton;
    public Button createLivingQuartersButton;
    public Button createMedicalTentButton;
    public Button createFenceButton;
    public Button createTownHallButton;

    #endregion

    #region Panel - SkillsPanel

    public GameObject skillsPanel;
    public Slider skillPointsSlider;
    public Text skillsUpgradeTitleText;
    public Text skillsInformationText;
    public Image skillsPointsToSpendImage;
    public Button[] AttackIncreaseButtons;
    public Button[] DefenseIncreaseButtons;
    public Button[] SightIncreaseButtons;
    public Button[] RepairIncreaseButtons;
    public Button HarvestIncreaseButtons;
    public Button HarvestTimeDecreaseButton;
    public Button RangeIncreaseButton;
    public Button CriticalHitIncreaseButton;

    #endregion

    #region Panel - ZombieStatsPanel
    public GameObject zombieStatsPanel;
    public Text zombieTitleText;
    public Text zombieDescriptionText;
    public Slider zombieHitPointsSlider;

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

    int selectedMapIndex = 3; // By default we select 0;
    public int WorldSize = 0; // The world size will be set at the start of the game and depends on what map is selected.
    public GameObject[] groundTiles; // This is where all of the active ground tiles will be stored.
    public GameObject TerrainBase; // This will be used so that the entire set of ground tiles can be set to be children of an object in the hierarchy.


    #endregion

    #region Variables - FXs

    public GameObject dirtSplatterFX;
    public GameObject APAnimationObject;

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
        CreateZombieAtRandomLocation(); // For now we will create 1 zombie at the start.
        CreateZombieAtRandomLocation(); // For now we will create 1 zombie at the start.

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

            // If a unit is moving we need to run this function.
            if (unitIsMoving)
            {
                MoveUnit(selectedUnit);
            }

            else if (unitIsAttacking)
            {
                // If the player clicks on any object.
                if (Input.GetMouseButtonDown(0))
                {
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {
                        UpdateGameWithMouseClickDuringAttack();
                    }
                }
            }

            // If a structure is being built we need to run this function.
            else if (structureIsBeingBuilt)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        BuildStructure(currentStructureSelectedToBuild);
                    }

                    else UpdateStructureSelector();
                }
                
            }

            // This section covers Ray Casting.
            // If the player clicks on any object.
            else if (Input.GetMouseButtonDown(0) && !IndividualUnitPanel.gameObject.activeSelf)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    UpdateGameWithMouseClickOnObject();
                }
            }

            // When the players left mouse button is down and they are mapping a units path.
            else if (Input.GetMouseButton(0) && IndividualUnitPanel)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    MapUnitMovementPath();
                }
            }

            // Closes the Individual Unit Panel when the mouse is released.
            else if (Input.GetMouseButtonUp(0) && IndividualUnitPanel.gameObject.activeSelf)
            {
                if (movementSelectedTiles.Count > 0)
                {
                    if (UnitInteractsWithNextGroundTileOnMove(selectedUnit, 0))
                    {
                     
                        unitIsMoving = true;
                        PlayUnitRun(selectedUnit); // Play the run animation.

                    }

                    else
                    {
                        selectedUnit.transform.LookAt(movementSelectedTiles[0].transform.position);
                        PlayUnitInteract(selectedUnit);
                    }
                }
                
                // Reset the player's abilty to select movements. 
                movementSelectionCanContinue = true;
                CloseIndividualUnitPanel();
            }
        }
    }

    #region Functions

    #region Camera Raycasting and Movement Fuctions

    public void UpdateCamPosition()
    {
        // This function handles key board inputs.

        if (Input.GetKey(KeyCode.W))
        {
            mainCam.transform.Translate(new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z) * camTranslateSpeed *Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.S))
        {
            mainCam.transform.Translate(-new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z) * camTranslateSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            mainCam.transform.Translate(-new Vector3(mainCam.transform.right.x, 0f, mainCam.transform.right.z) * camTranslateSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            mainCam.transform.Translate(new Vector3(mainCam.transform.right.x, 0f, mainCam.transform.right.z) * camTranslateSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.UpArrow) && !structureIsBeingBuilt && mainCam.transform.position.y < camTopBound)
        {
            mainCam.transform.Translate(Vector3.up * camZoomSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.DownArrow) && mainCam.transform.position.y > camBottomBounds)
        {
            mainCam.transform.Translate(-Vector3.up * camZoomSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            mainCam.transform.Rotate(-Vector3.up * camRotateSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.E))
        {
            mainCam.transform.Rotate(Vector3.up * camRotateSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Tab will select the next unit, 
            int currentIndex = unitsInPlay.IndexOf(selectedUnit);

            if (currentIndex < unitsInPlay.Count - 1) selectedUnit = unitsInPlay[currentIndex + 1];
            else selectedUnit = unitsInPlay[0];

            // and update the camera.
            UpdateCamPositionOnUnitSelection();

            // Set text to match unit's attributes.
            unitNameText.text = selectedUnit.GetComponent<UnitController>().unitName;


        }

        if (Input.GetKeyDown(KeyCode.R) && structureIsBeingBuilt)
        {
            if (currentStructureSelectedToBuild.name == "Farm Plot") farmPlotSelector.transform.Rotate(new Vector3(0f, 90f, 0f));
            else if (currentStructureSelectedToBuild.name == "Living Quarters") livingQuartersSelector.transform.Rotate(new Vector3(0f, 90f, 0f));
            else if (currentStructureSelectedToBuild.name == "Medical Facility") medicalFacilitySelector.transform.Rotate(new Vector3(0f, 90f, 0f));
            else if (currentStructureSelectedToBuild.name == "Wall") wallSelector.transform.Rotate(new Vector3(0f, 45f, 0f));
            else if (currentStructureSelectedToBuild.name == "Town Hall") townHallSelector.transform.Rotate(new Vector3(0f, 90f, 0f));
        }

        
    }

    public void UpdateCamPositionOnUnitSelection()
    {
        // This function position the camera on the selected unit.
        // It is used when the player selects a unit with the TAB key, the camera will move and rotate.
        
        mainCam.transform.position = new Vector3(selectedUnit.transform.position.x, 5f, selectedUnit.transform.position.z - 4);
        mainCam.transform.rotation = Quaternion.Euler(45f, 0f, 0);

        // Move the selector tile to the position.
        // We need to move the selector object to the tile's position.
        selector.transform.position = selectedUnit.transform.position;
        selector.transform.position += new Vector3(0f, .1f, 0f);



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

            // If the player has selected a structure, Farm Plot, Living Quarters, ect.
            else if (acceptableStructureTags.Contains(hit.collider.tag.ToString()))
            {
                OpenStructureStatsPanel(hit.transform.gameObject);
            }

            // If the player clicks on a unit.
            else if (hit.collider.tag.ToString() == "Unit")
            {
                selectedUnit = hit.collider.transform.gameObject;

                // Set the available action points so the mapping function will work.
                actionPointAvailable = selectedUnit.GetComponent<UnitController>().actionPoints;

                OpenIndividualUnitPanel(hit.transform.gameObject);
                PlaceMovementOptionTiles(hit.transform.gameObject);
            }


            // If the player has selected a zombie.
            else if (hit.collider.tag.ToString() == "Zombie")
            {
                // Open the zombie stats panel.
                OpenZombieStatsPanel(hit.collider.gameObject);
            }

            else
            {
                // If the player has selected a ground tile we need to check if it has any children.
                // If it does we need to open the appropriate menu.

                var childCount = hit.transform.childCount;
                for (var i = 0; i < childCount; i++)
                {
                    var child = hit.transform.GetChild(i);
                    var childTag = child.tag;

                    // Case 1 - The ground tile has a Tree, Rock, Zombie or Structures on it.
                    // In this case we want to open the structure panel and highlight the ground tile.
                    if (acceptableTags.Contains(childTag))
                    {
                        OpenStructurePanel(childTag);
                    }

                    // Case 2 - The ground title has a unit on it.
                    else if (acceptableUnitTags.Contains(childTag))
                    {
                        selectedUnit = child.transform.gameObject;
                        OpenIndividualUnitPanel(child.transform.gameObject);
                        PlaceMovementOptionTiles(child.transform.gameObject);
                    }
                }           
            }

            // We need to move the selector object to the tile's position.
            selector.transform.position = hit.transform.position;
            selector.transform.position += new Vector3(0f, .1f, 0f);


        }
    }

    public void UpdateGameWithMouseClickDuringAttack()
    {
        if (!IsPointerOverUIObject())
        {
            // If the player has selected a zombie.
            if (hit.collider.tag.ToString() == "Zombie")
            {
                UnitAttack(hit.collider.gameObject);
            }

            else
            {
                // If the player has selected a ground tile we need to check if it has any children.
                // If it does we need to open the appropriate menu.

                var childCount = hit.transform.childCount;
                for (var i = 0; i < childCount; i++)
                {
                    var child = hit.transform.GetChild(i);
                    var childTag = child.tag;

                    // Case 1 - The ground tile has a Tree, Rock, Zombie or Structures on it.
                    // In this case we want to open the structure panel and highlight the ground tile.
                    if (childTag == "Zombie")
                    {
                        UnitAttack(child.gameObject);
                    }
                }
            }
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
            var randomIndex = UnityEngine.Random.Range(0, groundTiles.Length);

            if (groundTiles[randomIndex] != null && groundTiles[randomIndex].tag == "GroundTile")
            {

                // Check to see if the ground tile is empty.
                if (groundTiles[randomIndex].GetComponent<GroundTileController>().terrainIsPassable)
                {
                    groundTileFound = true;
                    createUnitAtLocation(groundTiles[randomIndex].transform.position);

                    // Set the starting unit to the selected unit.
                    selectedUnit = unitsInPlay[0];

                    // Set text to match unit's attributes.
                    unitNameText.text = selectedUnit.GetComponent<UnitController>().unitName;
                }
            }
        }

        UpdateCamPositionOnUnitSelection();

    }

    public void createUnitAtLocation(Vector3 spawnLoc)
    {
        // Pick a unit type: Basic, Farmer, Soldier.
        var randomType = UnityEngine.Random.Range(0, 3);

        // Create game object.
        var temp = Instantiate(basicUnitPrefabs[randomType], spawnLoc, Quaternion.identity);

        // Set the random attributes for the unit.
        SetUnitAttributesAtCreation(temp);

        // Make the unit a child of the ground tile.
        var row = Mathf.RoundToInt(spawnLoc.x);
        var col = Mathf.RoundToInt(spawnLoc.z);
        temp.transform.SetParent(groundTiles[LocateIndexOfGroundTile(row, col)].transform);

        // Set the ground tiles attribute so that the terrain is no longer passable.
        groundTiles[LocateIndexOfGroundTile(row, col)].GetComponent<GroundTileController>().terrainIsPassable = false;

        // Add the unit to the list of units in play.
        unitsInPlay.Add(temp);

        // Update text.
        populationText.text = unitsInPlay.Count.ToString();
    }

    public void RemoveDeceasedUnit(GameObject unit)
    {
        // This function is called from the unit's script to remove the unit once it has been killed.
        // Remove it from unitsInPlay.
        unitsInPlay.Remove(unit);

        // Destory Unit.
        Destroy(unit);
    }

    public void SetUnitAttributesAtCreation(GameObject unit)
    {
        // Pick a random name for the unit.
        unit.GetComponent<UnitController>().unitName = Utils.GetName();

        // This can be expanded once differnt unit types are created.
    }

    public void updateCameraForPlayerTurn()
    {
        mainCam.transform.position = new Vector3(selectedUnit.transform.position.x, 5f, selectedUnit.transform.position.z - 4);
        mainCam.transform.rotation = Quaternion.Euler(45f, 0f, 0);
    }

    public void MoveUnit(GameObject unit)
    {
        NextPosition = new Vector3(movementSelectedTiles[nextPositionIndex].transform.position.x, 0f, movementSelectedTiles[nextPositionIndex].transform.position.z);


        if (selectedUnit.transform.position == NextPosition)
        {
            // Increment Index.
            nextPositionIndex++;
            PlayUnitRun(unit); //Play the run animation.


            // Check to see if the next ground tile requires the unit to move on top of it.
            if (nextPositionIndex < movementSelectedTiles.Count)
            {
                if (!UnitInteractsWithNextGroundTileOnMove(selectedUnit, nextPositionIndex))
                {
                    selectedUnit.transform.LookAt(movementSelectedTiles[nextPositionIndex].transform.position);
                    PlayUnitInteract(unit); // Play the interact with animation.
                    unitIsMoving = false;

                    nextPositionIndex = 0; // This is the index of the next object the unit needs to move towards.


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
                }
            }

            // If the list has no more elements.
            else if (nextPositionIndex >= movementSelectedTiles.Count)
            {
                unitIsMoving = false;

                PlayUnitIdle(unit); // Play the idle animation because unit has stopped.

                nextPositionIndex = 0; // This is the index of the next object the unit needs to move towards.

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
            }

        }

        else
        {
            selectedUnit.transform.LookAt(NextPosition);
            selectedUnit.transform.position = Vector3.MoveTowards(selectedUnit.transform.position, NextPosition, unitMovementSpeed * Time.deltaTime);
        }
    }

    public void PlayUnitIdle(GameObject unit)
    {
        // Call this function once when the unit needs to be set to idle.
        unit.GetComponent<UnitController>().PlayIdle();
    }

    public void PlayUnitRun(GameObject unit)
    {
        // Call this function once when the unit needs to be set to run.
        unit.GetComponent<UnitController>().PlayRun();
        //m_interact_A
    }

    public void PlayUnitInteract(GameObject unit)
    {
        // Call this function once when the unit needs to be set to interact.
        unit.GetComponent<UnitController>().PlayInteract();
    }

    #endregion


    #region Zombie Turn Function

    public void ZombieTurn()
    {
        // Set the players turn to false to disable player inputs.
        playersTurn = false;

        // Set the current zombie to the first zombie on the list, if there is at least 1 zombie on the list.
        if (zombiesInPlay.Count >= 1)
        {
            selectedZombie = zombiesInPlay[0];

            // Set camera for zombie turn.
            updateCameraForZombieTurn();

            // Set the bool on the current zombie to true to start zombie process.
            selectedZombie.GetComponent<ZombieController>().startTurn = true;
        }

        else UpdateTurn();

    }

    public void updateCameraForZombieTurn()
    {
        mainCam.transform.position = new Vector3(selectedZombie.transform.position.x, 12f, selectedZombie.transform.position.z - 10);
        mainCam.transform.rotation = Quaternion.Euler(45f, 0f, 0);
    }


    public Vector3 FindClearLocationForZombieSpawn()
    {
        bool locationFound = false;
        int attemptNumber = 20;

        while(locationFound != true)
        {
            if (attemptNumber <= 0) return Vector3.zero;

            attemptNumber -= 1;

            // We take a guess at the location.
            var guessLocationX = UnityEngine.Random.Range(0, WorldSize);
            var guessLocationZ = UnityEngine.Random.Range(0, WorldSize);

            // Check to see if the location is clear.
            if (groundTiles[LocateIndexOfGroundTile(guessLocationX, guessLocationZ)] != null)
            {
                if (groundTiles[LocateIndexOfGroundTile(guessLocationX, guessLocationZ)].GetComponent<GroundTileController>().terrainIsPassable)
                {
                    return new Vector3(guessLocationX, 0f, guessLocationZ);
                }
            }
        }

        return Vector3.zero;
    }

    public void CreateZombieAtRandomLocation()
    {
        var locationForSpawn = FindClearLocationForZombieSpawn();

        // If no location was found we will not spawn the zombie and send a message to Debug.
        if (locationForSpawn == Vector3.zero)
        {
            Debug.Log("No zombie spawn location could be found.");
        }

        else
        {
            // Create game object.
            var temp = Instantiate(zombiePrefab, locationForSpawn, Quaternion.identity);

            // Make the unit a child of the ground tile.
            var row = Mathf.RoundToInt(locationForSpawn.x);
            var col = Mathf.RoundToInt(locationForSpawn.z);
            temp.transform.SetParent(groundTiles[LocateIndexOfGroundTile(row, col)].transform);

            // Set the ground tiles attribute so that the terrain is no longer passable.
            groundTiles[LocateIndexOfGroundTile(row, col)].GetComponent<GroundTileController>().terrainIsPassable = false;

            // Add the unit to the list of units in play.
            zombiesInPlay.Add(temp);
        }
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

        // Update structures.
        UpdateStructuresAtEndOfRound();

        // Update Unit attributes.
        UpdateUnitsAtEndOfRound();

        // Update skills points.
        UpdateSkillPointsAtEndOfRound();

        if(zombiesInPlay.Count < zombieCap)
        {
            CreateZombieAtRandomLocation(); // For now we will create 1 zombie at the update.
        }
        

        // Update Current Zombies in Play.
        UpdateZombiesAtEndOfRound();

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
                unitsInPlay[count].GetComponent<UnitController>().actionPoints += 1;
        }
    }

    public void UpdateStructuresAtEndOfRound()
    {
        for (int structureIndex = 0; structureIndex < currentStructuresInGame.Count; structureIndex++)
        {
            // Update Farm Plots.
            if (currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().structureType == "Farm Plot")
            {
                if (currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().cropsPlanted)
                {
                    currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().daysSinceCropsPlanted += 1;

                    // Check to see if crops are ready for harvest.
                    currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().CropsAreReadyForHarvest();

                }
            }

            // Update Living Quarters.
            else if (currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().structureType == "Living Quarters")
            {
                // Iterate through the list of units in game and check to see if they are in range.
                // If they are they will get an action point boost.
                for (int unitIndex = 0; unitIndex < unitsInPlay.Count; unitIndex++)
                {
                    if (Mathf.Abs(currentStructuresInGame[structureIndex].transform.position.x - unitsInPlay[unitIndex].transform.position.x) 
                        <= currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().actionPointHealRange && 
                        Mathf.Abs(currentStructuresInGame[structureIndex].transform.position.z - unitsInPlay[unitIndex].transform.position.z) 
                        <= currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().actionPointHealRange)
                    {
                        // Use function on structure to heal action points.
                        currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().HealActionPoints(unitsInPlay[unitIndex]);
                    }
                }
            }

            // Update Medical Facilities.
            else if (currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().structureType == "Medical Facility")
            {
                // Iterate through the list of units in game and check to see if they are in range.
                // If they are they will get an action point boost.
                for (int unitIndex = 0; unitIndex < unitsInPlay.Count; unitIndex++)
                {
                    if (Mathf.Abs(currentStructuresInGame[structureIndex].transform.position.x - unitsInPlay[unitIndex].transform.position.x)
                        <= currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().hitPointHealRange &&
                        Mathf.Abs(currentStructuresInGame[structureIndex].transform.position.z - unitsInPlay[unitIndex].transform.position.z)
                        <= currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().hitPointHealRange)
                    {
                        // Use function on structure to heal action points.
                        currentStructuresInGame[structureIndex].gameObject.GetComponent<StructureContoller>().HealHitPoints(unitsInPlay[unitIndex]);
                    }
                }
            }

            // Update Walls.
            // Update Town Hall.
        }
    }

    public void UpdateSkillPointsAtEndOfRound()
    {
        // The player will receive 0.1 skill point for every unit that is in play at the end of each round.
        skillPointsAvailable += unitsInPlay.Count * 0.1f;

        UpdateSkillsPointSliderAndText();
    }

    public void UpdateZombiesAtEndOfRound()
    {
        for (int indexOfZombie = 0; indexOfZombie < zombiesInPlay.Count; indexOfZombie++)
        {
            zombiesInPlay[indexOfZombie].GetComponent<ZombieController>().actionPoints = 5;
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
                        // The adjacent tiles will also be made childen of the center tile and tagged so that they can be identified when unit moving.
                        // The function we need this in is UnitInteractsWithNextGroundTileOnMove

                        for (int row = -1; row < 2; row++)
                        {
                            for (int col = -1; col < 2; col++)
                            {
                                if (groundTiles[LocateIndexOfGroundTile(line + row, letter + col)] != null)
                                {
                                    groundTiles[LocateIndexOfGroundTile(line + row, letter + col)].GetComponent<GroundTileController>().terrainIsPassable = false;

                                    // Set tag to 'Holding Factory'
                                    groundTiles[LocateIndexOfGroundTile(line + row, letter + col)].transform.tag = "Holding Factory";

                                    // Make adjacent tiles children.
                                    if (row != 0 || col != 0)
                                    {
                                        groundTiles[LocateIndexOfGroundTile(line + row, letter + col)].transform.SetParent(groundTiles[LocateIndexOfGroundTile(line, letter)].transform);
                                    }
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
        return rotations[UnityEngine.Random.Range(0, 6)];
    }

    public float GetRandomOrthogonalRotation()
    {
        float[] rotations = { 0f, 90f, -90f, 180f};
        return rotations[UnityEngine.Random.Range(0, 4)];
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
        // Make this button disappear.
        EndOfDayTurnButton.gameObject.SetActive(false);

        // Start the zombie's turn.
        ZombieTurn();
    }

    public void UpdateBasicInformationText()
    {
        currentDayText.text = currentDay.ToString();
        skillPointsAvailableText.text = skillPointsAvailable.ToString();
        foodText.text = food.ToString();
        populationText.text = population.ToString();
        populationCapText.text = populationCap.ToString();
        UpdateSkillsPointSliderAndText();
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

        // Update the unit health slider.
        HealthUnitSlider.value = ((float)selectedUnit.GetComponent<UnitController>().hitPoints / (float)selectedUnit.GetComponent<UnitController>().hitPointLimit);

        IndividualUnitPanel.gameObject.SetActive(true);
    }

    public void CloseIndividualUnitPanel()
    {
        // Close the panel.
        IndividualUnitPanel.gameObject.SetActive(false);

        // Remove the Movement Option Tiles if they exist.
        for (int listIndex = 0; listIndex < movementOptionTiles.Count; listIndex++)
        {
            GameObject.Destroy(movementOptionTiles[listIndex]);
        }
        movementOptionTiles.Clear();
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
            actionPointAvailable += movementSelectedTiles.Count;

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
            // a. If the ground title is the same tile that the player is on, we will do nothing.
            if (selectedUnit.transform.IsChildOf(hit.transform)) return;

            // b. Check to see if the ground tile has the same location as a tile in the list movementSlectedTiles, if it is already listed we will ignore.
            for (int count = 0; count < movementSelectedTiles.Count; count++)
            {
                if (hit.transform.position.x == movementSelectedTiles[count].transform.position.x && hit.transform.position.z == movementSelectedTiles[count].transform.position.z)
                {
                    return;
                }
            }

            // c. Check to see if the ground tile is adjacent to the last tile in the list, if it is not then we will ignore.
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
            
            // d.  Check to see if the player has the action points and update them.
            if (hit.transform.gameObject.GetComponent<GroundTileController>().terrainIsPassable)
            {
                if (actionPointAvailable > 0)
                {
                    actionPointAvailable -= 1;
                }

                else return;
            }

            // e. Create a movement Selected Tile at the location of the ground tile and add it to the list if the ground tile also has a movement option tile.
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

        // 2. Check to see if the object the mouse is over is a structure and the selection option is enabled.
        if ((acceptableTags.Contains(hit.collider.tag.ToString()) || acceptableStructureTags.Contains(hit.collider.tag.ToString())) && movementSelectionCanContinue)
        {
            // b. Check to see if the ground tile has the same location as a tile in the list movementSlectedTiles, if it is already listed we will ignore.
            for (int count = 0; count < movementSelectedTiles.Count; count++)
            {
                if (hit.transform.parent.position.x == movementSelectedTiles[count].transform.position.x && hit.transform.parent.position.z == movementSelectedTiles[count].transform.position.z)
                {
                    return;
                }
            }

            // c. Check to see if the ground tile is adjacent to the last tile in the list, if it is not then we will ignore.
            if (movementSelectedTiles.Count > 0)
            {
                if ((Mathf.Abs(hit.transform.parent.position.x - movementSelectedTiles[movementSelectedTiles.Count - 1].transform.position.x) <= 0 &&
                    Mathf.Abs(hit.transform.parent.position.z - movementSelectedTiles[movementSelectedTiles.Count - 1].transform.position.z) <= 1) ||
                    (Mathf.Abs(hit.transform.parent.position.x - movementSelectedTiles[movementSelectedTiles.Count - 1].transform.position.x) <= 1 &&
                    Mathf.Abs(hit.transform.parent.position.z - movementSelectedTiles[movementSelectedTiles.Count - 1].transform.position.z) <= 0))
                {

                }

                else return;
            }

            else
            {
                if ((Mathf.Abs(hit.transform.parent.position.x - selectedUnit.transform.position.x) <= 0 &&
                    Mathf.Abs(hit.transform.parent.position.z - selectedUnit.transform.position.z) <= 1) ||
                    (Mathf.Abs(hit.transform.parent.position.x - selectedUnit.transform.position.x) <= 1 &&
                    Mathf.Abs(hit.transform.parent.position.z - selectedUnit.transform.position.z) <= 0))
                {

                }

                else return;
            }

            // d.  Check to see if the player has the action points and update them.
            if (hit.transform.parent.gameObject.GetComponent<GroundTileController>().terrainIsPassable)
            {
                if (actionPointAvailable > 0)
                {
                    actionPointAvailable -= 1;
                }

                else return;
            }

            // e. Create a movement Selected Tile at the location of the ground tile and add it to the list if the ground tile also has a movement option tile.
            // If the ground tile is set to not passable, then the list must end here.
            for (int count = 0; count < movementOptionTiles.Count; count++)
            {
                if (hit.transform.parent.position.x == movementOptionTiles[count].transform.position.x && hit.transform.parent.position.z == movementOptionTiles[count].transform.position.z)
                {
                    var tempTile = Instantiate(movementSelectedPrefab, new Vector3(hit.transform.parent.position.x, .1f, hit.transform.parent.position.z), Quaternion.identity);
                    tempTile.transform.SetParent(hit.transform.parent);
                    movementSelectedTiles.Add(tempTile);
                }

                if (!hit.transform.parent.gameObject.GetComponent<GroundTileController>().terrainIsPassable) movementSelectionCanContinue = false;
            }
        }


    }

    public bool UnitInteractsWithNextGroundTileOnMove(GameObject unit, int indexForTileArray)
    {
        // Returns a bool of true if the unit should move onto the selected ground tile.

        // If the movement selected list is not empty that means we need to move the unit.

        // If the ground title is passable then we can move the unit on to it.

        if (movementSelectedTiles[indexForTileArray].transform.parent.GetComponent<GroundTileController>().terrainIsPassable)
        {

            unit.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;
            unit.transform.SetParent(movementSelectedTiles[nextPositionIndex].transform.parent);
            unit.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = false;

            SpendActionPoint(1, movementSelectedTiles[indexForTileArray].transform.position);

            return true;
        }

        // If the ground tile is not passable it is because the in an object on it.
        // Factories work differently because there are 9 tiles in all that the factory sits on and the factory is only the child of the center.
        else if (movementSelectedTiles[indexForTileArray].transform.parent.tag.ToString() == "Holding Factory")
        {
            var childCount = movementSelectedTiles[indexForTileArray].transform.parent.parent.transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = movementSelectedTiles[indexForTileArray].transform.parent.parent.GetChild(i);
                var childTag = child.tag;

                if (childTag == "Abandoned Factory")
                {
                    selectedStructureForUse = child.transform.gameObject;
                    OpenInteractWithStructurePanel(child.transform.gameObject);
                }

            }

            return false;
        }

        else
        {
            
            // We need to handle the unit's interation with that object.
            
            var childCount = movementSelectedTiles[indexForTileArray].transform.parent.transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = movementSelectedTiles[indexForTileArray].transform.parent.GetChild(i);
                var childTag = child.tag;

                // If the ground tile contains a tree or rock we will harvest.
                if (childTag == "Tree" || childTag == "Rock") Harvest(child.transform.gameObject);


                // If the ground title is a Abandoned Structure or Structure we will bring up Interact with Structure Menu.
                else if (childTag == "Abandoned House" || childTag == "Abandoned Factory"
                    || childTag == "Abandoned Vehicle" || childTag == "Loot Box" || childTag == "Farm Plot" || childTag == "Living Quarters"
                    || childTag == "Medical Facility" || childTag == "Wall" || childTag == "Town Hall")
                    {
                    selectedStructureForUse = child.transform.gameObject;
                    OpenInteractWithStructurePanel(child.transform.gameObject);
                    }

                // If the ground title contains a zombie we will have the player attack.


                // If the ground title contains another player then nothing will happen.
                // Do something based on tag
            }

            

            return false;
        }
    }

    public void Harvest(GameObject itemToHarvest)
    {
        // If the item is a tree.
        if (itemToHarvest.transform.tag.ToString() == "Tree")
        {
            if (selectedUnit.GetComponent<UnitController>().actionPoints >= 1)
            {
                // Reduce action points.
                SpendActionPoint(1, itemToHarvest.transform.position);

                wood += 1;
                itemToHarvest.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;

                // Play splatter fx.
                StartCoroutine(PlayDirtSplatterFX(itemToHarvest.transform.position, itemToHarvest));
            }
        }

        // If the item is a rock.
        else if (itemToHarvest.transform.tag.ToString() == "Rock")
        {
            if (selectedUnit.GetComponent<UnitController>().actionPoints >= 1)
            {
                // Reduce action points.
                SpendActionPoint(1, itemToHarvest.transform.position);

                // Play splatter fx.
                PlayDirtSplatterFX(itemToHarvest.transform.position);

                stone += 1;
                itemToHarvest.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;

                // Play splatter fx.
                StartCoroutine(PlayDirtSplatterFX(itemToHarvest.transform.position, itemToHarvest));
            }
        }

        // If this item is not harvestable.
        else Debug.Log("Can not harvest this item.");
    }

    public bool SpendActionPoint(int numberOfPointsToSpend, Vector3 loc)
    {
        // Returns true if the unit has the action points to spend.

        if (selectedUnit.GetComponent<UnitController>().actionPoints >= numberOfPointsToSpend)
        {
            // Reduce action points.
            selectedUnit.GetComponent<UnitController>().actionPoints -= numberOfPointsToSpend;

            // Play animation.
            StartCoroutine(PlaySpendActionPoint(loc));

            return true;
        }

        else return false;
        
        
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
        SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);

        // Set the feedback panel to off.
        InteractWithStructureFeedBackPanel.SetActive(false);

        // Open the panel in game.
        InteractWithStructurePanel.SetActive(true);

        // Use if statments to open the panel with the correct options displayed.
        if (structureObject.transform.tag.ToString() == "Abandoned House" || structureObject.transform.tag.ToString() == "Abandoned Factory")
        {
            SetButtonToSeeThrough(false, scavangeButton);
            
        }

        // If the structure is a Loot Box we need to destory it.
        else if (structureObject.transform.tag.ToString() == "Loot Box" || structureObject.transform.tag.ToString() == "Abandoned Vehicle")
        {
            // Set the parent to passable.
            structureObject.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;

            // Set Scavange button to useable.
            SetButtonToSeeThrough(false, scavangeButton);
            

            // To destory loot box on scavenge we need to set it to the selected structure.
            selectedStructureForUse = structureObject;

        }

        else if (structureObject.transform.tag.ToString() == "Farm Plot")
        {
            // To set the correct button we need to check if the farm has crops planted.
            if (structureObject.GetComponent<StructureContoller>().cropsPlanted == false)
            {
                SetButtonToSeeThrough(false, plantCropsButton);
            }

            else if (structureObject.GetComponent<StructureContoller>().cropsReadyForHarvest)
            {
                SetButtonToSeeThrough(false, harvestButton);
            }
            
            SetButtonToSeeThrough(false, repairButton);
            SetButtonToSeeThrough(false, upgradeButton);
        }

        else if (structureObject.transform.tag.ToString() == "Farm Plot" || structureObject.transform.tag.ToString() == "Living Quarters"
            || structureObject.transform.tag.ToString() == "Medical Facility" || structureObject.transform.tag.ToString() == "Wall" 
            || structureObject.transform.tag.ToString() == "Town Hall")
        {
            SetButtonToSeeThrough(false, repairButton);
            SetButtonToSeeThrough(false, upgradeButton);
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
        // Reduce the action points.
        SpendActionPoint(1, selectedUnit.transform.position);

        // This can get more complicated but for now we will just randomly award food.
        var amtOfFoodToSavanged = UnityEngine.Random.Range(1, 5);
        food += amtOfFoodToSavanged;
        InteractWithStructureFeedbackText.text = "+ " + amtOfFoodToSavanged.ToString() + " Food";
        InteractWithStructureFeedBackPanel.SetActive(true);
        SetButtonToSeeThrough(true, scavangeButton);
        SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
        foodText.text = food.ToString();

        // And if the structure is a house or factory there is a chance we can get another survivor.
        if (selectedStructureForUse.tag == "Abandoned House" || selectedStructureForUse.tag == "Abandoned Factory")
        {
            if (UnityEngine.Random.Range(0, 10) < 9 && unitsInPlay.Count < populationCap) DiscoverSurvivorOnScavenge();
            {
                InteractWithStructureFeedbackText.text += "\nSurvivor Found!";
            }
        }

        // Destroy objects that should get destroyed.
        else if (selectedStructureForUse.tag == "Loot Box" || selectedStructureForUse.tag == "Abandoned Vehicle")
        {
            // Play splatter fx.
            StartCoroutine(PlayDirtSplatterFX(selectedStructureForUse.transform.position, selectedStructureForUse));
        }
        

        
    }

    public bool DiscoverSurvivorOnScavenge()
    {
        Debug.Log("Survivor Discover");

        // Find a location to place survivor.
        // Check all tiles around selected unit, if there is a clear one we will place the unit there.
        // Otherwise we will not discover a unit.

        bool positionSelected = false;
        int xPos = 0;
        int zPos = 0;

        for (int row = Mathf.RoundToInt(selectedUnit.transform.position.x - 1); row < selectedUnit.transform.position.x + 1; row++)
        {
            for(int col = Mathf.RoundToInt(selectedUnit.transform.position.z - 1); col < selectedUnit.transform.position.z + 1; col++)
            {
                // check to see if the ground tile is empty.
                if (groundTiles[LocateIndexOfGroundTile(row, col)] != null)
                {
                    if (groundTiles[LocateIndexOfGroundTile(row, col)].GetComponent<GroundTileController>().terrainIsPassable)
                    {
                        xPos = row;
                        zPos = col;
                        positionSelected = true;
                    }
                }

            }
        }

        if (positionSelected)
        {
            createUnitAtLocation(new Vector3(xPos, 0f, zPos));

            return true;
        }

        else return false;

    }

    public void ClickUpgrade()
    {
        // Check to see if structure is max upgrade.
        if (selectedStructureForUse.GetComponent<StructureContoller>().currentStructureLevel != selectedStructureForUse.GetComponent<StructureContoller>().structureObjects.Length)
        {
            //  Check to see if unit has required materials.
            if (CheckToSeeIfPlayerHasRequiredMaterialsToBuild(selectedStructureForUse))
            {
                selectedStructureForUse.GetComponent<StructureContoller>().UpgradeStructure();
                InteractWithStructurePanel.SetActive(false);

                // Remove materials from player.
                wood -= selectedStructureForUse.GetComponent<StructureContoller>().woodToBuild[selectedStructureForUse.GetComponent<StructureContoller>().currentStructureLevel];
                stone -= selectedStructureForUse.GetComponent<StructureContoller>().stoneToBuild[selectedStructureForUse.GetComponent<StructureContoller>().currentStructureLevel];
                food -= selectedStructureForUse.GetComponent<StructureContoller>().foodToBuild[selectedStructureForUse.GetComponent<StructureContoller>().currentStructureLevel];
            }

            else
            {
                InteractWithStructureFeedbackText.text = "Requires More Materials";
                InteractWithStructureFeedBackPanel.SetActive(true);
                SetButtonToSeeThrough(true, repairButton);
                SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
            }
        }

        else
        {
            InteractWithStructureFeedbackText.text = "Max Upgrade Reached.";
            InteractWithStructureFeedBackPanel.SetActive(true);
            SetButtonToSeeThrough(true, repairButton);
            SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
        }


    }

    public void ClickRepair()
    {
        //  Check to see if unit has at least 1 action point.
        if (selectedUnit.GetComponent<UnitController>().actionPoints >= 1)
        {

            // Check to see if structure is already full health.
            if (selectedStructureForUse.GetComponent<StructureContoller>().hitPoints < selectedStructureForUse.GetComponent<StructureContoller>().hitPointLimit)
            {
                var repairsMade = selectedStructureForUse.GetComponent<StructureContoller>().RepairStructure(selectedUnit.GetComponent<UnitController>().repairPoints);
                InteractWithStructureFeedbackText.text = "Actions Points: -1 \nRepairs Made: +" +  repairsMade.ToString();
                InteractWithStructureFeedBackPanel.SetActive(true);
                SetButtonToSeeThrough(true, repairButton);
                SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
            }

            else 
            {
                InteractWithStructureFeedbackText.text = "No Repair Needed";
                InteractWithStructureFeedBackPanel.SetActive(true);
                SetButtonToSeeThrough(true, repairButton);
                SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
            }

        }

        else
        {
            InteractWithStructureFeedbackText.text = "Not Enough Action Points";
            InteractWithStructureFeedBackPanel.SetActive(true);
            SetButtonToSeeThrough(true, repairButton);
            SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
        }

        
    }

    public void ClickPlantCrops()
    {

        // Check to see if conditions allow crops to be planted.
        if (food >= 1)
        {
            // Check to see if unit has action points.
            if (selectedUnit.GetComponent<UnitController>().actionPoints >= 1)
            {
                // Check to see if crops have already been planted.
                if (selectedStructureForUse.GetComponent<StructureContoller>().cropsPlanted == false)
                {
                    // Plant crops.
                    selectedStructureForUse.GetComponent<StructureContoller>().PlantCrops();

                    // Set text objects, buttons and panels.
                    InteractWithStructureFeedbackText.text = "Food: -1 \nCrops Planted: Harvest in " + selectedStructureForUse.GetComponent<StructureContoller>().turnsUntilCropsMature.ToString() + " turns.";
                    SetButtonToSeeThrough(true, plantCropsButton);
                    SetButtonToSeeThrough(true, harvestButton);
                    SetButtonToSeeThrough(true, repairButton);
                    SetButtonToSeeThrough(true, upgradeButton);
                    SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
                    InteractWithStructureFeedBackPanel.SetActive(true);

                    // Remove action points.
                    selectedUnit.GetComponent<UnitController>().actionPoints -= 1;

                    // Remove Food.
                    food -= 1;
                    
                }

                else
                {
                    InteractWithStructureFeedbackText.text = "Crops Have Aready Been Planted Here";
                    SetButtonToSeeThrough(true, plantCropsButton);
                    SetButtonToSeeThrough(true, harvestButton);
                    SetButtonToSeeThrough(true, repairButton);
                    SetButtonToSeeThrough(true, upgradeButton);
                    SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
                    InteractWithStructureFeedBackPanel.SetActive(true);
                }
            }

            else
            {
                InteractWithStructureFeedbackText.text = "Not Enough Action Points";
                SetButtonToSeeThrough(true, plantCropsButton);
                SetButtonToSeeThrough(true, harvestButton);
                SetButtonToSeeThrough(true, repairButton);
                SetButtonToSeeThrough(true, upgradeButton);
                SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
                InteractWithStructureFeedBackPanel.SetActive(true);
            }
        }

        else
        {
            InteractWithStructureFeedbackText.text = "Not Enough Food To Plant";
            SetButtonToSeeThrough(true, plantCropsButton);
            SetButtonToSeeThrough(true, harvestButton);
            SetButtonToSeeThrough(true, repairButton);
            SetButtonToSeeThrough(true, upgradeButton);
            SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
            InteractWithStructureFeedBackPanel.SetActive(true);
        }
        
    }

    public void ClickHarvestCrops()
    {
        // Check to see if conditions allow crops to be harvested.
        if (selectedStructureForUse.GetComponent<StructureContoller>().cropsReadyForHarvest)
        {
            // Check to see if the unit has an action point.
            if (selectedUnit.GetComponent<UnitController>().actionPoints >= 1)
            {
                // Set text objects, buttons and panels.
                InteractWithStructureFeedbackText.text = "Action Point: -1 \nCrops Harvested: " + 
                    (selectedStructureForUse.GetComponent<StructureContoller>().cropsAtHarvest * selectedUnit.GetComponent<UnitController>().cropsAtHarvestMultiplier).ToString();
                SetButtonToSeeThrough(true, plantCropsButton);
                SetButtonToSeeThrough(true, harvestButton);
                SetButtonToSeeThrough(true, repairButton);
                SetButtonToSeeThrough(true, upgradeButton);
                SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
                InteractWithStructureFeedBackPanel.SetActive(true);

                // Update food and harvest crops.
                food += selectedStructureForUse.GetComponent<StructureContoller>().HarvestCrops();

                // Remove action points.
                selectedUnit.GetComponent<UnitController>().actionPoints -= 1;
            }

            else
            {
                InteractWithStructureFeedbackText.text = "Not Enough Action Points.";
                SetButtonToSeeThrough(true, plantCropsButton); 
                SetButtonToSeeThrough(true, harvestButton);
                SetButtonToSeeThrough(true, repairButton);
                SetButtonToSeeThrough(true, upgradeButton);
                SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
                InteractWithStructureFeedBackPanel.SetActive(true);
            }
        }

        else
        {
            InteractWithStructureFeedbackText.text = "No Crops to Harvest.";
            SetButtonToSeeThrough(true, plantCropsButton);
            SetButtonToSeeThrough(true, harvestButton);
            SetButtonToSeeThrough(true, repairButton);
            SetButtonToSeeThrough(true, upgradeButton);
            SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);
            InteractWithStructureFeedBackPanel.SetActive(true);
        }

    }

    #endregion

    #region Build Panel Functions

    public void OpenCloseBuildPanel()
    {
        if (BuildPanel.activeSelf)
        {
            BuildPanel.SetActive(false);
        }

        else
        {
            // Center Camera on selected player.
            updateCameraForPlayerTurn();

            BuildPanel.SetActive(true);
        } 
            
    }

    public void ClickStructureToBuild(string nameOfStructure)
    {
        if (acceptableStructureTags.Contains(nameOfStructure))
        {
            // Change bool.
            structureIsBeingBuilt = true;

            // Set the structure to be built.
            currentStructureSelectedToBuild = PopulateCurrentStructureToBuild(nameOfStructure);

            // Place the build option tiles.
            PlaceBuildOptionTiles(selectedUnit);

            // Close the Build Panel.
            OpenCloseBuildPanel();
        }

        else Debug.Log("Invalid Name Of Structure To Build");
    }

    public void HoverStructureToBuildEnter(string nameOfStructure)
    {
        structureTitleText.text = nameOfStructure;

        if (nameOfStructure == "Farm Plot")
        {
            var requiredMaterialString = "Wood: " + farmPlotPrefab.GetComponent<StructureContoller>().woodToBuild[0].ToString() +
                " Stone: " + farmPlotPrefab.GetComponent<StructureContoller>().stoneToBuild[0].ToString() + " Food: " +
                farmPlotPrefab.GetComponent<StructureContoller>().foodToBuild[0].ToString();

            requiredMatsText.text = requiredMaterialString;
        }

        else if (nameOfStructure == "Living Quarters")
        {
            var requiredMaterialString = "Wood: " + livingQuartersPrefab.GetComponent<StructureContoller>().woodToBuild[0].ToString() +
                " Stone: " + livingQuartersPrefab.GetComponent<StructureContoller>().stoneToBuild[0].ToString() + " Food: " +
                livingQuartersPrefab.GetComponent<StructureContoller>().foodToBuild[0].ToString();

            requiredMatsText.text = requiredMaterialString;
        }

        else if (nameOfStructure == "Medical Facility")
        {
            var requiredMaterialString = "Wood: " + medicalFacilityPrefab.GetComponent<StructureContoller>().woodToBuild[0].ToString() +
                " Stone: " + medicalFacilityPrefab.GetComponent<StructureContoller>().stoneToBuild[0].ToString() + " Food: " +
                medicalFacilityPrefab.GetComponent<StructureContoller>().foodToBuild[0].ToString();

            requiredMatsText.text = requiredMaterialString;
        }

        else if (nameOfStructure == "Wall")
        {
            var requiredMaterialString = "Wood: " + wallPrefab.GetComponent<StructureContoller>().woodToBuild[0].ToString() +
                " Stone: " + wallPrefab.GetComponent<StructureContoller>().stoneToBuild[0].ToString() + " Food: " +
                wallPrefab.GetComponent<StructureContoller>().foodToBuild[0].ToString();

            requiredMatsText.text = requiredMaterialString;
        }

        else if (nameOfStructure == "Town Hall")
        {
            var requiredMaterialString = "Wood: " + townHallPrefab.GetComponent<StructureContoller>().woodToBuild[0].ToString() +
                " Stone: " + townHallPrefab.GetComponent<StructureContoller>().stoneToBuild[0].ToString() + " Food: " +
                townHallPrefab.GetComponent<StructureContoller>().foodToBuild[0].ToString();

            requiredMatsText.text = requiredMaterialString;
        }

        else Debug.Log("String put into HoverStructureToBuildEnter is not valid");

    }

    public void HoverStructureToBuildExit()
    {
        structureTitleText.text = "Select a structure to build.";
        requiredMatsText.text = "";
    }

    public GameObject PopulateCurrentStructureToBuild(string nameOfStructure)
    {
        if (nameOfStructure == "Farm Plot") return farmPlotPrefab;
        else if (nameOfStructure == "Living Quarters") return livingQuartersPrefab;
        else if (nameOfStructure == "Medical Facility") return medicalFacilityPrefab;
        else if (nameOfStructure == "Wall") return wallPrefab;
        else if (nameOfStructure == "Town Hall") return townHallPrefab;
        else return null;
    }

    public void PlaceBuildOptionTiles(GameObject unit)
    {
        // Delete the old build option titles if they exist.
        for (int listIndex = 0; listIndex < buildOptionTiles.Count; listIndex++)
        {
            GameObject.Destroy(buildOptionTiles[listIndex]);
        }
        buildOptionTiles.Clear();

        for (int row = Mathf.RoundToInt(unit.transform.position.x - 1); row <= Mathf.RoundToInt(unit.transform.position.x + 1); row++)
        {
            for (int col = Mathf.RoundToInt(unit.transform.position.z - 1); col <= Mathf.RoundToInt(unit.transform.position.z + 1); col++)
            {
                // Check to see if the ground title exists in array.
                if (groundTiles[LocateIndexOfGroundTile(row, col)] != null)
                {

                    // Check to see if the ground tile is passable.

                    if (groundTiles[LocateIndexOfGroundTile(row, col)].GetComponent<GroundTileController>().terrainIsPassable)
                    {
                        //Create a movement option tile.
                        var tempBuildOptionTile = Instantiate(buildOptionPrefab, new Vector3(row, .05f, col), Quaternion.identity);

                        // Add it to the list so we can easily delete them later.
                        buildOptionTiles.Add(tempBuildOptionTile);

                        // Make it a child of the ground title.
                        tempBuildOptionTile.transform.SetParent(groundTiles[LocateIndexOfGroundTile(row, col)].transform);
                    }
                }
            }
        }
    }

    public void BuildStructure(GameObject structure)
    {
        structureIsBeingBuilt = false;

        // Delete the old build option titles if they exist.
        for (int listIndex = 0; listIndex < buildOptionTiles.Count; listIndex++)
        {
            GameObject.Destroy(buildOptionTiles[listIndex]);
        }
        buildOptionTiles.Clear();

        // Check to see if the player has the required material.
        if (CheckToSeeIfPlayerHasRequiredMaterialsToBuild(structure))
        {
            // Check to see if the location is valid to build.
            int row = Mathf.RoundToInt(hit.transform.position.x);
            int col = Mathf.RoundToInt(hit.transform.position.z);

            bool hasBuildOptionTile = false;

            for (int child = 0; child < groundTiles[LocateIndexOfGroundTile(row, col)].transform.childCount; child++)
            {
                if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(child).tag.ToString() == "BuildOptionTile") hasBuildOptionTile = true;
            }

            if (hasBuildOptionTile)
            {
                // Collect the cost to build.
                wood -= structure.GetComponent<StructureContoller>().woodToBuild[structure.GetComponent<StructureContoller>().currentStructureLevel];
                stone -= structure.GetComponent<StructureContoller>().stoneToBuild[structure.GetComponent<StructureContoller>().currentStructureLevel];
                food -= structure.GetComponent<StructureContoller>().foodToBuild[structure.GetComponent<StructureContoller>().currentStructureLevel];

                // Create structure.
                var tempStructure = Instantiate(structure, new Vector3(hit.transform.position.x, 0f, hit.transform.position.z), GetStructureRotation());

                // Add the structure to the current structure list.
                currentStructuresInGame.Add(tempStructure);

                // Make the structure a child of the ground tile.
                tempStructure.transform.SetParent(groundTiles[LocateIndexOfGroundTile(row, col)].transform);

                // Set the title to not passable.
                groundTiles[LocateIndexOfGroundTile(row, col)].GetComponent<GroundTileController>().terrainIsPassable = false;

                // Hide the selectors
                HideStructureSelectors();
            }

            else
            {
                // Make the feedback panel pop up.
                BuildFeedbackText.text = "Location is out of range.";
                BuildFeedbackPanel.SetActive(true);

                HideStructureSelectors();
            }

        }

        else
        {
            // Make the feedback panel pop up.
            BuildFeedbackText.text = "Requires more materials.";
            BuildFeedbackPanel.SetActive(true);

            HideStructureSelectors();
        } 
            
    }

    public bool CheckToSeeIfPlayerHasRequiredMaterialsToBuild(GameObject structure)
    {
        var requiredWood = structure.GetComponent<StructureContoller>().woodToBuild;
        var requiredStone = structure.GetComponent<StructureContoller>().stoneToBuild;
        var requiredFood = structure.GetComponent<StructureContoller>().foodToBuild;

        if (structure.GetComponent<StructureContoller>().currentStructureLevel != structure.GetComponent<StructureContoller>().structureObjects.Length)
        {
            if (requiredWood[structure.GetComponent<StructureContoller>().currentStructureLevel] <= wood &&
            requiredStone[structure.GetComponent<StructureContoller>().currentStructureLevel] <= stone &&
            requiredFood[structure.GetComponent<StructureContoller>().currentStructureLevel] <= food) return true;

            else return false;
        }

        else return false;

    }

    public void UpdateStructureSelector()
    {
        if (currentStructureSelectedToBuild.name == "Farm Plot") farmPlotSelector.transform.position = hit.transform.position;
        else if (currentStructureSelectedToBuild.name == "Living Quarters") livingQuartersSelector.transform.position = hit.transform.position;
        else if (currentStructureSelectedToBuild.name == "Medical Facility") medicalFacilitySelector.transform.position = hit.transform.position;
        else if (currentStructureSelectedToBuild.name == "Wall") wallSelector.transform.position = hit.transform.position;
        else if (currentStructureSelectedToBuild.name == "Town Hall") townHallSelector.transform.position = hit.transform.position;
        else Debug.Log("UpdateStructureSelector has bad input.");
    }

    public Quaternion GetStructureRotation()
    {

        if (currentStructureSelectedToBuild.name == "Farm Plot") return farmPlotSelector.transform.rotation;
        else if (currentStructureSelectedToBuild.name == "Living Quarters") return livingQuartersSelector.transform.rotation;
        else if (currentStructureSelectedToBuild.name == "Medical Facility") return medicalFacilitySelector.transform.rotation;
        else if (currentStructureSelectedToBuild.name == "Wall") return wallSelector.transform.rotation;
        else if (currentStructureSelectedToBuild.name == "Town Hall") return townHallSelector.transform.rotation;
        else return Quaternion.identity;
    }

    public void HideStructureSelectors()
    {
        var hideLocation = new Vector3(WorldSize * 2, 0f, WorldSize * 2);

        farmPlotSelector.transform.position = hideLocation;
        livingQuartersSelector.transform.position = hideLocation;
        medicalFacilitySelector.transform.position = hideLocation;
        wallSelector.transform.position = hideLocation;
        townHallSelector.transform.position = hideLocation;
    }

    public void OpenStructureStatsPanel(GameObject structure)
    {
        // Open panel.
        StructureStatsPanel.SetActive(true);

        // Update all text objects.
        SelectedStructureTitleText.text = structure.GetComponentInParent<StructureContoller>().structureType;
        SelectedStructureCurrentLevelText.text = (structure.GetComponentInParent<StructureContoller>().currentStructureLevel + 1).ToString();
        SelectedStructureMaxLevelText.text = structure.GetComponentInParent<StructureContoller>().structureObjects.Length.ToString();
        RequiredWoodForUpgradeText.text = structure.GetComponentInParent<StructureContoller>().woodToBuild[structure.GetComponentInParent<StructureContoller>().currentStructureLevel + 1].ToString();
        RequiredStoneForUpgradeText.text = structure.GetComponentInParent<StructureContoller>().stoneToBuild[structure.GetComponentInParent<StructureContoller>().currentStructureLevel + 1].ToString();
        RequiredFoodForUpgradeText.text = structure.GetComponentInParent<StructureContoller>().foodToBuild[structure.GetComponentInParent<StructureContoller>().currentStructureLevel + 1].ToString();
        structureHitPointsSlider.value =  ((float) structure.GetComponentInParent<StructureContoller>().hitPoints / (float) structure.GetComponentInParent<StructureContoller>().hitPointLimit);
        Debug.Log((structure.GetComponentInParent<StructureContoller>().hitPoints / structure.GetComponentInParent<StructureContoller>().hitPointLimit));

    }

    public void ClickExitStructureStatsPanel()
    {
        StructureStatsPanel.SetActive(false);
    }

    #endregion

    #region Skills Panel Functions

    public void OpenCloseSkillsPanel()
    {
        if (skillsPanel.activeInHierarchy) skillsPanel.SetActive(false);
        else skillsPanel.SetActive(true);
    }

    public void UpdateSkillsTextOnHover(string upgradeTitle)
    {
        if (upgradeTitle == "AttackIncreaseBasic" || upgradeTitle == "AttackIncreaseFarmer" || upgradeTitle == "AttackIncreaseSoldier")
        {
            skillsUpgradeTitleText.text = "Attack Increase";
            skillsInformationText.text = "Unit attack will increase to 200%.";
        }

        else if (upgradeTitle == "DefenseIncreaseBasic" || upgradeTitle == "DefenseIncreaseFarmer" || upgradeTitle == "DefenseIncreaseSoldier")
        {
            skillsUpgradeTitleText.text = "Defense Increase";
            skillsInformationText.text = "Unit defense will increase to 200%.";
        }

        else if (upgradeTitle == "SightIncreaseBasic" || upgradeTitle == "SightIncreaseFarmer" || upgradeTitle == "SightIncreaseSoldier")
        {
            skillsUpgradeTitleText.text = "Sight Distance Increase";
            skillsInformationText.text = "Unit sight distance will increase to 200%.";
        }

        else if (upgradeTitle == "RepairIncreaseBasic" || upgradeTitle == "RepairIncreaseFarmer" || upgradeTitle == "RepairIncreaseSoldier")
        {
            skillsUpgradeTitleText.text = "Repair Increase";
            skillsInformationText.text = "Unit repair will increase to 200%.";
        }

        else if (upgradeTitle == "HarvestIncrease")
        {
            skillsUpgradeTitleText.text = "Harvest Increase";
            skillsInformationText.text = "Farmer's harvest will increase to 200%.";
        }

        else if (upgradeTitle == "HarvestTimeDecrease")
        {
            skillsUpgradeTitleText.text = "Time Until Harvest Decrease";
            skillsInformationText.text = "Time until crops are ready to harvest will decrease by one day.";
        }

        else if (upgradeTitle == "HarvestTimeDecrease")
        {
            skillsUpgradeTitleText.text = "Time Until Harvest Decrease";
            skillsInformationText.text = "Time until crops are ready to harvest will decrease by one day.";
        }

        else if (upgradeTitle == "RangeIncrease")
        {
            skillsUpgradeTitleText.text = "Range Increase";
            skillsInformationText.text = "Soldier's attack range will increase by 200%.";
        }

        else if (upgradeTitle == "CriticalHitIncrease")
        {
            skillsUpgradeTitleText.text = "Critical Hit Increase";
            skillsInformationText.text = "The chance of a soldier getting a critical hit increases by 50%.";
        }

        else if (upgradeTitle == "BasicUnit")
        {
            skillsUpgradeTitleText.text = "Basic Unit";
            var tempString = "Hit Point Limit: ";
            tempString += basicUnitPrefabs[0].GetComponent<UnitController>().hitPointLimit.ToString();
            tempString += "\nAttack: " + basicUnitPrefabs[0].GetComponent<UnitController>().attack.ToString();
            tempString += "\nAttack Range: " + basicUnitPrefabs[0].GetComponent<UnitController>().attackRange.ToString();
            tempString += "\nDefense: " + basicUnitPrefabs[0].GetComponent<UnitController>().defense.ToString();
            tempString += "\nRepair: " + basicUnitPrefabs[0].GetComponent<UnitController>().repairPoints.ToString();
            tempString += "\nCrops Yield: " + (basicUnitPrefabs[0].GetComponent<UnitController>().cropsAtHarvestMultiplier * 100).ToString() + "%";
            tempString += "\nCrops Harvest: " + basicUnitPrefabs[0].GetComponent<UnitController>().turnsUntilCropsMature.ToString() + " days.";
            tempString += "\nCritical Hit %: " + basicUnitPrefabs[0].GetComponent<UnitController>().criticalHitPercentage.ToString();
            skillsInformationText.text = tempString;
        }

        else if (upgradeTitle == "FarmerUnit")
        {
            skillsUpgradeTitleText.text = "Farmer Unit";
            var tempString = "Hit Point Limit: ";
            tempString += basicUnitPrefabs[1].GetComponent<UnitController>().hitPointLimit.ToString();
            tempString += "\nAttack: " + basicUnitPrefabs[1].GetComponent<UnitController>().attack.ToString();
            tempString += "\nAttack Range: " + basicUnitPrefabs[1].GetComponent<UnitController>().attackRange.ToString();
            tempString += "\nDefense: " + basicUnitPrefabs[1].GetComponent<UnitController>().defense.ToString();
            tempString += "\nRepair: " + basicUnitPrefabs[1].GetComponent<UnitController>().repairPoints.ToString();
            tempString += "\nCrops Yield: " + (basicUnitPrefabs[1].GetComponent<UnitController>().cropsAtHarvestMultiplier * 100).ToString() + "%";
            tempString += "\nCrops Harvest: " + basicUnitPrefabs[1].GetComponent<UnitController>().turnsUntilCropsMature.ToString() + " days.";
            tempString += "\nCritical Hit %: " + basicUnitPrefabs[1].GetComponent<UnitController>().criticalHitPercentage.ToString();
            skillsInformationText.text = tempString;
        }

        else if (upgradeTitle == "SoldierUnit")
        {
            skillsUpgradeTitleText.text = "Soldier Unit";
            var tempString = "Hit Point Limit: ";
            tempString += basicUnitPrefabs[2].GetComponent<UnitController>().hitPointLimit.ToString();
            tempString += "\nAttack: " + basicUnitPrefabs[2].GetComponent<UnitController>().attack.ToString();
            tempString += "\nAttack Range: " + basicUnitPrefabs[2].GetComponent<UnitController>().attackRange.ToString();
            tempString += "\nDefense: " + basicUnitPrefabs[2].GetComponent<UnitController>().defense.ToString();
            tempString += "\nRepair: " + basicUnitPrefabs[2].GetComponent<UnitController>().repairPoints.ToString();
            tempString += "\nCrops Yield: " + (basicUnitPrefabs[2].GetComponent<UnitController>().cropsAtHarvestMultiplier * 100).ToString() + "%";
            tempString += "\nCrops Harvest: " + basicUnitPrefabs[2].GetComponent<UnitController>().turnsUntilCropsMature.ToString() + " days.";
            tempString += "\nCritical Hit %: " + basicUnitPrefabs[2].GetComponent<UnitController>().criticalHitPercentage.ToString();
            skillsInformationText.text = tempString;
        }


        else Debug.Log("Upgrade Title Invalid.");
    }

    public void UpdateSkillsOnClick(string upgradeTitle)
    {
        // Check to see if there is a skill point to spend.
        if (skillPointsAvailable >= 1)
        {
            // Cash the skill point and update text.
            skillPointsAvailable -= 1;
            UpdateSkillsPointSliderAndText();

            #region This section holds all the possible upgrades.
            // This function is not complete.
            // Currently all it does it turn the button solid.
            if (upgradeTitle == "AttackIncreaseBasic")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(AttackIncreaseButtons[0]))
                {
                    // Need to Increase Attack for Basic Units.
                    basicUnitPrefabs[0].GetComponent<UnitController>().attack *= 2;

                    SetButtonToOpaque(AttackIncreaseButtons[0]);
                }
            }

            else if (upgradeTitle == "AttackIncreaseFarmer")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(AttackIncreaseButtons[1]))
                {
                    // Need to Increase Attack for Basic Units.
                    basicUnitPrefabs[1].GetComponent<UnitController>().attack *= 2;

                    SetButtonToOpaque(AttackIncreaseButtons[1]);
                }
            }

            else if (upgradeTitle == "AttackIncreaseSoldier")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(AttackIncreaseButtons[2]))
                {
                    // Need to Increase Attack for Basic Units.
                    basicUnitPrefabs[2].GetComponent<UnitController>().attack *= 2;

                    SetButtonToOpaque(AttackIncreaseButtons[2]);
                }
            }

            else if (upgradeTitle == "DefenseIncreaseBasic")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(DefenseIncreaseButtons[0]))
                {
                    // Need to Increase Defense for Basic Units.
                    basicUnitPrefabs[0].GetComponent<UnitController>().defense *= 2;

                    SetButtonToOpaque(DefenseIncreaseButtons[0]);
                }
            }

            else if (upgradeTitle == "DefenseIncreaseFarmer")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(DefenseIncreaseButtons[1]))
                {
                    // Need to Increase Defense for Basic Units.
                    basicUnitPrefabs[1].GetComponent<UnitController>().defense *= 2;

                    SetButtonToOpaque(DefenseIncreaseButtons[1]);
                }
            }

            else if (upgradeTitle == "DefenseIncreaseSoldier")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(DefenseIncreaseButtons[2]))
                {
                    // Need to Increase Defense for Basic Units.
                    basicUnitPrefabs[2].GetComponent<UnitController>().defense *= 2;

                    SetButtonToOpaque(DefenseIncreaseButtons[2]);
                }
            }

            else if (upgradeTitle == "SightIncreaseBasic")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(SightIncreaseButtons[0]))
                {
                    // Need to Increase Sight for Basic Units.
                    basicUnitPrefabs[0].GetComponent<UnitController>().sight *= 2;

                    SetButtonToOpaque(SightIncreaseButtons[0]);
                }
            }

            else if (upgradeTitle == "SightIncreaseFarmer")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(SightIncreaseButtons[1]))
                {
                    // Need to Increase Sight for Basic Units.
                    basicUnitPrefabs[1].GetComponent<UnitController>().sight *= 2;

                    SetButtonToOpaque(SightIncreaseButtons[1]);
                }
            }

            else if (upgradeTitle == "SightIncreaseSoldier")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(SightIncreaseButtons[2]))
                {
                    // Need to Increase Sight for Basic Units.
                    basicUnitPrefabs[2].GetComponent<UnitController>().sight *= 2;

                    SetButtonToOpaque(SightIncreaseButtons[2]);
                }
            }

            else if (upgradeTitle == "RepairIncreaseBasic")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(RepairIncreaseButtons[0]))
                {
                    // Need to Increase Repair for Basic Units.
                    basicUnitPrefabs[0].GetComponent<UnitController>().repairPoints *= 2;

                    SetButtonToOpaque(RepairIncreaseButtons[0]);
                }
            }

            else if (upgradeTitle == "RepairIncreaseFarmer")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(RepairIncreaseButtons[1]))
                {
                    // Need to Increase Repair for Basic Units.
                    basicUnitPrefabs[1].GetComponent<UnitController>().repairPoints *= 2;

                    SetButtonToOpaque(RepairIncreaseButtons[1]);
                }
            }

            else if (upgradeTitle == "RepairIncreaseSoldier")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(RepairIncreaseButtons[2]))
                {
                    // Need to Increase Repair for Basic Units.
                    basicUnitPrefabs[2].GetComponent<UnitController>().repairPoints *= 2;

                    SetButtonToOpaque(RepairIncreaseButtons[2]);
                }
            }

            else if (upgradeTitle == "HarvestIncrease")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(HarvestIncreaseButtons))
                {
                    // Need to Increase Harvest for Farmer Units.
                    basicUnitPrefabs[1].GetComponent<UnitController>().cropsAtHarvestMultiplier *= 2;

                    SetButtonToOpaque(HarvestIncreaseButtons);
                }
            }

            else if (upgradeTitle == "HarvestTimeDecrease")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(HarvestTimeDecreaseButton))
                {
                    // Need to Increase Attack for Basic Units.
                    basicUnitPrefabs[1].GetComponent<UnitController>().turnsUntilCropsMature -= 1;

                    SetButtonToOpaque(HarvestTimeDecreaseButton);
                }
            }

            else if (upgradeTitle == "RangeIncrease")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(RangeIncreaseButton))
                {
                    // Need to Increase AttackRange for Soldier Units.
                    basicUnitPrefabs[2].GetComponent<UnitController>().attackRange *= 2;

                    SetButtonToOpaque(RangeIncreaseButton);
                }
            }

            else if (upgradeTitle == "CriticalHitIncrease")
            {

                // Check to see if the button that was pushed has been activated already.
                if (SkillsUpgradeButtonHasNotBeenActivated(CriticalHitIncreaseButton))
                {
                    // Need to Increase Critical Hit for Soldiers Units.
                    basicUnitPrefabs[2].GetComponent<UnitController>().criticalHitPercentage += .5f;

                    SetButtonToOpaque(CriticalHitIncreaseButton);
                }
            }
            #endregion

            #region This section updates all current units after upgrade.

            for (int indexOfUnit = 0; indexOfUnit < unitsInPlay.Count; indexOfUnit++)
            {
                if (unitsInPlay[indexOfUnit].GetComponent<UnitController>().unitClass == UnitController.Class.Basic)
                {
                    // Make the update for the basic unit.
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().attack = basicUnitPrefabs[0].GetComponent<UnitController>().attack;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().defense = basicUnitPrefabs[0].GetComponent<UnitController>().defense;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().sight = basicUnitPrefabs[0].GetComponent<UnitController>().sight;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().repairPoints = basicUnitPrefabs[0].GetComponent<UnitController>().repairPoints;
                }

                else if (unitsInPlay[indexOfUnit].GetComponent<UnitController>().unitClass == UnitController.Class.Farmer)
                {
                    // Make the update for the Farmer unit.
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().attack = basicUnitPrefabs[1].GetComponent<UnitController>().attack;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().defense = basicUnitPrefabs[1].GetComponent<UnitController>().defense;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().sight = basicUnitPrefabs[1].GetComponent<UnitController>().sight;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().repairPoints = basicUnitPrefabs[1].GetComponent<UnitController>().repairPoints;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().cropsAtHarvestMultiplier = basicUnitPrefabs[1].GetComponent<UnitController>().cropsAtHarvestMultiplier;
                }

                else if (unitsInPlay[indexOfUnit].GetComponent<UnitController>().unitClass == UnitController.Class.Soldier)
                {
                    // Make the update for the Soldier unit.
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().attack = basicUnitPrefabs[2].GetComponent<UnitController>().attack;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().defense = basicUnitPrefabs[2].GetComponent<UnitController>().defense;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().sight = basicUnitPrefabs[2].GetComponent<UnitController>().sight;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().repairPoints = basicUnitPrefabs[2].GetComponent<UnitController>().repairPoints;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().attackRange = basicUnitPrefabs[2].GetComponent<UnitController>().attackRange;
                    unitsInPlay[indexOfUnit].GetComponent<UnitController>().criticalHitPercentage = basicUnitPrefabs[2].GetComponent<UnitController>().criticalHitPercentage;
                }
            }

            #endregion
        }

        else skillsUpgradeTitleText.text = "No Skill Points Available.";
        skillsInformationText.text = "";


    }

    public bool SkillsUpgradeButtonHasNotBeenActivated(Button button)
    {
        
        Color temp = button.image.color;

        if (temp.a != 1) return true;
        else
        {
            Debug.Log("Button Is already Opaque");
            return false;
        }

    }

    public void SetButtonToOpaque(Button button)
    {
        Color temp = button.image.color;
        temp.a = 1f;
        button.image.color = temp;
    }

    public void ClearSkillsTextOnHoverExit()
    {
        skillsUpgradeTitleText.text = "";
        skillsInformationText.text = "";
    }


    public void UpdateSkillsPointSliderAndText()
    {
        // Take the whole number off the available skill points and update the text.
        var wholeNumber = Mathf.Floor(skillPointsAvailable);
        skillPointsAvailableText.text = wholeNumber.ToString();

        // Now update the slider with the tenths place.
        skillPointsSlider.value = skillPointsAvailable - (float) wholeNumber;

        // If the skills points available is above 1 we will want to show the skill points to spend icon.
        if (wholeNumber > 0) skillsPointsToSpendImage.gameObject.SetActive(true);
        else skillsPointsToSpendImage.gameObject.SetActive(false);
    }
    #endregion

    #region Zombie Stats Panel Functions

    public void CloseZombieStatsPanel()
    {
        zombieStatsPanel.SetActive(false);
    }

    public void OpenZombieStatsPanel(GameObject zombie)
    {
        // Set the hit point slider.
        zombieHitPointsSlider.value = ((float)(zombie.GetComponent<ZombieController>().hitPoints) / (float) zombie.GetComponent<ZombieController>().hitPointLimit);

        // Set the zombie type.
        zombieTitleText.text = "Basic Zombie";

        // Set the description.
        zombieDescriptionText.text = "Stinky Zombie.";

        // Open the panel.
        zombieStatsPanel.SetActive(true);
    }

    #endregion

    #region Attack Functions

    public void ToggleAttackMode()
    {
        if (!unitIsAttacking)
        {
            // Check to see if the player has action points.
            if (selectedUnit.GetComponent<UnitController>().actionPoints > 0)
            {
                unitIsMoving = false;
                unitIsAttacking = true;

                // Set the image to active.
                attackingImage.SetActive(true);

                // Create and show the attack indicators.
                ShowZombiesInRangeOfAttack();
            }

            // If no action points then we provide the player feedback.
            else
            {
                attackFeedbackText.text = "No Action Points Left.";
                attackFeedbackPanel.SetActive(true);
            }
            
        }

        else
        {
            unitIsAttacking = false;

            // Detlete old attack indicators.
            DeleteZombieAttackIndicators();

            // Set the image to inactive.
            attackingImage.SetActive(false);
        }
    }

    public void ShowZombiesInRangeOfAttack()
    {
        // Check every tile in attack range of unit and if
        // there is a zombie on the tile we will add an attack indicator.
        attackRange = selectedUnit.GetComponent<UnitController>().attackRange;
        xPositionOfUnit = Mathf.RoundToInt(selectedUnit.transform.position.x);
        zPositionOfUnit = Mathf.RoundToInt(selectedUnit.transform.position.z);

        for (int row = -attackRange; row <= attackRange; row++)
        {
            for (int col = -attackRange; col <= attackRange; col++)
            {
                // Check to see if the absolute value of row + col < attack range.
                // This means the tile is in range.
                if ((Math.Abs(row) + Math.Abs(col)) < attackRange)
                {
                    
                    // Check to see if ground tile is in bounds.
                    if (LocateIndexOfGroundTile(xPositionOfUnit + row, zPositionOfUnit + col) >= 0 && LocateIndexOfGroundTile(xPositionOfUnit + row, zPositionOfUnit + col) < groundTiles.Count())
                    {
                        // Check to see if the ground tile exists.
                        if (groundTiles[LocateIndexOfGroundTile(xPositionOfUnit + row, zPositionOfUnit + col)] != null)
                        {
                            // Check to see if a zombie is on the tile.
                            for (int childIndex = 0; childIndex < groundTiles[LocateIndexOfGroundTile(xPositionOfUnit + row, zPositionOfUnit + col)].transform.childCount; childIndex++)
                            {
                                if (groundTiles[LocateIndexOfGroundTile(xPositionOfUnit + row, zPositionOfUnit + col)].transform.GetChild(childIndex).tag == "Zombie")
                                {
                                    // Place an attack indicator.
                                    var tempIndicator = Instantiate(attackIndicatorPrefab);

                                    tempIndicator.transform.position = groundTiles[LocateIndexOfGroundTile(xPositionOfUnit + row, zPositionOfUnit + col)].transform.position;
                                    tempIndicator.transform.Translate(Vector3.up);

                                    // Set the indicator as a child on the zombie.
                                    tempIndicator.transform.SetParent(groundTiles[LocateIndexOfGroundTile(xPositionOfUnit + row, zPositionOfUnit + col)].transform.GetChild(childIndex).transform);

                                    attackIndicators.Add(tempIndicator);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void DeleteZombieAttackIndicators()
    {
        for (int i = 0; i < attackIndicators.Count; i++)
        {
            Destroy(attackIndicators[i]);
        }

        attackIndicators.Clear();
    }

    public bool UnitAttack(GameObject zombie)
    {
        // Check to see if the zombie has an attack indicator.
        for (int childIndex = 0; childIndex < zombie.transform.childCount; childIndex++)
        {
            if (zombie.transform.GetChild(childIndex).transform.tag.ToString() == "AttackIndicator")
            {
                // Spend action points.
                SpendActionPoint(1, selectedUnit.transform.position);

                // Start the Attack.
                StartCoroutine(PlayUnitAttack(zombie));

                // Toggle Attack Mode.
                ToggleAttackMode();

                return true;
            }
        }

        ShowAttackFeedbackPanel("Zombie is out of range.");
        return false;
    }

    public void ShowAttackFeedbackPanel(string feedback)
    {
        attackFeedbackText.text = feedback;
        attackFeedbackPanel.SetActive(true);
    }

    public IEnumerator PlayUnitAttack(GameObject zombie)
    {
        // Face the unit towards the target.
        selectedUnit.transform.LookAt(zombie.transform);

        // Play the ranged attack animation if the unit is far away from the target.
        if (Vector3.Distance(selectedUnit.transform.position, zombie.transform.position) > 1.5)
        {
            selectedUnit.GetComponent<UnitController>().PlayRangedAttack();
        }

        // Play the close attack animation if the unit is close to the target.
        else
        {
            selectedUnit.GetComponent<UnitController>().PlayAttack();
        }
        

        yield return new WaitForSeconds(1);

        zombie.GetComponent<ZombieController>().ZombieTakeDamge(selectedUnit.GetComponent<UnitController>().attack);

        
    }

    #endregion

    #endregion


    #region FX Functions

    public IEnumerator PlayDirtSplatterFX(Vector3 loc, GameObject objectToDestroy=null)
    {
        // Feed in location for the dirt splatter.
        // The effect will play at the position and then hide itself.
        dirtSplatterFX.transform.position = loc;
        dirtSplatterFX.GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(dirtSplatterFX.GetComponent<ParticleSystem>().duration);

        // If an object needs to be destroyed after animation.
        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
        }
    }

    public IEnumerator PlaySpendActionPoint(Vector3 loc)
    {
        // Create a game object from prefab.
        var tempAPAnimation = Instantiate(APAnimationObject);

        // Get animation object to the correct location.
        tempAPAnimation.transform.position = loc;


        yield return new WaitForSeconds(.8f);

        // Destory the temp object.
        Destroy(tempAPAnimation);
    }

    #endregion

    #endregion
}
