using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARAnchorPlace : MonoBehaviour
{
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    ARRaycastManager raycastManager;
    ARAnchorManager anchorManager;
    List<ARAnchor> anchors;
    ARPlaneManager planeManager;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        planeManager = GetComponent<ARPlaneManager>();
        anchors = new List<ARAnchor>();
    }

    // Start is called before the first frame update
    void Start()
    {
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
            var pos = Input.touches[0].position;
            var hitResults = new List<ARRaycastHit>();
            if (raycastManager.Raycast(pos, hitResults, TrackableType.Planes))
            {
                var hitPose = hitResults[0].pose;
                var planeId = hitResults[0].trackableId;
                var newAnchor = anchorManager.AttachAnchor(planeManager.GetPlane(planeId), hitPose);

                if (newAnchor != null)
                {
                    Debug.Log("Added new anchor");
                    foreach (var anchor in anchors)
                    {
                        Debug.Log("Destroying old anchor");
                        Destroy(anchor);
                        Destroy(anchor.gameObject);
                    }

                    anchors.Clear();
                    anchors.Add(newAnchor);
                }
            }
        }
    }
}
