using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitsSpawner : MonoBehaviour
{
    [SerializeField] private UnitPrefabSelector archerPrefab;
    [SerializeField] private UnitPrefabSelector pikemanPrefab;
    [SerializeField] private UnitPrefabSelector horsemanPrefab;
    [SerializeField] private int numberOfArchers;
    [SerializeField] private int numberOfPikemans;
    [SerializeField] private int numberOfHorsemans;
    [SerializeField] private int maxDistanceFromCity;

    public List<Unit> LeftPlayerUnits => leftPlayerUnits;
    public List<Unit> RightPlayerUnits => rightPlayerUnits;
    public bool FinishedSpawning
    {
        get => finishedSpawning;
        set => finishedSpawning = value;
    }

    private List<Unit> leftPlayerUnits;
    private List<Unit> rightPlayerUnits;
    private bool finishedSpawning = false;
    private Unit currentlySpawnedUnit;
    private Player player;
    private int spawnedDuplicateUnits = 0;
    private UnitsSpawningStep unitsSpawningStep = UnitsSpawningStep.LeftArchers;
    private HexTile previousTile;
    private Camera mainCamera;
    private List<HexTile> inlandHexes;
    private int numberOfDuplicateUnits;

    private List<HexTile> leftPlayerSpawnTiles;
    private List<HexTile> rightPlayerSpawnTiles;
    
    public enum UnitsSpawningStep
    {
        LeftArchers,
        LeftPikemans,
        LeftHorsemans,
        RightArchers,
        RightPikemans,
        RightHorsemans,
        SpawningFinished
    }

    public void Initialize(City leftPlayerCity, City rightPlayerCity, List<HexTile> inlandHexes)
    {
        mainCamera = Camera.main;
        this.inlandHexes = inlandHexes;
        leftPlayerUnits = new List<Unit>();
        rightPlayerUnits = new List<Unit>();
        leftPlayerSpawnTiles = SetSpawnTiles(leftPlayerCity);
        rightPlayerSpawnTiles = SetSpawnTiles(rightPlayerCity);
        SpawnUnit();
    }

    public void SpawnAdditionalUnit(UnitsSpawningStep unitsSpawningStep)
    {
        this.unitsSpawningStep = unitsSpawningStep;
        SpawnUnit();
        numberOfDuplicateUnits = 1;
        this.unitsSpawningStep = UnitsSpawningStep.SpawningFinished;
        currentlySpawnedUnit.MarkAsMoved();
    }

    private void SpawnUnit()
    {
        SetSpawningUnitVariables(unitsSpawningStep);
        if (player == Player.Left)
        {
            foreach (var hex in inlandHexes)
            {
                if (!leftPlayerSpawnTiles.Contains(hex) || !hex.IsTileAccessible())
                    hex.ChangeHexVisibility(false);
                else
                    hex.ChangeHexVisibility(true);
            }
        }
        else
        {
            foreach (var hex in inlandHexes)
            {
                if (!rightPlayerSpawnTiles.Contains(hex) || !hex.IsTileAccessible())
                    hex.ChangeHexVisibility(false);
                else
                    hex.ChangeHexVisibility(true);
            }
        }
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var layerMask = LayerMask.GetMask("Ground");
        //layerMask = ~layerMask;
        if (!finishedSpawning && Physics.Raycast (ray, out var hit,500,layerMask))
        {
            var hitPosition = hit.point;
            currentlySpawnedUnit.transform.position = hitPosition;
        }

        if (finishedSpawning)
        {
            foreach (var hex in inlandHexes)
            {
                hex.ChangeHexVisibility(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlySpawnedUnit != null)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var layerMask = LayerMask.GetMask("Ground");
            //layerMask = ~layerMask;
            if (Physics.Raycast (ray, out var hit,500,layerMask))
            {
                var hitPosition = hit.point;
                currentlySpawnedUnit.transform.position = hitPosition;
            }
            else
                return;

            var hitHex = hit.collider.gameObject.GetComponent<HexTile>();
            if(previousTile == null)
                previousTile = hitHex;
            else if(previousTile != hitHex)
            {
                previousTile.DisableHexHighlight();
                previousTile = hitHex;
            }

            var isHexGoodForSpawningUnit = true;
            var coordinates = hitHex.coordinates;
            if (player == Player.Left)
            {
                if (!leftPlayerSpawnTiles.Contains(hitHex) || !hitHex.IsTileAccessible())
                    isHexGoodForSpawningUnit = false;
            }
            else
            {
                if (!rightPlayerSpawnTiles.Contains(hitHex) || !hitHex.IsTileAccessible())
                    isHexGoodForSpawningUnit = false;
            }
            
            if(isHexGoodForSpawningUnit)
                hitHex.SelectHex();
            else
                hitHex.HighlightAsInaccessible();

            
            if (Input.GetMouseButtonUp(0) && isHexGoodForSpawningUnit)
            {
                hitHex.unitOnHex = currentlySpawnedUnit;
                currentlySpawnedUnit.Player = player;

                currentlySpawnedUnit.transform.position = HexDirections.hexPositionBasedOnCoordinates(coordinates, currentlySpawnedUnit.transform.position.y);
                
                currentlySpawnedUnit.CurrentHex = hitHex;
                hitHex.DisableHexHighlight();
                currentlySpawnedUnit = null;
                spawnedDuplicateUnits++;
                if (spawnedDuplicateUnits < numberOfDuplicateUnits)
                {
                    SpawnUnit();
                }
                else
                {
                    unitsSpawningStep = GetToNextSpawningStep(unitsSpawningStep);
                    spawnedDuplicateUnits = 0;
                    SpawnUnit();
                }
            }
        }
    }

    private UnitsSpawningStep GetToNextSpawningStep(UnitsSpawningStep unitsSpawningStep)
    {
        return unitsSpawningStep switch
        {
            UnitsSpawningStep.LeftArchers => UnitsSpawningStep.LeftPikemans,
            UnitsSpawningStep.LeftPikemans => UnitsSpawningStep.LeftHorsemans,
            UnitsSpawningStep.LeftHorsemans => UnitsSpawningStep.RightArchers,
            UnitsSpawningStep.RightArchers => UnitsSpawningStep.RightPikemans,
            UnitsSpawningStep.RightPikemans => UnitsSpawningStep.RightHorsemans,
            UnitsSpawningStep.RightHorsemans => UnitsSpawningStep.SpawningFinished,
            UnitsSpawningStep.SpawningFinished => UnitsSpawningStep.SpawningFinished,
        };
    }

    private void DisablePlayerUnits(List<Unit> units)
    {
        foreach (var unit in units)
        {
            unit.gameObject.SetActive(false);
        }
    }

    private void SetSpawningUnitVariables(UnitsSpawningStep unitsSpawningStep)
    {
        switch (unitsSpawningStep)
        {
            case UnitsSpawningStep.LeftArchers:
                numberOfDuplicateUnits = numberOfArchers;
                player = Player.Left;
                currentlySpawnedUnit = Instantiate(archerPrefab.GetPlayerUnit(player)).GetComponent<Unit>();
                leftPlayerUnits.Add(currentlySpawnedUnit);
                break;
            case UnitsSpawningStep.LeftPikemans:
                numberOfDuplicateUnits = numberOfPikemans;
                player = Player.Left;
                currentlySpawnedUnit = Instantiate(pikemanPrefab.GetPlayerUnit(player)).GetComponent<Unit>();
                leftPlayerUnits.Add(currentlySpawnedUnit);
                break;
            case UnitsSpawningStep.LeftHorsemans:
                numberOfDuplicateUnits = numberOfHorsemans;
                player = Player.Left;
                currentlySpawnedUnit = Instantiate(horsemanPrefab.GetPlayerUnit(player)).GetComponent<Unit>();
                leftPlayerUnits.Add(currentlySpawnedUnit);
                break;
            case UnitsSpawningStep.RightArchers:
                numberOfDuplicateUnits = numberOfArchers;
                DisablePlayerUnits(leftPlayerUnits);
                player = Player.Right;
                currentlySpawnedUnit = Instantiate(archerPrefab.GetPlayerUnit(player)).GetComponent<Unit>();
                rightPlayerUnits.Add(currentlySpawnedUnit);
                break;
            case UnitsSpawningStep.RightPikemans:
                numberOfDuplicateUnits = numberOfPikemans;
                player = Player.Right;
                currentlySpawnedUnit = Instantiate(pikemanPrefab.GetPlayerUnit(player)).GetComponent<Unit>();
                rightPlayerUnits.Add(currentlySpawnedUnit);
                break;
            case UnitsSpawningStep.RightHorsemans:
                numberOfDuplicateUnits = numberOfHorsemans;
                player = Player.Right;
                currentlySpawnedUnit = Instantiate(horsemanPrefab.GetPlayerUnit(player)).GetComponent<Unit>();
                rightPlayerUnits.Add(currentlySpawnedUnit);
                break;
            case UnitsSpawningStep.SpawningFinished:
                finishedSpawning = true;
                break;
        }
        if(currentlySpawnedUnit != null)
            currentlySpawnedUnit.transform.Rotate(Vector3.up,180);
    }

    private List<HexTile> SetSpawnTiles(City playerCity)
    {
        var tiles = new List<HexTile>();
        
        var accessibleHexes = playerCity.GetHexNeighbours(false);
        accessibleHexes.Add(playerCity);
        for (int i = 1; i < maxDistanceFromCity; i++)
        {
            HexTile[] list = new HexTile[accessibleHexes.Count];
            accessibleHexes.CopyTo(list);
            foreach (var hex in list)
            {
                if(hex.tileType != TileType.Border)
                    accessibleHexes.AddRange(hex.GetHexNeighbours(false));
            }
        }
        accessibleHexes = new HashSet<HexTile>(accessibleHexes).ToList();
        foreach (var hex in accessibleHexes)
        {
            if(hex.tileType == TileType.Inland || hex.tileType == TileType.Road)
                tiles.Add(hex);
        }
        return tiles;
    }
    
}
