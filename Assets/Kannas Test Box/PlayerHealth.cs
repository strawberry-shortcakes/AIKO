using UnityEngine;
using UnityEngine.UI;  // Import UI namespace

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;     // Maximum health of the player
    public float currentHealth;        // Current health of the player
    public Slider healthBar;           // Reference to the Slider (health bar)

    void Start()
    {
        currentHealth = maxHealth;     // Set initial health
        healthBar.maxValue = maxHealth; // Set the health bar's max value
        healthBar.value = currentHealth; // Set the initial value of the health bar
    }

    // Function to take damage
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0; // Ensure health doesn't go below 0

        // Update the health bar's value
        healthBar.value = currentHealth;

        // Check if health is 0 (player dies)
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Function to heal the player
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth; // Ensure health doesn't exceed maxHealth

        // Update the health bar's value
        healthBar.value = currentHealth;
    }

    // Function to handle player's death
    private void Die()
    {
        // Implement death logic (e.g., play death animation, restart level, etc.)
        Debug.Log("Player has died!");
        // Optionally, restart the game or show a Game Over screen here
    }
}