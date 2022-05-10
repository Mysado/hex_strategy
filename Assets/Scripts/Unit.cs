using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    Archer,
    Pikeman,
    Horseman
}

public enum AtackType
{
    Melee,
    Ranged
}

public enum Player
{
    Left,
    Right
}
public class Unit : MonoBehaviour
{
    [SerializeField] private UnitColorSettings unitColorSettings;
    [SerializeField] private Transform unitRenderer;
    [SerializeField] private UnitAnimator unitAnimator;
    [SerializeField] private UnitType unitType;
    [SerializeField] private Player player;
    [SerializeField] private int viewDistance;
    [SerializeField] private int movement;
    [SerializeField] private bool movedThisTurn = false;
    [SerializeField] private MeshRenderer highlightMeshRenderer;
    [SerializeField] private HexTile currentHex;
    [SerializeField] private HexTile destinationHex;
    [SerializeField] private List<UnitType> canDamage;
    [SerializeField] private AtackType atackType;
    [SerializeField] private bool destroyed;
    
    public UnitType UnitType => unitType;
    public Player Player
    {
        get => player;
        set => player = value;
    }

    public int ViewDistance => viewDistance;
    public int Movement => movement;
    public bool MovedThisTurn
    {
        get => movedThisTurn;
        set => movedThisTurn = value;
    }

    public MeshRenderer HighlightMeshRenderer => highlightMeshRenderer;
    public HexTile CurrentHex
    {
        get => currentHex;
        set => currentHex = value;
    }

    public HexTile DestinationHex
    {
        get => destinationHex;
        set => destinationHex = value;
    }

    public List<UnitType> CanDamage => canDamage;
    public AtackType AtackType => atackType;
    public bool Destroyed => destroyed;

    public Transform UnitRenderer => unitRenderer;
    public UnitAnimator UnitAnimator => unitAnimator;

    public void HighlightAsInaccessibleOrDestroyed()
    {
        highlightMeshRenderer.material.color = unitColorSettings.InaccessibleColor;
    }

    public void Select()
    {
        highlightMeshRenderer.material.color = unitColorSettings.SelectedColor;
    }
    
    public void Highlight()
    {
        highlightMeshRenderer.material.color = Color.Lerp(unitColorSettings.HighlightColor1, unitColorSettings.HighlightColor2, Mathf.PingPong(Time.time * unitColorSettings.FlashingSpeed, 1));
    }

    public void DisableHighlight()
    {
        highlightMeshRenderer.material.color = unitColorSettings.InactiveColor;
    }
    
    public void MarkAsMoved()
    {
        highlightMeshRenderer.material.color = unitColorSettings.MovedColor;
        movedThisTurn = true;
    }

    public void ChangeColor(Color color)
    {
        highlightMeshRenderer.material.color = color;

    }
    
    public void MarkAsDestroyed(float fractionOfJourney)
    {
        highlightMeshRenderer.material.color = Color.Lerp(unitColorSettings.InactiveColor, unitColorSettings.InaccessibleColor, fractionOfJourney);
        destroyed = true;
    }
}
