using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class City : HexTile
{
    [SerializeField] private Button archer;
    [SerializeField] private Button PikeMan;
    [SerializeField] private Button horseMan;
    [SerializeField] private bool spawnedUnit;
    [SerializeField] private int cityThreatenedInTurn = -1;
    [SerializeField] private bool cityThreatened;
    [SerializeField] private bool cityDestroyed;
    [SerializeField] private List<MeshRenderer> cityFlags;
    [SerializeField] private Material leftPlayerFlag;
    [SerializeField] private Material rightPlayerFlag;
    
    public PlayerData ControllingPlayer;
    public UnitsSpawner UnitsSpawner;
    public int CityThreatenedInTurn
    {
        get => cityThreatenedInTurn;
        set => cityThreatenedInTurn = value;
    }

    public bool CityThreatened
    {
        get => cityThreatened;
        set => cityThreatened = value;
    }

    public bool CityDestroyed
    {
        get => cityDestroyed;
        set => cityDestroyed = value;
    }


    public bool SpawnedUnit => spawnedUnit;
    private bool startedSpawn = false;

    public void Initialize(PlayerData playerData)
    {
        ControllingPlayer = playerData;
        foreach (var cityFlag in cityFlags)
        {
            cityFlag.material = ControllingPlayer.Player == Player.Left ? leftPlayerFlag : rightPlayerFlag;
        }
    }
    
    private void Update()
    {
        if (startedSpawn)
        {
            if (UnitsSpawner.FinishedSpawning)
            {
                spawnedUnit = true;
                startedSpawn = false;
            }        
        }
    }

    public void SelectCity()
    {
        archer.gameObject.SetActive(true);
        PikeMan.gameObject.SetActive(true);
        horseMan.gameObject.SetActive(true);
    }

    public void DeselectCity()
    {
        archer.gameObject.SetActive(false);
        PikeMan.gameObject.SetActive(false);
        horseMan.gameObject.SetActive(false);
    }

    public void SpawnArcher()
    {
        if(!startedSpawn)
        {
            UnitsSpawner.SpawnAdditionalUnit(ControllingPlayer.Player == Player.Left
                ? UnitsSpawner.UnitsSpawningStep.LeftArchers
                : UnitsSpawner.UnitsSpawningStep.RightArchers);
            startedSpawn = true;
        }
    }
    
    public void SpawnPikeMan()
    {
        if (!startedSpawn)
        {
            UnitsSpawner.SpawnAdditionalUnit(ControllingPlayer.Player == Player.Left
                ? UnitsSpawner.UnitsSpawningStep.LeftPikemans
                : UnitsSpawner.UnitsSpawningStep.RightPikemans);
            startedSpawn = true;
        }
    }
    
    public void SpawnHorseMan()
    {
        if (!startedSpawn)
        {
            UnitsSpawner.SpawnAdditionalUnit(ControllingPlayer.Player == Player.Left
                ? UnitsSpawner.UnitsSpawningStep.LeftHorsemans
                : UnitsSpawner.UnitsSpawningStep.RightHorsemans);
            startedSpawn = true;
        }
    }
}
