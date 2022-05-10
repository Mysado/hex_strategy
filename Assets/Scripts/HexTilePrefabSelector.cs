using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/HexTilePrefabSelector")]
public class HexTilePrefabSelector : ScriptableObject
{
    [SerializeField] private List<GameObject> hexTiles;
    [SerializeField] private List<int> weights;
    private List<int> weightedList;
    [SerializeField] private bool weightListInitialized = false;
    public GameObject GetRandomTile()
    {
        if (weightedList == null || !weightListInitialized || (weightedList != null && weightedList.Count == 0))
        {
            weightedList = new List<int>();
            for (int i = 0; i < hexTiles.Count; i++)
            {
                for (int j = 0; j < weights[i]; j++)
                {
                    weightedList.Add(i);
                }
            }
            weightListInitialized = true;
        }
        var random = Random.Range(0, weightedList.Count);
        return hexTiles[weightedList[random]];
    }
}
