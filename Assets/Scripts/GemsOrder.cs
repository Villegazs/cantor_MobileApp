using UnityEngine;
using System.Collections.Generic;
using Match3;

// 1. ScriptableObject to create dialogue assets in the Inspector.
// This decouples the data from the code.

[CreateAssetMenu(fileName = "NewGemOrder", menuName = "Levels/GemOrder", order = 1)]
public class GemOrderData : ScriptableObject
{
    // A list of all "Lines" or "Steps" that make up this conversation.
    public List<GemType> gemList = new List<GemType>();
}