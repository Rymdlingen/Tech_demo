using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    // TODO is ID better?
    public string Name
    {
        get;
        [SerializeField]
        private set;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Travel(Portal toPortal)
    {
        Vector3 eulerRotation = new Vector3(transform.eulerAngles.x, toPortal.transform.eulerAngles.y, transform.eulerAngles.z);


        Vector3 direction = toPortal.transform.rotation * Vector3.forward;

        transform.position = toPortal.transform.position;
        transform.rotation = Quaternion.Euler(direction);
        //transform.rotation = toPortal.transform.rotation;
        Debug.Log("Traveled to " + toPortal.name);
    }
}
