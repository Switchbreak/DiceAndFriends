using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    private const int MAX_RAYCAST_DISTANCE = 1000;

    [SerializeField]
    float speed = 0.1f;

    int layerMask;

    // Start is called before the first frame update
    void Start()
    {
        layerMask = LayerMask.GetMask("Collision Plane");
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnMouseDrag()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out var hitInfo, MAX_RAYCAST_DISTANCE, layerMask))
        {
            var drag = hitInfo.point - this.transform.position;
            this.transform.Translate(drag.x, 0, drag.z, Space.World);
        }
    }
}
