using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class globalFlock : MonoBehaviour
{
    public GameObject fishObj;
    static int numFish = 20;
    public int tankSize = 5;
    public static GameObject[] fish = new GameObject[numFish];
    public float maxNeighborDist = 5.0F;
    public float avoidDist = 3.0F;
    public float rotationSpeed = 3.0F;
    public int start_x = 62;
    public int start_y = 50;
    public int start_z = 237;

    public Vector3 goal = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        goal = new Vector3(start_x, start_y, start_z);
        for (int i = 0; i < numFish; i++){
            Vector3 pos = new Vector3(Random.Range(start_x - tankSize, start_x + tankSize),
                                      Random.Range(start_y, start_y + 2 * tankSize),
                                      Random.Range(start_z - tankSize, start_z + tankSize));
            fish[i] = (GameObject) Instantiate(fishObj, pos, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0, 5) < 1){
            move_all();
        }
        
        if (Random.Range(0, 100) < 1) {
            goal = new Vector3(Random.Range(start_x - tankSize, start_x + tankSize),
                                      Random.Range(start_y, start_y + 2 * tankSize),
                                      Random.Range(start_z - tankSize, start_z + tankSize));
        }
    }

    void move_all(){

        //TODO: set a limit on where the fish can travel to

        // go through each fish
        foreach (GameObject f1 in fish) {
            Vector3 center = Vector3.zero;
            Vector3 avoid = Vector3.zero;
            int group = 0;
            float newSpeed = 0.1F;
            flock fish1 = f1.GetComponent<flock>();

            // go through each fish again
            foreach(GameObject f2 in fish) {
                if (f1 != f2) {

                    // find the distance to see if they're in a group
                    float distance = Vector3.Distance(f1.transform.position, f2.transform.position);
                    if (distance <= maxNeighborDist){
                        center += f2.transform.position;
                        group++;

                        // if they're too close use the avoid equation
                        if (distance < avoidDist){
                            avoid = avoid + (f2.transform.position - f1.transform.position);
                        }
                        flock fish2 = f2.GetComponent<flock>();
                        newSpeed += fish2.speed;
                    }
                }
            }

            if (group > 0){

                // use the center equation
                center = center / group + (goal - f1.transform.position);

                // find the new speed, there's a cap of 2.0
                fish1.speed = (newSpeed/group <= 2.0F) ? newSpeed/group : 2.0F;
                
                // Use the direction equation to get the rotation of the fish
                Vector3 direction = (center + avoid) - fish1.transform.position;
                if (direction != Vector3.zero) {
                    fish1.transform.rotation = Quaternion.Slerp(fish1.transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
}
