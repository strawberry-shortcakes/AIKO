using UnityEngine;

public class WeaponSwapManager : MonoBehaviour
{
    // Assign the GameObject of the gun and the melee script
    public GameObject gunObject;        // The Gun GameObject
    public MeleeAttack meleeScript;     // The MeleeAttack script attached to the player

    private void Start()
    {
        // Ensure both the gun object and melee script are assigned
        if (gunObject == null || meleeScript == null)
        {
            Debug.LogError("Gun object or Melee script is not assigned!");
            return;
        }

        SwitchToMelee();  // Set melee as the default weapon on start
    }

    private void Update()
    {
        // Swap weapons using the Q key
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Toggle between melee and gun
            if (meleeScript.enabled)
            {
                SwitchToGun();
            }
            else
            {
                SwitchToMelee();
            }
        }
    }

    // Switch to Melee Attack (enable the melee script)
    private void SwitchToMelee()
    {
        meleeScript.enabled = true;      // Enable Melee Attack Script
        gunObject.SetActive(false);      // Disable Gun GameObject
    }

    // Switch to Gun Attack (disable the melee script)
    private void SwitchToGun()
    {
        meleeScript.enabled = false;     // Disable Melee Attack Script
        gunObject.SetActive(true);       // Enable Gun GameObject
    }
}