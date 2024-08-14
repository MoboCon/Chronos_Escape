using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        SpeedBoost,
        DoubleCoins,
        Shield,
        MagnetCoin,
        FreezeEnemy,
        HealthBoost,
        ScoreMultiplier // New power-up type
    }

    public PowerUpType powerUpType;
    public float duration = 5f;
    public float speedMultiplier = 1.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!GameManager.Instance.IsPowerUpActive())
            {
                ActivatePowerUp();
                gameObject.SetActive(false);
            }
        }
    }

    private void ActivatePowerUp()
    {
        GameManager.Instance.ActivatePowerUp(powerUpType, duration, speedMultiplier);
    }
}
