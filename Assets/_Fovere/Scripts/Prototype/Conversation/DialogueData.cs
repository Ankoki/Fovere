using System.Collections.Generic;
using UnityEngine;

namespace Fovere
{
    [CreateAssetMenu(fileName = "New Conversation Data", menuName = "Conversation Data")]
    public class DialogueData : ScriptableObject
    {
        [TextArea(4, 4)] public List<string> conversationBlock;
    }
}