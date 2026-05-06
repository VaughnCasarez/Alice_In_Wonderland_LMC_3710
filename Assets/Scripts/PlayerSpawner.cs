using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance.playerPrefab != null)
        {
            Instantiate(
                GameManager.Instance.playerPrefab,
                transform.position,
                transform.rotation
            );
        }
        else
        {
            Debug.LogError("No player prefab set in GameManager!");
        }
    }
}