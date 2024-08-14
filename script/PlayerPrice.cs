[System.Serializable]
public class PlayerPrice
{
    public int amount;
    public bool isGems; // true if price is in gems, false if in coins
    public int requiredLevel; // New field to specify the required terrain level
}
