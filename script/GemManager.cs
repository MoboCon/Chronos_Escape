using UnityEngine;

public class GemManager : MonoBehaviour
{
    public static GemManager Instance;

    private int gemsCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void LoadGems()
    {
        gemsCount = PlayerPrefs.GetInt("GemsCount", 0);
        UIManager.Instance.UpdateGemsText(gemsCount);
    }

    public int GetGemsCount()
    {
        return gemsCount;
    }

    public bool SpendGems(int amount)
    {
        if (gemsCount >= amount)
        {
            gemsCount -= amount;
            PlayerPrefs.SetInt("GemsCount", gemsCount);
            UIManager.Instance.UpdateGemsText(gemsCount);
            return true;
        }
        return false;
    }

    public void AddGems(int amount)
    {
        gemsCount += amount;
        PlayerPrefs.SetInt("GemsCount", gemsCount);
        UIManager.Instance.UpdateGemsText(gemsCount);
    }

    // Method to add gems from inspector or other scripts
    public void AddGemsFromInspector(int amount)
    {
        AddGems(amount);
    }
}
