using System.Collections.Generic;
using UnityEngine;

public class DirectionInfo
{
    public HexDirection hexDirection;
    public bool occupied = false;
    public bool availableForOccupation = true;
    public HexTile HexTile;
}

public enum TileType
{
    Border,
    Ocean,
    Inland,
    City,
    Mountain,
    Lake,
    Road
}

public class HexTile : MonoBehaviour
{
    [SerializeField] private HexColorSettings hexColorSettings;
    [SerializeField] private MeshRenderer highlightMeshRenderer;
    [SerializeField] private MeshRenderer tileMesh;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material outsideOfVisionMaterial;
    
    public List<DirectionInfo> DirectionInfos;
    public bool[] occupiedDirections;

    public Vector2Int coordinates;
    public TileType tileType;
    public Unit unitOnHex;
    
    public void Initialize(Vector2Int coordinates, TileType tileType)
    {
        DirectionInfos = new List<DirectionInfo>();
        for (var i = 0; i < HexDirections.HexDirectionsAmount(); i++)
        {
            var directionInfo = new DirectionInfo();
            directionInfo.hexDirection = (HexDirection) i;
            DirectionInfos.Add(directionInfo);
        }
        this.coordinates = coordinates;
        occupiedDirections = new bool [6];
        this.tileType = tileType;
    }

    public void UpdateDirection(HexDirection hexDirection, HexTile hexTile, bool occupied = true,
        bool availableForOccupation = false)

    {
        for (var i = 0; i < DirectionInfos.Count; i++)
        {
            if (DirectionInfos[i].hexDirection != hexDirection) continue;
            occupiedDirections[i] = occupied;
            DirectionInfos[i].HexTile = hexTile;
            DirectionInfos[i].occupied = occupied;
            DirectionInfos[i].availableForOccupation = availableForOccupation;
        }
    }
    public bool IsDirectionOccupied(HexDirection hexDirection)
    {
        for (var i = 0; i < DirectionInfos.Count; i++)
        {
            if (DirectionInfos[i].hexDirection == hexDirection)
                return DirectionInfos[i].occupied;
            
        }

        return false;
    }
    
    public bool IsDirectionAvailable(HexDirection hexDirection)
    {
        for (var i = 0; i < DirectionInfos.Count; i++)
        {
            if (DirectionInfos[i].hexDirection == hexDirection)
                return DirectionInfos[i].availableForOccupation;
        }

        return false;
    }

    public int NumberOfOccupiedDirections()
    {
        var amount = 0;
        for (var i = 0; i < DirectionInfos.Count; i++)
        {
            if (DirectionInfos[i].occupied)
                amount++;
        }

        return amount;
    }
    
    public void HighlightAsInaccessible()
    {
        highlightMeshRenderer.material.color = hexColorSettings.InaccessibleColor;
    }

    public void SelectHex()
    {
        highlightMeshRenderer.material.color = hexColorSettings.SelectedColor;
    }
    
    public void HighlightHex()
    {
        highlightMeshRenderer.material.color = Color.Lerp(hexColorSettings.HighlightColor1, hexColorSettings.HighlightColor2, Mathf.PingPong(Time.time * hexColorSettings.FlashingSpeed, 1));
    }

    public void DisableHexHighlight()
    {
        highlightMeshRenderer.material.color = hexColorSettings.InactiveColor;
    }

    public bool IsTileAccessible()
    {
        return unitOnHex == null && tileType is TileType.Inland or TileType.Road;
    }
    
    public List<HexTile> GetHexNeighbours(bool isHexAccessible)
    {
        var neighbourHexes = new List<HexTile>();
        
        foreach (var hexNeighbour in DirectionInfos)
        {
            if (isHexAccessible)
            {
                if(hexNeighbour.HexTile.IsTileAccessible())
                    neighbourHexes.Add(hexNeighbour.HexTile);
            }
            else
                neighbourHexes.Add(hexNeighbour.HexTile);
        }
        
        return neighbourHexes;
    }

    public void ChangeHexVisibility(bool visibility)
    {
        if (visibility)
            tileMesh.material = defaultMaterial;
        else if (outsideOfVisionMaterial != null)
        {
            tileMesh.material = outsideOfVisionMaterial;
        }
    }
}
