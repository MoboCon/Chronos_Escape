using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class adgem : MonoBehaviour
{
    public int gemsToAdd = 50;

    void Start()
    {
        if (GemManager.Instance != null)
        {
            GemManager.Instance.AddGemsFromInspector(gemsToAdd);
        }
        else
        {
            Debug.LogError("GemManager instance not found!");
        }
    }
}
