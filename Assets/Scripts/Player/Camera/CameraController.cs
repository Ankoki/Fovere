using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    /// <summary>
    /// Declares all camera types used in this controller.
    /// </summary>
    public class CameraType
    {
        public static readonly CameraType Standard = new(0);
        public static readonly CameraType Zoom = new(1);
        public static readonly CameraType ObstructedView = new(2);
        
        public int Index { get; }
        
        private CameraType(int index)
        {
            Index = index;
        }
        
    }
    
    [Header("Camera Objects")]
    public CinemachineCamera standardCamera;
    public CinemachineCamera zoomCamera;
    public CinemachineCamera obstructedViewCamera;

    [Header("Look Settings")] 
    public float sensitivity = 0.5f;
    
    private CameraType _currentCamera;
    
    private void Start()
    {
        ShowStandardCamera();
    }

    /// <summary>
    /// Shows the standard camera.
    /// </summary>
    public void ShowStandardCamera()
    {
        standardCamera.Priority.Value = 1;
        zoomCamera.Priority.Value = 0;
        obstructedViewCamera.Priority.Value = 0;
        _currentCamera = CameraType.Standard;
    }

    /// <summary>
    /// Shows the camera for an obstructed view.
    /// </summary>
    public void ShowObstructedViewCamera()
    {
        standardCamera.Priority.Value = 0;
        zoomCamera.Priority.Value = 0;
        obstructedViewCamera.Priority.Value = 1;
        _currentCamera = CameraType.ObstructedView;
    }
    
    /// <summary>
    /// Gets the type of camera that is currently used.
    /// </summary>
    /// <returns>The current camera type.</returns>
    public CameraType GetCurrentCamera()
    {
        return _currentCamera;
    }

    public void ShowZoomBetween(Vector3 origin, Vector3 target)
    {
        var x = (origin.x + target.x) / 2;
        var y = (origin.y + target.y) / 2;
        var z = (origin.z + target.z) / 2;
        var pos = new Vector3(x, y, z);
        var middle = new GameObject();
        middle.transform.position = pos;
        zoomCamera.Follow = middle.transform;
        ShowZoomCamera();
    }

    /// <summary>
    /// Shows the zoomed camera for conversation.
    /// </summary>
    private void ShowZoomCamera()
    {
        standardCamera.Priority.Value = 0;
        zoomCamera.Priority.Value = 1;
        obstructedViewCamera.Priority.Value = 0;
        _currentCamera = CameraType.Zoom;
    }
    
}
