using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using utils;
using System.IO;
using UnityEngine.Audio;

public class MainController : MonoBehaviour
{

    public int currentDay = 1;
    int currentHour = 15; // This can have values of 0 - 23.
    float skillPointsAvailable = 2;
    int food = 1;
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
    private string[] sexes = new string[] { "M", "F"};

    public GameObject movementOptionPrefab;
    public GameObject[] movementSelectedPrefab;
    List<GameObject> movementOptionTiles = new List<GameObject>(); // Temporaly holds these objects as they exist.
    public List<GameObject> movementSelectedTiles = new List<GameObject>(); // Temporaly holds these objects as they exist.
    private int indexOfFinalLocation = 0; // Used for unit movement during movement phase.
    bool movementSelectionCanContinue = true; // When a player ends the selection on an object that is not passable then the selection abilty must be disabled.
    public bool unitIsMoving = false; // When the unit is moving we do not want the player to input any controls.
    public bool unitIsAttacking = false; // When the unit is attacking, this should be set to true.
    public GameObject selectedUnit; // When a unit is selected they will be tracked by this object.
    #endregion

    float unitMovementSpeed = 5;
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

    #region Variables - Update

    public bool UpdatingAP = false; // This helps with the update phase of the game.
    public bool UpdatingHP = false; // This helps with the update phase of the game.
    public bool UpdatingFood = false; // This helps with the update phase of the game.
    int unitIndexForUpdate = 0;


    #endregion

    #region Variables - Camera - Light - Raycasting

    public Camera mainCam;
    public Light mainLightSourceSun;
    Ray ray;
    RaycastHit hit;
    string[] acceptableTags = new string[] {"Abandoned House", "Abandoned Factory", "Grave Yard", "Abandoned Vehicle", "Loot Box", "Tree", "Rock", "Trash"};
    string[] acceptableStructureTags = new string[] { "Farm Plot", "Living Quarters", "Medical Facility", "Wall", "Wall Angled", "Town Hall", "Trap" };
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
        ["Trash"] = "This item is of no use to you.",
        ["Trap"] = "This is a trap."
    };

    #endregion

    #region Panel - Individual Unit Panel

    public GameObject StaticIndividualUnitPanel;
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
    public Button scrapButton;
    public Button ExitInteractWithStructurePanel;

    #endregion

    #region Panel - BuildPanel

    #region Structure Prefabs

    public GameObject farmPlotPrefab;
    public GameObject livingQuartersPrefab;
    public GameObject medicalFacilityPrefab;
    public GameObject wallPrefab;
    public GameObject townHallPrefab;
    public GameObject trapPrefab;

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
    public GameObject wallOnWallSelector; // Used to move the individual wall within the selector.
    public GameObject townHallSelector;
    public GameObject trapSelector;

    // This variable was created to keep track of the gameObject the player is trying to call methods from.
    GameObject selectedStructureForUse;

    // To keep track of structures at update, we need a list to hold them.
    public List<GameObject> currentStructuresInGame = new List<GameObject>();

    public Button ToggleBuildPanel;
    public GameObject BuildPanel;
    public Button createFarmPlotButton;
    public Button createLivingQuartersButton;
    public Button createMedicalTentButton;
    public Button createFenceButton;
    public Button createTownHallButton;

    // Used for placing walls;
    public float wallOffsetX = 0;
    public float wallOffsetZ = 0;

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

    #region Panel - Main

    public GameObject MainPanel;
    public GameObject ObjectivesPanel;
    public GameObject UnitsPanel;
    public GameObject GamePanel;
    public GameObject SettingsPanel;

    public Text GameSaveFeedbackText;
    public Button OverwriteSaveButtonYes;
    public Button OverwriteSaveButtonNo;

    private string[] savePath = new string[4];
    private int SlotToOverwite = 0;

    // Unit panel variables
    public Image unitImage;
    public Sprite[] unitSprites;
    public Text unitNameUnitPanelText;
    public Text unitClassUnitPanelText;
    public Text actionPointsUnitPanelText;
    public Text actionPointLimitUnitPanelText;
    public Text hitPointsUnitPanelText;
    public Text hitPointLimitUnitPanelText;
    public Text attackUnitPanelText;
    public Text attackRangeUnitPanelText;
    public Text defenseUnitPanelText;
    public Text sightUnitPanelText;
    public Slider actionPointunitPanelSlider;
    public Slider hitPointunitPanelSlider;
    public Text currentUnitNumText;
    public Text unitlimitText;
    int currentUnitSelected = 0;



    #endregion

    #endregion

    #region Variables - Sound

    public AudioMixerGroup MusicMixer;
    public AudioMixerGroup SFXMixer;
    public AudioSource SFXSource;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider ZombieAnimationSpeedSlider;
    private int ZombieAnimationSpeed;
    public Toggle BeginningOfDayAnimationsToggle;
    private bool BeginningOfDayAnimations;

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
    public GameObject[] GraveYardPrefab;
    #endregion

    int selectedMapIndex = 1; // By default we select 0. 3 = Clear Map for testing.
    public int WorldSize = 0; // The world size will be set at the start of the game and depends on what map is selected.
    public GameObject[] groundTiles; // This is where all of the active ground tiles will be stored.
    public GameObject TerrainBase; // This will be used so that the entire set of ground tiles can be set to be children of an object in the hierarchy.

    public GameObject waterPlanePrefab;

    public string ActiveSavePath; // Global Variable.
    public string activeMapText;
    #endregion

    #region Variables - FXs

    public GameObject dirtSplatterFX;
    public GameObject APAnimationObject;
    public GameObject HPAnimationObject;
    public GameObject FeedAnimationObject;
    public GameObject StarveAnimationObject;
    private int rewardActionPoints = 0;
    private int rewardHitPoints = 0;

    #endregion

    #region Variables - Items

    //public GameObject[] MainBackpack = new GameObject[9];
    public Image[] MainBackpackImagesOnButtons;
    public Image[] unitItemImagesOnButtons;
    public Sprite[] ItemImages; // Mealkit, Medkit, boots, chovel
    public Sprite UIMask;
    private string[] itemNames = {"Mealkit", "Medkit", "Boots", "Shovel" };
    public String[] itemMealkit = { "Mealkit", "Increases range by 3."};
    public String[] itemKit = { "Medkit", "Hit Points are fully restored each turn."};
    public String[] itemBoots = { "Boots", "Action Points are fully restored each turn."};
    public String[] itemShovel = { "Shovel", "Unit can now destroy gave sites."};

    #endregion

    void Start()
    {

        // Set the save paths.
        // There are 3 save files that will hold maps and the paths to the files are stored in a string array.
        savePath[0] = Application.dataPath + "/Saves/Save00";
        savePath[1] = Application.dataPath + "/Saves/Save01";
        savePath[2] = Application.dataPath + "/Saves/Save02";
        savePath[3] = Application.dataPath + "/Saves/Save03";

        #region Terrain Generation
        GetActiveMapName();
        LoadGroundTitlesFromMap();
        PlaceWaterPlane();
        #endregion

        // Set sound settings.
        SFXSource = SoundManager.SoundManagerInstance.SFXSource;
        //GlobalControl.Instance.ActiveSavePath = path;

        //(System.IO.File.Exists("myfile.txt"))
        if (System.IO.File.Exists(GlobalControl.Instance.ActiveSavePath + "/SavedData.txt"))
        {
            LoadGameData();
            // Set the starting unit to the selected unit.
            selectedUnit = unitsInPlay[0];

            // Set text to match unit's attributes.
            unitNameText.text = selectedUnit.GetComponent<UnitController>().unitName;

            UpdateCamPositionOnUnitSelection(selectedUnit.transform.position);
        }

        else
        {
            SetGameSettingsOnStart(false);
            SpawnUnitsAtStartOfGame();
            CreateZombieAtRandomLocation(); // For now we will create 1 zombie at the start.
            CreateZombieAtRandomLocation(); // For now we will create 1 zombie at the start.
            SetSliderOnStart(false);
        }

        #region Text Object Updates

        UpdateBasicInformationText();

        #endregion

        // Start Player's Turn.
        PlayerTurn();

        // Testing Backpack.
        //for (int i  = 0; i < 4; i++)
        //{
        //    CreateItem();
        //}
        
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

            // When the players Space button is down and they are mapping a units path.
            else if (Input.GetKeyDown(KeyCode.Space) && !IndividualUnitPanel.activeInHierarchy)
            {
                // Update cam so it is on the selected unit. 
                UpdateCamPositionOnUnitSelection(selectedUnit.transform.position);

                // Set the available action points so the mapping function will work.
                actionPointAvailable = selectedUnit.GetComponent<UnitController>().actionPoints;

                OpenIndividualUnitPanel(selectedUnit);
                PlaceMovementOptionTiles(selectedUnit);

            }

            // Closes the Individual Unit Panel when the Space is released.
            else if (Input.GetKeyUp(KeyCode.Space) && IndividualUnitPanel.gameObject.activeSelf)
            {
                // Move the unit to the last green tile on the movement list.
                indexOfFinalLocation = -1;

                if (movementSelectedTiles.Count > 0)
                {
                    for (int i = 0; i < movementSelectedTiles.Count; i++)
                    {
                        if (movementSelectedTiles[i].transform.name == "MovementSelectedTile(Clone)")
                        {
                            indexOfFinalLocation = (i);
                        }
                    }

                    //Set the start postion if a green tile can be found on movement list, otherwise we will cancel the movement and simply try and interact with the first tile.
                    if (indexOfFinalLocation != -1)
                    {
                        NextPosition = new Vector3(movementSelectedTiles[0].transform.position.x, 0f, movementSelectedTiles[0].transform.position.z);
                        

                        unitIsMoving = true;

                        // Set the ground tile that the unit is leaving to passable.
                        groundTiles[LocateIndexOfGroundTile((int)selectedUnit.transform.position.x, (int)selectedUnit.transform.position.z)].GetComponent<GroundTileController>().terrainIsPassable = true;
                    }

                    else
                    {
                        selectedUnit.transform.LookAt(movementSelectedTiles[0].transform);
                        UnitInteractsWithNextGroundTileOnMove(selectedUnit, 0);
                        ClearMovementSelectionTiles();

                    }
                }

                // Reset the player's abilty to select movements. 
                movementSelectionCanContinue = true;
                CloseIndividualUnitPanel();

            }
        }

        else if (UpdatingAP)
        {
            // Update units at the end of the round.
            // Cycle through the list of units and update them.
            if (unitIndexForUpdate < unitsInPlay.Count)
            {
                // Action Points.
                if (unitsInPlay[unitIndexForUpdate].GetComponent<UnitController>().actionPoints < unitsInPlay[unitIndexForUpdate].GetComponent<UnitController>().actionPointsLimit)
                {
                    // Place the camera on the unit to be updated.
                    UpdateCamPositionOnUnitSelection(unitsInPlay[unitIndexForUpdate].transform.position);

                    UpdatingAP = false;
                    StartCoroutine("PlayEarnActionPoint", unitsInPlay[unitIndexForUpdate]);
                }

                else
                {
                    unitIndexForUpdate += 1;
                }
            }

            else
            {
                // The next update we need to do is hit points.
                UpdatingAP = false;
                UpdatingHP = true;

                // Reset index.
                unitIndexForUpdate = 0;

            }
        }

        else if (UpdatingHP)
        {
            // Update units at the end of the round.
            // Cycle through the list of units and update them.
            if (unitIndexForUpdate < unitsInPlay.Count)
            {
                // Hit Points.
                if (unitsInPlay[unitIndexForUpdate].GetComponent<UnitController>().hitPoints < unitsInPlay[unitIndexForUpdate].GetComponent<UnitController>().hitPointLimit)
                {
                    // Place the camera on the unit to be updated.
                    UpdateCamPositionOnUnitSelection(unitsInPlay[unitIndexForUpdate].transform.position);

                    UpdatingHP = false;
                    StartCoroutine("PlayEarnHitPoints", unitsInPlay[unitIndexForUpdate]);
                }

                else
                {
                    unitIndexForUpdate += 1;
                }
            }

            else
            {
                // Once through we need to reset the index and start the feed phase.
                unitIndexForUpdate = 0;
                UpdatingAP = false;
                UpdatingHP = false;
                UpdatingFood = true;
            }
        }

        else if (UpdatingFood)
        {
            // Update units at the end of the round.
            // Cycle through the list of units and update them.
            if (unitIndexForUpdate < unitsInPlay.Count)
            {
                // Hit Points.
                if (food > 0 || unitsInPlay[unitIndexForUpdate].GetComponent<UnitController>().DoesUnitHaveItem("Mealkit"))
                {
                    // Place the camera on the unit to be updated.
                    UpdateCamPositionOnUnitSelection(unitsInPlay[unitIndexForUpdate].transform.position);

                    UpdatingFood = false;
                    StartCoroutine("PlayFeedUnit", unitsInPlay[unitIndexForUpdate]);
                }

                else
                {
                    // Place the camera on the unit to be updated.
                    UpdateCamPositionOnUnitSelection(unitsInPlay[unitIndexForUpdate].transform.position);

                    UpdatingFood = false;
                    StartCoroutine("PlayStarveUnit", unitsInPlay[unitIndexForUpdate]);
                }
            }

            else
            {
                // Remove dead units.
                for (int indexOfUnit = 0; indexOfUnit < unitsInPlay.Count; indexOfUnit++)
                {
                    // Check to see if they are dead.
                    if (unitsInPlay[indexOfUnit].GetComponent<UnitController>().hitPoints <= 0)
                    {
                        var temp = unitsInPlay[indexOfUnit];

                        // Reset the ground tile to passable.
                        temp.GetComponentInParent<GroundTileController>().terrainIsPassable = true;

                        // Remove them from the list.
                        unitsInPlay.Remove(temp);

                        // Destroy the Unit.
                        Destroy(temp);
                    }
                }

                // End Update and Start Player Turn, we also need to set the player's turn bool to true to allow player input.
                playersTurn = true;
                UpdatingAP = false;
                UpdatingHP = false;
                UpdatingFood = false;
                PlayerTurn();
            }
        }
    }

    #region Functions


    #region SavingData Functions
    public void SaveGameData(int SaveSlot)
    {
        string[] Map = new string[WorldSize];

        Map = BuildMapFromWorld();

        if (SaveSlot == 0) // Quick Save will use what ever path was loaded.
        {
            // Check to see if there is a current saved file.
            if ((GlobalControl.Instance.ActiveSavePath.Substring(GlobalControl.Instance.ActiveSavePath.Length - 2, 2) == "00"))
            {
                GameSaveFeedbackText.text = "Save Unsuccessful, Select a Slot.";
            }

            else
            {
                WriteMapToTxtFile(GlobalControl.Instance.ActiveSavePath + "/Map.txt", Map);
                WriteFameDataToSaveFile(GlobalControl.Instance.ActiveSavePath + "/SavedData.txt", BuildSaveInformation());
                GameSaveFeedbackText.text = "Save Successful.";
            }
            
        }

        else
        {
            // Check to see if a file is already saved in the slot. If it is we will as for overwrite auth.
            //if (System.IO.File.Exists("myfile.txt"))
            if (System.IO.File.Exists(savePath[SaveSlot] + "/Map.txt") && !OverwriteSaveButtonYes.gameObject.activeInHierarchy)
            {
                // Set overwrite buttons to active.
                OverwriteSaveButtonYes.gameObject.SetActive(true);
                OverwriteSaveButtonNo.gameObject.SetActive(true);

                // Give feedback.
                GameSaveFeedbackText.text = "Overwrite Save?";

                // Set current save slot to help select the correct slot when the user presses YES.
                SlotToOverwite = SaveSlot;

            }

            else
            {
                WriteMapToTxtFile(savePath[SaveSlot] + "/Map.txt", Map);
                WriteFameDataToSaveFile(savePath[SaveSlot] + "/SavedData.txt", BuildSaveInformation());
                GameSaveFeedbackText.text = "Save Successful.";
            }
            
        }
    }

    public void ClickOverwriteSave(bool overwrite) 
    {
        if (overwrite)
        {
            SaveGameData(SlotToOverwite);

            // Set overwrite buttons to active.
            OverwriteSaveButtonYes.gameObject.SetActive(false);
            OverwriteSaveButtonNo.gameObject.SetActive(false);

            // Give feedback.
            GameSaveFeedbackText.text = "Save Successful.";

            // Set current save slot to help select the correct slot when the user presses YES.
            SlotToOverwite = 0;
        }

        else
        {
            // Set overwrite buttons to active.
            OverwriteSaveButtonYes.gameObject.SetActive(false);
            OverwriteSaveButtonNo.gameObject.SetActive(false);

            // Give feedback.
            GameSaveFeedbackText.text = "Save Canceled.";

            // Set current save slot to help select the correct slot when the user presses YES.
            SlotToOverwite = 0;
        }
    }

    public string[] BuildMapFromWorld()
    {
        string[] tempMap = new string[WorldSize];
        string tempChar = "";
        bool objectFound = false;

        // Iterate through the ground tiles.
        for (int row = 0; row < WorldSize; row++)
        {
            for (int col = 0; col < WorldSize; col++)
            {
                //Debug.Log(groundTiles[LocateIndexOfGroundTile(row, col)]);
                // If the ground tile is NUll, it should be saved as water.
                if (groundTiles[LocateIndexOfGroundTile(row, col)] == null)
                {
                    tempMap[row] += ".";
                }

                else
                {
                    for (int childInd = 0; childInd < groundTiles[LocateIndexOfGroundTile(row, col)].transform.childCount; childInd++)
                    {
                        if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).tag == "Tree")
                        {
                            tempChar = "^";
                            objectFound = true;
                        }

                        else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).tag == "Rock")
                        {
                            tempChar = "*";
                            objectFound = true;
                        }

                        else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).tag == "Loot Box")
                        {
                            tempChar = "&";
                            objectFound = true;
                        }

                        else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).tag == "Abandoned Vehicle")
                        {
                            tempChar = "3";
                            objectFound = true;
                        }

                        else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).tag == "Abandoned Factory")
                        {
                            tempChar = "2";
                            objectFound = true;
                        }

                        else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).tag == "Abandoned House")
                        {
                            tempChar = "1";
                            objectFound = true;
                        }

                        // Here are straight roads, either vertival or horizontal.
                        else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).transform.name == "tile-road-straight (1)")
                        {
                            if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).transform.rotation.y == 0 || 
                                groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).transform.rotation.y == 180)
                            {
                                tempChar = "-";
                            }

                            else
                            {
                                tempChar = "|";
                            }
                            
                            objectFound = true;
                        }

                        // Here are the 4 flavors of T intersections.
                        else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).transform.name == "tile-road-intersection-t")
                        {
                            if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.eulerAngles.y == 0)
                            {
                                tempChar = "┤";
                            }

                            else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.eulerAngles.y == 90)
                            {
                                tempChar = "┬";
                            }

                            else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.eulerAngles.y == 180)
                            {
                                tempChar = "├";
                            }

                            else
                            {
                                tempChar = "┴";
                            }

                            objectFound = true;
                        }

                        // Here are the 4 flavors of curved roads.
                        else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.GetChild(childInd).transform.name == "tile-road-curve")
                        {
                            if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.eulerAngles.y == 0)
                            {
                                tempChar = "└";
                            }

                            else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.eulerAngles.y == 90)
                            {
                                tempChar = "┌";
                            }

                            else if (groundTiles[LocateIndexOfGroundTile(row, col)].transform.eulerAngles.y == 180)
                            {
                                tempChar = "┐";
                            }

                            else
                            {
                                tempChar = "┘";
                            }

                            objectFound = true;
                        }


                        else if(groundTiles[LocateIndexOfGroundTile(row, col)].transform.name == "RoadBlockTile01(Clone)")
                        {
                            tempChar = "x";
                            objectFound = true;
                        }
                        

                        else
                        {
                            if (!objectFound)
                            {
                                tempChar = "0";
                                objectFound = true;
                            }
                            
                        }
                            
                    }

                    // Add the correct char to the row.
                    tempMap[row] += tempChar;
                    objectFound = false;

                }
            }


            /*
             string[] acceptableTags = new string[] {"Abandoned House", "Abandoned Factory", "Abandoned Vehicle", "Loot Box", "Tree", "Rock", "Trash"};
    string[] acceptableStructureTags = new string[] { "Farm Plot", "Living Quarters", "Medical Facility", "Wall", "Wall Angled", "Town Hall", "Trap" };
    string[] acceptableGroundTilesTags = new string[] { "GroundTile", "Holding Factory"};
    string[] acceptableUnitTags = new string[] { "Unit" };
            */
        }

        return tempMap;
    }

    public List<string> BuildSaveInformation()
    {
        List<string> tempSaveData = new List<string>();


        // Save Basic Game Information.
        tempSaveData.Add("Day: [" + currentDay.ToString() + "]");
        tempSaveData.Add("Hour: [" + currentHour.ToString() + "]");
        tempSaveData.Add("Food: [" + food.ToString() + "]");
        tempSaveData.Add("Wood: [" + wood.ToString() + "]");
        tempSaveData.Add("Stone: [" + stone.ToString() + "]");
        tempSaveData.Add("PopulationCap: [" + populationCap.ToString() + "]");
        tempSaveData.Add("MusicVol: [" + musicSlider.value.ToString() + "]");
        tempSaveData.Add("SFXVol: [" + sfxSlider.value.ToString() + "]");

        string backpackContents = "MainBackpack: [";
        for (int i = 0; i < MainBackpackImagesOnButtons.Length - 1; i++)
        {
            backpackContents += MainBackpackImagesOnButtons[i].sprite.name + ", ";
        }
        backpackContents += MainBackpackImagesOnButtons[MainBackpackImagesOnButtons.Length-1].sprite.name;
        tempSaveData.Add(backpackContents + "]");

        for (int unitCount = 0; unitCount< unitsInPlay.Count; unitCount++)
        {
            var unitName = unitsInPlay[unitCount].GetComponent<UnitController>().unitName;
            var unitGender = unitsInPlay[unitCount].GetComponent<UnitController>().sex;
            var unitClass = unitsInPlay[unitCount].GetComponent<UnitController>().unitClass;
            var unitPosX = unitsInPlay[unitCount].transform.position.x;
            var unitPosZ = unitsInPlay[unitCount].transform.position.z;
            var unitAP = unitsInPlay[unitCount].GetComponent<UnitController>().actionPoints;
            var unitAPL = unitsInPlay[unitCount].GetComponent<UnitController>().actionPointsLimit;
            var unitHP = unitsInPlay[unitCount].GetComponent<UnitController>().hitPoints;
            var unitHPL = unitsInPlay[unitCount].GetComponent<UnitController>().hitPointLimit;
            var unitAttack = unitsInPlay[unitCount].GetComponent<UnitController>().attack;
            var unitAttackR = unitsInPlay[unitCount].GetComponent<UnitController>().attackRange;
            var unitDefense = unitsInPlay[unitCount].GetComponent<UnitController>().defense;
            var unitSight = unitsInPlay[unitCount].GetComponent<UnitController>().sight;
            var unitRP = unitsInPlay[unitCount].GetComponent<UnitController>().repairPoints;
            var unitCritHit = unitsInPlay[unitCount].GetComponent<UnitController>().criticalHitPercentage;
            var unitCropsAtHarvestMlt = unitsInPlay[unitCount].GetComponent<UnitController>().cropsAtHarvestMultiplier;
            var unitTurnsUntilCropsMature = unitsInPlay[unitCount].GetComponent<UnitController>().turnsUntilCropsMature;
            var unitBackpackItem01 = unitsInPlay[unitCount].GetComponent<UnitController>().unitBackpack[0];
            var unitBackpackItem02 = unitsInPlay[unitCount].GetComponent<UnitController>().unitBackpack[1];

            // Saves Data on Units Name, Sex, Class, PositionX, PositionZ, Action Points, Action Point Limit, Hit Points, Hit Point Limit, Attack,
            // Attack Range, Defense, Sight, Repair Points, Critical Hit Percentage, Crops At Harvest Multiplier, Turns Until Crops Mature
            tempSaveData.Add("Unit: ["  + unitName + ", " + unitGender + ", " + unitClass.ToString() + ", " + unitPosX + ", " + +unitPosZ + ", " + unitAP + ", " + unitAPL 
                + ", " + unitHP + ", " + unitHPL + ", " + unitAttack + ", " + unitAttackR + ", " + unitDefense + ", " + unitSight + ", " 
                + unitRP + ", " + unitCritHit + ", " + unitCropsAtHarvestMlt + ", " + unitTurnsUntilCropsMature + ", " + unitBackpackItem01 + "," + unitBackpackItem02 +"]");
        }

        for (int zombieCount = 0; zombieCount < zombiesInPlay.Count; zombieCount++)
        {
            var zombiePosX = Convert.ToInt32(zombiesInPlay[zombieCount].transform.position.x);
            var zombiePosZ = Convert.ToInt32(zombiesInPlay[zombieCount].transform.position.z);
            var zombieHP = zombiesInPlay[zombieCount].GetComponent<ZombieController>().hitPoints;
            tempSaveData.Add("Zombie: [" + zombiePosX + ", " + zombiePosZ + ", " + zombieHP + "]");
        }

        for (int structureCount = 0; structureCount < currentStructuresInGame.Count; structureCount++)
        {
            var structureType = currentStructuresInGame[structureCount].GetComponent<StructureContoller>().structureType;
            var structurePosX = Convert.ToInt32(currentStructuresInGame[structureCount].transform.position.x);
            var structurePosZ = Convert.ToInt32(currentStructuresInGame[structureCount].transform.position.z);
            var structureHP = currentStructuresInGame[structureCount].GetComponent<StructureContoller>().hitPoints;
            var structureHPL = currentStructuresInGame[structureCount].GetComponent<StructureContoller>().hitPointLimit;
            var structureLevel = currentStructuresInGame[structureCount].GetComponent<StructureContoller>().currentStructureLevel;
            var structureRotation = currentStructuresInGame[structureCount].transform.eulerAngles.y;

            if (currentStructuresInGame[structureCount].GetComponent<StructureContoller>().structureType == "Wall")
            {
                structureRotation = currentStructuresInGame[structureCount].GetComponent<StructureContoller>().wallRot.eulerAngles.y;
            }

            tempSaveData.Add("Structure: [" + structureType + ", " + structurePosX + ", " + structurePosZ + ", " + structureHP + ", " + structureHPL + ", " + structureLevel + ", " + structureRotation + "]");

        }


        return tempSaveData;
    }

    private void WriteMapToTxtFile(string path, string[] inMap)
    {
        File.WriteAllText(path, "");

        // Add the map.
        for (int numOfLines = 0; numOfLines < WorldSize; numOfLines++)
        {
            File.AppendAllText(path, inMap[numOfLines] + "\n");
        }
        
    }

    private void WriteFameDataToSaveFile(string path, List<string> gameData)
    {
        File.WriteAllText(path, "");

        // Add the map.
        for (int numOfLines = 0; numOfLines < gameData.Count; numOfLines++)
        {
            File.AppendAllText(path, gameData[numOfLines] + "\n");
        }

    }

    #endregion


    #region LoadingData Functions

    private void LoadGameData()
    {
        // This should only be used if the player has selected to load a previously saved game.
        // To check this we need to check if the ActiveSavePath on the Global Controler had a file named SavedData.txt
        LoadBasicInformation(GlobalControl.Instance.ActiveSavePath + "/SavedData.txt");
    }

    private void LoadBasicInformation(string path)
    {
        var saveDataText = File.ReadAllLines(path);

        for(int line = 0; line < saveDataText.Length; line++)
        {
            // Pull the next line's data and get title.
            var currentLine = saveDataText[line];
            var indexOfFirstWhiteSpace = currentLine.IndexOf(" ");
            var title = currentLine.Substring(0, indexOfFirstWhiteSpace);

            // Now that we have the next line we can decide what information it has and what to do with it.
            // Basic data.
            if (title == "Day:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");
                currentDay = Convert.ToInt32((currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1)));
            }

            else if (title == "Hour:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");
                currentHour = Convert.ToInt32((currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1)));
            }

            else if (title == "Food:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");
                food = Convert.ToInt32((currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1)));
            }

            else if (title == "Wood:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");
                wood = Convert.ToInt32((currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1)));
            }

            else if (title == "Stone:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");
                stone = Convert.ToInt32((currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1)));
            }

            else if (title == "PopulationCap:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");
                populationCap = Convert.ToInt32((currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1)));
            }

            else if(title == "MusicVol:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");
                musicSlider.value = float.Parse((currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1)));
            }

            else if (title == "SFXVol:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");
                sfxSlider.value = float.Parse((currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1)));
            }

            else if (title == "MainBackpack:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");
                var backpackInfo = currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1);
                string[] backpackInfoArray = backpackInfo.Split(',');
               
                for (int i = 0; i < MainBackpackImagesOnButtons.Length; i++)
                {
                    if (backpackInfoArray[i].Replace(" ", "") == "blue23") MainBackpackImagesOnButtons[i].sprite = ItemImages[0];
                    else if (backpackInfoArray[i].Replace(" ", "") == "blue7") MainBackpackImagesOnButtons[i].sprite = ItemImages[1];
                    else if (backpackInfoArray[i].Replace(" ", "") == "graytwo8") MainBackpackImagesOnButtons[i].sprite = ItemImages[2];
                    else if (backpackInfoArray[i].Replace(" ", "") == "graytwo30") MainBackpackImagesOnButtons[i].sprite = ItemImages[3];
                    else MainBackpackImagesOnButtons[i].sprite = UIMask;
                }
            }

            else if (title == "Unit:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");

                var unitInfo = currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1);

                string[] unitInfoArray = unitInfo.Split(',');

                var unitName = unitInfoArray[0].ToString();
                var unitGender = unitInfoArray[1].ToString().Replace(" ", "");
                var unitClass = unitInfoArray[2].ToString().Replace(" ", "");
                var unitPosX = Convert.ToInt32(unitInfoArray[3]);
                var unitPosZ = Convert.ToInt32(unitInfoArray[4]);
                var unitAP = Convert.ToInt32(unitInfoArray[5]);
                var unitAPL = Convert.ToInt32(unitInfoArray[6]);
                var unitHP = Convert.ToInt32(unitInfoArray[7]);
                var unitHPL = Convert.ToInt32(unitInfoArray[8]);
                var unitAttack = Convert.ToInt32(unitInfoArray[9]);
                var unitAttackR = Convert.ToInt32(unitInfoArray[10]);
                var unitDefense = Convert.ToInt32(unitInfoArray[11]);
                var unitSight = Convert.ToInt32(unitInfoArray[12]);
                var unitRP = Convert.ToInt32(unitInfoArray[13]);
                var unitCritHit = float.Parse(unitInfoArray[14]);
                var unitCropsAtHarvestMlt = Convert.ToInt32(unitInfoArray[15]);
                var unitTurnsUntilCropsMature = Convert.ToInt32(unitInfoArray[16]);
                var item01 = unitInfoArray[17].Replace(" ", "");
                var item02 = unitInfoArray[18].Replace(" ", "");
                print(item01);
                print(item02);

                //Debug.Log("Unit Action Points: " + unitAP + " Type: " + unitAP.GetType());
                // Create this unit.
                createUnitAtLoad(unitName, unitGender, unitClass, unitPosX, unitPosZ, unitAP, unitAPL, unitHP, unitHPL, unitAttack, unitAttackR, unitDefense, unitSight, unitRP, unitCritHit, unitCropsAtHarvestMlt, unitTurnsUntilCropsMature, item01, item02);



            }

            else if (title == "Zombie:")
            {
                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");

                var zombieInfo = currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1);

                string[] zombieInfoArray = zombieInfo.Split(',');

                var zombiePosX = Convert.ToInt32(zombieInfoArray[0]);
                var zombiePosZ = Convert.ToInt32(zombieInfoArray[1]);
                var zombieHP = Convert.ToInt32(zombieInfoArray[2]);

                // Create the instance of this zombie.
                createZombieAtLoad(zombiePosX, zombiePosZ, zombieHP);
            }

            else if (title == "Structure:")
            {
                

                var indexOfLeftB = currentLine.IndexOf("[");
                var indexOfRightB = currentLine.IndexOf("]");

                var structureInfo = currentLine.Substring(indexOfLeftB + 1, (indexOfRightB - indexOfLeftB) - 1);

                string[] structureInfoArray = structureInfo.Split(',');

                var structureType = (structureInfoArray[0]).ToString();
                var structurePosX = Convert.ToInt32(structureInfoArray[1]);
                var structurePosZ = Convert.ToInt32(structureInfoArray[2]);
                var structureHP = Convert.ToInt32(structureInfoArray[3]);
                var structureHPL = Convert.ToInt32(structureInfoArray[4]);
                var structureLevel = Convert.ToInt32(structureInfoArray[5]);
                var structureRotation = float.Parse(structureInfoArray[6]);

                // Create the instance of this structure.
                CreateStructuresAtLoad(structureType, structurePosX, structurePosZ, structureHP, structureHPL, structureLevel, structureRotation);
            }

            else
            {
                Debug.Log("Data is not loading correctly");
                Debug.Log(title);
            }
        }

    }

    #endregion


    #region Camera Raycasting and Movement Fuctions

    public void UpdateCamPosition()
    {
        // This function handles key board inputs.

        // When the player is not mapping movement.
        if (Input.GetKey(KeyCode.Space) == false)
        {
            if (Input.GetKey(KeyCode.W))
            {
                mainCam.transform.Translate(new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z) * camTranslateSpeed * Time.deltaTime, Space.World);
            }

            if (Input.GetKey(KeyCode.S))
            {
                mainCam.transform.Translate(-new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z) * camTranslateSpeed * Time.deltaTime, Space.World);
            }

            if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) && !structureIsBeingBuilt)
            {
                mainCam.transform.Translate(-new Vector3(mainCam.transform.right.x, 0f, mainCam.transform.right.z) * camTranslateSpeed * Time.deltaTime, Space.World);
            }

            if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) && !structureIsBeingBuilt)
            {
                mainCam.transform.Translate(new Vector3(mainCam.transform.right.x, 0f, mainCam.transform.right.z) * camTranslateSpeed * Time.deltaTime, Space.World);
            }

            if (Input.GetKey(KeyCode.UpArrow) && !structureIsBeingBuilt && mainCam.transform.position.y < camTopBound)
            {
                mainCam.transform.Translate(Vector3.up * camZoomSpeed * Time.deltaTime, Space.World);
            }

            if (Input.GetKey(KeyCode.DownArrow) && !structureIsBeingBuilt && mainCam.transform.position.y > camBottomBounds)
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
                UpdateCamPositionOnUnitSelection(selectedUnit.transform.position);

                // Set text to match unit's attributes.
                unitNameText.text = selectedUnit.GetComponent<UnitController>().unitName;


            }

            if (Input.GetKeyDown(KeyCode.R) && structureIsBeingBuilt)
            {
                if (currentStructureSelectedToBuild.name == "Farm Plot") farmPlotSelector.transform.Rotate(new Vector3(0f, 90f, 0f));
                else if (currentStructureSelectedToBuild.name == "Living Quarters") livingQuartersSelector.transform.Rotate(new Vector3(0f, 90f, 0f));
                else if (currentStructureSelectedToBuild.name == "Medical Facility") medicalFacilitySelector.transform.Rotate(new Vector3(0f, 90f, 0f));
                else if (currentStructureSelectedToBuild.name == "Wall") wallOnWallSelector.transform.Rotate(new Vector3(0f, 45f, 0f));
                else if (currentStructureSelectedToBuild.name == "Town Hall") townHallSelector.transform.Rotate(new Vector3(0f, 90f, 0f));
                else if (currentStructureSelectedToBuild.name == "Trap") townHallSelector.transform.Rotate(new Vector3(0f, 90f, 0f));
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) && structureIsBeingBuilt && currentStructureSelectedToBuild.name == "Wall")
            {
                if (wallOffsetZ < .4f)
                {
                    wallOffsetZ += .1f;
                    wallOnWallSelector.transform.position = new Vector3((wallSelector.transform.position.x + wallOffsetX), 0f, (wallSelector.transform.position.z + wallOffsetZ));
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) && structureIsBeingBuilt && currentStructureSelectedToBuild.name == "Wall")
            {
                if (wallOffsetZ > -.4f)
                {
                    wallOffsetZ -= .1f;
                    wallOnWallSelector.transform.position = new Vector3(wallSelector.transform.position.x + wallOffsetX, 0f, wallSelector.transform.position.z + wallOffsetZ);
                }
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) && structureIsBeingBuilt && currentStructureSelectedToBuild.name == "Wall")
            {
                if (wallOffsetX < .4f)
                {
                    wallOffsetX += .1f;
                    wallOnWallSelector.transform.position = new Vector3((wallSelector.transform.position.x + wallOffsetX), 0f, (wallSelector.transform.position.z + wallOffsetZ));
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) && structureIsBeingBuilt && currentStructureSelectedToBuild.name == "Wall")
            {
                if (wallOffsetX > -.4f)
                {
                    wallOffsetX -= .1f;
                    wallOnWallSelector.transform.position = new Vector3(wallSelector.transform.position.x + wallOffsetX, 0f, wallSelector.transform.position.z + wallOffsetZ);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (MainPanel.activeInHierarchy) CloseMainMenu();
                else OpenMainMenu("Objectives");
            }
        }

        // When the player is mapping movement.
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MapUnitMovementPath("U");
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MapUnitMovementPath("D");
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MapUnitMovementPath("L");
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MapUnitMovementPath("R");
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                // Clears the movement path.
                ClearMovementSelectionTiles();
            }
        }


        

    }

    public void UpdateCamPositionOnUnitSelection(Vector3 position)
    {
        // This function position the camera on the selected unit.
        // It is used when the player selects a unit with the TAB key, the camera will move and rotate.
        
        mainCam.transform.position = new Vector3(position.x, 5f, position.z - 4);
        mainCam.transform.rotation = Quaternion.Euler(45f, 0f, 0);

        // Move the selector tile to the position.
        // We need to move the selector object to the tile's position.
        selector.transform.position = position;
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

        // Turn on the static unit panel.
        StaticIndividualUnitPanel.SetActive(true);

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

        UpdateCamPositionOnUnitSelection(selectedUnit.transform.position);

    }

    public void createUnitAtLoad(string unitName, string unitGender, string unitClass, int unitPosX, int unitPosZ, int unitAP, int unitAPL, 
        int unitHP, int unitHPL, int unitAttack, int unitAttackR, int unitDefense, int unitSight, int unitRP, float unitCritHit, int unitCropsAtHarv, int unitTurnsUntilMature, string item01, string item02)
    {

        // Create game object.
        var temp = new GameObject();

        var unitPosVec = new Vector3(unitPosX, 0f, unitPosZ);


        // Create unit and set Gender, Class
        if (unitClass == "Basic")
        {
            if (unitGender == "M")
            {
                temp = Instantiate(basicUnitPrefabs[0], unitPosVec, Quaternion.identity);
                temp.GetComponent<UnitController>().sex = "M";
                temp.GetComponent<UnitController>().unitClass = UnitController.Class.Basic;
            }

            else
            {
                temp = Instantiate(basicUnitPrefabs[3], unitPosVec, Quaternion.identity);
                temp.GetComponent<UnitController>().sex = "F";
                temp.GetComponent<UnitController>().unitClass = UnitController.Class.Basic;
            }
        }

        else if (unitClass == "Farmer")
        {
            if (unitGender == "M")
            {
                temp = Instantiate(basicUnitPrefabs[1], unitPosVec, Quaternion.identity);
                temp.GetComponent<UnitController>().sex = "M";
                temp.GetComponent<UnitController>().unitClass = UnitController.Class.Farmer;
            }

            else
            {
                temp = Instantiate(basicUnitPrefabs[4], unitPosVec, Quaternion.identity);
                temp.GetComponent<UnitController>().sex = "F";
                temp.GetComponent<UnitController>().unitClass = UnitController.Class.Farmer;
            }
        }

        else
        {
            if (unitGender == "M")
            {
                temp = Instantiate(basicUnitPrefabs[2], unitPosVec, Quaternion.identity);
                temp.GetComponent<UnitController>().sex = "M";
                temp.GetComponent<UnitController>().unitClass = UnitController.Class.Soldier;
            }

            else
            {
                temp = Instantiate(basicUnitPrefabs[5], unitPosVec, Quaternion.identity);
                temp.GetComponent<UnitController>().sex = "F";
                temp.GetComponent<UnitController>().unitClass = UnitController.Class.Soldier;
            }

        }

        // Set the rest of the atts.
        temp.GetComponent<UnitController>().unitName = unitName;
        temp.GetComponent<UnitController>().actionPoints = unitAP;
        temp.GetComponent<UnitController>().actionPointsLimit = unitAPL;
        temp.GetComponent<UnitController>().hitPoints = unitHP;
        temp.GetComponent<UnitController>().hitPointLimit = unitHPL;
        temp.GetComponent<UnitController>().attack = unitAttack;
        temp.GetComponent<UnitController>().attackRange = unitAttackR;
        temp.GetComponent<UnitController>().defense = unitDefense;
        temp.GetComponent<UnitController>().sight = unitSight;
        temp.GetComponent<UnitController>().repairPoints = unitRP;
        temp.GetComponent<UnitController>().criticalHitPercentage = unitCritHit;
        temp.GetComponent<UnitController>().cropsAtHarvestMultiplier = unitCropsAtHarv;
        temp.GetComponent<UnitController>().turnsUntilCropsMature = unitTurnsUntilMature;
        if (item01 != "") temp.GetComponent<UnitController>().AddItem(item01);
        if (item02 != "") temp.GetComponent<UnitController>().AddItem(item02);


        // Make the unit a child of the ground tile.
        temp.transform.SetParent(groundTiles[LocateIndexOfGroundTile(unitPosX, unitPosZ)].transform);

        // Set the ground tiles attribute so that the terrain is no longer passable.
        groundTiles[LocateIndexOfGroundTile(unitPosX, unitPosZ)].GetComponent<GroundTileController>().terrainIsPassable = false;

        // Add the unit to the list of units in play.
        unitsInPlay.Add(temp);

        // Update text.
        populationText.text = unitsInPlay.Count.ToString();

        
    }

    public void createZombieAtLoad(int zombiePosX, int zombiePosZ, int zombieHP)
    {
        // Create game object and set atts.
        var zombiePosVec = new Vector3(zombiePosX, 0f, zombiePosZ);
        var temp = Instantiate(zombiePrefab, zombiePosVec, Quaternion.identity);
        temp.GetComponent<ZombieController>().hitPoints = zombieHP;
  
        // Make the unit a child of the ground tile.
        temp.transform.SetParent(groundTiles[LocateIndexOfGroundTile(zombiePosX, zombiePosZ)].transform);

        // Set the ground tiles attribute so that the terrain is no longer passable.
        groundTiles[LocateIndexOfGroundTile(zombiePosX, zombiePosZ)].GetComponent<GroundTileController>().terrainIsPassable = false;

        // Add the unit to the list of units in play.
        zombiesInPlay.Add(temp);
    }

    public void CreateStructuresAtLoad(string structureName, int structurePosX, int structurePosZ, int structureHP, int structureHPL, int structureLevel, float structureRotation)
    {
        var tempStructure = gameObject;
        var PosVect = new Vector3(structurePosX, 0f, structurePosZ);

        if (structureName == "Farm Plot")
        {
            // Create structure.
            tempStructure = Instantiate(farmPlotPrefab, PosVect, Quaternion.Euler(0, structureRotation, 0));
        }

        else if (structureName == "Living Quarters")
        {
            // Create structure.
            tempStructure = Instantiate(livingQuartersPrefab, PosVect, Quaternion.Euler(0, structureRotation, 0));
        }

        else if (structureName == "Medical Facility")
        {
            // Create structure.
            tempStructure = Instantiate(medicalFacilityPrefab, PosVect, Quaternion.Euler(0, structureRotation, 0));
        }

        else if (structureName == "Wall")
        {

            // Create structure.
            tempStructure = Instantiate(wallPrefab, PosVect, Quaternion.identity);

            tempStructure.GetComponent<StructureContoller>().wallRot = Quaternion.Euler(0f, structureRotation, 0f);
        }

        else if (structureName == "Town Hall")
        {
            // Create structure.
            tempStructure = Instantiate(townHallPrefab, PosVect, Quaternion.Euler(0, structureRotation, 0));
        }

        else if (structureName == "Trap")
        {
            // Create structure.
            tempStructure = Instantiate(trapPrefab, PosVect, Quaternion.identity);
        }

        else
        {
            Debug.Log("Error Found While Loading St4ructure: Invalid Type");
        }

        // Add the structure to the current structure list.
        currentStructuresInGame.Add(tempStructure);
        
        // Make the structure a child of the ground tile.
        tempStructure.transform.SetParent(groundTiles[LocateIndexOfGroundTile(structurePosX, structurePosZ)].transform);

        // Set the title to not passable.
        groundTiles[LocateIndexOfGroundTile(structurePosX, structurePosZ)].GetComponent<GroundTileController>().terrainIsPassable = false;

        // Upgrade the structure if needed.
        for (int upgradeCount = 0; upgradeCount < structureLevel; upgradeCount++)
        {
            tempStructure.GetComponent<StructureContoller>().levelUpgradeFromSave = structureLevel;
        }

    }

    public void createUnitAtLocation(Vector3 spawnLoc)
    {
        // Pick a unit type: Basic, Farmer, Soldier.
        var randomType = UnityEngine.Random.Range(0, 3);

        // Pick the gender of the unit.
        var gender = sexes[UnityEngine.Random.Range(0, sexes.Length)];

        // Create game object.
        var temp = new GameObject();

        if (gender == "F") temp = Instantiate(basicUnitPrefabs[randomType + 3], spawnLoc, Quaternion.identity);
        else temp = Instantiate(basicUnitPrefabs[randomType], spawnLoc, Quaternion.identity);

        // Set the random attributes for the unit.
        SetUnitAttributesAtCreation(temp, gender);

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

    public void SetUnitAttributesAtCreation(GameObject unit, string gender)
    {
        // Pick a random name for the unit.
        unit.GetComponent<UnitController>().unitName = Utils.GetName(gender);

        unit.GetComponent<UnitController>().sex = unit.GetComponent<UnitController>().sex = gender;

        // This can be expanded once differnt unit types are created.
    }

    public void updateCameraForPlayerTurn()
    {
        mainCam.transform.position = new Vector3(selectedUnit.transform.position.x, 5f, selectedUnit.transform.position.z - 4);
        mainCam.transform.rotation = Quaternion.Euler(45f, 0f, 0);
    }

    public void MoveUnit(GameObject unit)
    {
        
        // Unit has reached the next position but still needs to continue.
        if (selectedUnit.transform.position == NextPosition && (nextPositionIndex < indexOfFinalLocation))
        {
            // Increment Index.
            nextPositionIndex++;
            PlayUnitRun(unit); //Play the run animation.

            // Reset the next position.
            NextPosition = new Vector3(movementSelectedTiles[nextPositionIndex].transform.position.x, 0f, movementSelectedTiles[nextPositionIndex].transform.position.z);

            // Spend AP.
            SpendActionPoint(1, selectedUnit.transform.position);
        }

        if(selectedUnit.transform.position == NextPosition && (nextPositionIndex == indexOfFinalLocation))
        {
            // Spend AP.
            SpendActionPoint(1, selectedUnit.transform.position);

            // Place unit on the tile.
            selectedUnit.transform.SetParent(groundTiles[LocateIndexOfGroundTile((int)selectedUnit.transform.position.x, (int)selectedUnit.transform.position.z)].transform);

            // Set the ground title to not passable.
            groundTiles[LocateIndexOfGroundTile((int)selectedUnit.transform.position.x, (int)selectedUnit.transform.position.z)].GetComponent<GroundTileController>().terrainIsPassable = false;

            // Check to see if the unit should interact with an object on the next tile.
            if (nextPositionIndex < movementSelectedTiles.Count - 1)
            {
                selectedUnit.transform.LookAt(movementSelectedTiles[nextPositionIndex + 1].transform);
                UnitInteractsWithNextGroundTileOnMove(selectedUnit, nextPositionIndex + 1);
            }

            // Reset the index.
            nextPositionIndex = 0;

            // CLose out this move.
            unitIsMoving = false;
            ClearMovementSelectionTiles();

            
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
        //UpdateTimeOfDay(); TO DO
        //UpdateLightSource(); To DO

        // Remove dead units.
        for (int indexOfUnit = 0; indexOfUnit < unitsInPlay.Count; indexOfUnit++)
        {
            // Check to see if they are dead.
            if (unitsInPlay[indexOfUnit].GetComponent<UnitController>().hitPoints <= 0)
            {
                var temp = unitsInPlay[indexOfUnit];

                // Reset the ground tile to passable.
                temp.GetComponentInParent<GroundTileController>().terrainIsPassable = true;

                // Remove them from the list.
                unitsInPlay.Remove(temp);

                // Destroy the Unit.
                Destroy(temp);
            }
        }

        // Update Text Objects.
        UpdateAllText();

        // Remove dead zombies.
        for (int indexOfZombie = 0; indexOfZombie < zombiesInPlay.Count; indexOfZombie++)
        {
            // Check to see if they are dead.
            if (zombiesInPlay[indexOfZombie].GetComponent<ZombieController>().hitPoints <= 0)
            {
                var temp = zombiesInPlay[indexOfZombie];

                // Reset the ground tile to passable.
                temp.GetComponentInParent<GroundTileController>().terrainIsPassable = true;

                // Remove them from the list.
                zombiesInPlay.Remove(temp);

                // Destroy the zombie.
                Destroy(temp);
            }
        }

        

        // Check to see if all the units have been killed.
        if (unitsInPlay.Count == 0) print("All units have been killed. Game Over.");

        // Update structures.
        UpdateStructuresAtEndOfRound();

        // Set the unit index to zero.
        unitIndexForUpdate = 0;

        // For now we will create 1 zombie at the update unit the cap is reached.
        if (zombiesInPlay.Count < zombieCap)
        {
            CreateZombieAtRandomLocation(); 
        }

        // Update Current Zombies in Play.
        UpdateZombiesAtEndOfRound();

        // Set the bool to true so the update starts.
        UpdatingAP = true;

        // Update skills points.
        UpdateSkillPointsAtEndOfRound();
        
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
        //if (currentHour >= 4 && currentHour < 12) mainLightSourceSun.intensity += .1f;

        // Then decrease in intensity from 4 pm until 12 am.
        //else if (currentHour >= 16 && currentHour <= 23) mainLightSourceSun.intensity -= .1f;
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

    public int ActionPointsRewardFromLivingQuarters(GameObject unit)
    {
        for (int structureIndex = 0; structureIndex < currentStructuresInGame.Count; structureIndex++)
        {
            // Check to see if the structure is a Living Quaters.
            if (currentStructuresInGame[structureIndex].GetComponent<StructureContoller>().structureType == "Living Quarters")
            {
                // Check the distance versus the actionPointHealRange attribute.
                if (Vector3.Distance(unit.transform.position, currentStructuresInGame[structureIndex].transform.position) < currentStructuresInGame[structureIndex].GetComponent<StructureContoller>().actionPointHealRange)
                {
                    return currentStructuresInGame[structureIndex].GetComponent<StructureContoller>().amountOfActionPointsHealed;
                }
            }

            
        }

        return 0;

    }

    public int HitPointsRewardFromMedical(GameObject unit)
    {
        for (int structureIndex = 0; structureIndex < currentStructuresInGame.Count; structureIndex++)
        {
            // Check to see if the structure is a Medical Facility.
            if (currentStructuresInGame[structureIndex].GetComponent<StructureContoller>().structureType == "Medical Facility")
            {
                // Check the distance versus the actionPointHealRange attribute.
                if (Vector3.Distance(unit.transform.position, currentStructuresInGame[structureIndex].transform.position) < currentStructuresInGame[structureIndex].GetComponent<StructureContoller>().hitPointHealRange)
                {
                    return currentStructuresInGame[structureIndex].GetComponent<StructureContoller>().amountOfHitPointsHealed;
                }
            }


        }

        return 0;

    }
    #endregion


    #region Terrain Generation Functions

    public string GetActiveMapName()
    {
        // If the game is started from the Main scene while developing we still want to load a map.
        if (GlobalControl.Instance.ActiveSavePath == null)
        {
            ActiveSavePath = Application.dataPath + "/Saves/Save00/Map.txt";
        }

        else
        {
            ActiveSavePath = GlobalControl.Instance.ActiveSavePath;
        }
        

        

        return ActiveSavePath;
    }

    public void PlaceWaterPlane()
    {
        var water = Instantiate(waterPlanePrefab);
        water.transform.position = new Vector3((WorldSize / 2), -1f, (WorldSize / 2));

        // Scale the plane.
        // The plane will need to be scaled up by 1 for every 100 in world size.
        var scaleFactor = Math.Round(WorldSize / 100d, 0) * 1;
        water.transform.localScale = new Vector3(1 + (float)scaleFactor, 1f, 1 + (float)scaleFactor);
    }

    public void LoadGroundTitlesFromMap()
    {
        //0. Load the map in at index 0.
        // Path to file.
        string path = ActiveSavePath + "/Map.txt";

        activeMapText = File.ReadAllText(path);

        // 1. Check to see if the world map is square. If it is not we need to throw an error.
        // This strips all characters that are not 0, ., |, -, └, ┘, ┐, ┴, ┬, ├, ┤, ^, *, &, 1, 2, 3, 4, x and ┌
        //string allCharsInString = System.Text.RegularExpressions.Regex.Replace(TerrainMaps[selectedMapIndex].text, @"[^.0|┐└┌┘┴┬├┤^&*1234x-]", "");
        string allCharsInString = System.Text.RegularExpressions.Regex.Replace(activeMapText, @"[^.0|┐└┌┘┴┬├┤^&*1234gx-]", "");

        //int numOfCols = TerrainMaps[selectedMapIndex].text.Replace(" ", "").IndexOf('\n');
        //int numOfRows = allCharsInString.Length / numOfCols;

        int numOfCols = activeMapText.Replace(" ", "").IndexOf('\n');
        int numOfRows = allCharsInString.Length / numOfCols;

        if (numOfCols != numOfRows)
        {
            Debug.Log("The Map Is Not Square!");
            print(allCharsInString.Length / (TerrainMaps[selectedMapIndex].text.Replace(" ", "").IndexOf('\n') - 1));
            //print((TerrainMaps[selectedMapIndex].text.Replace(" ", "").IndexOf('\n') - 1));
        }

        // 2. Save the WorldSize variable based on the map side length.
        //else WorldSize = TerrainMaps[selectedMapIndex].text.Replace(" ", "").IndexOf('\n');
        else WorldSize = activeMapText.Replace(" ", "").IndexOf('\n');

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
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '3' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '4' 
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == 'g')
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
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '2' || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == '3' 
                    || allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == 'g')
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

                    // Grave Yards.
                    else if (allCharsInString.Substring(line * WorldSize, WorldSize)[letter] == 'g')
                    {
                        Instantiate(GraveYardPrefab[0], new Vector3(line, 0, letter), Quaternion.Euler(0f, 0f, 0f)).transform.SetParent(groundTiles[LocateIndexOfGroundTile(line, letter)].transform);

                        // Set the ground tile's attribute terrainIsPassable to false.
                        groundTiles[LocateIndexOfGroundTile(line, letter)].GetComponent<GroundTileController>().terrainIsPassable = true;
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
        MainPanel.SetActive(false);
    }

    #region Basic Information Panel Functions

    public void ClickEndTurn()
    {
        // Make a sound.
        SFXSource.PlayOneShot(mouseClick01);

        // Turn off the static unit panel.
        StaticIndividualUnitPanel.SetActive(false);

        // Make this button disappear.
        EndOfDayTurnButton.gameObject.SetActive(false);

        // Remove dead zombies.
        for (int indexOfZombie = 0; indexOfZombie < zombiesInPlay.Count; indexOfZombie++)
        {
            // Check to see if they are dead.
            if (zombiesInPlay[indexOfZombie].GetComponent<ZombieController>().hitPoints <= 0)
            {
                var temp = zombiesInPlay[indexOfZombie];

                // Reset the ground tile to passable.
                temp.GetComponentInParent<GroundTileController>().terrainIsPassable = true;

                // Remove them from the list.
                zombiesInPlay.Remove(temp);

                // Destroy the zombie.
                Destroy(temp);
            }
        }

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

    public void ClearMovementSelectionTiles()
    {
        //Delete the old movement option titles if they exist.
        for (int listIndex = 0; listIndex < movementSelectedTiles.Count; listIndex++)
        {
            GameObject.Destroy(movementSelectedTiles[listIndex]);
        }
        movementSelectedTiles.Clear();
    }

    public bool MapUnitMovementPath(string dir)
    {
        int previousTileLocX = 0;
        int previousTileLocZ = 0;
        int newTileLocX = 0;
        int newTileLocZ = 0;

        // Store this previous tiles location.
        if (movementSelectedTiles.Count > 0)
        {
            previousTileLocX = (int)movementSelectedTiles[movementSelectedTiles.Count-1].transform.position.x;
            previousTileLocZ = (int)movementSelectedTiles[movementSelectedTiles.Count - 1].transform.position.z;
        }

        else
        {
            previousTileLocX = (int)selectedUnit.transform.position.x;
            previousTileLocZ = (int)selectedUnit.transform.position.z;
        }

        // Add this new input to the movement selected tiles list, if the ground tile exists.
        if (dir == "U")
        {
            newTileLocX = previousTileLocX;
            newTileLocZ = previousTileLocZ + 1;
        }

        else if (dir == "D")
        {
            newTileLocX = previousTileLocX;
            newTileLocZ = previousTileLocZ - 1;
        }

        else if (dir == "L")
        {
            newTileLocX = previousTileLocX - 1;
            newTileLocZ = previousTileLocZ;
        }

        else if (dir == "R")
        {
            newTileLocX = previousTileLocX + 1;
            newTileLocZ = previousTileLocZ;
        }

        if (groundTiles[LocateIndexOfGroundTile(newTileLocX, newTileLocZ)] == null)
        {
            print("Unable to map movement because ground tile does not exist.");
            return false;
        }

        

        // Check to see if the location has already been stored in the movement list.
        int duplicateIndex = -1;

        for (int i = 0; i < movementSelectedTiles.Count; i++)
        {
            if ((int)movementSelectedTiles[i].transform.position.x == newTileLocX && (int)movementSelectedTiles[i].transform.position.z == newTileLocZ)
            {
                // To delete the duplicates we will delete all the tiles in the list after the first duplicate.
                duplicateIndex = i;
            }
        }

        if (duplicateIndex != -1)
        {
            for (int i = movementSelectedTiles.Count - 1; i >= duplicateIndex; i--)
            {
                var tempTileToDelete = movementSelectedTiles[i];
                movementSelectedTiles.RemoveAt(i);
                Destroy(tempTileToDelete);
            }
        }

        // Check to see if the last tile on list is red, if it is, then no more tiles can be added.
        if (movementSelectedTiles.Count > 0)
        {
            if (movementSelectedTiles[movementSelectedTiles.Count - 1].name == "MovementSelectedTileRed(Clone)")
            {
                print("Unable to map because last tile is red.");
                return false;
            }
        }

        // Check to see if the unit has enough APs.
        if (movementSelectedTiles.Count >= selectedUnit.GetComponent<UnitController>().actionPoints)
        {
            print("Not enough APs.");
            return false;
        }

        // Place movement tile. If the ground title is empty the tile will be green, if it has something like a farm plot then it will be yellow, and if it is not passable it will be red.
        int indexForMovementTilePrefab = 0;

        if (groundTiles[LocateIndexOfGroundTile(newTileLocX, newTileLocZ)].GetComponent<GroundTileController>().terrainIsPassable == false)
        {
            indexForMovementTilePrefab = 1;

            // The only structures that will cause the tile to not be passable are, wall, house, or factory.
            for (int i = 0; i < groundTiles[LocateIndexOfGroundTile(newTileLocX, newTileLocZ)].transform.childCount; i++)
            {
                
                if (groundTiles[LocateIndexOfGroundTile(newTileLocX, newTileLocZ)].transform.GetChild(i).tag == "Abandoned House" || groundTiles[LocateIndexOfGroundTile(newTileLocX, newTileLocZ)].transform.GetChild(i).tag == "Wall")
                {
                    indexForMovementTilePrefab = 2;
                }

                if (groundTiles[LocateIndexOfGroundTile(newTileLocX, newTileLocZ)].transform.tag == "Holding Factory") indexForMovementTilePrefab = 2;
            }
        }

        var tempTile = Instantiate(movementSelectedPrefab[indexForMovementTilePrefab], new Vector3(newTileLocX, .1f, newTileLocZ), Quaternion.identity);
        tempTile.transform.SetParent(groundTiles[LocateIndexOfGroundTile(newTileLocX, newTileLocZ)].transform);
        movementSelectedTiles.Add(tempTile);


        return true;

    }

    public bool UnitInteractsWithNextGroundTileOnMove(GameObject unit, int indexForTileArray)
    {
        // Returns a bool of true if the unit should move onto the selected ground tile.

        // If the movement selected list is not empty that means we need to move the unit.

        // If the ground title is passable then we can move the unit on to it.

        bool unitShouldContinueToMove = false;

        if (movementSelectedTiles[indexForTileArray].transform.parent.GetComponent<GroundTileController>().terrainIsPassable)
        {

            unit.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = true;
            unit.transform.SetParent(movementSelectedTiles[nextPositionIndex].transform.parent);
            unit.transform.parent.GetComponent<GroundTileController>().terrainIsPassable = false;

            SpendActionPoint(1, movementSelectedTiles[indexForTileArray].transform.position);

            return unitShouldContinueToMove;
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
                if ((childTag == "Tree" || childTag == "Rock")) Harvest(child.transform.gameObject);


                // If the ground title is a Abandoned Structure or Structure we will bring up Interact with Structure Menu.
                else if (childTag == "Abandoned House" || childTag == "Abandoned Factory"
                    || childTag == "Abandoned Vehicle" || childTag == "Loot Box" || childTag == "Farm Plot" || childTag == "Living Quarters"
                    || childTag == "Medical Facility" || childTag == "Wall" || childTag == "Town Hall" || childTag == "Trap")
                    {
                    selectedStructureForUse = child.transform.gameObject;
                    OpenInteractWithStructurePanel(child.transform.gameObject);
                    }

                // If the ground title contains a zombie we will have the player attack.


                // If the ground title contains another player then nothing will happen.
                // Do something based on tag
            }

            

            return unitShouldContinueToMove;
        }
    }

    public void Harvest(GameObject itemToHarvest)
    {
        PlayUnitInteract(selectedUnit); // Play the interact with animation.

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
        SetButtonToSeeThrough(true, scrapButton);
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
            SetButtonToSeeThrough(false, scrapButton);
        }

        else if (structureObject.transform.tag.ToString() == "Farm Plot" || structureObject.transform.tag.ToString() == "Living Quarters"
            || structureObject.transform.tag.ToString() == "Medical Facility" || structureObject.transform.tag.ToString() == "Wall" 
            || structureObject.transform.tag.ToString() == "Town Hall" || structureObject.transform.tag.ToString() == "Trap")
        {
            SetButtonToSeeThrough(false, repairButton);
            SetButtonToSeeThrough(false, upgradeButton);
            // Set Scrap button to useable.
            SetButtonToSeeThrough(false, scrapButton);
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

        // There will be a 60% chance that food is found, 10% an item is found, 10% that a human is found, and 20% that nothing is found.
        int randomNumber = UnityEngine.Random.Range(0, 100);

        // Food
        if (randomNumber < 60)
        {
            var amtOfFoodToSavanged = UnityEngine.Random.Range(1, 5);
            food += amtOfFoodToSavanged;
            InteractWithStructureFeedbackText.text = "+ " + amtOfFoodToSavanged.ToString() + " Food";
            foodText.text = food.ToString();
        }
        // Item
        else if(randomNumber < 70)
        {
            CreateItem();
            InteractWithStructureFeedbackText.text = "New Item Found!";
        }
        // Human
        else if (randomNumber < 80)
        {
            if (selectedStructureForUse.tag == "Abandoned House" || selectedStructureForUse.tag == "Abandoned Factory")
            {
                if (unitsInPlay.Count < populationCap) DiscoverSurvivorOnScavenge();
                {
                    InteractWithStructureFeedbackText.text += "Survivor Found!";
                }
            }
        }
        // Nothing
        else
        {
            InteractWithStructureFeedbackText.text = "Nothing Found";
        }

        // Make Panel visible.
        InteractWithStructureFeedBackPanel.SetActive(true);
        SetButtonToSeeThrough(true, scavangeButton);
        SetButtonToSeeThrough(false, ExitInteractWithStructurePanel);

        // Destroy objects that should get destroyed.
        if (selectedStructureForUse.tag == "Loot Box" || selectedStructureForUse.tag == "Abandoned Vehicle")
        {
            // Play splatter fx.
            StartCoroutine(PlayDirtSplatterFX(selectedStructureForUse.transform.position, selectedStructureForUse));
        }
        
    }

    public bool DiscoverSurvivorOnScavenge()
    {
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

    public void ClickScrap()
    {
        // This function will scrap the selected structure.
        var tempStructure = selectedStructureForUse;

        // Set ground title to passable.
        groundTiles[LocateIndexOfGroundTile(Convert.ToInt32(selectedStructureForUse.transform.position.x), Convert.ToInt32(selectedStructureForUse.transform.position.z))].GetComponent<GroundTileController>().terrainIsPassable = true;

        // Remove it from the structure list.
        currentStructuresInGame.Remove(selectedStructureForUse);

        // Destroy it.
        Destroy(selectedStructureForUse);
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

        else if (nameOfStructure == "Trap")
        {
            var requiredMaterialString = "Wood: " + trapPrefab.GetComponent<StructureContoller>().woodToBuild[0].ToString() +
                " Stone: " + trapPrefab.GetComponent<StructureContoller>().stoneToBuild[0].ToString() + " Food: " +
                trapPrefab.GetComponent<StructureContoller>().foodToBuild[0].ToString();

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
        else if (nameOfStructure == "Trap") return trapPrefab;
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

                // If the structure is a wall we need to set the rotation.
                tempStructure.GetComponent<StructureContoller>().wallRot = wallOnWallSelector.transform.rotation;

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
        else if (currentStructureSelectedToBuild.name == "Trap") trapSelector.transform.position = hit.transform.position;
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
        trapSelector.transform.position = hideLocation;
    }

    public void OpenStructureStatsPanel(GameObject structure)
    {
        // Open panel.
        StructureStatsPanel.SetActive(true);

        // Update all text objects.
        SelectedStructureTitleText.text = structure.GetComponentInParent<StructureContoller>().structureType;
        SelectedStructureCurrentLevelText.text = (structure.GetComponentInParent<StructureContoller>().currentStructureLevel + 1).ToString();
        SelectedStructureMaxLevelText.text = structure.GetComponentInParent<StructureContoller>().structureObjects.Length.ToString();

        // Check to see if the structure can still be upgraded.
        if (structure.GetComponentInParent<StructureContoller>().currentStructureLevel == 2)
        {
            RequiredWoodForUpgradeText.text = "NA";
            RequiredStoneForUpgradeText.text = "NA";
            RequiredFoodForUpgradeText.text = "NA";

        }

        else
        {
            RequiredWoodForUpgradeText.text = structure.GetComponentInParent<StructureContoller>().woodToBuild[structure.GetComponentInParent<StructureContoller>().currentStructureLevel + 1].ToString();
            RequiredStoneForUpgradeText.text = structure.GetComponentInParent<StructureContoller>().stoneToBuild[structure.GetComponentInParent<StructureContoller>().currentStructureLevel + 1].ToString();
            RequiredFoodForUpgradeText.text = structure.GetComponentInParent<StructureContoller>().foodToBuild[structure.GetComponentInParent<StructureContoller>().currentStructureLevel + 1].ToString();
        }
        
        structureHitPointsSlider.value =  ((float) structure.GetComponentInParent<StructureContoller>().hitPoints / (float) structure.GetComponentInParent<StructureContoller>().hitPointLimit);

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

    #region Main Panel Functions

    public void OpenMainMenu(string subPanel)
    {
        MainPanel.SetActive(true);

        CloseAllSubPanelsOfMainMenu();

        if (subPanel == "Objectives") ObjectivesPanel.SetActive(true);
        else if (subPanel == "Units")
        {
            UnitsPanel.SetActive(true);
            UpdateUnitNextUnitInformation(selectedUnit);
        }
            
            
        else if (subPanel == "Game") GamePanel.SetActive(true);
        else if (subPanel == "Settings") SettingsPanel.SetActive(true);
        else ObjectivesPanel.SetActive(true);
    }

    public void CloseMainMenu()
    {
        MainPanel.SetActive(false);
    }

    private void CloseAllSubPanelsOfMainMenu()
    {
        ObjectivesPanel.SetActive(false);
        UnitsPanel.SetActive(false);
        GamePanel.SetActive(false);
        SettingsPanel.SetActive(false);
    }

    public void HoverGamePanel(string NameOfButton)
    {
        if (NameOfButton == "Quick Save") GameSaveFeedbackText.text = "Quick Save?";
        else GameSaveFeedbackText.text = "";
    }

    public void UpdateUnitNextUnitInformation(GameObject unit)
    {
        unitNameUnitPanelText.text = unit.GetComponent<UnitController>().unitName;
        unitClassUnitPanelText.text = unit.GetComponent<UnitController>().unitClass.ToString();
        actionPointsUnitPanelText.text = unit.GetComponent<UnitController>().actionPoints.ToString();
        actionPointLimitUnitPanelText.text = unit.GetComponent<UnitController>().actionPointsLimit.ToString();
        attackUnitPanelText.text = unit.GetComponent<UnitController>().attack.ToString();
        hitPointsUnitPanelText.text = unit.GetComponent<UnitController>().hitPoints.ToString();
        hitPointLimitUnitPanelText.text = unit.GetComponent<UnitController>().hitPointLimit.ToString();
        attackRangeUnitPanelText.text = unit.GetComponent<UnitController>().attackRange.ToString();
        defenseUnitPanelText.text = unit.GetComponent<UnitController>().defense.ToString();
        sightUnitPanelText.text = unit.GetComponent<UnitController>().sight.ToString();
        currentUnitNumText.text = (unitsInPlay.IndexOf(unit) + 1).ToString();
        currentUnitSelected = unitsInPlay.IndexOf(unit);
        unitlimitText.text = unitsInPlay.Count.ToString();
        if ((int)unit.GetComponent<UnitController>().unitClass == 0)
        {
            if (unit.GetComponent<UnitController>().sex == "M") unitImage.sprite = unitSprites[0];
            else unitImage.sprite = unitSprites[3];
        }

        else if ((int)unit.GetComponent<UnitController>().unitClass == 1)
        {
            if (unit.GetComponent<UnitController>().sex == "M") unitImage.sprite = unitSprites[1];
            else unitImage.sprite = unitSprites[4];
        }

        else
        {
            if (unit.GetComponent<UnitController>().sex == "M") unitImage.sprite = unitSprites[2];
            else unitImage.sprite = unitSprites[5];
        }


        actionPointunitPanelSlider.value = (float)(unit.GetComponent<UnitController>().actionPoints) / (float)(unit.GetComponent<UnitController>().actionPointsLimit);
        hitPointunitPanelSlider.value = (float)(unit.GetComponent<UnitController>().hitPoints) / (float)(unit.GetComponent<UnitController>().hitPointLimit);

        // Update the images for item.
        unitItemImagesOnButtons[0].sprite = UIMask;
        unitItemImagesOnButtons[1].sprite = UIMask;

        for (int i = 0; i < unit.GetComponent<UnitController>().unitBackpack.Length; i++)
        {
            string itemName = unit.GetComponent<UnitController>().unitBackpack[i];

            if (itemNames.Contains(itemName))
            {
                if (itemName == itemNames[0]) unitItemImagesOnButtons[i].sprite = ItemImages[0];
                else if (itemName == itemNames[1]) unitItemImagesOnButtons[i].sprite = ItemImages[1];
                else if (itemName == itemNames[2]) unitItemImagesOnButtons[i].sprite = ItemImages[2];
                else if (itemName == itemNames[3]) unitItemImagesOnButtons[i].sprite = ItemImages[3];

            }


        }
    }

    public void ClickNextUnitInfo(bool forward)
    {
        if (forward)
        {
            if (currentUnitSelected == unitsInPlay.Count - 1) UpdateUnitNextUnitInformation(unitsInPlay[0]);

            else UpdateUnitNextUnitInformation(unitsInPlay[currentUnitSelected + 1]);
        }

        else
        {
            if (currentUnitSelected == 0) UpdateUnitNextUnitInformation(unitsInPlay[unitsInPlay.Count - 1]);

            else UpdateUnitNextUnitInformation(unitsInPlay[currentUnitSelected - 1]);
        }
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

    public IEnumerator PlayEarnActionPoint(GameObject unit)
    {
        // This function is to be called from the update loop only.

        // Set the amount of action points to reward, if the unit has boots then they will get full action points restored.
        if (unit.GetComponent<UnitController>().DoesUnitHaveItem("Boots"))
        {
            rewardActionPoints = unit.GetComponent<UnitController>().actionPointsLimit - unit.GetComponent<UnitController>().actionPoints;
        }

        else
        {
            rewardActionPoints = 1;
            rewardActionPoints += ActionPointsRewardFromLivingQuarters(unit);
        }
        

        while (rewardActionPoints > 0)
        {
            // Create a game object from prefab.
            var tempAPAnimation = Instantiate(APAnimationObject);

            // Get animation object to the correct location.
            tempAPAnimation.transform.position = unit.transform.position;

            // Add an action point.
            unit.GetComponent<UnitController>().actionPoints += 1;

            // Play the correct animation.
            tempAPAnimation.GetComponent<Animator>().Play("EarnAP");

            yield return new WaitForSeconds(.8f);

            // Destory the temp object.
            Destroy(tempAPAnimation);

            //Decrement action points.
            rewardActionPoints -= 1;

        }

        // These help the update loop continue to function.
        unitIndexForUpdate += 1;
        UpdatingAP = true;
    }

    public IEnumerator PlayEarnHitPoints(GameObject unit)
    {
        // This function is to be called from the update loop only.

        // Set the amount of action points to reward, if the unit has a medkit then full hit points will be restored.
        if (unit.GetComponent<UnitController>().DoesUnitHaveItem("Medkit"))
        {
            rewardHitPoints = unit.GetComponent<UnitController>().hitPointLimit - unit.GetComponent<UnitController>().hitPoints;
        }

        else rewardHitPoints = HitPointsRewardFromMedical(unit);
        

        while (rewardHitPoints >= 5)
        {
            // Create a game object from prefab.
            var tempHPAnimation = Instantiate(HPAnimationObject);

            // Get animation object to the correct location.
            tempHPAnimation.transform.position = unit.transform.position;

            // Add an action point.
            unit.GetComponent<UnitController>().hitPoints += 5;

            // Play the correct animation.
            tempHPAnimation.GetComponent<Animator>().Play("EarnHP");

            yield return new WaitForSeconds(.8f);

            // Destory the temp object.
            Destroy(tempHPAnimation);

            //Decrement action points.
            rewardHitPoints -= 5;

        }

        // These help the update loop continue to function.
        unitIndexForUpdate += 1;
        UpdatingHP = true;
    }

    public IEnumerator PlayFeedUnit(GameObject unit)
    {
        // This function is to be called from the update loop only.
      
        // Create a game object from prefab.
        var tempFeedAnimation = Instantiate(FeedAnimationObject);

        // Get animation object to the correct location.
        tempFeedAnimation.transform.position = unit.transform.position;

        // Reduce Food.
        if (food > 0) food -= 1;

        // Play the correct animation.
        tempFeedAnimation.GetComponent<Animator>().Play("Eat");

        yield return new WaitForSeconds(.8f);

        // Destory the temp object.
        Destroy(tempFeedAnimation);

        // These help the update loop continue to function.
        unitIndexForUpdate += 1;
        UpdatingFood = true;
    }

    public IEnumerator PlayStarveUnit(GameObject unit)
    {
        // This function is to be called from the update loop only.

        // Create a game object from prefab.
        var tempStarveAnimation = Instantiate(StarveAnimationObject);

        // Get animation object to the correct location.
        tempStarveAnimation.transform.position = unit.transform.position;

        // Play the correct animation.
        tempStarveAnimation.GetComponent<Animator>().Play("Starve");

        yield return new WaitForSeconds(.8f);

        // Reduce unit health.
        unit.GetComponent<UnitController>().TakeDamage(1);

        // Destory the temp object.
        Destroy(tempStarveAnimation);

        // These help the update loop continue to function.
        unitIndexForUpdate += 1;
        UpdatingFood = true;
    }

    #endregion


    #region Sound and Game Functions

    public void SetSliderOnStart(bool loadingGame)
    {
        // If the game is being loaded then the function will need to grab the settings from the load.
        if (loadingGame)
        {

        }

        else
        {
            float sliderValue = new float();
            MusicMixer.audioMixer.GetFloat("MusicVol", out sliderValue);
            musicSlider.value = sliderValue;
            SFXMixer.audioMixer.GetFloat("SFXVol", out sliderValue);
            sfxSlider.value = sliderValue;
        }
    }

    public void SetGameSettingsOnStart(bool loadingGame)
    {
        if (loadingGame)
        {

        }

        else
        {
            // Set the zombie speed for animation
            ZombieAnimationSpeed = GlobalControl.Instance.ZombieAnimationSpeed;
            ZombieAnimationSpeedSlider.value = ZombieAnimationSpeed;

            // Set the beginning of day animations
            BeginningOfDayAnimations = GlobalControl.Instance.BeginningOfDayAnimations;
            BeginningOfDayAnimationsToggle.isOn = BeginningOfDayAnimations;
        }
    }


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

    public void ToggleBeginningOfDayAnimations(bool animationOn)
    {
        if (animationOn) BeginningOfDayAnimations = true;
        else BeginningOfDayAnimations = false;
    }

    public void UpdateZombieAnimationSpeed(bool increase)
    {
        if (increase && ZombieAnimationSpeed < 4)
        {
            ZombieAnimationSpeed += 1;
            ZombieAnimationSpeedSlider.value = ZombieAnimationSpeed;
        }

        else if (!increase && ZombieAnimationSpeed > 1)
        {
            ZombieAnimationSpeed -= 1;
            ZombieAnimationSpeedSlider.value = ZombieAnimationSpeed;
        }
    }

    #endregion


    #region Items

    public string CreateItem(string itemname = "random")
    {
        // This function creates an item, if there is room in the backpack, at random unless the name of an item is given.
        // Returns the string of the item and changes the image in the backpack. No game object is actually created, just the image.

        int mainBackpackItemIndex = 9;
       

        // Check to see if there is a free place in the backpack.
        for (int i = 0; i < MainBackpackImagesOnButtons.Length; i++)
        {
            if (MainBackpackImagesOnButtons[i].sprite.name == "UIMask")
            {
                mainBackpackItemIndex = i;
                break;
            }
        }

        // If there is room we will create an item.
        if (mainBackpackItemIndex != 9)
        {
            if (itemNames.Contains(itemname)){
                PlaceItemInBackpack(itemname, mainBackpackItemIndex);
                return "Pistol";
            }

            else
            {
                PlaceItemInBackpack(itemNames[UnityEngine.Random.Range(0, itemNames.Length)], mainBackpackItemIndex);
                return "Random";
            }
            
            
        }

        // Otherwise we will return a string of "No Room In Backpack".
        else
        {
            return "No Room In Backpack";
        }

        
    }

    public void PlaceItemInBackpack(string itemname, int indexInBackpack)
    {
        if (itemname == "Mealkit")
        {
            MainBackpackImagesOnButtons[indexInBackpack].sprite = ItemImages[0];
        }

        else if (itemname == "Medkit")
        {
            MainBackpackImagesOnButtons[indexInBackpack].sprite = ItemImages[1];
        }

        else if (itemname == "Boots")
        {
            MainBackpackImagesOnButtons[indexInBackpack].sprite = ItemImages[2];
        }

        else if (itemname == "Shovel")
        {
            MainBackpackImagesOnButtons[indexInBackpack].sprite = ItemImages[3];
        }

        else
        {
            print("Item name is incorrect when inputed into PlaceItemInBackpack");
        }
    }

    public bool MoveItemFromBackpackToUnit(int itemIndex, GameObject unit)
    {
        // Check to see if the unit has room.
        if (unit.GetComponent<UnitController>().UnitHasSpaceForAnItem() == false)
        {
            return false;
        }

        // Check to see if there is an item in the slot.
        if(MainBackpackImagesOnButtons[itemIndex].sprite.name != "UIMask")
        {
            if (MainBackpackImagesOnButtons[itemIndex].sprite.name == "blue23")
            {
                unit.GetComponent<UnitController>().AddItem(itemNames[0]);
            }

            else if (MainBackpackImagesOnButtons[itemIndex].sprite.name == "blue7")
            {
                unit.GetComponent<UnitController>().AddItem(itemNames[1]);
            }

            else if (MainBackpackImagesOnButtons[itemIndex].sprite.name == "graytwo8")
            {
                unit.GetComponent<UnitController>().AddItem(itemNames[2]);
            }

            else if (MainBackpackImagesOnButtons[itemIndex].sprite.name == "graytwo30")
            {
                unit.GetComponent<UnitController>().AddItem(itemNames[3]);
            }

            // Remove the item from the main backpack.
            MainBackpackImagesOnButtons[itemIndex].sprite = UIMask;

            // Update the unit item icon.
            ClickNextUnitInfo(true);
            ClickNextUnitInfo(false);

            return true;
        }

        else
        {
            return false;
        }
    }

    public bool MoveItemFromUnitToBackpack(string itemName)
    {
        print("Placing " + itemName + " in backpack.");
        int indexOfFreeSpace = 9;

        for (int i = 0; i < MainBackpackImagesOnButtons.Length; i++)
        {
            if (MainBackpackImagesOnButtons[i].sprite.name == "UIMask") indexOfFreeSpace = i;
        }

        if (indexOfFreeSpace != 9)
        {
            if (itemName == "Mealkit") MainBackpackImagesOnButtons[indexOfFreeSpace].sprite = ItemImages[0];
            else if (itemName == "Medkit") MainBackpackImagesOnButtons[indexOfFreeSpace].sprite = ItemImages[1];
            else if (itemName == "Boots") MainBackpackImagesOnButtons[indexOfFreeSpace].sprite = ItemImages[2];
            else if (itemName == "Shovel") MainBackpackImagesOnButtons[indexOfFreeSpace].sprite = ItemImages[3];
        }
        return true;
    }

    public void ClickMoveItemFromBackpack(int buttonIndex)
    {
        MoveItemFromBackpackToUnit(buttonIndex, unitsInPlay[currentUnitSelected]);
    }

    public void ClickMoveItemFromUnitpack(int buttonIndex)
    {
        string itemName = "";

        if (unitItemImagesOnButtons[buttonIndex].sprite.name == "blue23") itemName = "Mealkit";
        else if (unitItemImagesOnButtons[buttonIndex].sprite.name == "blue7") itemName = "Medkit";
        else if (unitItemImagesOnButtons[buttonIndex].sprite.name == "graytwo8") itemName = "Boots";
        else if (unitItemImagesOnButtons[buttonIndex].sprite.name == "graytwo30") itemName = "Shovel";
        else print("Sprite does not match");

        unitsInPlay[currentUnitSelected].GetComponent<UnitController>().RemoveItem(itemName);

        // Update the unit item icon.
        ClickNextUnitInfo(true);
        ClickNextUnitInfo(false);

        // Place the item in the main backpack if there is room, otherwise it will be discarded.
        MoveItemFromUnitToBackpack(itemName);
    }

    #endregion

    #endregion
}
