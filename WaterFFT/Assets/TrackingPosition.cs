using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingPosition : MonoBehaviour
{
    public Transform trackedObject;
    public bool trackX, trackY, trackZ;
    
    void Update()
    {
        transform.position = new Vector3(trackX ? trackedObject.position.x : transform.position.x,
            trackY ? trackedObject.position.y : transform.position.y,
            trackZ ? trackedObject.position.z : transform.position.z);
    }
}
