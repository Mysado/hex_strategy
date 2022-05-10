using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum HexDirection
{
    Up,
    UpRight,
    DownRight,
    Down,
    DownLeft,
    UpLeft
}

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}

public class HexDirections : MonoBehaviour
{
    private static float hexDiagonal = 1;

    private static float hexSideLength = hexDiagonal * 0.5f;

    private static float hexr = hexSideLength * Mathf.Sqrt(3) * 0.5f;
    private static float hexR = hexDiagonal * 0.75f;

    public static float HexDiagonal => hexDiagonal;
    public static float HexSideLength => hexSideLength;
    public static float Hexr => hexr;
    public static float HexR => hexR;



    public static HexDirection RandomizeHexDirectionBasedOnDirection(Direction direction)
    {
        List<HexDirection> directions;
        switch (direction)
        {
            case Direction.Up:
                directions = new List<HexDirection>{ HexDirection.Up, HexDirection.UpLeft, HexDirection.UpRight};
                return directions.ElementAt(Random.Range(0, directions.Count));
            case Direction.Right:
                directions = new List<HexDirection>{ HexDirection.DownRight, HexDirection.UpRight};
                return directions.ElementAt(Random.Range(0, directions.Count));
            case Direction.Down:
                directions = new List<HexDirection>{ HexDirection.Down, HexDirection.DownRight, HexDirection.DownLeft};
                return directions.ElementAt(Random.Range(0, directions.Count));
            case Direction.Left:
                directions = new List<HexDirection>{ HexDirection.DownLeft, HexDirection.UpLeft,};
                return directions.ElementAt(Random.Range(0, directions.Count));
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public static HexDirection RandomizeHexDirection()
    {
        var values = Enum.GetValues(typeof(HexDirection));
        return (HexDirection)Random.Range(0, values.Length);
    }

    public static int HexDirectionsAmount()
    {
        return Enum.GetValues(typeof(HexDirection)).Length;
    }
    
    public static int DirectionsAmount()
    {
        return Enum.GetValues(typeof(Direction)).Length;
    }

    public static HexDirection GetOppositeHexDirection(HexDirection hexDirection)
    {
        return hexDirection switch
        {
            HexDirection.Up => HexDirection.Down,
            HexDirection.UpRight => HexDirection.DownLeft,
            HexDirection.UpLeft => HexDirection.DownRight,
            HexDirection.Down => HexDirection.Up,
            HexDirection.DownRight => HexDirection.UpLeft,
            HexDirection.DownLeft => HexDirection.UpRight,
        };
    }
    public static Vector2Int GetArrayDirectionOffset(HexDirection hexDirection, bool evenRow)
    {
        var arrayOffset = new Vector2Int();
        switch (hexDirection)
        {
            case HexDirection.Up:
                arrayOffset.y += 1;
                break;
            case HexDirection.Down:
                arrayOffset.y -= 1;
                break;
            case HexDirection.UpRight:
                arrayOffset.x += 1;
                if(!evenRow) arrayOffset.y += 1;
                break;
            case HexDirection.DownRight:
                arrayOffset.x += 1;
                if(evenRow) arrayOffset.y -= 1;
                break;
            case HexDirection.UpLeft:
                arrayOffset.x -= 1;
                if(!evenRow) arrayOffset.y += 1;
                break;
            case HexDirection.DownLeft:
                arrayOffset.x -= 1;
                if(evenRow) arrayOffset.y -= 1;
                break;
        };
        return arrayOffset;
    }

    public static HexDirection GetNextHexDirection(HexDirection hexDirection)
    {
        return hexDirection switch
        {
            HexDirection.Up => HexDirection.UpRight,
            HexDirection.UpRight => HexDirection.DownRight,
            HexDirection.DownRight => HexDirection.Down,
            HexDirection.Down => HexDirection.DownLeft,
            HexDirection.DownLeft => HexDirection.UpLeft,
            HexDirection.UpLeft => HexDirection.Up,
        }; 
    }
    
    public static HexDirection GetPreviousHexDirection(HexDirection hexDirection)
    {
        return hexDirection switch
        {
            HexDirection.Up => HexDirection.UpLeft,
            HexDirection.UpRight => HexDirection.Up,
            HexDirection.DownRight => HexDirection.UpRight,
            HexDirection.Down => HexDirection.DownRight,
            HexDirection.DownLeft => HexDirection.Down,
            HexDirection.UpLeft => HexDirection.DownLeft,
        }; 
    }
    
    public static Direction GetNextDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Right,
            Direction.Right => Direction.Down,
            Direction.Down => Direction.Left,
            Direction.Left => Direction.Up,
        }; 
    }
    
    public static Direction GetPreviousDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Left,
            Direction.Right => Direction.Up,
            Direction.Down => Direction.Right,
            Direction.Left => Direction.Down,
        }; 
    }

    public static Vector3 hexPositionBasedOnCoordinates(Vector2Int coordinates, float positionY)
    {
        var hexPosition = new Vector3( hexR * coordinates.x,
            positionY, hexr * 2 * coordinates.y);
        if (Mathf.Abs(coordinates.x) % 2 == 1)
            hexPosition.z += hexr;
        return hexPosition;
    }
}
