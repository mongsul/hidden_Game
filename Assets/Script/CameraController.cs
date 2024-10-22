using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    Camera cam;

    public List<Bounds> moveSpace;
    public int spaFloor;

    public float camMoveSpeed = 10;
    public float camZoomSpeed = 100;

    public float maxZoomSize = 12;
    public float minZoomSize = 5;

    Vector3 curMousePos;

    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        cam.orthographicSize -= Input.GetAxisRaw("Mouse ScrollWheel") * Time.deltaTime * camZoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoomSize, maxZoomSize);



        if (EventSystem.current.IsPointerOverGameObject()) return;

        curMousePos = Input.mousePosition;

        if(Input.touchCount > 0)
        {
            Touch touchZero = Input.GetTouch(0);

            cam.transform.position += new Vector3(touchZero.deltaPosition.x, touchZero.deltaPosition.y, 0) * -camMoveSpeed;
        }

        curMousePos = Input.mousePosition;

        cam.transform.position = new Vector3(
             Mathf.Clamp(cam.transform.position.x, moveSpace[spaFloor].min.x, moveSpace[spaFloor].max.x),
             Mathf.Clamp(cam.transform.position.y, moveSpace[spaFloor].min.y, moveSpace[spaFloor].max.y), -10);
    }
}
