using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemySwiper : MonoBehaviour
{
    public bool isAlive, isWandering, isAgro, hasTarget;
    public float moveCDMin, moveCDMax, moveCDStamp;
    public float agroCDMin, agroCDMax;
    public Vector3 target;
    public GameObject targetEnemy;
    public float minTargetRange, maxTargetRange;
    public float moveSpread;
    public float minMove, maxMove, newTargetDistance;
    public float agroMinMove, agroMaxMove;
    public Rigidbody rb;
    public float agroLeashRange, agroTime, agroCD, agroTimerStamp, agroCDStamp;
    public float rotationSpeed, minRotateThreshhold, agroRotSpeed;
    public float targetCD, targetCDStamp;
    public float ranChanceWorseOption, chanceRunAway;
    public GameObject[] otherPlayers;
    public GameObject closestTarget;
    public float closestTargetDistance;
    public Vector3 closestTargetDir;
    public float rayLength, bestAngleDist, minMoveDistance;
    public bool isSlow;
    public float chanceSlow, slowCDTime, slowCDStamp, slowTime, slowTimeStamp; // chance of being slow, how long the cooldown is before they can be slow again, how long they are slow for
    private bool seesPlayer;
    public float curMoveSpeed;
    public Vector3 curTargetDir;
    public bool beingBumped;
    public float bumpTime, bumpStamp;
    public float dashRange;
    public bool isDashing = true;
    public float dashForce;
    public float dashTime, dashStamp;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        findOthers();
        //newTarget();
        pathFind();
        newMoveSpeed(false);
    }

    // Update is called once per frame
    void Update()
    {
        /*
         *set target destination
         *          - make sure not by  wall/ big thing
         * make random swips towards that area
         * if close enough, set new target
         * if agro by another player, set that player as target while they are in range
         *
         *
         * */

        if (isAlive)
        {

            if(beingBumped)
            {
                if(Time.time >= bumpStamp)
                {
                    beingBumped = false;
                }
            }
            else
            {
                if (isSlow)
                {
                    if (Time.time >= slowTimeStamp)
                    {
                        slowCDStamp = Time.time + slowCDTime;
                        isSlow = false;
                    }
                }

                faceTarget();
                //Debug.DrawLine(this.transform.position, target, Color.white);

                if (isAgro)
                {
                    if (Vector3.Distance(transform.position, targetEnemy.transform.position) > agroLeashRange || Time.time >= agroTimerStamp)
                    {
                        isAgro = false;
                        agroCDStamp = Time.time + agroCD;

                    }

                    if (Time.time >= moveCDStamp)
                    {
                        //target = targetEnemy.transform.position;

                        if(Vector3.Distance(transform.position, targetEnemy.transform.position) <= dashRange)
                        {
                            if(Time.time >= dashStamp)
                            {
                                doDash();
                            }
                            
                        }
                        else
                        {
                            if (seesPlayer)
                            {
                                pathFind();
                                newMoveSpeed(true);
                            }
                            else
                            {
                                randomPathfind();
                                newMoveSpeed(true);
                            }
                        }

                        

                        //swipeEnemy(target);
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, target) <= newTargetDistance || Time.time >= targetCDStamp)
                    //if (Time.time >= moveCDStamp)
                    {
                        if (!isSlow && Time.time >= slowCDStamp)
                        {
                            trySlow();
                        }

                        if (seesPlayer)
                        {
                            pathFind();
                            newMoveSpeed(false);
                            //moveCDStamp = Time.time + m
                        }
                        else
                        {
                            randomPathfind();
                            newMoveSpeed(false);
                        }

                        //swipeEnemy(target);
                    }
                }
            }

            
            /*if (hasTarget)
            {
                Debug.DrawLine(this.transform.position, target, Color.white);
                faceTarget();
                
                if (isAgro)
                {
                    if (Vector3.Distance(transform.position, targetEnemy.transform.position) > agroLeashRange || Time.time >= agroTimerStamp)
                    {
                        isAgro = false;

                    }

                    if (Time.time >= moveCDStamp)
                    {
                        target = targetEnemy.transform.position;
                        swipeEnemy(target);
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, target) <= newTargetDistance || Time.time >= targetCDStamp)
                    {
                        hasTarget = false;
                        //Debug.Log("hit target");
                    }
                    
                    if (Time.time >= moveCDStamp)
                    {

                        swipeEnemy(target);
                    }
                }

            }
            else
            {
                //newTarget();
                pathFind();

            }*/
        }

    }

    public void FixedUpdate()
    {
        if (target != null && isAlive)
        {
            steadyMove(target);
        }
    }

    public void checkClosestPlayer() // run before each move, find closest player
    {
        //findOthers();
        seesPlayer = false;
        closestTarget = null;
        foreach (GameObject p in otherPlayers)
        {

            RaycastHit inf = new RaycastHit();
            Vector3 pDir = p.transform.position - transform.position;
            //Debug.Log("hi " + Time.time);
            if (p != this.gameObject)
            {
                if (p.GetComponent<collisionDetector>().isPlayer)
                {
                    if (closestTarget == null)
                    {

                        //Debug.Log("can draw line to play: " + Physics.Raycast(transform.position, pDir, out inf, pDir.magnitude, LayerMask.GetMask("wall", "big stuff")));
                        // Debug.DrawRay(this.transform.position, pDir * pDir.magnitude, Color.blue, 2f);
                        //Debug.Log(Physics.Linecast(transform.position, p.transform.position));
                        if (!Physics.Raycast(transform.position, pDir, out inf, pDir.magnitude, LayerMask.GetMask("wall", "big stuff")))
                        {
                            closestTarget = p;
                            closestTargetDistance = Vector3.Distance(transform.position, p.transform.position);
                            seesPlayer = true;
                        }

                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, p.transform.position) < closestTargetDistance)
                        {

                            if (!Physics.Raycast(transform.position, pDir, out inf, pDir.magnitude, LayerMask.GetMask("wall", "big stuff")))
                            {
                                closestTarget = p;
                                closestTargetDistance = Vector3.Distance(transform.position, p.transform.position);
                                seesPlayer = true;
                            }
                        }
                    }

                }
                else
                {
                    if (p.GetComponent<enemySwiper>().isAlive)
                    {
                        if (closestTarget == null)
                        {
                            //Physics.Linecast(transform.position, p.transform.position);
                            if (!Physics.Raycast(transform.position, pDir, out inf, pDir.magnitude, LayerMask.GetMask("wall", "big stuff")))
                            {
                                closestTarget = p;
                                closestTargetDistance = Vector3.Distance(transform.position, p.transform.position);
                                seesPlayer = true;
                            }
                        }
                        else
                        {
                            if (!Physics.Raycast(transform.position, pDir, out inf, pDir.magnitude, LayerMask.GetMask("wall", "big stuff")))
                            {
                                closestTarget = p;
                                closestTargetDistance = Vector3.Distance(transform.position, p.transform.position);
                                seesPlayer = true;
                            }
                        }
                    }
                }
            }
        }
        if (seesPlayer)
        {
            closestTargetDir = (transform.position - closestTarget.transform.position);
            if (closestTargetDistance <= agroLeashRange && Time.time >= agroCDStamp)
            {
                becomeAgro(closestTarget);
            }
        }

        //Debug.DrawLine(this.transform.position, closestTarget.transform.position, Color.black, 2f);

    }

    public void pathFind() // determines the best vector to move in
    {
        checkClosestPlayer(); // find closest player

        // shoot rays in 16 directions
        // check which ones aren't obscured by obstacles
        // determine the ray that moves closest to closest player
        // move in that direction
        Vector3 newDir = new Vector3(0, 0, 0);
        bestAngleDist = 0;

        for (int i = 0; i < 16; i++)
        {
            float angleOfRaycast = i * 360 / 16;
            //figure out vector of that angle
            Vector3 v = new Vector3(Mathf.Cos(angleOfRaycast * Mathf.Deg2Rad), 0, Mathf.Sin(angleOfRaycast * Mathf.Deg2Rad));
            RaycastHit inf = new RaycastHit();
            //Debug.Log(Vector3.Distance(v.normalized, closestTargetDir.normalized));
            if (!Physics.Raycast(transform.position, v, out inf, rayLength, LayerMask.GetMask("wall", "big stuff"))) // for angles that DONT hit any walls in range
            {
                Debug.DrawRay(this.transform.position, v * rayLength, Color.green, 2f);
                if (Vector3.Distance(v.normalized, closestTargetDir.normalized) > bestAngleDist)
                {


                    float ranNum = (int)Random.Range(1, 101);
                    /*if (ranNum < ranChanceWorseOption) //if the ran num is higher than ranchance, the "better option" is not selected
                    {
                        bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                        newDir = v;
                    }*/

                    bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                    newDir = v;
                }
            }

            else
            {
                //Debug.Log("ray distance[" +i + "]: " + Vector3.Distance(transform.position, inf.transform.position));
                if (Vector3.Distance(transform.position, inf.transform.position) > minMoveDistance)
                {
                    //Debug.DrawRay(this.transform.position, v * minMoveDistance, Color.yellow, 2f);
                    if (Vector3.Distance(v.normalized, closestTargetDir.normalized) > bestAngleDist)
                    {
                        //bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                        //newDir = v;
                    }
                }
                else
                {
                    Debug.DrawRay(this.transform.position, v * minMoveDistance, Color.red, 2f);
                }

            }
        }
        //add in slightly random rotation
       // Vector3 varianceDir = new Vector3(0, Random.Range(-moveSpread, moveSpread), 0).normalized;
        //newDir += varianceDir;


        float targetRange = Random.Range(minTargetRange, maxTargetRange);
        target = transform.position + (newDir * targetRange);

        targetCDStamp = Time.time + targetCD;
        hasTarget = true;
        //Debug.DrawRay(this.transform.position, newDir * minMoveDistance, Color.yellow, 2f);
    }


    public void randomPathfind()
    {
        //Debug.Log("so random");
        Vector3 newDir = new Vector3(0, 0, 0);
        float biggestMove = 0;
        for (int i = 0; i < 16; i++)
        {
            float angleOfRaycast = i * 360 / 16;
            //figure out vector of that angle
            Vector3 v = new Vector3(Mathf.Cos(angleOfRaycast * Mathf.Deg2Rad), 0, Mathf.Sin(angleOfRaycast * Mathf.Deg2Rad));
            RaycastHit inf = new RaycastHit();
            //Debug.Log(Vector3.Distance(v.normalized, closestTargetDir.normalized));
            if (!Physics.Raycast(transform.position, v, out inf, maxTargetRange, LayerMask.GetMask("wall", "big stuff"))) // for angles that have clear path ahead
            {
                biggestMove = maxTargetRange; //this is the further possible we check foir
                newDir = v; //set to be new direction to move in
            }
            else
            {
                if (Vector3.Distance(transform.position, inf.collider.transform.position) > biggestMove) // if cant move at max range, check shorter paths
                {
                    biggestMove = Vector3.Distance(transform.position, inf.collider.transform.position);
                    newDir = v; //pick biggest short path
                }

            }
        }
        float targetRange = Random.Range(minTargetRange, maxTargetRange);
        target = transform.position + (newDir * targetRange);
        targetCDStamp = Time.time + targetCD;
        hasTarget = true;
        Debug.DrawRay(this.transform.position, newDir * minMoveDistance, Color.yellow, 2f);
    }


    public void newTarget() // dont use this any more
    {


        // find closest enemy
        // do a scatter shot
        //  *** randomize scatter a bit more
        // determine which rays are 
        // find ray that is closest to target direction
        // **** small chance just go in a random direction instead
        //if can go min distance without hitting something, go that way
        //else go in next closest ray

        // go slower around obstacles


        // set target within range, cast rays that way to make sure it's a good path, if not try again
        //if after so many tries, just move into the wall

        checkClosestPlayer();

        Vector3 newDir = new Vector3(0, 0, 0);
        bestAngleDist = 0;
        //rayLength = minMoveDistance;
        for (int i = 0; i < 8; i++)
        {
            float angleOfRaycast = i * 360 / 8;
            //figure out vector of that angle
            Vector3 v = new Vector3(Mathf.Cos(angleOfRaycast * Mathf.Deg2Rad), 0, Mathf.Sin(angleOfRaycast * Mathf.Deg2Rad));
            RaycastHit inf = new RaycastHit();

            if (!Physics.Raycast(transform.position, v, out inf, rayLength, LayerMask.GetMask("wall", "big stuff"))) // for angles that have clear path ahead
            {

                Debug.DrawRay(this.transform.position, v * rayLength, Color.green, 2f);

                if (Vector3.Distance(v.normalized, closestTargetDir.normalized) < bestAngleDist)
                //if(Vector3.Dot(v.normalized, closestTargetDir.normalized) < bestAngleDist)
                {
                    bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                    newDir = v;
                    //Debug.Log("angle to player: " + closestTargetDir.normalized + " closest angle: " + v.normalized);

                }


                //Debug.Log(angleOfRaycast + "... HIT: " + inf.collider.gameObject, inf.collider.gameObject);
                /*if(Vector3.Distance(transform.position, inf.transform.position) > minRange)
                {

                    //Debug.Log("biggest distance: " + Vector3.Distance(transform.position, inf.collider.transform.position) + " with vector: " + v, inf.collider.gameObject);
                    if(minRange > 0)
                    {
                        float ranNum = (int)Random.Range(1, 101);
                        if(ranNum > ranChanceWorseOption) //if the ran num is higher than ranchance, the "better option" is not selected
                        {
                            minRange = Vector3.Distance(transform.position, inf.collider.transform.position);
                            newDir = v;
                        }
                    }
                    else
                    {
                        minRange = Vector3.Distance(transform.position, inf.collider.transform.position);
                        newDir = v;
                    }*/
            }
            else
            {

                //for angles that hit something



                //Debug.DrawRay(this.transform.position, v * rayLength, Color.yellow, 2f);
                //Debug.Log("enemy target vector magnitude: " + Vector3.Dot(v.normalized, closestTargetDir.normalized));
                if (Vector3.Distance(transform.position, inf.transform.position) >= minMoveDistance)
                //(Vector3.Dot(v.normalized, closestTargetDir.normalized) < bestAngleDist && Vector3.Distance(transform.position, inf.transform.position) >= minMoveDistance)
                {
                    if (Vector3.Distance(v.normalized, closestTargetDir.normalized) < bestAngleDist)
                    {
                        bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                        newDir = v;
                    }
                    //Debug.Log("new target");
                    //bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                    //newDir = v;

                }
            }

            //Debug.DrawLine(this.transform.position, target, Color.yellow, 10f);
            // move in newDir
        }
        float targetRange = Random.Range(minTargetRange, maxTargetRange);
        target = transform.position + (newDir * targetRange);
        targetCDStamp = Time.time + targetCD;
        hasTarget = true;
        Debug.DrawRay(this.transform.position, newDir * minMoveDistance, Color.yellow, 2f);
        //
        //Debug.Log(randomVec);
        /*RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(randomVec), out hit, targetRange))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(randomVec) * hit.distance, Color.yellow);
            Debug.Log("Did Hit");
            transform.Rotate(randomVec);
            hasTarget = true;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(randomVec) * targetRange, Color.white);
            Debug.Log("Did not Hit");
            //newTarget();
        }
        hasTarget = true;*/

    }


    /* public void backOff()
     {
         Debug.Log("backing off");
         float varianceDir = Random.Range(-moveSpread, moveSpread) + 180;
         transform.Rotate(transform.up * varianceDir);
         float thisMoveForce = Random.Range(minMove, maxMove);
         rb.AddForce(transform.forward * thisMoveForce, ForceMode.Impulse);
         float ranCD = Random.Range(moveCDMin, moveCDMax);
         moveCDStamp = Time.time + ranCD;
     }*/

    public void faceTarget() // rotate towards target vector
    {
        Vector3 direction = target - transform.position;
        //Quaternion toRotation = Quaternion.FromToRotation(transform.forward, direction);
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        if (isAgro)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, agroRotSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }


    }

    public void swipeEnemy(Vector3 loc) // move in "swipe like" motions
    {
        Debug.Log("swipe");
        //transform.LookAt(target);
        // random range of strength, variable from target
        if (isAgro)
        {
            //float runChance = 50f;
            float totalChance = Random.Range(1, 101);
            if (totalChance < chanceRunAway) // run away from player
            {
                //Debug.Log("totally random");
                float varianceDir = Random.Range(-moveSpread, moveSpread);
                transform.Rotate(transform.up * varianceDir);
                float thisMoveForce = Random.Range(agroMinMove, agroMaxMove);
                rb.AddForce(-transform.forward * thisMoveForce, ForceMode.Impulse);
                float ranCD = Random.Range(agroCDMin, agroCDMax);
                moveCDStamp = Time.time + ranCD;
            }
            else // dash towards player
            {
                //float varianceDir = Random.Range(-moveSpread / 2, moveSpread / 2);
                //transform.Rotate(transform.up * varianceDir);
                float thisMoveForce = Random.Range(agroMinMove, agroMaxMove);
                rb.AddForce(transform.forward * thisMoveForce, ForceMode.Impulse);
                float ranCD = Random.Range(agroCDMin, agroCDMax);
                moveCDStamp = Time.time + ranCD;
            }

        }
        else
        {

            if (isSlow)
            {
                float varianceDir = Random.Range(-moveSpread, moveSpread);
                transform.Rotate(transform.up * varianceDir);
                float thisMoveForce = Random.Range(minMove, maxMove);
                thisMoveForce /= 2f; // slow down
                rb.AddForce(transform.forward * thisMoveForce, ForceMode.Impulse);
                float ranCD = Random.Range(moveCDMin, moveCDMax);
                moveCDStamp = Time.time + ranCD;
            }
            else
            {
                float varianceDir = Random.Range(-moveSpread, moveSpread);
                transform.Rotate(transform.up * varianceDir);
                float thisMoveForce = Random.Range(minMove, maxMove);
                rb.AddForce(transform.forward * thisMoveForce, ForceMode.Impulse);
                float ranCD = Random.Range(moveCDMin, moveCDMax);
                moveCDStamp = Time.time + ranCD;
            }

        }

    }

    public void steadyMove(Vector3 myDir) // move player per frame
    {

        curTargetDir = (myDir - this.transform.position).normalized;
        Debug.Log("cur target dir: " + curTargetDir);
        curTargetDir = curTargetDir * curMoveSpeed;
        
        if (!beingBumped)
        {
            if (isSlow)
            {
                curTargetDir = curTargetDir / 2f;
            }

            rb.MovePosition(transform.position + curTargetDir * Time.fixedDeltaTime);
            if (isDashing)
            {
                Debug.DrawLine(this.transform.position, curTargetDir * dashRange, Color.cyan, 2f);
                agroCDStamp = Time.time + agroCD;
                dashStamp = Time.time + dashTime;
                isAgro = false;
                isDashing = false;
            }
        }
        else
        {
            rb.MovePosition(transform.position + curTargetDir * Time.fixedDeltaTime);
           
        }
    }

    public void becomeAgro(GameObject other)
    {
        //Debug.Log("become agro");
        isAgro = true;
        agroTimerStamp = Time.time + agroTime;
        targetEnemy = other.gameObject;
        slowCDStamp = Time.time + slowCDTime;
        isSlow = false;
    }
    public void findOthers()
    {
        otherPlayers = GameObject.FindGameObjectsWithTag("Player");

    }

    public void trySlow()
    {
        //Debug.Log("trying slow");
        float totalChance = Random.Range(1, 101);
        if (totalChance < chanceSlow) // run away from player
        {
            isSlow = true;
            slowTimeStamp = Time.time + slowTime;
        }
    }

    public void newMoveSpeed(bool agro)
    {
        if (agro)
        {
            curMoveSpeed = Random.Range(agroMinMove, agroMaxMove);
        }
        else
        {
            curMoveSpeed = Random.Range(minMove, maxMove);
        }
    }

    public void getBumped(Vector3 newDir, float bumpForce)
    {
        target = newDir * bumpForce;
        curMoveSpeed = bumpForce;
        beingBumped = true;
        bumpStamp = Time.time + bumpTime;
        //Debug.Log("enemy got bumped");
        //Quaternion newLook = Quaternion.LookRotation(newDir);
        //rb.MoveRotation(Quaternion.Lerp(transform.rotation, newLook, rotationSpeed * Time.deltaTime));
        //canKick = false;
    }

    public void doDash()
    {
        
            //Debug.Log("dash!");
        isDashing = true;
        isSlow = false;
        /*float totalChance = Random.Range(1, 101);
        if (totalChance < chanceRunAway) // run away from player
        {
            curMoveSpeed = -dashForce;
        }
        else
        {
            curMoveSpeed = dashForce;
        }*/
        curMoveSpeed = dashForce;

    }
}
