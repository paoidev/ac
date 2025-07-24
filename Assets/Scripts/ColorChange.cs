using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour, IInteractable
{
    Material mat;

    private void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    public string GetDescription()
    {
        return "Change to random color";
    }

    public void Interact()
    {
        mat.color = new Color(Random.value, Random.value, Random.value);
    }

}
