using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shoe_rotate : MonoBehaviour
{
    public GameObject R_shoe;
    public GameObject L_shoe;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            R_shoe.transform.Rotate(new Vector3(0, 10, 0), Space.Self);
            L_shoe.transform.Rotate(new Vector3(0, 10, 0), Space.Self);
        }
           
    }
}
