using UnityEngine;

public class CameraRigRotate : MonoBehaviour {
    public float RotateSpeed = 25.0f;

    private Transform _transform;

    private void Awake()
    {
        _transform = gameObject.transform;
    }

    public void Update()
    {
        _transform.localRotation = Quaternion.AngleAxis(RotateSpeed * Time.deltaTime, Vector3.up) * _transform.localRotation;
    }
}
