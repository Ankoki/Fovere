using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Class to handle the game starting mechanisms, and responsible for
/// storage and creation of all objects in the main game scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    
    private static readonly string[] PlayerStructure =
    {
        "user_id",
        "last_position",
        "inventory_data",
        "quest_data",
        "profession_data",
        "abilities_data",
        "interest_data",
        "economy_data",
        "expansion_data"
    };

    private static readonly string[] WorldStructure =
    {
        "label",
        "seed",
        "worldPosition",
        "generator",
        "worldXLength",
        "worldZLength",
        "chunks"
    };

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject residentPrefab;
    
    private Dictionary<string, object> _rawPlayerData = new();
    private Dictionary<string, object> _rawWorldsData = new();
    
    private PlayerHandler _playerHandler;
    private PlayerData _playerData;
    private World _world;
    
    private void Awake()
    {
        ItemType.Initialize(); // Initialize all of our ItemType values.
        var transmitter = DDOLTransmitter.Instance;
        // Player Loading
        if (transmitter.HasPlayerTransferData() && DataHelpers.ValidateKeys(transmitter.RetrieveTransferredPlayer(), PlayerStructure))
        {
            _rawPlayerData = transmitter.RetrieveTransferredPlayer();
            Debug.Log($"Player[{_rawPlayerData[""]}] received.");
        }
        else
        {
            Debug.Log("No player transmitted. Default player generating...");
            _rawPlayerData = new Dictionary<string, object>
            {
                { "user_id", transmitter.RetrieveTitleId() },
                { "last_position", "100,100,100,Village" },
                { "inventory_data",  new PlayerInventory().Serialize() },
                { "quest_data", new QuestData().Serialize() },
                { "profession_data", new ProfessionData().Serialize() },
                { "abilities_data", new AbilitiesData().Serialize() },
                { "interest_data", new InterestData().Serialize() },
                { "economy_data", new EconomyData().Serialize() },
                { "expansion_data", new ExpansionData().Serialize() }
            };
        }
        _playerData = PlayerData.Deserialize(_rawPlayerData);
        // World Loading
        if (transmitter.HasWorldTransferData() && 
            transmitter.RetrieveTransferredPlayer().ContainsKey("worlds"))
        {
            _rawWorldsData = (Dictionary<string, object>) transmitter.RetrieveTransferredWorlds()["worlds"];
            Debug.Log($"Worlds data retrieved.");
        }
        else
        {
            Debug.Log("No world data retrieved. Starter village generating...");
            var inner = new Dictionary<string, object>
            {
                { "createWorld", true }
            };
            _rawWorldsData = new Dictionary<string, object>
            {
                { "createWorld", inner }
            };
        }
        // Deserialize and load the world the player is in. The rest are stored for later usage.
        if (!_rawWorldsData.ContainsKey(_playerData.LastWorld))
            _playerData.LastWorld = "createWorld";
        //_world = World.Deserialize(_rawWorldsData[_playerData.LastWorld] as Dictionary<string, object>, _playerData.LastPosition);
    }
    
    private void Start()
    {
        var spawned = Instantiate(playerPrefab, transform);
        var cameraController = spawned.GetComponent<CameraController>();
        cameraController.standardCamera = GameObject.Find("CMStandard").GetComponent<CinemachineCamera>();
        cameraController.zoomCamera = GameObject.Find("CMZoom").GetComponent<CinemachineCamera>();
        cameraController.obstructedViewCamera = GameObject.Find("CMObstructed").GetComponent<CinemachineCamera>();
        var cameraTarget = GameObject.Find("CameraTarget");
        cameraController.standardCamera.Follow = cameraTarget.transform;
        cameraController.zoomCamera.Follow = cameraTarget.transform;
        cameraController.obstructedViewCamera.Follow = cameraTarget.transform;
        _playerHandler = spawned.GetComponent<PlayerHandler>();
        _playerHandler.SetPlayerData(_playerData);
        //_world.OnWorldLoadEvent += OnWorldLoad;
    }

    private void OnWorldLoad()
    {
        Debug.Log($"World[{_world.label}] loaded and world load event called.");
        //_playerHandler.SafeTeleport(_world, _playerData.LastPosition.x, _playerData.LastPosition.z);
        _playerHandler.SpawnPlayer();
        // Spawn residents in this world. For now, test residents.
        var spawnedResident = Instantiate(residentPrefab, transform);
        var container = spawnedResident.gameObject.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
        var text = container.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        var dialogueTrigger = _playerHandler.GetComponent<DialogueTrigger>();
        dialogueTrigger.uiContainer = container;
        dialogueTrigger.buttonText = text;
        var residentHandler = spawnedResident.GetComponent<ResidentHandler>();
        var residentData = ScriptableObject.CreateInstance<ResidentData>();
        residentData.residentName = "Tester";
        //residentHandler.SafeTeleport(_world, 100, 100);
    }
    
}