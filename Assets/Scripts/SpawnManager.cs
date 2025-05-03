using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public GameObject powerUpPrefab;    
    public GameObject AddHealthPrefab;             


    public Transform spawnPoint;
    private PlayerController playerController;

    void Start()
    {
        
        InvokeRepeating("SpawnObstacle", 2f, 3f);

        
        InvokeRepeating("SpawnPowerUp", 5f, 30f); 
        InvokeRepeating("SpawnAddHealth", 10f, 50f); 


        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    void SpawnObstacle()
    {
        if (!playerController.IsGameOver)
        {
            Instantiate(obstaclePrefab, spawnPoint.position, obstaclePrefab.transform.rotation);
        }
    }

    void SpawnPowerUp()
    {
        if (!playerController.IsGameOver)
        {
            Instantiate(powerUpPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
    void SpawnAddHealth()
    {
        if (!playerController.IsGameOver)
        {
            Instantiate(AddHealthPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    
}