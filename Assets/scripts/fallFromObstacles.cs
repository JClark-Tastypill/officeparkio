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
        //Debug.Log(collision.gameObject.tag);
        if (!onColCD)
        {
            if (isPlayer)
            {                
                if (collision.gameObject.tag == "Player" && !GetComponent<PlayerMovement>().beingBumped) //when colliding with other players, bump both of them 
                {
                    if (!GetComponent<PlayerMovement>().canKick && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("big bump should happen");
                        playerDoBump(collision, 1);
                    }
                    if (!GetComponent<PlayerMovement>().canKick && collision.gameObject.GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("medium bump should happen");
                        playerDoBump(collision, 2);
                    }
                    if (GetComponent<PlayerMovement>().canKick && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("small bump should happen");
                        playerDoBump(collision, 3);
                    }
                }
                if (collision.gameObject.tag == "obstacle" && GetComponent<PlayerMovement>().beingBumped) //when obstacles, see if they can fall off
                {
                    if (GetComponent<PlayerMovement>().lastPlayerInput.magnitude >= fallOffVel)
                    {   
                        startDeath();
                    }
                }
            }

            else
            {
                if(!GetComponent<enemySwiper>().beingBumped)
                {
                    if (collision.gameObject.tag == "Player") //when colliding with other players, bump both of them 
                    {
                        //if high enough speed, do bump
                        if (collision.gameObject.GetComponent<fallFromObstacles>().isPlayer)
                        {
                            if (GetComponent<enemySwiper>().isDashing && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                            {
                                //Debug.Log("big bump should happen");
                                enemyDoBump(collision, 1);
                            }
                            if (GetComponent<enemySwiper>().isDashing && collision.gameObject.GetComponent<enemySwiper>().isDashing)
                            {
                                //Debug.Log("medium bump should happen");
                                enemyDoBump(collision, 2);
                            }
                            if (!GetComponent<enemySwiper>().isDashing && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                            {
                                //Debug.Log("small bump should happen");
                                enemyDoBump(collision, 3);
                            }
                        }
                        else
                        {
                            if (GetComponent<enemySwiper>().isDashing && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                            {
                                //Debug.Log("big bump should happen");
                                enemyDoBump(collision, 1);
                            }
                            if (GetComponent<enemySwiper>().isDashing && collision.gameObject.GetComponent<enemySwiper>().isDashing)
                            {
                                //Debug.Log("medium bump should happen");
                                enemyDoBump(collision, 2);
                            }
                            if (!GetComponent<enemySwiper>().isDashing && !collision.gameObject.GetComponent<enemySwiper>().isDashing)
                            {
                                //Debug.Log("small bump should happen");
                                enemyDoBump(collision, 3);
                            }
                        }
                    }
                
                    
                    //Debug.DrawRay(this.transform.position, collision.relativeVelocity * 50f, Color.white, 5f);
                    //Debug.DrawRay(this.transform.position, collision.impulse * 50f, Color.black, 5f);
                }
                if (collision.gameObject.tag == "obstacle") //when obstacles, see if they can fall off
                {
                    if (GetComponent<enemySwiper>().curMoveSpeed >= fallOffVel && GetComponent<enemySwiper>().beingBumped)// && GetComponent<swipeToMove>().isAlive)
                    {
                        //newCalculateProbability(collision);
                        startDeath();
                    }
                    else
                    {
                        GetComponent<enemySwiper>().determineState();
                    }
                }
            }
        }
    }

    public void goOnCD()
    {
        colCDTimestamp = Time.time + colCDTime;
        onColCD = true;
    }

    public void newCalculateProbability(Collision c)
    {
        float chance = basicProbability + (extraChance * (c.relativeVelocity.magnitude / maxVel));
        if (chance > basicProbability + extraChance)
        {
            chance = basicProbability + extraChance;
            //Debug.Log("chance to fall: " + chance);
        }
        //Debug.Log("chance to fall: " + chance);
        float ranChance = Random.Range(1, 101);
        //Debug.Log("random number is " + ranChance);
        if (ranChance <= chance)
        {
            //timeToDie = true;
            startDeath();
            //deathTimer = Time.time + deathTime;
        }
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

    public void playerDoBump(Collision c, int bumpType)
    {
        Debug.Log("player is bumping:" + bumpType);
        ContactPoint contact = c.GetContact(0);
        Vector3 newDir = new Vector3(contact.point.x - transform.position.x, 0, contact.point.z - transform.position.z);
        newDir = newDir.normalized;
        Debug.DrawRay(this.transform.position, newDir * 500f, Color.white, 5f);

        if (bumpType == 1) // do big bump to another player
        {
            c.gameObject.GetComponent<enemySwiper>().getBumped(newDir, bigBumpForce);
            GetComponent<PlayerMovement>().getBumped(-newDir, smallBumpForce); // bump self backwards
        }

        if (bumpType == 2) // medium bump to both players
        {
            c.gameObject.GetComponent<enemySwiper>().getBumped(newDir, mediumBumpForce);
            GetComponent<PlayerMovement>().getBumped(-newDir, mediumBumpForce); // bump self backwards
        }

        if (bumpType == 3) // small bump to both players
        {
            c.gameObject.GetComponent<enemySwiper>().getBumped(newDir, smallBumpForce);
            GetComponent<PlayerMovement>().getBumped(-newDir, smallBumpForce); // bump self backwards

        }
        colCDTimestamp = Time.time + colCDTime;
        onColCD = true;
    }

    public void enemyDoBump(Collision c, int bumpType)
    {
        Debug.Log("enemy do a bump");
        ContactPoint contact = c.GetContact(0);
        Vector3 newDir = new Vector3(contact.point.x - transform.position.x, 0, contact.point.z - transform.position.z);
        newDir = newDir.normalized;
        Debug.DrawRay(this.transform.position, newDir * 500f, Color.white, 5f);

        if(bumpType == 1) // do big bump to another player
        {
            if (c.gameObject.GetComponent<fallFromObstacles>().isPlayer)
            {
                c.gameObject.GetComponent<PlayerMovement>().getBumped(newDir, bigBumpForce);
            }
            else
            {
                c.gameObject.GetComponent<enemySwiper>().getBumped(newDir, bigBumpForce);
            }
            GetComponent<enemySwiper>().getBumped(-newDir, smallBumpForce); // bump self backwards
        }

        if (bumpType == 2) // medium bump to both players
        {
            if (c.gameObject.GetComponent<fallFromObstacles>().isPlayer)
            {
                c.gameObject.GetComponent<PlayerMovement>().getBumped(newDir, mediumBumpForce);
            }
            else
            {
                c.gameObject.GetComponent<enemySwiper>().getBumped(newDir, mediumBumpForce);
            }
            GetComponent<enemySwiper>().getBumped(-newDir, smallBumpForce); // bump self backwards
        }

        if (bumpType == 3) // small bump to both players
        {
            if (c.gameObject.GetComponent<fallFromObstacles>().isPlayer)
            {
                c.gameObject.GetComponent<PlayerMovement>().getBumped(newDir, smallBumpForce);
            }
            else
            {
                c.gameObject.GetComponent<enemySwiper>().getBumped(newDir, smallBumpForce);
            }
            GetComponent<enemySwiper>().getBumped(-newDir, smallBumpForce); // bump self backwards
      
        }
        colCDTimestamp = Time.time + colCDTime;
        onColCD = true;
    }
}
