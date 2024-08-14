using UnityEngine;

public class Example : MonoBehaviour
{
    [SerializeField] private GameObject someObject;

    private void Start()
    {
        if (someObject == null)
        {
            Debug.LogError("SomeObject is not assigned!", this);
        }
        else
        {
            // Proceed with using someObject
        }
    }
}
