using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Training : MonoBehaviour
{
    // Booleans to check if the hitpoints are reached
    public bool hitpoint1Reached = false;
    public bool hitpoint2Reached = false;

    // Update is called once per frame
    void Update()
    {
        // Check if both hitpoints are reached and trigger the 'next' button
        if (hitpoint1Reached && hitpoint2Reached)
        {
            TriggerNextButton();
        }
    }

    // Triggered when something enters the trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the local player that enters the trigger
        if (other.gameObject.name == "character_test4_local")
        {
            // Check if the player has entered either of the teleportation hit points
            if (other.gameObject.name == "TeleportHitPoint_1")
            {
                hitpoint1Reached = true;
                Debug.Log("Teleportation Hit Point 1 reached by local player");
            }
            else if (other.gameObject.name == "TeleportHitPoint_2")
            {
                hitpoint2Reached = true;
                Debug.Log("Teleportation Hit Point 2 reached by local player");
            }
        }
    }

    // Call this method to proceed to the next part of the training
    void TriggerNextButton()
    {
        Debug.Log("Both Teleportation Hit Points reached by local player. Proceed to the next stage.");
        // Here you can add the code to enable the next button or trigger the next stage
    }
}