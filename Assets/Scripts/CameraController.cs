using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 2.0f;

    private Vector3 lastMousePosition;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition; 
        }

        if (Input.GetMouseButton(1))
        {
            float deltaX = Input.mousePosition.x - lastMousePosition.x;
            float deltaZ = Input.mousePosition.y - lastMousePosition.y;

            float positionX = transform.position.x - deltaX * sensitivity * Time.deltaTime;
            float positionZ = transform.position.z - deltaZ * sensitivity * Time.deltaTime; 

            transform.position = new Vector3(positionX, transform.position.y, positionZ); 

            lastMousePosition = Input.mousePosition;
        }
    }
}
