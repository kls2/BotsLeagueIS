using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapControl : MonoBehaviour
{
    public float maxZoom;
    public float minZoom;
    public float panSpeed = -1;

    Vector3 bottomLeft;
    Vector3 topRight;

    float cameraMaxY;
    float cameraMinY;
    float cameraMaxX;
    float cameraMinX;

    void SetCameraDefaults()
    {
        Camera.main.transform.position = new Vector3(-0.33f,-0.86f,0.0f);
        Camera.main.orthographicSize = 0.12f;
    }

    void Start()
    {
        //set max camera bounds (assumes camera is max zoom and centered on Start)
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, -transform.position.z));
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));
        cameraMaxX = topRight.x;
        cameraMaxY = topRight.y;
        cameraMinX = bottomLeft.x;
        cameraMinY = bottomLeft.y;

        SetCameraDefaults();

    }

    void Update()
    {
        //click and drag
        if (Input.GetMouseButton(0))
        {
            float x = Input.GetAxis("Mouse X") * panSpeed;
            float y = Input.GetAxis("Mouse Y") * panSpeed;
            transform.Translate(-x, -y, 0);
        }

        //zoom
        if ((Input.GetAxis("Mouse ScrollWheel") > 0) && Camera.main.orthographicSize > minZoom ) // forward
        {
            Camera.main.orthographicSize = Camera.main.orthographicSize - 0.05f;
        }

        if ((Input.GetAxis("Mouse ScrollWheel") < 0) && Camera.main.orthographicSize < maxZoom) // back            
        {
            Camera.main.orthographicSize = Camera.main.orthographicSize + 0.05f;
        }


        //check if camera is out-of-bounds, if so, move back in-bounds
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, -transform.position.z));
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));

        if (topRight.x > cameraMaxX)
        {
            transform.position = new Vector3(transform.position.x - (topRight.x - cameraMaxX), transform.position.y, transform.position.z);
        }

        if (topRight.y > cameraMaxY)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (topRight.y - cameraMaxY), transform.position.z);
        }

        if (bottomLeft.x < cameraMinX)
        {
            transform.position = new Vector3(transform.position.x + (cameraMinX - bottomLeft.x), transform.position.y, transform.position.z);
        }

        if (bottomLeft.y < cameraMinY)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + (cameraMinY - bottomLeft.y), transform.position.z);
        }
    }
}