using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionDetector : MonoBehaviour
{
    public float fallOffVel, maxVel;
    public GameObject myHuman;
    public float exForce, exRadius;
    public bool isPlayer, timeToDie;
    public float basicProbability, extraChance;
    public float myVel;
    public bool onColCD;
    public float colCDTime, colCDTimestamp;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        myVel = GetComponent<Rigidbody>().velocity.magnitude;
        
    }

    private void Update()
    {
        if(onColCD)
        {
            if(Time.time >= colCDTimestamp)
            {
                onColCD = false;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {

        //Debug.Log(collision.gameObject.tag);

        //Debug.Log(collision.relativeVelocity.magnitude);
        if(!onColCD)
        {
            if (isPlayer)
            {
                if (collision.gameObject.tag == "Player") //when colliding with other players, only the slower player has a chance to fall off
                {
                   // Debug.Log("myvel: " + myVel + ", other player vel: " + collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude);

                    if (myVel < collision.gameObject.GetComponent<collisionDetector>().myVel && GetComponent<swipeToMove>().isAlive)
                    {
                        //Debug.Log("player tries to fall over");
                        newCalculateProbability(collision);
                        goOnCD();
                    }
                    else
                    {
                        //Debug.Log("actually they fall over");
                    }
                }
                else
                {
                    if (collision.relativeVelocity.magnitude > fallOffVel)// && GetComponent<swipeToMove>().isAlive)
                    {
                        newCalculateProbability(collision);
                    }
                }
            }
            else
            {
                if (collision.gameObject.tag == "Player") //when colliding with other players, only the slower player has a chance to fall off
                {
                    //Debug.Log("myvel: " + myVel + ", other player vel: " + collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude);

                    if (myVel < collision.gameObject.GetComponent<collisionDetector>().myVel && GetComponent<enemySwiper>().isAlive)
                    {
                        Debug.Log("enemy tries to fall over");
                        newCalculateProbability(collision);
                        goOnCD();
                    }
                }
                else
                {
                    if (collision.relativeVelocity.magnitude > fallOffVel && GetComponent<enemySwiper>().isAlive)
                    {
                        newCalculateProbability(collision);
                    }
                    if (collision.gameObject.tag == "obstacle") //if hitting a wall, find new target
                    {
                        //GetComponent<enemySwiper>().backOff();
                        GetComponent<enemySwiper>().pathFind();
                    }
                }
            }
        }




        /*if (isPlayer)
        {


            

            if (collision.relativeVelocity.magnitude > fallOffVel && GetComponent<swipeToMove>().isAlive)
            {
                calculateProbability(collision);
                if(timeToDie)
                {
                    startDeath();
                    Collider[] colliders = Physics.OverlapSphere(transform.position, exRadius);
                    foreach (Collider hit in colliders)
                    {
                        Rigidbody rb = hit.GetComponent<Rigidbody>();

                        if (rb != null)
                            rb.AddExplosionForce(exForce, transform.position, exRadius, 3);
                    }
                }
                
                
            }
        }
        else // colissions for enemies
        {
            /*if (collision.relativeVelocity.magnitude > fallOffVel && GetComponent<enemySwiper>().isAlive)
            {

                calculateProbability(collision);
                if (timeToDie)
                {
                    startDeath();
                    Collider[] colliders = Physics.OverlapSphere(transform.position, exRadius);
                    foreach (Collider hit in colliders)
                    {
                        Rigidbody rb = hit.GetComponent<Rigidbody>();

                        if (rb != null)
                            rb.AddExplosionForce(exForce, transform.position, exRadius, 3);
                    }
                }


            }
        }*/
        
       
            
    }


    public void startDeath()
    {
        timeToDie = true; ;

        if (isPlayer)
        {
            GetComponent<swipeToMove>().isAlive = false;
        }
        else
        {
            GetComponent<enemySwiper>().isAlive = false;
        }
        if(isPlayer)
        {
            myHuman.GetComponent<ragdollController>().startPlayerRagdoll();
        }
        else
        {
            myHuman.GetComponent<ragdollController>().startEnemyRagdoll();
        }
        
        
    }

    public void calculateProbability(Collision c)
    {

        

        if(c.gameObject.tag == "Player")
        {
            if (c.rigidbody.velocity.magnitude > GetComponent<Rigidbody>().velocity.magnitude)
            {

                float chance = basicProbability + (extraChance * (c.relativeVelocity.magnitude / maxVel));
                if (chance > basicProbability + extraChance)
                {
                    chance = basicProbability + extraChance;
                }
                //Debug.Log("chance to fall: " + chance);
                if (Random.Range(1, 101) <= chance)
                {
                    timeToDie = true;
                    //deathTimer = Time.time + deathTime;
                }
            }
        }

        else
        {
            float chance = basicProbability + (extraChance * (c.relativeVelocity.magnitude / maxVel));
            if (chance > basicProbability + extraChance)
            {
                chance = basicProbability + extraChance;
            }
            //Debug.Log("chance to fall: " + chance);
            if (Random.Range(1, 101) <= chance)
            {
                timeToDie = true;
                //deathTimer = Time.time + deathTime;
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
        Debug.Log("calculating");

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

}
