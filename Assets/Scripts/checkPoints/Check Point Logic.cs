using UnityEngine;

/// <summary>
/// Controls checkpoint behavior in the game.
/// Detects when a player reaches a checkpoint and registers it with the GameManager.
/// </summary>
public class CheckPointLogic : MonoBehaviour
{
    /// <summary>
    /// Called when another collider enters the trigger collider attached to this object.
    /// If the entering object is tagged "Player", it registers the checkpoint with the GameManager
    /// and removes this checkpoint object from the game.
    /// </summary>
    /// <param name="other">The collider that entered the trigger area.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager manager = FindObjectOfType<GameManager>();
            manager?.RegisterCheckpoint();

            Destroy(gameObject);
        }
    }
}
