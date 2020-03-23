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
            Debug.Log("play collides with: " + collision.gameObject.tag + ", at velocity: " + GetComponent<PlayerMovement>().lastPlayerInput.magnitude + ", and being bumped = " + GetComponent<PlayerMovement>().beingBumped);
            if (collision.gameObject.tag == "Player" && !onColCD) //when colliding with other players, bump both of them 
            {               
                //Debug.Log("player bumped into: " + collision.gameObject.tag);
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
                if (GetComponent<PlayerMovement>().canKick)
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

        else //enemy logic
        {
            if (collision.gameObject.tag == "Player" && !onColCD) //when colliding with other players, bump both of them 
            {
                if (collision.gameObject.GetComponent<fallFromObstacles>().isPlayer)
                {
                    if (GetComponent<enemySwiper>().isDashing && collision.gameObject.GetComponent<PlayerMovement>().canKick)
                    { 
                        //Debug.Log("big bump should happen");
                        enemyDoBump(collision, 1);
                    }
                    if (GetComponent<enemySwiper>().isDashing && !collision.gameObject.GetComponent<PlayerMovement>().canKick)
                    {
                        //Debug.Log("medium bump should happen");
                        enemyDoBump(collision, 2);
                    }
                    if (!GetComponent<enemySwiper>().isDashing)
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
                        //enemyBumpEnemy(collision);
                    }
                    if (GetComponent<enemySwiper>().isDashing && collision.gameObject.GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("medium bump should happen");
                        enemyDoBump(collision, 2);
                        //enemyBumpEnemy(collision);
                    }
                    if (!GetComponent<enemySwiper>().isDashing)
                    {
                        //Debug.Log("small bump should happen");
                        enemyDoBump(collision, 3);
                        //enemyBumpEnemy(collision);
                    }
                }
            }
            if (collision.gameObject.tag == "obstacle") //when obstacles, see if they can fall off
            {
                if (GetComponent<enemySwiper>().curMoveSpeed >= fallOffVel && GetComponent<enemySwiper>().beingBumped)// && GetComponent<swipeToMove>().isAlive)
                {
                    startDeath();
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
        Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.normal);
        newDir = newDir.normalized;
        Debug.DrawRay(this.transform.position, newDir * 500f, Color.black, 5f);
        //Debug.DrawRay(this.transform.position, reflectDir * 500f, Color.black, 5f);

        if (bumpType == 1) // do big bump to another player
        {
            c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, bigBumpForce);
        }

        if (bumpType == 2) // medium bump to both players
        {
            c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, mediumBumpForce);
        }

        if (bumpType == 3) // small bump to both players
        {
            c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, smallBumpForce);

        }
        colCDTimestamp = Time.time + colCDTime;
        onColCD = true;
    }

    public void enemyDoBump(Collision c, int bumpType)
    {
        Debug.Log("enemy do a bump " + bumpType);
        ContactPoint contact = c.GetContact(0);
        Debug.Log("my contact: " + contact.point);
        if (bumpType == 1) // do big bump to another player
        {
            if (c.gameObject.GetComponent<fallFromObstacles>().isPlayer)
            {
                Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<PlayerMovement>().lastPlayerInput.normalized, contact.normal); 
                newDir = newDir.normalized;
                if (newDir != Vector3.zero)
                {
                    Debug.DrawRay(this.transform.position, newDir * 500f, myDebugColor, 5f);
                    c.gameObject.GetComponent<PlayerMovement>().getBumped(newDir, bigBumpForce);
                }
                else
                {
                    c.gameObject.GetComponent<PlayerMovement>().getBumped(transform.forward, bigBumpForce);
                }                
            }
            else
            {
                Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.normal);
                newDir = newDir.normalized;
                Debug.DrawRay(this.transform.position, newDir * 500f, myDebugColor, 5f);
                c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, bigBumpForce);
            }
        }

        if (bumpType == 2) // medium bump to both players
        {
            if (c.gameObject.GetComponent<fallFromObstacles>().isPlayer)
            {
                Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<PlayerMovement>().lastPlayerInput.normalized, contact.normal);
                newDir = newDir.normalized;
                if (newDir != Vector3.zero)
                {
                    Debug.DrawRay(this.transform.position, newDir * 500f, myDebugColor, 5f);
                    c.gameObject.GetComponent<PlayerMovement>().getBumped(newDir, bigBumpForce);
                }
                else
                {
                    c.gameObject.GetComponent<PlayerMovement>().getBumped(transform.forward, bigBumpForce);
                }
                c.gameObject.GetComponent<PlayerMovement>().getBumped(newDir, mediumBumpForce);
            }
            else
            {
                Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.normal) ;
                newDir = newDir.normalized;
                Debug.DrawRay(this.transform.position, newDir * 500f, myDebugColor, 5f);
                c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, mediumBumpForce);
            }
        }

        if (bumpType == 3) // small bump to both players
        {
            if (c.gameObject.GetComponent<fallFromObstacles>().isPlayer)
            {
                Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<PlayerMovement>().lastPlayerInput.normalized, contact.normal);
                newDir = newDir.normalized;
                if (newDir != Vector3.zero)
                {
                    Debug.DrawRay(this.transform.position, newDir * 500f, myDebugColor, 5f);
                    c.gameObject.GetComponent<PlayerMovement>().getBumped(newDir, bigBumpForce);
                }
                else
                {
                    c.gameObject.GetComponent<PlayerMovement>().getBumped(transform.forward, bigBumpForce);
                }
            }
            else
            {
                Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.normal); 
                newDir = newDir.normalized;
                Debug.DrawRay(this.transform.position, newDir * 500f, Color.black, 5f);
                c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, smallBumpForce);
            }
      
        }
        colCDTimestamp = Time.time + colCDTime;
        onColCD = true;
    }

   /* public void enemyBumpEnemy(Collision c)
    {
        ContactPoint contact = c.GetContact(0);
        Vector3 newDir = Vector3.Reflect(c.gameObject.GetComponent<enemySwiper>().curTargetDir.normalized, contact.normal);
        //newDir = newDir.normalized;
        Debug.Log("my reflection " + newDir);
        Debug.DrawRay(this.transform.position, newDir * 500f, myDebugColor, 5f);
        c.gameObject.GetComponent<enemySwiper>().goToBumped(newDir, smallBumpForce);
    }*/
}
