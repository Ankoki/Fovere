using UnityEngine;

namespace Fovere
{
    
    [CreateAssetMenu(fileName = "New Resident", menuName = "Resident")]
    public class ResidentData : ScriptableObject
    {

        public string residentName = "Resident";
        public Color residentColour = Color.darkBlue; // TODO color by typing.
        public Color invertColour = Color.white;
        public Material residentMaterial;
        // public DialogueData dialogue; // TODO store whole conversations separately and grab to execute.

    }
    
}