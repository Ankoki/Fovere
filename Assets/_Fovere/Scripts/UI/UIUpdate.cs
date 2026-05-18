using UnityEngine;

namespace Fovere
{
    public class UIUpdate : MonoBehaviour
    {

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private GameObject parent;

        private void LateUpdate()
        {
            transform.LookAt(cameraTransform);
        }

    }
}