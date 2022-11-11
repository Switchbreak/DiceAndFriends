using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    private const int MAX_RAYCAST_DISTANCE = 1000;
    private const int COLLISION_PLANE_SIZE = 5;
    private const int COLLISION_PLANE_LAYER = 3;

    [SerializeField]
    float speed = 0.1f;

    GameObject collisionPlane;

    private void OnMouseDown()
    {
        collisionPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        collisionPlane.transform.localScale = new Vector3(COLLISION_PLANE_SIZE, COLLISION_PLANE_SIZE, COLLISION_PLANE_SIZE);
        collisionPlane.GetComponent<MeshRenderer>().enabled = false;
        collisionPlane.layer = COLLISION_PLANE_LAYER;
    }

    private void OnMouseDrag()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out var hitInfo, MAX_RAYCAST_DISTANCE, 1 << COLLISION_PLANE_LAYER))
        {
            var drag = hitInfo.point - this.transform.position;
            this.transform.Translate(drag.x, 0, drag.z, Space.World);
        }
    }

    private void OnMouseUp()
    {
        Destroy(collisionPlane);
    }
}
