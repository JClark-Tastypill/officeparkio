using System.Collections;
using System.Collections.Generic;
using Tastypill.Debug;
using UnityEngine;

public class fallFromObstacles : MonoBehaviour
{

    public float fallOffVel, maxVel;
    public GameObject myHuman;
    public float exForce, exRadius;
    public bool isPlayer, timeToDie;
    public float basicProbability, extraChance;
    public float myVel;
    public bool onColCD;
    public float colCDTime, colCDTimestamp, bumpCD, bumpCDStamp;
    //public float bigBumpF, mediumBumpF, smallBumpF;
    public bool isBumped;
    public Color myDebugColor;
    public int debugPlayerNum;
    public float minReflectValue; // if dot product is below this, do opposite bump
    public DebugFloat bigBumpF = 70f, mediumBumpF = 40f, smallBumpF = 15f, fallOffVelocity = 30f;

    // Start is called before the first frame update
    void Start()
    {
        DebugMenu.instance.CreateDebugSlider("small bump force", Color.white, smallBumpF, 100, 0);
        DebugMenu.instance.CreateDebugSlider("medium bump force", Color.white, mediumBumpF, 100, 0);
        DebugMenu.instance.CreateDebugSlider("large bump force", Color.white, bigBumpF, 100, 0);
        DebugMenu.instance.CreateDebugSlider("fall out of chair velocity", Color.white, fallOffVelocity, 100, 0);
    }

    void FixedUpdate()
    {
        myVel = GetComponent<Rigidbody>().velocity.magnitude;

    }

    // Update is called once per frame
    void Update()
    {
        if(onColCD)
        {
            if(Time.time >= colCDTimestamp)
            {
                onColCD = false;
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (isPlayer) // player logic
        {
           if (collision.gameObject.tag == "Player" && !onColCD) //when colliding with other players, bump both of them 
            {               
                //Debug.Log("player bumped into: " + collision.gameObject.tag);
                if (!GetComponent<PlayerMovement>().canKick && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                {
                    //Debug.Log("big bump should happen");
                    doBump(collision, bigBumpF);
                }
                if (!GetComponent<PlayerMovement>().canKick && collision.gameObject.GetComponent<enemySwiper>().isDashing)
                {
                    //Debug.Log("medium bump should happen");
                    doBump(collision, mediumBumpF);
                }
                if (GetComponent<PlayerMovement>().canKick)
                {
                    //Debug.Log("small bump should happen");
                    doBump(collision, smallBumpF);
                }
                goOnCD();
            }
            if (collision.gameObject.tag == "obstacle") //when obstacles, see if they can fall off, otherwise bounce off
            {
                if(GetComponent<PlayerMovement>().beingBumped)
                {
                    if (GetComponent<PlayerMovement>().lastPlayerInput.magnitude >= fallOffVelocity)
                    {
                        startDeath();
                    }
                    else
                    {
                        bounceOffWall(collision);
                    }
                }
                else
                {
                    bounceOffWall(collision);
                }
            }
        }

        else //enemy logic
        {
            if (collision.gameObject.tag == "Player" && !onColCD) //when colliding with other players, bump both of them 
            {
                if (collision.gameObject.GetComponent<fallFromObstacles>().isPlayer)
                {
                    if (GetComponent<enemySwiper>().isDashing && collision.gameObject.GetComponent<PlayerMovement>().canKick)
                    {
                        //Debug.Log("big bump should happen");
                        doBump(collision, bigBumpF);
                    }
                    if (GetComponent<enemySwiper>().isDashing && !collision.gameObject.GetComponent<PlayerMovement>().canKick)
                    {
                        //Debug.Log("medium bump should happen");
                        doBump(collision, mediumBumpF);
                    }
                    if (!GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("small bump should happen");
                        doBump(collision, smallBumpF);
                    }
                }
                else
                {
                    if (GetComponent<enemySwiper>().isDashing && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("big bump should happen");
                        //enemyDoBump(collision, 1);
                        doBump(collision, bigBumpF);
                    }
                    if (GetComponent<enemySwiper>().isDashing && collision.gameObject.GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("medium bump should happen");
                        //enemyDoBump(collision, 2);
                        doBump(collision, mediumBumpF);
                    }
                    if (!GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("small bump should happen");
                        //enemyDoBump(collision, 3);
                        doBump(collision, smallBumpF);
                    }
                }
                goOnCD();
            }
            if (collision.gameObject.tag == "obstacle") //when obstacles, see if they can fall off
            {
                if (GetComponent<enemySwiper>().beingBumped)
                {
                    if (GetComponent<enemySwiper>().curTargetDir.magnitude >= fallOffVelocity)
                    {
                        startDeath();
                    }
                    else
                    {
                        Debug.Log("enemy reflect!");
                        bounceOffWall(collision);
                    }
                }
                else
                {
                    GetComponent<enemySwiper>().determineState();
                }
            }
        }
    }

    public void bounceOffWall(Collision c)
    {
        ContactPoint contact = c.GetContact(0);
        Vector3 bounceDir = contact.normal;
        if(isPlayer)
        {
            float curForce = GetComponent<PlayerMovement>().lastPlayerInput.magnitude;
            GetComponent<PlayerMovement>().lastPlayerInput = bounceDir * curForce;
            Debug.DrawRay(this.transform.position, bounceDir * 100f, Color.cyan, 5f);
        }
        else
        {
            GetComponent<enemySwiper>().curTargetDir = bounceDir * GetComponent<enemySwiper>().curMoveSpeed;
            Debug.DrawRay(this.transform.position, bounceDir * GetComponent<enemySwiper>().curMoveSpeed, Color.cyan, 5f);
        }
        
        Debug.Log("I am bouncing like a good boy");
    }

    public void goOnCD()
    {
        colCDTimestamp = Time.time + colCDTime;
        onColCD = true;
    }

    public void startDeath()
    {
        timeToDie = true; ;

        if (isPlayer)
        {
            GetComponent<PlayerMovement>().isAlive = false;
            myHuman.GetComponent<ragdollController>().startPlayerRagdoll();
        }
        else
        {
            GetComponent<enemySwiper>().isAlive = false;
            myHuman.GetComponent<ragdollController>().startEnemyRagdoll();
        }
    }

    public void doBump(Collision c, float bForce)
    {
        ContactPoint contact = c.GetContact(0);
        if(c.gameObject.GetComponent<fallFromObstacles>().isPlayer) // when an AI collides with a player
        {

            Vector3 newDir = -contact.normal;
            if (newDir != Vector3.zero) // make sure player has movement vector
            {
                Debug.DrawRay(this.transform.position, newDir * 500f, Color.white, 5f);
                c.gameObject.GetComponent<PlayerMovement>().getBumped(newDir, bForce);
            }
            else // otherwise just use vector of AI
            {
                Debug.Log("hi");
                Debug.DrawRay(this.transform.position, newDir * 500f, Color.white, 5f);
                c.gameObject.GetComponent<PlayerMovement>().getBumped(transform.forward, bForce);
            }
        }
        else //when either player or AI bumps into player
        {
            if(gameObject.GetComponent<fallFromObstacles>().isPlayer)
            {
                Vector3 newDir = -contact.normal;
                if (newDir != Vector3.zero) // make sure player has movement vector
                {
                    Debug.DrawRay(this.transform.position, newDir * 500f, Color.white, 5f);
                    c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, bForce);
                }
                else // otherwise just use vector of AI
                {
                    Debug.DrawRay(this.transform.position, newDir * 500f, Color.white, 5f);
                    c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, bForce);
                }
            }
            else
            {
                Vector3 newDir = -contact.normal;
                Debug.DrawRay(this.transform.position, newDir * 500f, myDebugColor, 5f);
                c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, bForce);
            }            

        }
    }
}
