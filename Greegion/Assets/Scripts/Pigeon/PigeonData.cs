using Sirenix.OdinInspector;
using UnityEngine;

namespace Pigeon
{
    [CreateAssetMenu(menuName = "Create PigeonData", fileName = "PigeonData", order = 0)]
    [InlineEditor]
    public class PigeonData : ScriptableObject
    {
        public float speed = 5f;
        public float smoothTime = 0.1f;
        public float jumpHeight = 3f;
    }
}
