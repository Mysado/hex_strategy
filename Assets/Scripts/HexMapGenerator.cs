using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
    [SerializeField] private int gridSize;
    [SerializeField] private int borderOffset;
    [SerializeField] private int obstacleGenerationOffset;
    [SerializeField] private HexTilePrefabSelector oceanHexPrefab;
    [SerializeField] private HexTilePrefabSelector inlandHexPrefab;
    [SerializeField] private HexTilePrefabSelector cityHexPrefab;
    [SerializeField] private HexTilePrefabSelector mountainHexPrefab;
    [SerializeField] private HexTilePrefabSelector lakeHexPrefab;

    private HexTile[,] populationMatrix;

    private Vector2Int maxMinX;
    private Vector2Int maxMinY;
    private int doubledGridSize;

    private City leftCity;
    private City rightCity;

    private int mapSize;

    private int mapSizeOffset;

    private Vector3 hexPosition;
    private Vector2Int coordinates;

    private float r;
    private float R;

    public City LeftCity => leftCity;
    public City RightCity => rightCity;

    public List<HexTile> inlandHexes;
    // Start is called before the first frame update
    public void GenerateMap()
    {
        r = HexDirections.Hexr;
        R = HexDirections.HexR;
        doubledGridSize = gridSize * 2;
        mapSizeOffset = (int) (gridSize * 0.25f);
        maxMinX = new Vector2Int(doubledGridSize,doubledGridSize);
        maxMinY = new Vector2Int(doubledGridSize,doubledGridSize);
        inlandHexes = new List<HexTile>();
        BorderGeneration();
        OceanGeneration();
        CityGeneration();
        RoadGeneration();
        LakeGeneration();
        MountainGeneration();
        InlandGeneration();
    }

    private void BorderGeneration()
    {
        hexPosition = new Vector3();
        coordinates = new Vector2Int(0, 0);
        populationMatrix = new HexTile[doubledGridSize * 2+1, doubledGridSize * 2*1];
        
        var newHex = InstantiateAndInitialize(oceanHexPrefab.GetRandomTile(),TileType.Border);
        var buildDirection = Direction.Right;
        
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < gridSize; j++)
            {
                var hexDirection = HexDirections.RandomizeHexDirectionBasedOnDirection(buildDirection);
                
                if(!newHex.IsDirectionAvailable(hexDirection) && newHex.IsDirectionOccupied(hexDirection))
                    continue;
                
                if(buildDirection == Direction.Up)
                {
                    if (coordinates.y >= -1)
                        break;
                    if (coordinates.x >= 0)
                        hexDirection = HexDirection.UpLeft;
                }

                ChangeHexPosition(hexDirection);
                ChangeHexCoordinates(hexDirection);

                newHex = InstantiateAndInitialize(oceanHexPrefab.GetRandomTile(),TileType.Border);
                
                UpdateMinMax();
                UpdateHexNeighbours(newHex);
            }

            buildDirection = HexDirections.GetNextDirection(buildDirection);
        }
        for (var i = 0; i < gridSize; i++)
        {
            if(coordinates.y == -1 && coordinates.x == 0)
                break;
            
            if (coordinates.x == -1)
                if(coordinates.y is 0 or -1)
                    break;

            var hexDirection = coordinates.x switch
            {
                0 when coordinates.y < -1 => HexDirection.Up,
                < 0 when coordinates.y <= -1 => HexDirection.UpRight,
                < 0 when coordinates.y >= -1 => HexDirection.DownRight,
                _ => HexDirection.Up
            };

            ChangeHexPosition(hexDirection);
            ChangeHexCoordinates(hexDirection);
                
            newHex = InstantiateAndInitialize(oceanHexPrefab.GetRandomTile(),TileType.Border);
            UpdateHexNeighbours(newHex);
            UpdateMinMax();
        }

        mapSize = (maxMinX.x - maxMinX.y) * (maxMinY.x - maxMinY.y);
        
        maxMinY.x += borderOffset;
        maxMinY.y -= borderOffset;
        
        maxMinX.x += borderOffset;
        maxMinX.y -= borderOffset;
    }

    private void OceanGeneration()
    {
        hexPosition = new Vector3();
        coordinates = new Vector2Int();

        for (var i = maxMinY.y; i < maxMinY.x; i++)
        {
            coordinates.x = -doubledGridSize + maxMinX.y;
            coordinates.y = doubledGridSize - i;

            hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);

            for (var j = maxMinX.y; j < maxMinX.x; j++)
            {
                var hexDirection = j % 2 == 0 ? HexDirection.UpRight : HexDirection.DownRight;
                if(populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                {
                    var hex = InstantiateAndInitialize(oceanHexPrefab.GetRandomTile(),TileType.Ocean);
                    UpdateHexNeighbours(hex);
                }
                else if(populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize].tileType == TileType.Border)
                    break;
                
                ChangeHexPosition(hexDirection);
                ChangeHexCoordinates(hexDirection);
            }
        }
        
        for (var i = maxMinY.y; i < maxMinY.x; i++)
        {
            coordinates.x = -doubledGridSize + maxMinX.x;
            coordinates.y = doubledGridSize - i;
            
            hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);
            
            for (var j = maxMinX.x; j > maxMinX.y; j--)
            {
                var hexDirection = j % 2 == 0 ? HexDirection.UpLeft : HexDirection.DownLeft;
                if(populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                {
                    var hex = InstantiateAndInitialize(oceanHexPrefab.GetRandomTile(),TileType.Ocean);
                    UpdateHexNeighbours(hex);
                }
                else if(populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize].tileType == TileType.Border)
                    break;
                
                ChangeHexCoordinates(hexDirection);
                ChangeHexPosition(hexDirection);
            }
        }
        
        for (var i = maxMinX.y; i < maxMinX.x; i++)
        {
            coordinates.x = -doubledGridSize + i;
            coordinates.y = doubledGridSize - maxMinY.y;
            
            hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);

            for (var j = maxMinY.y; j < maxMinY.x; j++)
            {
                if(populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                {
                    var hex = InstantiateAndInitialize(oceanHexPrefab.GetRandomTile(),TileType.Ocean);
                    UpdateHexNeighbours(hex);
                }
                else if(populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize].tileType == TileType.Border)
                    break;
                
                ChangeHexCoordinates(HexDirection.Down);
                ChangeHexPosition(HexDirection.Down);
            }
        }

        for (var i = maxMinX.y; i < maxMinX.x; i++)
        {
            coordinates.x = -doubledGridSize + i;
            coordinates.y = doubledGridSize - maxMinY.x;
            
            hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);
            
            for (var j = maxMinY.x; j > maxMinY.y; j--)
            {
                if(populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                {
                    var hex = InstantiateAndInitialize(oceanHexPrefab.GetRandomTile(),TileType.Ocean);
                    UpdateHexNeighbours(hex);
                }
                else if(populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize].tileType == TileType.Border)
                    break;
                
                ChangeHexCoordinates(HexDirection.Up);
                ChangeHexPosition(HexDirection.Up);
            }
        }
         
    }

    private void InlandGeneration()
    {
        for (var i = maxMinY.y; i < maxMinY.x; i++)
        {
            coordinates.x = -doubledGridSize + maxMinX.y;
            coordinates.y = doubledGridSize - i;
            
            hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);
            
            for (var j = maxMinX.y; j < maxMinX.x; j++)
            {
                var hexDirection = j % 2 == 0 ? HexDirection.UpRight : HexDirection.DownRight;
                if(populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                {
                    var hex = InstantiateAndInitialize(inlandHexPrefab.GetRandomTile(),TileType.Inland);
                    UpdateHexNeighbours(hex);
                    inlandHexes.Add(hex);
                }

                ChangeHexPosition(hexDirection);
                ChangeHexCoordinates(hexDirection);
            }
        }
    }

    private void CityGeneration()
    {
        maxMinY.x -= borderOffset;
        maxMinY.y += borderOffset;
        
        maxMinX.x -= borderOffset;
        maxMinX.y += borderOffset;
        coordinates.x = -doubledGridSize + maxMinX.y + mapSizeOffset;
        var cityBuild = false;
        for (var i = maxMinY.y; i < maxMinY.x; i++)
        {
            coordinates.y = doubledGridSize - i;

            if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null)
                continue;

            for (var j = mapSizeOffset; j >= 0; j--)
            {
                coordinates.y = doubledGridSize - i - j;
                if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                {
                    coordinates.y = doubledGridSize - i - (int) (j * 0.5f);
                    if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                    {
                        hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);

                        leftCity = InstantiateAndInitialize(cityHexPrefab.GetRandomTile(),TileType.City) as City;
                        UpdateHexNeighbours(leftCity);
                        cityBuild = true;
                        break;
                    }
                }
            }

            if (!cityBuild)
            {
                coordinates.x += 1;
                i = maxMinY.y;
            }
            else
                break;
        }

        cityBuild = false;
        
        coordinates.x = -doubledGridSize + maxMinX.x - mapSizeOffset;
        for (var i = maxMinY.x; i > maxMinY.y; i--)
        {
            coordinates.y = doubledGridSize - i;

            if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null) 
                continue;

            for (var j = mapSizeOffset; j >= 0; j--)
            {
                coordinates.y = doubledGridSize - i + j;
                if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                {
                    coordinates.y = doubledGridSize - i + (int)(j * 0.5f);
                    if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                    {
                        hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);

                        rightCity = InstantiateAndInitialize(cityHexPrefab.GetRandomTile(),TileType.City) as City;
                        UpdateHexNeighbours(rightCity);
                        cityBuild = true;
                        break;
                    }
                }
            }
            if (!cityBuild)
            {
                coordinates.x -= 1;
                i = maxMinY.x;
            }            
            else
                break;
        }

    }

    private void RoadGeneration()
    {
        coordinates = leftCity.coordinates;
        hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);
        
        for (var i = 0; i < doubledGridSize; i++)
        {
            if (coordinates.x < rightCity.coordinates.x)
            {
                if (coordinates.y > rightCity.coordinates.y + 1)
                {
                    var hexDirection = HexDirection.DownRight;
                    ChangeHexCoordinates(hexDirection);
                    if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null)
                    {
                        ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                        hexDirection = HexDirection.UpRight;
                        ChangeHexCoordinates(hexDirection);
                        if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null)
                        {
                            ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                            hexDirection = HexDirection.Down;
                            ChangeHexCoordinates(hexDirection);
                            if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null)
                            {
                                ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                                hexDirection = HexDirection.Up;
                                ChangeHexCoordinates(hexDirection);
                            }
                        }
                    }
                    ChangeHexPosition(hexDirection);

                    var newHex = InstantiateAndInitialize(inlandHexPrefab.GetRandomTile(),TileType.Road);
                    UpdateHexNeighbours(newHex);
                    inlandHexes.Add(newHex);
                }
                else if (coordinates.y > rightCity.coordinates.y - 1)
                {
                    var hexDirection = HexDirection.UpRight;
                    ChangeHexCoordinates(hexDirection);
                    if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null && populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize].tileType != TileType.City)
                    {
                        ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                        hexDirection = HexDirection.DownRight;
                        ChangeHexCoordinates(hexDirection);
                        if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null && populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize].tileType != TileType.City)
                        {
                            ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                            hexDirection = HexDirection.Up;
                            ChangeHexCoordinates(hexDirection);
                            if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null && populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize].tileType != TileType.City)
                            {
                                ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                                hexDirection = HexDirection.Down;
                                ChangeHexCoordinates(hexDirection);
                            }
                        }
                    }
                    ChangeHexPosition(hexDirection);
                
                    var newHex = InstantiateAndInitialize(inlandHexPrefab.GetRandomTile(),TileType.Road);
                    UpdateHexNeighbours(newHex);
                    inlandHexes.Add(newHex);
                }
                else if (coordinates.y < rightCity.coordinates.y)
                {
                    var hexDirection = HexDirection.UpRight;
                    ChangeHexCoordinates(hexDirection);
                    if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null)
                    {
                        ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                        hexDirection = HexDirection.DownRight;
                        ChangeHexCoordinates(hexDirection);
                        if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null)
                        {
                            ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                            hexDirection = HexDirection.Up;
                            ChangeHexCoordinates(hexDirection);
                            if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null)
                            {
                                ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                                hexDirection = HexDirection.Down;
                                ChangeHexCoordinates(hexDirection);
                            }
                        }
                    }
                    ChangeHexPosition(hexDirection);
                
                    var newHex = InstantiateAndInitialize(inlandHexPrefab.GetRandomTile(),TileType.Road);
                    UpdateHexNeighbours(newHex);
                    inlandHexes.Add(newHex);
                }
                
            } 
            else if (coordinates.x == rightCity.coordinates.x && coordinates.y > rightCity.coordinates.y + 1)
            {
                var hexDirection = HexDirection.Down;
                ChangeHexPosition(hexDirection);
                ChangeHexCoordinates(hexDirection);
                
                var newHex = InstantiateAndInitialize(inlandHexPrefab.GetRandomTile(),TileType.Road);
                UpdateHexNeighbours(newHex);
                inlandHexes.Add(newHex);
            }
            else if (coordinates.x == rightCity.coordinates.x && coordinates.y < rightCity.coordinates.y - 1)
            {
                var hexDirection = HexDirection.Up;
                ChangeHexPosition(hexDirection);
                ChangeHexCoordinates(hexDirection);
                
                var newHex = InstantiateAndInitialize(inlandHexPrefab.GetRandomTile(),TileType.Road);
                UpdateHexNeighbours(newHex);
                inlandHexes.Add(newHex);
            }
            else
                break;
            
        }
    }

    private void LakeGeneration()
    {
        var forestsToGenerate = mapSize / obstacleGenerationOffset;
        var foundRandomPosition = false;
        

        for (var i = 0; i < forestsToGenerate; i++)
        {
            while (!foundRandomPosition)
            {
                coordinates = new Vector2Int( Random.Range(maxMinX.y + 5, maxMinX.x - 5) - doubledGridSize,doubledGridSize - Random.Range(maxMinY.y + 5, maxMinY.x - 5));
                if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                {
                    foundRandomPosition = true;
                    hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);
                    
                    var newHex = InstantiateAndInitialize(lakeHexPrefab.GetRandomTile(),TileType.Lake);
                    UpdateHexNeighbours(newHex);
                    var randomizedDirections = new List<HexDirection>();
                    var randomDirections = new List<int> {0, 1, 2, 3, 4, 5};
                    for (var j = 0; j < (mapSizeOffset - 1 > 6 ? 5 : mapSizeOffset - 1); j++)
                    {
                        var random = Random.Range(0, randomDirections.Count);
                        randomizedDirections.Add((HexDirection) randomDirections[random]);
                        randomDirections.RemoveAt(random);
                    }

                    foreach (var hexDirection in randomizedDirections)
                    {
                        ChangeHexCoordinates(hexDirection);
                        if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null)
                            ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                        else
                        {
                            ChangeHexPosition(hexDirection);
                
                            newHex = InstantiateAndInitialize(lakeHexPrefab.GetRandomTile(),TileType.Lake);
                            UpdateHexNeighbours(newHex);
                        }
                    }
                }
            }
            foundRandomPosition = false;
        }
    }

    private void MountainGeneration()
    {
        var mountainsToGenerate = mapSize / obstacleGenerationOffset;
        var foundRandomPosition = false;

        for (var i = 0; i < mountainsToGenerate; i++)
        {
            while (!foundRandomPosition)
            {
                coordinates = new Vector2Int( Random.Range(maxMinX.y + 5, maxMinX.x - 2) - doubledGridSize,doubledGridSize - Random.Range(maxMinY.y + 2, maxMinY.x - 2));
                if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] == null)
                {
                    foundRandomPosition = true;
                    hexPosition = HexDirections.hexPositionBasedOnCoordinates(coordinates, 0);
                    
                    var newHex = InstantiateAndInitialize(mountainHexPrefab.GetRandomTile(),TileType.Mountain);
                    UpdateHexNeighbours(newHex);

                    for (var j = 0; j < mapSizeOffset - 1; j++)
                    {
                        var randomDirections = new List<HexDirection> {HexDirection.Up,HexDirection.UpLeft,HexDirection.UpRight};
                        for (var k = 0; k < 3; k++)
                        {
                            var hexDirection = randomDirections[Random.Range(0,randomDirections.Count)];
                            ChangeHexCoordinates(hexDirection);
                            if (populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] != null)
                            {
                                ChangeHexCoordinates(HexDirections.GetOppositeHexDirection(hexDirection));
                                randomDirections.Remove(hexDirection);
                            }
                            else
                            {
                                ChangeHexPosition(hexDirection);
                
                                newHex = InstantiateAndInitialize(mountainHexPrefab.GetRandomTile(),TileType.Mountain);
                                UpdateHexNeighbours(newHex);
                                break;
                            }
                        }
                    }
                }
            }
            foundRandomPosition = false;
        }
    }

    private HexTile InstantiateAndInitialize(GameObject hexPrefab, TileType tileType)
    {
        var hex = Instantiate(hexPrefab, new Vector3(hexPosition.x, hexPosition.y, hexPosition.z ), Quaternion.identity, gameObject.transform).GetComponentInChildren<HexTile>();
        hex.Initialize(coordinates, tileType);
        populationMatrix[coordinates.x + doubledGridSize, coordinates.y + doubledGridSize] = hex;
        return hex;
    }

    private void UpdateMinMax()
    {
        if (maxMinY.x < doubledGridSize - coordinates.y)
            maxMinY.x = doubledGridSize - coordinates.y;
                
        if (maxMinY.y > doubledGridSize - coordinates.y)
            maxMinY.y = doubledGridSize - coordinates.y;
                
        if (maxMinX.x < doubledGridSize + coordinates.x)
            maxMinX.x = doubledGridSize + coordinates.x;
                
        if (maxMinX.y > doubledGridSize + coordinates.x)
            maxMinX.y = doubledGridSize + coordinates.x;
    }

    private void ChangeHexPosition(HexDirection hexDirection)
    {
        switch (hexDirection)
        {
            case HexDirection.Up:
                hexPosition.z += r * 2;
                break;
            case HexDirection.Down:
                hexPosition.z -= r * 2;
                break;
            case HexDirection.DownRight:
                hexPosition.x += R;
                hexPosition.z -= r;
                break;
            case HexDirection.DownLeft:
                hexPosition.x -= R;
                hexPosition.z -= r;
                break;
            case HexDirection.UpRight:
                hexPosition.x += R;
                hexPosition.z += r;
                break;
            case HexDirection.UpLeft:
                hexPosition.x -= R;
                hexPosition.z += r;
                break;
        }
    }

    private void ChangeHexCoordinates(HexDirection hexDirection)
    {
        var isCoordinateXOdd = Mathf.Abs(coordinates.x) % 2 == 1;
        switch (hexDirection)
        {
            case HexDirection.Up:
                coordinates.y++;
                break;
            case HexDirection.Down:
                coordinates.y--;
                break;
            case HexDirection.DownRight:
                coordinates.x++;
                if (!isCoordinateXOdd) coordinates.y--;
                break;
            case HexDirection.DownLeft:
                coordinates.x--;
                if (!isCoordinateXOdd) coordinates.y--;
                break;
            case HexDirection.UpRight:
                coordinates.x++;
                if (isCoordinateXOdd) coordinates.y++;
                break;
            case HexDirection.UpLeft:
                coordinates.x--;
                if (isCoordinateXOdd) coordinates.y++;
                break;
        }
    }

    private void UpdateHexNeighbours(HexTile hex)
    {
        for (int k = 0; k < HexDirections.HexDirectionsAmount(); k++)
        {
            var direction = (HexDirection) k;
            if(hex.IsDirectionOccupied( direction ))
                continue;
            var arrayOffset = HexDirections.GetArrayDirectionOffset(direction, coordinates.x % 2 == 0);
            var nextHex = populationMatrix[doubledGridSize + coordinates.x + arrayOffset.x,
                doubledGridSize + coordinates.y + arrayOffset.y];
            if (nextHex != null)
            {
                hex.UpdateDirection(direction, nextHex);
                nextHex.UpdateDirection(HexDirections.GetOppositeHexDirection(direction),hex);
            }
        }
    }
}
