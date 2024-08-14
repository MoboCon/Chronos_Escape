using UnityEngine;

public class ExampleScript : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    private MyCustomType _unity_self;  // Use the correct custom type

    void Awake()
    {
        // Properly initialize _unity_self
        _unity_self = new MyCustomType { Name = "InitialName" };
    }

    void Start()
    {
        SomeMethod();
    }

    void SomeMethod()
    {
        if (_unity_self == null)
        {
            Debug.LogError("The _unity_self parameter is null. Ensure it is properly initialized.");
            return; // Exit the method if null
        }

        // Continue with the logic if _unity_self is not null
    }
    [System.Serializable]
    public class MyCustomType
    {
        // Define your custom type properties and methods
        public string Name;
    }
    public void OnAfterDeserialize()
    {
        if (_unity_self == null)
        {
            Debug.LogError("The _unity_self parameter is null after deserialization.");
            // Handle the null case appropriately
        }
    }

    public void OnBeforeSerialize()
    {
        // Implementation for OnBeforeSerialize, if needed
    }
}
