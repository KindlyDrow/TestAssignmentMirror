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

        float targetAngle = target.eulerAngles.y; // угол поворота цели (игрока)
        float targetHeight = target.position.y + height; // высота цели (игрока)

        float currentAngle = transform.eulerAngles.y; // текущий угол поворота камеры
        currentHeight = transform.position.y; // текущая высота камеры

        if (Player.LocalInstance.GetPlayerMove())
        {
            // поворот камеры
            currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationDamping * Time.deltaTime);


            // изменение высоты камеры
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, heightDamping * Time.deltaTime);
        }

        // приближение/отдаление камеры
        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minZoom, maxZoom);

        if (Player.LocalInstance.GetPlayerMove())
        {
            // поворот камеры
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationDamping * Time.deltaTime);
        
        // изменение высоты камеры
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, heightDamping * Time.deltaTime);
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        currentAngle += mouseX;

        // поворот камеры по вертикали (вверх/вниз)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minRotationX, maxRotationX);

        // расчет новой позиции камеры
        Vector3 direction = new Vector3(0, 0, -currentDistance);
        Quaternion rotation = Quaternion.Euler(rotationX, currentAngle, 0);
        Vector3 position = target.position + rotation * direction;

        // применение новой позиции и поворота камеры
        transform.position = position;
        transform.LookAt(target);
    }
}
