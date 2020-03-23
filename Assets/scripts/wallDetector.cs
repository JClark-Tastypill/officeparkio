using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wallDetector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "obstacle")
        {
            //Debug.Log("wall dected");
            if(!transform.parent.GetComponent<enemySwiper>().isAgro)
            {
                //transform.parent.GetComponent<enemySwiper>().newTarget();
            }
            
        }
    }

}
