using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/UnitPrefabSelector")]
public class UnitPrefabSelector : ScriptableObject
{
    [SerializeField] private GameObject leftPlayerUnit;
    [SerializeField] private GameObject rightPlayerUnit;

    public GameObject GetPlayerUnit(Player player)
    {
        return player switch
        {
            Player.Left => leftPlayerUnit,
            Player.Right => rightPlayerUnit,
        };
    }
}
