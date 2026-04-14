using UnityEngine;

public class RotationHandler : MonoBehaviour
{
    
    private Quaternion _rotation;

    private void Start()
    {
        _rotation =  transform.rotation;
    }

    private void Update()
    {
        Vector3.MoveTowards(transform.position, transform.parent.position, 1);
        // transform.rotation = _rotation; // Test unlocking the rotation to see how it looks.
    }
}
