using UnityEngine;

// このファイルはカメラの回転・ズーム操作を担当する。
// 仕様: Assets/Script/Core/Camera/Camera.md
public sealed class CameraController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform target;

    [Header("Drag")]
    [SerializeField] private float dragRotateSpeed = 0.2f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 30f;

    [Header("Key Rotate")]
    [SerializeField] private float keyRotateSpeed = 45f;

    private float _yaw;
    private float _pitch;
    private float _distance = 10f;

    private void OnEnable()
    {
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
            _yaw += Input.GetAxis("Mouse X") * dragRotateSpeed;
            _pitch -= Input.GetAxis("Mouse Y") * dragRotateSpeed;
        }

        if (Input.GetKey(KeyCode.R))
        {
            _yaw += keyRotateSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.F))
        {
            _yaw -= keyRotateSpeed * Time.deltaTime;
        }

        var scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            _distance = Mathf.Clamp(_distance - scroll * zoomSpeed, minDistance, maxDistance);
        }

        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    private void LateUpdate()
    {
        var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
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
