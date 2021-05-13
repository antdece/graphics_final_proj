using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flock : MonoBehaviour
{

    public float maxSpeed = 2.0F;
    public float minSpeed = 0.5F;

    public float speed;
    public static GameObject globalScript;
    public float rotationSpeed = 3.0F;

    // Start is called before the first frame update
    void Start()
    {
        globalScript = GameObject.Find("Fish Management");
        speed = Random.Range(minSpeed, maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        globalFlock gf = globalScript.GetComponent<globalFlock>();
        float start_x = gf.start_x;
        float start_y = gf.start_y;
        float start_z = gf.start_z;
        float tankSize = gf.tankSize;
        Vector3 goal = gf.goal;
        transform.Translate(0, 0, Time.deltaTime * speed);
        if (transform.position.x > start_x + tankSize || transform.position.x < start_x - tankSize){
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(goal), 4.0F * Time.deltaTime);
        }
        else if (transform.position.y > start_y + 2 * tankSize || transform.position.y < start_y){
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(goal), 4.0F * Time.deltaTime);
        }
        else if (transform.position.z > start_z + tankSize || transform.position.z < start_z - tankSize){
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(goal), 4.0F * Time.deltaTime);
        }
    }
}
