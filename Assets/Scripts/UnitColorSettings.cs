using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/UnitColorSettings")]
public class UnitColorSettings : HexColorSettings
{
    [SerializeField] private Color movedColor;
    
    public Color MovedColor => movedColor;
}