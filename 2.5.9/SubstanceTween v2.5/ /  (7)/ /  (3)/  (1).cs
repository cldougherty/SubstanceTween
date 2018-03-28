using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedPrefabInstantiate : MonoBehaviour {

    public GameObject animatedPrefab;
    public GameObject spawnPrefab;
    public Transform spawnLocation;
    public bool spawnOnlyOnce;
    int objectsSpawned;
    public int spawnLimit = 5;
    public float uniqueObjectLimit = 5;
    public List<ProceduralMaterial> uniqueSpawnedObjects;

    // Use this for initialization
    void Start()
    {
        uniqueSpawnedObjects = new List<ProceduralMaterial>();
        if (spawnLocation == null)
            spawnLocation = this.transform;
        objectsSpawned = 0;
    }

    void OnTriggerEnter(Collider other)
    {
     if ((!spawnOnlyOnce && objectsSpawned <= spawnLimit) || (spawnOnlyOnce && objectsSpawned < 1))
        {
            objectsSpawned++;
        }
    }

    /*private void OnTriggerExit(Collider other)
    {
        if ((!spawnOnlyOnce && objectsSpawned <= spawnLimit) || (spawnOnlyOnce && objectsSpawned <= 1))
        {
            Instantiate(spawnPrefab, this.transform, false);
        }
    }*/

    private void OnTriggerExit(Collider other)
    {
        if ((!spawnOnlyOnce && objectsSpawned <= spawnLimit) || (spawnOnlyOnce && objectsSpawned <= 1))
        {
         GameObject lastSpawnedObject =  Instantiate(spawnPrefab, this.transform, false);
            if (uniqueSpawnedObjects.Count < uniqueObjectLimit)
            {
                lastSpawnedObject.GetComponent<PrefabProperties>().useSharedMaterial = false;
                uniqueSpawnedObjects.Add(lastSpawnedObject.GetComponent<Renderer>().material as ProceduralMaterial);
            }
            if (uniqueSpawnedObjects.Count >= uniqueObjectLimit)
            {
                lastSpawnedObject.GetComponent<PrefabProperties>().useSharedMaterial = true;
                lastSpawnedObject.GetComponent<PrefabProperties>().isCustomInstance = true;
                InstanceSubstance(lastSpawnedObject, uniqueSpawnedObjects[Random.Range(0, uniqueSpawnedObjects.Count)]);
            }
                //lastSpawnedObject.GetComponent<Renderer>().sharedMaterial = uniqueSpawnedObjects[Random.Range(0, uniqueSpawnedObjects.Count)];
        }
    }

    private void InstanceSubstance(GameObject go, ProceduralMaterial mat)
    {
        GameObject tmpGo = new GameObject("_tmpObj");
        MeshRenderer tmpRenderer = tmpGo.AddComponent<MeshRenderer>();

        if (uniqueSpawnedObjects.Count < uniqueObjectLimit)
        {
            tmpRenderer.material = mat;
            go.GetComponent<MeshRenderer>().material = tmpRenderer.material;
        }
        if (uniqueSpawnedObjects.Count >= uniqueObjectLimit )
        {
            tmpRenderer.sharedMaterial = mat;
            go.GetComponent<MeshRenderer>().sharedMaterial = tmpRenderer.sharedMaterial;
        }
            Destroy(tmpGo);
    }
}
