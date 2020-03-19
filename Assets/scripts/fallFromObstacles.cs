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
    public float bumpForce;
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
                
                if (collision.gameObject.tag == "Player") //when colliding with other players, bump both of them 
                {
                    //Debug.Log(GetComponent<PlayerMovement>().)
                    //if high enough speed, do bump
                    if(GetComponent<PlayerMovement>().lastPlayerInput.magnitude > collision.gameObject.GetComponent<enemySwiper>().curMoveSpeed && !GetComponent<PlayerMovement>().beingBumped)
                    {
                        Debug.Log("player speed: " + GetComponent<PlayerMovement>().lastPlayerInput.magnitude + ", enemy speed: " + collision.gameObject.GetComponent<enemySwiper>().curMoveSpeed);
                        doBump(collision);

                    }
                    
                    //Debug.DrawRay(this.transform.position, collision.relativeVelocity * 500f, Color.red, 5f);
                    //Debug.DrawRay(this.transform.position, collision.impulse * 500f, Color.blue, 5f);
                    //Debug.Log("relativeVel: " + collision.relativeVelocity + ", impulse: " + collision.impulse);
                }
                if (collision.gameObject.tag == "obstacle") //when obstacles, see if they can fall off
                {
                    //if high enough speed, try to fall
                    //newCalculateProbability(collision);
                    //Debug.Log("relativeVel: " + collision.relativeVelocity.magnitude + ", impulse: " + collision.impulse.magnitude);
                    if (GetComponent<PlayerMovement>().lastPlayerInput.magnitude >= fallOffVel && GetComponent<PlayerMovement>().beingBumped)// && GetComponent<swipeToMove>().isAlive)
                    {
                        //newCalculateProbability(collision);
                        startDeath();
                    }
                }

            }
            else
            {
                if(collision.gameObject.tag == "Player") //when colliding with other players, bump both of them 
                {
                    //if high enough speed, do bump
                    if(collision.gameObject.GetComponent<fallFromObstacles>().isPlayer)
                    {
                        if (GetComponent<enemySwiper>().curMoveSpeed > collision.gameObject.GetComponent<PlayerMovement>().lastPlayerInput.magnitude && !GetComponent<enemySwiper>().beingBumped)
                        {
                            Debug.Log("enemy speed: " + GetComponent<enemySwiper>().curMoveSpeed + ", player speed: " + collision.gameObject.GetComponent<PlayerMovement>().lastPlayerInput.magnitude);
                            doBump(collision);
                            
                        }
                    }
                    else
                    {
                        if (GetComponent<enemySwiper>().curMoveSpeed > collision.gameObject.GetComponent<enemySwiper>().curMoveSpeed && !GetComponent<enemySwiper>().beingBumped)
                        {
                            doBump(collision);
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

    public void doBump(Collision c)
    {
        //Debug.DrawRay(this.transform.position, c.impulse * 500f, Color.black, 5f);
        //Debug.Log("impulse: " + c.impulse);
        ContactPoint contact = c.GetContact(0);
        //Vector3 newDir = c.impulse.normalized;
        Vector3 newDir = new Vector3(contact.point.x - transform.position.x, 0, contact.point.z - transform.position.z);
        //Debug.Log("newDir " + newDir);
        newDir = newDir.normalized;
        Debug.DrawRay(this.transform.position, newDir * 500f, Color.white, 5f);
        
        //GetComponent<Rigidbody>().AddForce(newDir * bumpForce, ForceMode.VelocityChange);
       // c.gameObject.GetComponent<Rigidbody>().AddForce(newDir * bumpForce, ForceMode.VelocityChange);
       if(c.gameObject.GetComponent<fallFromObstacles>().isPlayer)
        {
            c.gameObject.GetComponent<PlayerMovement>().getBumped(newDir, bumpForce);
        }
        else
        {
            c.gameObject.GetComponent<enemySwiper>().getBumped(newDir, bumpForce);
        }
        colCDTimestamp = Time.time + colCDTime;
        onColCD = true;
    }
}
