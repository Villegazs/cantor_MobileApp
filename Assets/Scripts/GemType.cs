using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "NewGemType", menuName = "Match3/Gem Type")]
    public class GemType : ScriptableObject
    {
        public Sprite sprite;
    }
}