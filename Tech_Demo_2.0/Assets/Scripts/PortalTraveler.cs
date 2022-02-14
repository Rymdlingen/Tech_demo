using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PortalTraveler : MonoBehaviour
{
    public GameObject originalTraveler { get; private set; }
    public GameObject cloneTraveler { get; private set; }

    public Material[] playerMaterials { get; private set; }
    public Material[] cloneMaterials { get; private set; }

    public Portal lastUsedPortal { get; private set; }

    public event Action<PortalTraveler> traveled;

    // Start is called before the first frame update
    protected virtual void Start() // Why is this protected? TODO
    {
        // Used for cloning the traveler if they have a mesh.
        originalTraveler = gameObject.transform.GetComponentInChildren<MeshRenderer>()?.gameObject;
    }

    public virtual void EnterPortal()
    {
        // If the traveler didn't have a mesh, do nothing.
        if (!originalTraveler)
        {
            return;
        }

        // Set the clone.
        if (cloneTraveler == null)
        {
            cloneTraveler = Instantiate(originalTraveler);
            // cloneCharacter.transform.parent = playerCharacter.transform.parent;
            cloneTraveler.transform.localScale = originalTraveler.transform.localScale;
            // cloneMaterials = GetMaterials(cloneCharacter);
        }
        else
        {
            cloneTraveler.SetActive(true);
        }
    }

    public virtual void ExitPortal()
    {
        ClearLastUsedPortal();

        // If the traveler didn't have a mesh, don't continue.
        if (!originalTraveler)
        {
            return;
        }

        cloneTraveler.SetActive(false);
    }

    // Move the traveler from a portal to the destination portal.
    public void Travel(Vector3 toPosition, Quaternion newRotation, Portal traveledFrom, Portal traveledTo)
    {
        lastUsedPortal = traveledFrom;

        transform.position = toPosition;
        transform.rotation = newRotation;

        // Tell that a travel happened.
        traveled?.Invoke(this);
    }

    public void ClearLastUsedPortal()
    {
        lastUsedPortal = null;
    }

    /* For slicing the player, not implemented.
    private Material[] GetMaterials(GameObject model)
    {
        List<Material> materials = new List<Material> { };

        materials.AddRange(GetListOfMaterials(model));

        return materials.ToArray();
    }

    private List<Material> GetListOfMaterials(GameObject model)
    {
        MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>();
        Material[] modelMaterials = model.GetComponent<MeshRenderer>().materials;

        List<Material> materials = new List<Material> { };

        for (int material = 0; material < modelMaterials.Length; material++)
        {
            materials.Add(modelMaterials[material]);
        }

        foreach (MeshRenderer child in renderers)
        {
            materials.AddRange(GetMaterials(child.gameObject));
        }

        return materials;
    }
    */
}
