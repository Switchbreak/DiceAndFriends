using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    private const int RIGHT_MOUSE_BUTTON = 1;
    private const int MIDDLE_MOUSE_BUTTON = 2;
    private const int DRAG_CLAMP = 100;
    private const int RAYCAST_MAX_DISTANCE = 1000;
    private const int GAME_PIECES_LAYER = 6;

    [SerializeField]
    Vector3 lookAt = default;

    [SerializeField]
    float orbitMouseSpeed = 50;

    [SerializeField]
    float orbitTouchSpeed = 10;

    [SerializeField]
    float zoomWheelSpeed = 1;

    [SerializeField]
    float zoomTouchSpeed = 0.0001f;

    [SerializeField]
    float panMouseSpeed = 0.01f;

    [SerializeField]
    float panTouchSpeed = 0.01f;

    [SerializeField]
    public bool pieceDragging = false;

    Vector3 mousePosition;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.LookAt(lookAt);
        mousePosition = Input.mousePosition;

        Input.simulateMouseWithTouches = false;
    }

    // Update is called once per frame
    void Update()
    {
        MouseInput();
        TouchInput();
    }

    private void TouchInput()
    {
        // Orbit camera on single touch
        if (Input.touchCount == 1 && !pieceDragging)
        {
            if (RaycastTouchPoint(Input.touches[0], out var hitInfo))
            {
                hitInfo.collider.GetComponent<ChessPiece>()!.TouchPiece(Input.touches[0]);
                pieceDragging = true;
            }
            else
            {
                Orbit(Input.touches[0].deltaPosition, orbitTouchSpeed);
            }
        }

        if (Input.touchCount == 2)
        {
            var moveMagnitude1 = Vector2.Dot(Input.touches[0].deltaPosition, Input.touches[1].position - Input.touches[0].position);
            var moveMagnitude2 = Vector2.Dot(Input.touches[1].deltaPosition, Input.touches[0].position - Input.touches[1].position);

            if (Mathf.Sign(moveMagnitude1) == Mathf.Sign(moveMagnitude2))
            {
                this.GetComponent<Camera>().fieldOfView += zoomTouchSpeed * (moveMagnitude1 + moveMagnitude2);
            }

            var panMagnitude = Vector2.Dot(Input.touches[0].deltaPosition, Input.touches[1].deltaPosition);
            if (panMagnitude > 0)
            {
                var averageDragVector = (Input.touches[0].deltaPosition + Input.touches[1].deltaPosition) / 2;
                Pan(averageDragVector, panTouchSpeed);
            }
        }
    }

    private bool RaycastTouchPoint(Touch touch, out RaycastHit hitInfo)
    {
        if (touch.phase != TouchPhase.Began)
        {
            hitInfo = default;
            return false;
        }

        var ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
        return Physics.Raycast(ray, out hitInfo, RAYCAST_MAX_DISTANCE, 1 << GAME_PIECES_LAYER);
    }

    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON))
        {
            mousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(RIGHT_MOUSE_BUTTON))
        {
            Orbit(Input.mousePosition - mousePosition, orbitMouseSpeed);
        }
        if (Input.GetMouseButton(MIDDLE_MOUSE_BUTTON))
        {
            Pan(Input.mousePosition - mousePosition, panMouseSpeed);
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            this.GetComponent<Camera>().fieldOfView -= zoomWheelSpeed * Input.mouseScrollDelta.y;
        }

        mousePosition = Input.mousePosition;
    }

    private void Pan(Vector3 drag, float panSpeed)
    {
        var panOffset = this.transform.position;
        this.transform.transform.Translate(-drag * panSpeed, Space.Self);
        lookAt += (this.transform.position - panOffset);
    }

    private void Orbit(Vector3 drag, float orbitSpeed)
    {
        var dragX = Mathf.Clamp(drag.x, -DRAG_CLAMP, DRAG_CLAMP);
        var dragY = -Mathf.Clamp(drag.y, -DRAG_CLAMP, DRAG_CLAMP);

        this.transform.RotateAround(lookAt, Vector3.up, dragX * orbitSpeed);
        this.transform.RotateAround(lookAt, this.transform.right, dragY * orbitSpeed);
        this.transform.LookAt(lookAt);
    }
}
