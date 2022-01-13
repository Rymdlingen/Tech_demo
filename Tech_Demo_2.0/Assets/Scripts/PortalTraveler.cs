using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    public Vector3 previousPosition { get; set; }

    private GameObject playerCharacter;
    public GameObject cloneCharacter { get; private set; }

    public Material[] playerMaterials { get; private set; }
    public Material[] cloneMaterials { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        playerCharacter = gameObject.transform.GetComponentInChildren<MeshRenderer>().gameObject;
        // playerMaterials = GetMaterials(playerCharacter);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void EnterPortal()
    {
        if (cloneCharacter == null)
        {
            cloneCharacter = Instantiate(playerCharacter);
            // cloneCharacter.transform.parent = playerCharacter.transform.parent;
            cloneCharacter.transform.localScale = playerCharacter.transform.localScale;
            // cloneMaterials = GetMaterials(cloneCharacter);
        }
        else
        {
            cloneCharacter.SetActive(true);
        }
    }

    public virtual void ExitPortal()
    {
        cloneCharacter.SetActive(false);
    }

    public void Travel(Vector3 toPosition, Quaternion newRotation)
    {
        transform.position = toPosition;
        transform.rotation = newRotation;
    }

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
}
