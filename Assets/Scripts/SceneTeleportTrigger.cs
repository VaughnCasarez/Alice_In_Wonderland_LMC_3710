using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleportTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int sceneToLoadIndex = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Store player scale before scene change
            Vector3 playerScale = other.transform.localScale;

            // Save scale using PlayerPrefs (simple persistence)
            PlayerPrefs.SetFloat("PlayerScaleX", playerScale.x);
            PlayerPrefs.SetFloat("PlayerScaleY", playerScale.y);
            PlayerPrefs.SetFloat("PlayerScaleZ", playerScale.z);

            PlayerPrefs.Save();

            // Load next scene
            SceneManager.LoadScene(sceneToLoadIndex);
        }
    }
}