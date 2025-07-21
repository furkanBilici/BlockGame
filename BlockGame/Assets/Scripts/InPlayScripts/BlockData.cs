using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName ="NewBlockData",menuName ="ScriptableObjects/BlockData")]

public class BlockData : ScriptableObject
{
    public List<Vector2Int> cells;
    public GameObject blockCellPrefab;
}
