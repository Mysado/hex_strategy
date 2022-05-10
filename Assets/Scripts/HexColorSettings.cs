using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/HexColorSettings")]
public class HexColorSettings : ScriptableObject
{
    [SerializeField] private Color inactiveColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color inaccessibleColor;
    [SerializeField] private Color highlightColor1;
    [SerializeField] private Color highlightColor2;
    [SerializeField] private float flashingSpeed;

    public Color InactiveColor => inactiveColor;
    public Color SelectedColor => selectedColor;
    public Color InaccessibleColor => inaccessibleColor;
    public Color HighlightColor1 => highlightColor1;
    public Color HighlightColor2 => highlightColor2;
    public float FlashingSpeed => flashingSpeed;
    
}