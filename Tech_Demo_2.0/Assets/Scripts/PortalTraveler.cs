using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    public Vector3 previousPosition { get; set; }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Travel(Vector3 toPosition, Quaternion newRotation)
    {
        transform.position = toPosition;
        transform.rotation = newRotation;
    }
}
