using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public GameObject nextPlayerPrefab; // assign the NEW prefab here

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Set which prefab should be used in next scene
            GameManager.Instance.playerPrefab = nextPlayerPrefab;

            SceneManager.LoadScene(sceneName);
        }
    }
}