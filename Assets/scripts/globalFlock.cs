using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class globalFlock : MonoBehaviour
{
    public GameObject fishObj;
    static int numFish = 10;
    public static int tankSize = 5;
    public static GameObject[] fish = new GameObject[numFish];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numFish; i++){
            Vector3 pos = new Vector3(Random.Range(-tankSize, tankSize),
                                      Random.Range(-tankSize, tankSize),
                                      Random.Range(-tankSize, tankSize));
            fish[i] = (GameObject) Instantiate(fishPrefab, pos, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
