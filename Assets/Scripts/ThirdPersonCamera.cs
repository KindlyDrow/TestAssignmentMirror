using System.Collections;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    private Transform target;

    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float height = 2.0f;
    [SerializeField] private float rotationDamping = 3.0f;
    [SerializeField] private float heightDamping = 2.0f;
    [SerializeField] private float zoomSpeed = 2.0f;
    [SerializeField] private float maxZoom = 10.0f;
    [SerializeField] private float minZoom = 1.0f;
    [SerializeField] private float mouseSensitivityX = 3.0f;
    [SerializeField] private float mouseSensitivityY = 3.0f;
    [SerializeField] private float maxRotationX = 90f;
    [SerializeField] private float minRotationX = -90f;

    private float currentDistance; 
    private float currentHeight; 
    private float rotationX;

    private void Start()
    {
        StartCoroutine(GetTarget());
    }

    private IEnumerator GetTarget()
    {
        while (Player.LocalInstance == null)
        {
            yield return null;
        }
        target = Player.LocalInstance.transform;
        currentDistance = distance;
        currentHeight = transform.position.y - target.position.y;
        rotationX = transform.eulerAngles.x;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float targetAngle = target.eulerAngles.y;
        float targetHeight = target.position.y + height;

        float currentAngle = transform.eulerAngles.y;
        currentHeight = transform.position.y;

        if (Player.LocalInstance.GetPlayerMove())
        {
            currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationDamping * Time.deltaTime);


            currentHeight = Mathf.Lerp(currentHeight, targetHeight, heightDamping * Time.deltaTime);
        }

        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minZoom, maxZoom);


        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        currentAngle += mouseX;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minRotationX, maxRotationX);

        Vector3 direction = new Vector3(0, 0, -currentDistance);
        Quaternion rotation = Quaternion.Euler(rotationX, currentAngle, 0);
        Vector3 position = target.position + rotation * direction;

        transform.position = position;
        transform.LookAt(target);
    }
}
