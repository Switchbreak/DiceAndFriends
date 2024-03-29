using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
//using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARAnchorPlace : MonoBehaviour
{
    private const int RAYCAST_MAX_DISTANCE = 1000;
    private const int GAME_PIECES_LAYER = 6;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    ARRaycastManager raycastManager;
    ARAnchorManager anchorManager;
    ARPlaneManager planeManager;
    Unity.XR.CoreUtils.XROrigin sessionOrigin;

    [SerializeField]
    Material tableBasisPlaneMaterial;

    [SerializeField]
    GameObject tabletop;

    ARAnchor anchor;
    ARPlane anchorPlane;
    GameObject tableBasis;
    Vector3 tabletopStartPosition;

    public bool tablePlaced = false;
    public bool pieceDragging = false;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        planeManager = GetComponent<ARPlaneManager>();
        sessionOrigin = GetComponent<Unity.XR.CoreUtils.XROrigin>();

        tabletopStartPosition = tabletop.transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        // FIXME: Temp code, hide all game objects until the basis has been placed
        var gameMeshes = FindObjectsOfType<MeshRenderer>();

        foreach(var mesh in gameMeshes)
        {
            mesh.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 1)
        {
            if (!tablePlaced)
            {
                if (Input.touches[0].phase == TouchPhase.Began)
                {
                    PlaceAnchorPoint();
                }
                else if (anchor != null && Input.touches[0].phase == TouchPhase.Moved)
                {
                    SetTableBasis(Input.touches[0]);
                }
                else if (anchor != null && tableBasis != null && Input.touches[0].phase == TouchPhase.Ended)
                {
                    ShowTabletop();
                }
            }
            else if (!pieceDragging)
            {
                if (RaycastTouchPoint(Input.touches[0], out var hitInfo))
                {
                    hitInfo.collider.GetComponent<ChessPiece>()?.TouchPiece(Input.touches[0], hitInfo);
                    pieceDragging = true;
                }
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

    private void ShowTabletop()
    {
        tabletop.transform.parent = anchor.transform;
        tabletop.transform.localScale = tableBasis.transform.localScale * 20;
        tabletop.transform.position = tableBasis.transform.position;
        tabletop.transform.eulerAngles = tableBasis.transform.eulerAngles;

        // Correct position
        var offset = tabletopStartPosition;
        offset.Scale(tableBasis.transform.localScale);
        tabletop.transform.Translate(offset, Space.Self);

        // FIXME: Temp code, hide all game objects until the basis has been placed
        var gameMeshes = FindObjectsOfType<MeshRenderer>();

        foreach (var mesh in gameMeshes)
        {
            mesh.enabled = true;
        }

        tableBasis.GetComponent<MeshRenderer>().enabled = false;
        anchor.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;

        tablePlaced = true;
    }

    private void SetTableBasis(Touch touch)
    {
        var pos = Input.touches[0].position;

        var worldSpaceRay = Camera.current.ScreenPointToRay(pos);
        Ray sessionSpaceRay = sessionOrigin.TrackablesParent.InverseTransformRay(worldSpaceRay);
        var hits = planeManager.Raycast(sessionSpaceRay, TrackableType.PlaneWithinInfinity, Allocator.Temp);

        foreach (var hit in hits)
        {
            if (hit.trackableId == anchorPlane.trackableId)
            {
                if (tableBasis == null)
                {
                    tableBasis = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    tableBasis.GetComponent<MeshRenderer>().material = tableBasisPlaneMaterial;
                    tableBasis.transform.parent = anchor.transform;
                }

                var worldSpacePose = sessionOrigin.TrackablesParent.TransformPose(hit.pose);
                float scale = GetScale(worldSpacePose);
                float rotation = GetRotation(worldSpacePose.position);

                tableBasis.transform.position = anchor.transform.position;
                tableBasis.transform.localScale = Vector3.one * scale;
                tableBasis.transform.eulerAngles = Vector3.up * (rotation + 45);
            }
        }
    }

    private float GetScale(Pose worldSpacePose)
    {
        var scale = Vector3.Distance(anchor.transform.position, worldSpacePose.position);

        return Mathf.Sqrt((scale * scale) / 2) / 5;
    }

    // This is a bit of a hack since it assumes that our two points must be on a perfectly horizontal plane, but it should work
    private float GetRotation(Vector3 position)
    {
        var vector = position - anchor.transform.position;
        var angle = Vector3.Angle(Vector3.forward, vector);
        var cross = Vector3.Cross(Vector3.forward, vector);

        return cross.y > 0 ? angle : -angle;
    }

    private void PlaceAnchorPoint()
    {
        var pos = Input.touches[0].position;
        if (raycastManager.Raycast(pos, hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneEstimated))
        {
            var hitPose = hits[0].pose;
            var planeId = hits[0].trackableId;
            var plane = planeManager.GetPlane(planeId);
            var newAnchor = anchorManager.AttachAnchor(plane, hitPose);

            if (newAnchor != null)
            {
                RemoveOldAnchors();

                anchor = newAnchor;
                anchorPlane = plane;
                Debug.Log("Added new anchor");
            }
        }
    }

    private void RemoveOldAnchors()
    {
        if (anchor != null)
        {
            tabletop.transform.parent = null;

            Debug.Log("Destroying old anchor");
            Destroy(anchor);
            Destroy(anchor.gameObject);

            anchor = null;
        }

        if (tableBasis != null)
        {
            Debug.Log("Table basis destroyed");
            Destroy(tableBasis);
            tableBasis = null;
        }
    }

    public void SetPieceDragging(bool isDragging)
    {
        pieceDragging = isDragging;
    }
}
