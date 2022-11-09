using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    private const int LEFT_MOUSE_BUTTON = 0;
    private const int DRAG_CLAMP = 100;

    [SerializeField]
    Vector3 lookAt = default;

    [SerializeField]
    float speed = 50;

    Vector3 up = new(0, 1, 0);
    Vector3 mousePosition;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.LookAt(lookAt);
        mousePosition = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(LEFT_MOUSE_BUTTON))
        {
            var drag = Input.mousePosition - mousePosition;

            var dragX = Mathf.Clamp(drag.x, -DRAG_CLAMP, DRAG_CLAMP);
            var dragY = -Mathf.Clamp(drag.y, -DRAG_CLAMP, DRAG_CLAMP);

            this.transform.RotateAround(lookAt, up, dragX * speed * Time.deltaTime);
            this.transform.RotateAround(lookAt, this.transform.right, dragY * speed * Time.deltaTime);
            this.transform.LookAt(lookAt);
        }

        mousePosition = Input.mousePosition;
    }
}
