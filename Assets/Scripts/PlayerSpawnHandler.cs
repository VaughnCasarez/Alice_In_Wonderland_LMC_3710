using UnityEngine;

public class PlayerSpawnHandler : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Set position to (0, 0, 0)
            player.transform.position = Vector3.zero;

            // Restore scale
            float x = PlayerPrefs.GetFloat("PlayerScaleX", 1f);
            float y = PlayerPrefs.GetFloat("PlayerScaleY", 1f);
            float z = PlayerPrefs.GetFloat("PlayerScaleZ", 1f);

            player.transform.localScale = new Vector3(x, y, z);
        }
    }
}