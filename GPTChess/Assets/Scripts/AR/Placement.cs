using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class Placement : MonoBehaviour
{
    public GameObject gameAsset;

    bool placed;

    [SerializeField] ARRaycastManager raycastMgr;
    [SerializeField] ARPlaneManager m_ARPlaneManager;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Start is called before the first frame update
    void Start()
    {
        placed = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            if (raycastMgr.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitpose = hits[0].pose;

                if (!placed)
                {
                    gameAsset.transform.position = hitpose.position;

                    placed = true;

                    m_ARPlaneManager.enabled = !m_ARPlaneManager.enabled;
                    SetAllPlanesActive(false);

                    gameObject.AddComponent<ARAnchor>();
                }
            }
        }
    }

    void SetAllPlanesActive(bool value)
    {
        foreach (var plane in m_ARPlaneManager.trackables)
            plane.gameObject.SetActive(value);
    }
}
