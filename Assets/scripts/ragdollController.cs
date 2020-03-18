using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ragdollController : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator anim;
    public GameObject myParent, myChair, myCam;
    public float clearTime, clearTimer;
    public bool ragdollActive, isPlayer;

    void Start()
    {
        //SetKinematic(true);
    }
    private void OnEnable()
    {
        SetKinematic(true);
        setColliders(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire2"))
        {
            //startRagdoll();
        }
        if(ragdollActive)
        {
            if(Time.time >= clearTimer)
            {
                if (!isPlayer)
                {
                    cleanRagdoll();
                }
                
            }
        }
    }

    void SetKinematic(bool newValue) 
    {
        //Debug.Log("switch");
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in bodies)
        {
            //rb. = newValue;
            rb.isKinematic = newValue;
        }

    }

    void setColliders(bool newValue) // 
    {
        Collider[] collides = GetComponentsInChildren<Collider>();
        foreach (Collider c in collides)
        {
            //rb. = newValue;
            c.enabled = newValue;
        }
    }

    void animationToggle(bool newValue)
    {
        GetComponent<Animator>().enabled = newValue;
    }

    public void toggleChairColliders(bool newValue)
    {
        myParent.GetComponent<BoxCollider>().enabled = newValue;
        Collider[] collides = myChair.GetComponentsInChildren<Collider>();
        foreach (Collider c in collides)
        {
            //rb. = newValue;
            c.enabled = !newValue;
        }

    }

    public void startRagdoll()
    {
        //Time.timeScale = .2f;

        SetKinematic(false);
        setColliders(true);
        animationToggle(false);
        toggleChairColliders(false);
        myCam.GetComponent<followPlayer>().isWatchingRagdoll = true;
        clearTimer = Time.time + clearTime;
        ragdollActive = true;
    }


    public void startPlayerRagdoll()
    {
        //Debug.Log("player falls");
        SetKinematic(false);
        setColliders(true);
        animationToggle(false);
        toggleChairColliders(false);
        myCam.GetComponent<followPlayer>().isWatchingRagdoll = true;
        clearTimer = Time.time + clearTime;
        ragdollActive = true;
    }


    public void startEnemyRagdoll()
    {
        SetKinematic(false);
        setColliders(true);
        animationToggle(false);
        toggleChairColliders(false);
        clearTimer = Time.time + clearTime;
        ragdollActive = true;
        //myCam.GetComponent<followPlayer>().isWatchingRagdoll = true;
    }

    public void cleanRagdoll()
    {

        toggleChairColliders(true);
        gameObject.SetActive(false);
        myParent.gameObject.SetActive(false);
        
        
    }
}
