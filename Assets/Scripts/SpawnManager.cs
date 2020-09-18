using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpawnManager : MonoBehaviour
{
    public GameObject target;
    public List<Transform> spawnPoints;

    private int currSpawnPoint;
    private Transform targetTransform;

    // Start is called before the first frame update
    void Start()
    {
        targetTransform = Instantiate(target, target.transform.position, target.transform.rotation).transform;
        targetTransform.gameObject.GetComponent<TargetController>().OnCollected += ChangeTargetLocation;
        SetTargetLocation(true);

    }

    public void ChangeTargetLocation() => SetTargetLocation();


    private void SetTargetLocation(bool firstSpawn = false)
    {
        int r = 0;
        do
        {
            r = Random.Range(0, spawnPoints.Count);
        } while (!firstSpawn && r == currSpawnPoint);
        currSpawnPoint = r;
        targetTransform.position = spawnPoints[currSpawnPoint].position;
    }

    [ContextMenu("Get All Spawn Points")]
    void GetAllSpawnPoints()
    {
        spawnPoints = FindObjectsOfType<Transform>()
            .Where(t => t.tag == "SpawnPoint").ToList();
        
    }
}
