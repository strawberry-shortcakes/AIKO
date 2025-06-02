using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI")]
    public Image healthBarFill; // Assign this in the inspector to your health bar's fill image

    [Header("References")]
    public PlayerMovement playerMovement; // Assign your PlayerMovement script here

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }

        UpdateHealthUI();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    private void Die()
    {
        Debug.Log("Player is dead.");
        playerMovement.isDead = true;
        // Optional: Disable movement or play a death animation
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    // For testing in editor
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) TakeDamage(1);
        if (Input.GetKeyDown(KeyCode.J)) Heal(1);
    }
}