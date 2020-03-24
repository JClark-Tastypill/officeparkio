using System.Collections;
using System.Collections.Generic;
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
    public float bigBumpForce, mediumBumpForce, smallBumpForce;
    public bool isBumped;
    public Color myDebugColor;
    public int debugPlayerNum;
    public float minReflectValue; // if dot product is below this, do opposite bump
    // Start is called before the first frame update
    void Start()
    {
        
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
            //Debug.Log("play collides with: " + collision.gameObject.tag + ", at velocity: " + GetComponent<PlayerMovement>().lastPlayerInput.magnitude + ", and being bumped = " + GetComponent<PlayerMovement>().beingBumped);
            if (collision.gameObject.tag == "Player" && !onColCD) //when colliding with other players, bump both of them 
            {               
                //Debug.Log("player bumped into: " + collision.gameObject.tag);
                if (!GetComponent<PlayerMovement>().canKick && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                {
                    //Debug.Log("big bump should happen");
                    //playerDoBump(collision, 1);
                    doBump(collision, bigBumpForce);
                }
                if (!GetComponent<PlayerMovement>().canKick && collision.gameObject.GetComponent<enemySwiper>().isDashing)
                {
                    //Debug.Log("medium bump should happen");
                    //playerDoBump(collision, 2);
                    doBump(collision, mediumBumpForce);
                }
                if (GetComponent<PlayerMovement>().canKick)
                {
                    //Debug.Log("small bump should happen");
                    //playerDoBump(collision, 3);
                    doBump(collision, smallBumpForce);
                }
            }
            if (collision.gameObject.tag == "obstacle") //when obstacles, see if they can fall off, otherwise bounce off
            {
                if(GetComponent<PlayerMovement>().beingBumped)
                {
                    if (GetComponent<PlayerMovement>().lastPlayerInput.magnitude >= fallOffVel)
                    {
                        startDeath();
                    }
                    else
                    {
                        Vector3 reflectDir = Vector3.Reflect(GetComponent<PlayerMovement>().lastPlayerInput, collision.GetContact(0).point.normalized);
                        GetComponent<PlayerMovement>().lastPlayerInput = reflectDir;
                    }
                }
                else
                {
                    Vector3 reflectDir = Vector3.Reflect(GetComponent<PlayerMovement>().lastPlayerInput, collision.GetContact(0).point.normalized);
                    GetComponent<PlayerMovement>().lastPlayerInput = reflectDir;
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
                        doBump(collision, bigBumpForce);
                    }
                    if (GetComponent<enemySwiper>().isDashing && !collision.gameObject.GetComponent<PlayerMovement>().canKick)
                    {
                        //Debug.Log("medium bump should happen");
                        doBump(collision, mediumBumpForce);
                    }
                    if (!GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("small bump should happen");
                        doBump(collision, smallBumpForce);
                    }
                }
                else
                {
                    if (GetComponent<enemySwiper>().isDashing && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("big bump should happen");
                        //enemyDoBump(collision, 1);
                        doBump(collision, bigBumpForce);
                    }
                    if (GetComponent<enemySwiper>().isDashing && collision.gameObject.GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("medium bump should happen");
                        //enemyDoBump(collision, 2);
                        doBump(collision, mediumBumpForce);
                    }
                    if (!GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("small bump should happen");
                        //enemyDoBump(collision, 3);
                        doBump(collision, smallBumpForce);
                    }
                }
            }
            if (collision.gameObject.tag == "obstacle") //when obstacles, see if they can fall off
            {
                if (GetComponent<enemySwiper>().beingBumped)
                {
                    if (GetComponent<enemySwiper>().curTargetDir.magnitude >= fallOffVel)
                    {
                        startDeath();
                    }
                    else
                    {
                        Vector3 reflectDir = Vector3.Reflect(GetComponent<enemySwiper>().curTargetDir, collision.GetContact(0).point.normalized);
                        GetComponent<enemySwiper>().curTargetDir = reflectDir;
                    }
                }
                else
                {
                    GetComponent<enemySwiper>().determineState();
                }
            }
        }
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

  /*  public void enemyBumpEnemy(Collision c)
    {
        ContactPoint contact = c.GetContact(0);
        float dot = Vector3.Dot(GetComponent<enemySwiper>().curTargetDir.normalized, c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized);
        Debug.Log("player " + debugPlayerNum + " dot is " + dot);
        Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.normal);
        //newDir = newDir.normalized;
        Debug.Log("my reflection " + newDir);
        Debug.DrawRay(this.transform.position, newDir * 500f, myDebugColor, 5f);
        c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, smallBumpForce);
    }*/

    public void doBump(Collision c, float bForce)
    {
        ContactPoint contact = c.GetContact(0);    
        
        if(c.gameObject.GetComponent<fallFromObstacles>().isPlayer) // when an AI collides with a player
        {

            Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<PlayerMovement>().lastPlayerInput.normalized, contact.normal);
            newDir = newDir.normalized;
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
            /*for debugging, trying to figure out dot product for collisions
            float dot = Vector3.Dot(GetComponent<enemySwiper>().curTargetDir.normalized, contact.point.normalized);
            Debug.Log("player " + debugPlayerNum + " dot to point is " + dot);
            float dot2 = Vector3.Dot(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.point.normalized);
            Debug.Log("player " + debugPlayerNum + " dot to other player is " + dot2);


            bool isOpposite = ((dot > 0 && dot2 < 0) || (dot < 0 && dot2 > 0)); //when dot products are opposite, players bounce off each other normally*/
            if(gameObject.GetComponent<fallFromObstacles>().isPlayer)
            {
                //Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.normal);
                Vector3 myDir = GetComponent<PlayerMovement>().lastPlayerInput.normalized;
                if (myDir != Vector3.zero) // make sure player has movement vector
                {
                    Debug.DrawRay(this.transform.position, myDir * 500f, Color.white, 5f);
                    c.gameObject.GetComponent<enemySwiper>().goToBumped(myDir, bForce);
                }
                else // otherwise just use vector of AI
                {
                    Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.normal);
                    Debug.DrawRay(this.transform.position, newDir * 500f, Color.white, 5f);
                    c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, bForce);
                }
            }
            else
            {
                //Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.normal);
                Vector3 myDir = GetComponent<enemySwiper>().curTargetDir.normalized;
                Debug.DrawRay(this.transform.position, myDir * 500f, myDebugColor, 5f);
                c.gameObject.GetComponent<enemySwiper>().goToBumped(myDir, bForce);
            }            

        }
    }
}
