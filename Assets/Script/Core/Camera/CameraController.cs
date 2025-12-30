using UnityEngine;

// このファイルはカメラの回転・ズーム操作を担当する。
// 仕様: Assets/Script/Core/Camera/Camera.md
public sealed class CameraController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform target;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 0.5f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float minOrthoSize = 2f;
    [SerializeField] private float maxOrthoSize = 20f;

    [Header("Key Rotate")]
    [SerializeField] private float keyRollSpeed = 45f;

    [Header("Middle Rotate")]
    [SerializeField] private float middleRotateSpeed = 0.2f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 60f;

    private float _yaw;
    private float _pitch;
    private float _distance = 10f;
    private float _roll;
    private Camera _camera;

    private void OnEnable()
    {
        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }

        if (target == null)
        {
            target = new GameObject("CameraPivot").transform;
            target.position = Vector3.zero;
        }

        var offset = transform.position - target.position;
        _distance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);
        var euler = transform.rotation.eulerAngles;
        _yaw = euler.y;
        _pitch = NormalizePitch(euler.x);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var panX = -Input.GetAxis("Mouse X") * panSpeed;
            var panY = -Input.GetAxis("Mouse Y") * panSpeed;
            var right = transform.right;
            var up = transform.up;
            target.position += right * panX + up * panY;
        }

        if (Input.GetMouseButton(2))
        {
            _yaw += Input.GetAxis("Mouse X") * middleRotateSpeed;
            _pitch -= Input.GetAxis("Mouse Y") * middleRotateSpeed;
        }

        if (Input.GetKey(KeyCode.R))
        {
            _roll += keyRollSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.F))
        {
            _roll -= keyRollSpeed * Time.deltaTime;
        }

        var scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            if (_camera != null && _camera.orthographic)
            {
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - scroll * zoomSpeed, minOrthoSize, maxOrthoSize);
            }
            else
            {
                _distance = Mathf.Clamp(_distance - scroll * zoomSpeed, minDistance, maxDistance);
            }
        }

        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    private void LateUpdate()
    {
        var rotation = Quaternion.Euler(_pitch, _yaw, _roll);
        var offset = rotation * new Vector3(0f, 0f, -_distance);
        transform.position = target.position + offset;
        transform.rotation = rotation;
    }

    private static float NormalizePitch(float pitch)
    {
        if (pitch > 180f)
        {
            pitch -= 360f;
        }

        return pitch;
    }
}
