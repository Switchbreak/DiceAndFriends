using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

public class ChessPiece : MonoBehaviour
{
    private const int MAX_RAYCAST_DISTANCE = 1000;
    private const int COLLISION_PLANE_SIZE = 5;
    private const int COLLISION_PLANE_LAYER = 3;

    ARAnchorPlace arInputManager;
    OrbitCamera cameraInputManager;

    GameObject collisionPlane;
    Vector3 offset = default;
    int? fingerId = null;

    private void Start()
    {
        arInputManager = FindObjectOfType<XROrigin>()?.GetComponent<ARAnchorPlace>();
        cameraInputManager = Camera.main.GetComponent<OrbitCamera>();
    }

    private void Update()
    {
        if (fingerId.HasValue)
        {
            bool touchFound = false;
            foreach(var touch in Input.touches)
            {
                if (touch.fingerId == fingerId && touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    touchFound = true;
                    DragPiece(Input.touches[0].position);
                }
            }

            if (!touchFound)
            {
                Debug.Log("Touch not found, ending drag");
                DestroyCollisionPlane();
                fingerId = null;

                // FIXME: Hack
                arInputManager?.SetPieceDragging(false);
                cameraInputManager?.SetPieceDragging(false);
            }
        }
    }

    public void TouchPiece(Touch touch, RaycastHit hitInfo)
    {
        Debug.Log($"Piece {gameObject.name} touched");

        CreateCollisionPlane(hitInfo);
        GetOffset(hitInfo);

        fingerId = touch.fingerId;
    }

    private void OnMouseDown()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, MAX_RAYCAST_DISTANCE, 1 << COLLISION_PLANE_LAYER))
        {
            CreateCollisionPlane(hitInfo);
            GetOffset(hitInfo);
        }
    }

    private void OnMouseDrag()
    {
        DragPiece(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        // FIXME: Hack
        arInputManager?.SetPieceDragging(false);
        cameraInputManager?.SetPieceDragging(false);

        DestroyCollisionPlane();
    }

    private void CreateCollisionPlane(RaycastHit hitInfo)
    {
        if (collisionPlane != null)
        {
            DestroyCollisionPlane();
        }

        collisionPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        collisionPlane.transform.localScale = new Vector3(COLLISION_PLANE_SIZE, COLLISION_PLANE_SIZE, COLLISION_PLANE_SIZE);
        collisionPlane.transform.position = hitInfo.point;

        collisionPlane.GetComponent<MeshRenderer>().enabled = false;
        collisionPlane.layer = COLLISION_PLANE_LAYER;

        // FIXME: Hack
        arInputManager?.SetPieceDragging(true);
        cameraInputManager?.SetPieceDragging(true);
    }

    private void DestroyCollisionPlane()
    {
        Destroy(collisionPlane);
        collisionPlane = null;
    }

    private void GetOffset(RaycastHit hitInfo)
    {
        offset = hitInfo.point - this.transform.position;
    }

    private void DragPiece(Vector3 position)
    {
        var ray = Camera.main.ScreenPointToRay(position);

        if (Physics.Raycast(ray, out var hitInfo, MAX_RAYCAST_DISTANCE, 1 << COLLISION_PLANE_LAYER))
        {
            var drag = hitInfo.point - offset - this.transform.position;
            this.transform.Translate(drag.x, 0, drag.z, Space.World);
        }
    }
}
