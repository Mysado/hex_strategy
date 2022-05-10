using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MusicSettings musicSettings;
    [SerializeField] private int cityViewDistance = 2;
    [SerializeField] private int turnToDestroyCity = 2;
    [SerializeField] private GameObject leftPlayerWin;
    [SerializeField] private GameObject rightPlayerWin;
    [SerializeField] private GameObject draw;
    [SerializeField] private GameObject restartPrompt;
    [SerializeField] private GameObject exitPrompt;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float destructionSpeed;

    public bool Initialized => initialized;
    public bool GameOver => gameOver;
    
    private bool initialized;
    private Unit currentlyActivatedUnit;
    private float startTime;
    private float journeyLength;
    private HexTile previousTile;
    private List<HexTile> inlandHexes;
    private bool resolveMovement;
    private bool resolveFights;
    private bool gameOver;
    private List<Unit> allUnits;
    private int turnCounter = 1;
    private City citySelected;
    private PlayerData leftPlayer;
    private PlayerData rightPlayer;
    private PlayerData currentPlayer;
    private List<Unit> movingUnits;
    private List<Unit> destroyedUnits;

    public void Initialize(PlayerData leftPlayer, PlayerData rightPlayer, List<HexTile> inlandHexes)
    {
        this.leftPlayer = leftPlayer;
        this.rightPlayer = rightPlayer;
        this.inlandHexes = inlandHexes;
        allUnits = new List<Unit>();
        allUnits.AddRange(leftPlayer.PlayerUnits);
        allUnits.AddRange(rightPlayer.PlayerUnits);
        DisableAllUnits();
        currentPlayer = leftPlayer;
        StartTurn();
        initialized = true;
    }
    
    private void StartTurn()
    {
        DisableAllUnits();
        currentPlayer = currentPlayer.Player switch
        {
            Player.Left => leftPlayer,
            Player.Right => rightPlayer,
        };
        ChangeUnitVisibility(true, currentPlayer);
        ChangeHexesVisibility(false);
    }
    private void FinishTurn(bool allInvisible)
    {
        ChangeUnitVisibility(false, currentPlayer);
        ChangeHexesVisibility(allInvisible);
    }
    
    void Update()
    {
        if(!initialized)
            return;
        
        if(allUnits.Count != leftPlayer.PlayerUnits.Count + rightPlayer.PlayerUnits.Count)
            RefreshUnits();
        
        CheckVictoryCondition();
        
        if(gameOver)
        {
            return;
        }
        
        if (resolveMovement)
        {
            ResolveMovement(); 
            return;
        }

        if (resolveFights)
        {
            ResolveFights();
            return;
        }

        if (Input.GetKeyUp("space"))
        {
            ResetUnitMovementStatus(currentPlayer);
            if (currentPlayer.Player == Player.Left)
            {
                FinishTurn(false);
                currentPlayer = rightPlayer;
                StartTurn();
            }
            else
            {
                FinishTurn(true);
                StartMovement();
            }
        }

        if (currentlyActivatedUnit == null)
        {
            SpawnAdditionalUnitsInCity();
        }   
        
        if (currentPlayer.PlayerUnits != null)
        {
            PlanMovement();
        }


    }

    private void StartMovement()
    {
        audioSource.clip = musicSettings.MovementMusic;
        audioSource.Play();
        resolveMovement = true;
        movingUnits = new List<Unit>();
        movingUnits.AddRange(rightPlayer.PlayerUnits);
        movingUnits.AddRange(leftPlayer.PlayerUnits);
        DisableAllUnits();
        ShowUnitsThatSeeEachOther();
    }

    private void StartFight()
    {
        audioSource.clip = musicSettings.BattleMusic;
        audioSource.Play();
        startTime = Time.time;
        resolveFights = true;
        destroyedUnits = new List<Unit>();
        for (var i = 0; i < allUnits.Count; i++)
        {
            var unit = allUnits[i];
            var attackHexes = unit.CurrentHex.GetHexNeighbours(false);
                
            if(unit.AtackType == AtackType.Ranged)
            {
                var rangedHexes = attackHexes.ToList();
                foreach (var hex in attackHexes)
                {
                    rangedHexes.AddRange(hex.GetHexNeighbours(false));
                }
                rangedHexes = new HashSet<HexTile>(rangedHexes).ToList();
                foreach (var hex in attackHexes)
                {
                    rangedHexes.Remove(hex);
                }

                rangedHexes.Remove(unit.CurrentHex);
                attackHexes = rangedHexes;
            }
            
            var enemyPlayer = unit.Player switch
            {
                Player.Left => rightPlayer,
                Player.Right => leftPlayer,
            };
            var noLongerNearCity = true;
            foreach (var hex in attackHexes)
            {
                if (hex.unitOnHex != null && hex.unitOnHex.Player != unit.Player)
                {
                    unit.UnitAnimator.Fight();
                    if (unit.CanDamage.Contains(hex.unitOnHex.UnitType))
                    {
                        var ownPlayer = unit.Player switch
                        {
                            Player.Left => leftPlayer,
                            Player.Right => rightPlayer,
                        };
                        
                        destroyedUnits.Add(hex.unitOnHex);
                        hex.unitOnHex.MarkAsDestroyed(0);
                        if (ownPlayer.EnemiesThreateningCity.Contains(hex.unitOnHex))
                        {
                            ownPlayer.EnemiesThreateningCity.Remove(hex.unitOnHex);
                            if(ownPlayer.EnemiesThreateningCity.Count == 0)
                            {
                                ownPlayer.PlayerCity.CityThreatened = false;
                                ownPlayer.PlayerCity.CityDestroyed = false;
                                ownPlayer.PlayerCity.CityThreatenedInTurn = -1;
                            }
                        }
                    }                
                }

                if (unit.Destroyed == false)
                {
                    
                    if (hex.tileType == TileType.City && hex.coordinates == enemyPlayer.PlayerCity.coordinates)
                    {
                        noLongerNearCity = false;
                        if(!enemyPlayer.EnemiesThreateningCity.Contains(unit))
                            enemyPlayer.EnemiesThreateningCity.Add(unit);
                        
                        if(enemyPlayer.PlayerCity.CityThreatened == false)
                        {
                            enemyPlayer.PlayerCity.CityThreatened = true;
                            enemyPlayer.PlayerCity.CityThreatenedInTurn = turnCounter;
                        }
                        else
                        {
                            if (enemyPlayer.PlayerCity.CityThreatenedInTurn + turnToDestroyCity < turnCounter && enemyPlayer.PlayerCity.CityThreatenedInTurn > -1)
                            {
                                enemyPlayer.PlayerCity.CityDestroyed = true;
                            }
                        }
                    }
                }
            }

            if (noLongerNearCity)
                enemyPlayer.EnemiesThreateningCity.Remove(unit);

            if (enemyPlayer.EnemiesThreateningCity.Count == 0)
            {
                enemyPlayer.PlayerCity.CityThreatened = false;
                enemyPlayer.PlayerCity.CityDestroyed = false;
                enemyPlayer.PlayerCity.CityThreatenedInTurn = -1;
            }
                
        }
        turnCounter++;
    }

    private void ResolveFights()
    {
        for (var i = 0; i < destroyedUnits.Count; i++)
        {
            var unit = destroyedUnits[i];
            var distCovered = (Time.time - startTime) * destructionSpeed;

            var fractionOfJourney = distCovered / 20;

            // Set our position as a fraction of the distance between the markers.
            unit.MarkAsDestroyed(fractionOfJourney);
            if (fractionOfJourney + 0.01f >= 1)
            {
                destroyedUnits.RemoveAt(i);
                rightPlayer.PlayerUnits.Remove(unit);
                leftPlayer.PlayerUnits.Remove(unit);
                allUnits.Remove(unit);
                unit.UnitAnimator.SetDeath();
                //Destroy(unit.gameObject);
                i--;
            }
        }
            
        if (destroyedUnits.Count == 0)
        {
            resolveFights = false;
            currentPlayer = leftPlayer;
            Debug.Log("fight over");
            audioSource.clip = musicSettings.DefaultMusic;
            audioSource.Play();
            StartTurn();
        }
    }

    private void ResolveMovement()
    {
        for (var i = 0; i < movingUnits.Count; i++)
        {
            var unit = movingUnits[i];
            unit.UnitAnimator.Movement(true);
            if (unit.DestinationHex == null || unit.CurrentHex == unit.DestinationHex)
            {
                movingUnits.RemoveAt(i);
                i--;
                continue;
            }

            var startingPosition = new Vector3(unit.CurrentHex.transform.position.x, unit.transform.position.y,
                unit.CurrentHex.transform.position.z);
            var endPosition = new Vector3(unit.DestinationHex.transform.position.x, unit.transform.position.y,
                unit.DestinationHex.transform.position.z);
            
            Vector3 targetDirection = endPosition - unit.transform.position;
            float singleStep = 2 * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(unit.UnitRenderer.forward, targetDirection, singleStep, 0.0f);
            float angle = 0.5f;

            if (Vector3.Angle(unit.UnitRenderer.forward, endPosition - unit.UnitRenderer.position) < angle)
            {
                journeyLength = Vector3.Distance(startingPosition, endPosition);
                var distCovered = (Time.time - startTime) * movementSpeed;

                var fractionOfJourney = distCovered / journeyLength;
                if (fractionOfJourney + 0.01f >= 1)
                {
                    unit.CurrentHex.unitOnHex = null;
                    unit.CurrentHex = unit.DestinationHex;
                    unit.DestinationHex = null;
                    unit.transform.position = endPosition;
                    unit.UnitAnimator.Movement(false);
                    movingUnits.RemoveAt(i);
                    i--;
                }
                else
                    unit.transform.position = Vector3.Lerp(startingPosition, endPosition, fractionOfJourney);
            }
            else
            {
                unit.UnitRenderer.rotation = Quaternion.LookRotation(newDirection);
                startTime = Time.time;
            }
        }
        if (movingUnits.Count == 0)
        {
            resolveMovement = false;
            StartFight();
            Debug.Log("movement over");
        }   
    }

    private void PlanMovement()
    {
        var notActivatedUnits = 0;
        for (var i = 0; i < currentPlayer.PlayerUnits.Count; i++)
        {
            if (currentPlayer.PlayerUnits[i].GetComponent<Unit>().MovedThisTurn == false)
                notActivatedUnits++;
        }

        if (notActivatedUnits == 0)
        {
            ResetUnitMovementStatus(currentPlayer);
            if (currentPlayer.Player == Player.Left)
            {
                FinishTurn(false);
                currentPlayer = rightPlayer;
                StartTurn();
            }
            else
            {
                FinishTurn(true);
                StartMovement();
            }
        }


        if (currentlyActivatedUnit == null)
        {
            if (Input.GetMouseButtonUp(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var layerMask = LayerMask.GetMask("Unit");
                if (Physics.Raycast(ray, out var hit, 500, layerMask))
                {
                    var hitTarget = hit.collider.gameObject.GetComponent<Unit>();
                    if (hitTarget != null && hitTarget.Player == currentPlayer.Player &&
                        hitTarget.MovedThisTurn == false)
                    {
                        currentlyActivatedUnit = hitTarget;
                        currentlyActivatedUnit.Select();
                    }
                }
            }
        }
        else
        {
            var accessibleHexes = currentlyActivatedUnit.CurrentHex.GetHexNeighbours(true);

            for (int i = 1; i < currentlyActivatedUnit.Movement; i++)
            {
                HexTile[] list = new HexTile[accessibleHexes.Count];
                accessibleHexes.CopyTo(list);
                foreach (var hex in list)
                {
                    accessibleHexes.AddRange(hex.GetHexNeighbours(true));
                }
            }

            accessibleHexes = new HashSet<HexTile>(accessibleHexes).ToList();
            foreach (var hex in accessibleHexes)
            {
                hex.HighlightHex();
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var layerMask = LayerMask.GetMask("Ground");
            if (Physics.Raycast(ray, out var hit, 500, layerMask))
            {
                var hitTarget = hit.collider.gameObject.GetComponent<HexTile>();

                if (previousTile == null)
                    previousTile = hitTarget;
                else if (previousTile != hitTarget)
                {
                    previousTile.DisableHexHighlight();
                    previousTile = hitTarget;
                }

                if (hitTarget.IsTileAccessible() && accessibleHexes.Contains(hitTarget))
                    hitTarget.SelectHex();
                else
                    hitTarget.HighlightAsInaccessible();


                if (Input.GetMouseButtonUp(0) && accessibleHexes.Contains(hitTarget))
                {
                    currentlyActivatedUnit.DestinationHex = hitTarget;
                    currentlyActivatedUnit.MarkAsMoved();
                    hitTarget.unitOnHex = currentlyActivatedUnit;
                    currentlyActivatedUnit = null;
                    hitTarget.DisableHexHighlight();
                    foreach (var hex in accessibleHexes)
                    {
                        hex.DisableHexHighlight();
                    }

                    hitTarget.ChangeHexVisibility(false);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    currentlyActivatedUnit.DisableHighlight();
                    currentlyActivatedUnit = null;
                    hitTarget.DisableHexHighlight();
                    foreach (var hex in accessibleHexes)
                    {
                        hex.DisableHexHighlight();
                    }
                }
            }

        }
    }

    private void ChangeUnitVisibility(bool visibility, PlayerData player)
    {
        for (var i = 0; i < player.PlayerUnits.Count; i++)
        {
            player.PlayerUnits[i].gameObject.SetActive(visibility);
        }
    }

    private void ResetUnitMovementStatus(PlayerData player)
    {
        for (var i = 0; i < player.PlayerUnits.Count; i++)
        {
            player.PlayerUnits[i].DisableHighlight();
            player.PlayerUnits[i].MovedThisTurn = false;
        }
    }

    private void ChangeHexesVisibility(bool allInvisible)
    {
        foreach (var hex in inlandHexes)
        {
            hex.ChangeHexVisibility(false);
        }
        if(!allInvisible)
            ChangeUnitsHexVisibility(currentPlayer, true);
    }

    private void ChangeUnitsHexVisibility(PlayerData player,bool visible)
    {
        foreach (var unit in player.PlayerUnits)
        {
            if(unit.CurrentHex == null)
                continue;
            var accessibleHexes = unit.CurrentHex.GetHexNeighbours(false);
            accessibleHexes.Add(unit.CurrentHex);
            for (int i = 1; i < unit.ViewDistance; i++)
            {
                HexTile[] list = new HexTile[accessibleHexes.Count];
                accessibleHexes.CopyTo(list);
                foreach (var hex in list)
                {
                    if(hex.tileType != TileType.Mountain && hex.tileType != TileType.Border)
                        accessibleHexes.AddRange(hex.GetHexNeighbours(false));
                }
            }
            accessibleHexes = new HashSet<HexTile>(accessibleHexes).ToList();
            foreach (var hex in accessibleHexes)
            {
                if(hex.tileType == TileType.Inland || hex.tileType == TileType.Road)
                    hex.ChangeHexVisibility(visible);
                if(hex.unitOnHex != null && hex.unitOnHex.Player != unit.Player && hex.unitOnHex.DestinationHex != hex)
                    hex.unitOnHex.gameObject.SetActive(true);
            }
        }
        
        var cityViewHexes = player.PlayerCity.GetHexNeighbours(false);
        for (int i = 1; i < cityViewDistance; i++)
        {
            HexTile[] list = new HexTile[cityViewHexes.Count];
            cityViewHexes.CopyTo(list);
            foreach (var hex in list)
            {
                if(hex.tileType != TileType.Mountain && hex.tileType != TileType.Border)
                    cityViewHexes.AddRange(hex.GetHexNeighbours(false));
            }
        }
        cityViewHexes = new HashSet<HexTile>(cityViewHexes).ToList();
        foreach (var hex in cityViewHexes)
        {
            if(hex.tileType == TileType.Inland || hex.tileType == TileType.Road)
                hex.ChangeHexVisibility(true);
            if(hex.unitOnHex != null && hex.unitOnHex.Player != player.Player)
                hex.unitOnHex.gameObject.SetActive(true);
        }
        
    }

    private void ShowUnitsThatSeeEachOther()
    {
        foreach (var unit in allUnits)
        {
            if (unit.DestinationHex == null)
            {
                unit.DestinationHex = unit.CurrentHex;
            }            
            var accessibleHexes = unit.DestinationHex.GetHexNeighbours(false);
            accessibleHexes.Add(unit.DestinationHex);
            for (int i = 1; i < unit.ViewDistance; i++)
            {
                HexTile[] list = new HexTile[accessibleHexes.Count];
                accessibleHexes.CopyTo(list);
                foreach (var hex in list)
                {
                    if(hex.tileType != TileType.Mountain && hex.tileType != TileType.Border)
                        accessibleHexes.AddRange(hex.GetHexNeighbours(false));
                }
            }
            
            accessibleHexes = new HashSet<HexTile>(accessibleHexes).ToList();
            foreach (var hex in accessibleHexes)
            {
                if (hex.unitOnHex != null && unit != hex.unitOnHex && unit.Player != hex.unitOnHex.Player)
                {
                    if (hex.unitOnHex.DestinationHex == null)
                    {
                        hex.unitOnHex.DestinationHex = hex.unitOnHex.CurrentHex;
                    }
                    unit.gameObject.SetActive(true);
                    unit.DestinationHex.ChangeHexVisibility(true);
                    hex.unitOnHex.gameObject.SetActive(true);
                    hex.unitOnHex.DestinationHex.ChangeHexVisibility(true);

                }
            }
        }
    }

    private void CheckVictoryCondition()
    {
        if(!gameOver)
        {
            if ((rightPlayer.PlayerUnits.Count == 0 || rightPlayer.PlayerCity.CityDestroyed) &&
                (leftPlayer.PlayerUnits.Count == 0 || leftPlayer.PlayerCity.CityDestroyed))
            {
                draw.SetActive(true);
                gameOver = true;
            }
            else if (rightPlayer.PlayerUnits.Count == 0 || rightPlayer.PlayerCity.CityDestroyed)
            {
                leftPlayerWin.SetActive(true);
                gameOver = true;
            }
            else if (leftPlayer.PlayerUnits.Count == 0 || leftPlayer.PlayerCity.CityDestroyed)
            {
                rightPlayerWin.SetActive(true);
                gameOver = true;
            }

            if (gameOver)
            {
                audioSource.clip = musicSettings.VictoryMusic;
                audioSource.Play();
                restartPrompt.SetActive(true);
                exitPrompt.SetActive(true);
            }
        }
    }

    private void RefreshUnits()
    {
        if(currentPlayer.PlayerCity.SpawnedUnit)
        {
            allUnits = new List<Unit>();
            allUnits.AddRange(leftPlayer.PlayerUnits);
            allUnits.AddRange(rightPlayer.PlayerUnits);
            ChangeUnitVisibility(true, currentPlayer);
            ChangeHexesVisibility(false);
        }
    }

    private void DisableAllUnits()
    {
        foreach (var unit in allUnits)
        {
            unit.gameObject.SetActive(false);
        }
    }

    private void SpawnAdditionalUnitsInCity()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var layerMask = LayerMask.GetMask("Ground");
            if (Physics.Raycast(ray, out var hit, 500, layerMask))
            {
                var currentPlayerCity = currentPlayer.Player switch
                {
                    Player.Left => leftPlayer.PlayerCity,
                    Player.Right => rightPlayer.PlayerCity,
                };
                var hitHex = hit.collider.gameObject.GetComponent<City>();

                if (citySelected == null && hitHex != null && hitHex == currentPlayerCity && !hitHex.SpawnedUnit)
                {
                    hitHex.SelectCity();
                    citySelected = hitHex;
                }
                else if (citySelected != null && hitHex == null)
                {
                    citySelected.DeselectCity();
                    citySelected = null;
                }
            }
        }
    }
}
