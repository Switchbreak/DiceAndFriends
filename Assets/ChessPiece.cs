using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    private const int MAX_RAYCAST_DISTANCE = 1000;
    private const int COLLISION_PLANE_SIZE = 5;
    private const int COLLISION_PLANE_LAYER = 3;

    GameObject collisionPlane;
    Vector3 offset = default;
    int? fingerId = null;

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
                DestroyCollisionPlane();
                fingerId = null;
                Camera.main.GetComponent<OrbitCamera>()!.pieceDragging = false;
            }
        }
    }

    public void TouchPiece(Touch touch)
    {
        CreateCollisionPlane();
        GetOffset(touch.position);

        fingerId = touch.fingerId;
    }

    private void OnMouseDown()
    {
        CreateCollisionPlane();
        GetOffset(Input.mousePosition);
    }

    private void OnMouseDrag()
    {
        DragPiece(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        Camera.main.GetComponent<OrbitCamera>()!.pieceDragging = false;
        DestroyCollisionPlane();
    }

    private void CreateCollisionPlane()
    {
        if (collisionPlane != null)
        {
            DestroyCollisionPlane();
        }

        collisionPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        collisionPlane.transform.localScale = new Vector3(COLLISION_PLANE_SIZE, COLLISION_PLANE_SIZE, COLLISION_PLANE_SIZE);
        collisionPlane.GetComponent<MeshRenderer>().enabled = false;
        collisionPlane.layer = COLLISION_PLANE_LAYER;

        Camera.main.GetComponent<OrbitCamera>()!.pieceDragging = true;
    }

    private void DestroyCollisionPlane()
    {
        Destroy(collisionPlane);
        collisionPlane = null;
    }

    private void GetOffset(Vector3 position)
    {
        var ray = Camera.main.ScreenPointToRay(position);

        if (Physics.Raycast(ray, out var hitInfo, MAX_RAYCAST_DISTANCE, 1 << COLLISION_PLANE_LAYER))
        {
            offset = hitInfo.point - this.transform.position;
        }
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
