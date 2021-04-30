using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{

    public float speed = 1.0F;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Jump") != 0) {
            transform.position += speed * new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Jump"), Input.GetAxisRaw("Vertical")) * Time.deltaTime;
        }

        if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0){
            transform.Rotate((new Vector3(- Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"), 0)));
        }
    }
}
