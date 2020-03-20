using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemySwiper : MonoBehaviour
{
    public bool isAlive, isAgro, hasTarget;
    //public float moveCDMin, moveCDMax, moveCDStamp;
    //public float agroCDMin, agroCDMax;
    public float roamTime, agroTime, bumpTime, dashTime, dashCDTime;
    public float moveStamp; // only stamp used to determine when enemy should make their next move
    public float dashStamp; // so enemies only dash once in a while, it's on a cooldown
    public Vector3 target;
    public GameObject targetEnemy;
    public float reachTargetRange;
    public float minTargetRange, maxTargetRange;
    public float minMove, maxMove, newTargetDistance;
    public float agroMinMove, agroMaxMove;
    public Rigidbody rb;
    public float agroLeashRange;//, agroTime, agroCD, agroTimerStamp, agroCDStamp;
    public float rotationSpeed;
    //public float targetCD, targetCDStamp;
    public GameObject[] otherPlayers;
    public GameObject closestTarget;
    public float closestTargetDistance;
    public Vector3 closestTargetDir;
    public float rayLength, bestAngleDist, minMoveDistance;
    public bool seesPlayer;
    public float curMoveSpeed;
    public Vector3 curTargetDir;
    public bool beingBumped;
    //public float bumpTime, bumpStamp;
    public float dashRange;
    public bool isDashing = true;
    public float dashForce;
    public float bumpForce;
    
    public enum EnemyState
    {
        None,
        Roam,
        Aggro,
        Dash,
        Bumped
    };
    private EnemyState currentState = EnemyState.None;

    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        findOthers(); // runs once, creates array of other players in the stage
        determineState(); 
    }
    
 public void FixedUpdate()
 {
        if (!isAlive)
        {
            return;
        }

        Vector3 moveDirection = Vector3.zero;
        switch (currentState)
        {
            case EnemyState.Roam:
                moveDirection = HandleRoam();
                break;
            case EnemyState.Aggro:
                moveDirection = HandleAggro();
                break;
            case EnemyState.Dash:
                moveDirection = HandleDash();
                break;
            case EnemyState.Bumped:
                moveDirection = HandleBumped();
                break;

        }

        steadyMove(moveDirection);
 }
    public void gotoRoam()
    {
        Debug.Log("go to roam");
        checkClosestPlayer();
        if (Time.fixedTime >= dashStamp)
        {
            if (seesPlayer)
            {
                pathFind();
            }
            else
            {
                randomPathfind(); // if player just made a dash, move in random direction
            }
        }
        else
        {
            randomPathfind();
        }
        newMoveSpeed(1);
        moveStamp = Time.time + roamTime;
        currentState = EnemyState.Roam;
    }

    public void goToAggro()
    {
        Debug.Log("go to aggro");
        checkClosestPlayer();
        pathFind();
        newMoveSpeed(2);
        moveStamp = Time.time + agroTime;
        currentState = EnemyState.Aggro;
    }

    public void goToDash()
    {
        Debug.Log("go to dash");
        checkClosestPlayer();
        pathFind();
        newMoveSpeed(3);
        moveStamp = Time.time + dashTime;
        
        currentState = EnemyState.Dash;
    }

    public void goToBumped()
    {
        Debug.Log("go to bumped");
        //newMoveSpeed(false);
        newMoveSpeed(4);
        moveStamp = Time.time + bumpTime;
        currentState = EnemyState.Bumped;
    }

    public Vector3 HandleRoam() 
    {
        /*if time is up or reach target, next move
         *
         *
         */
        //Debug.Log("distance to target: " + Vector3.Distance(transform.position, target) + ", target range: " + reachTargetRange);
        if(Vector3.Distance(transform.position, target) <= reachTargetRange || Time.fixedTime>= moveStamp)
        {
            
            determineState();
        }
        faceTarget();
        return target;
    }
    public Vector3 HandleAggro()
    {
        //if time is up , next move
        if (Vector3.Distance(transform.position, target) >= agroLeashRange || Time.fixedTime >= moveStamp || Vector3.Distance(transform.position, target) <= reachTargetRange)
        {
            determineState();
        }
        if(Vector3.Distance(transform.position, target) <= dashRange)
        {
            if(Time.fixedTime >= dashStamp)
            {
                goToDash();
            }
            else
            {
                determineState();
            }
            
        }

            // if close enough for a dash, do a dash
            faceTarget();
        return target;
    }
    public Vector3 HandleDash()
    {
        //quick dash forward, cooldown on aggro, next move
        if (Time.fixedTime >= moveStamp)
        {
            dashStamp = Time.fixedTime + dashStamp;
            //gotoRoam();
            determineState();
        }
        return target;
    }
    public Vector3 HandleBumped()
    {
        // move in bump direction until time is up
        if(Time.fixedTime >= moveStamp)
        {
            beingBumped = false;
            determineState();
        }
        return target;
    }

    public void determineState() //see which state enemy should go in based on cooldowns and position/visability of other players
    {
        Debug.Log("determining state");
        if(Time.time >= dashStamp)
        {
            checkClosestPlayer(); // find closest player, and if they are visable
            if (seesPlayer)
            {
                closestTargetDir = (transform.position - closestTarget.transform.position);
                if (closestTargetDistance <= agroLeashRange)
                {
                    goToAggro();
                }
                else
                {
                    gotoRoam();
                }
            }
            else
            {
                //randomPathfind();
                gotoRoam();
            }
        }
        else
        {
            gotoRoam();
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
         if (p != this.gameObject)
         {
             if (p.GetComponent<fallFromObstacles>().isPlayer)
             {
                    if(p.GetComponent<PlayerMovement>().isAlive)
                    {
                        if (closestTarget == null)
                        {
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
     
 }

 public void pathFind() // determines the best vector to move in
 {
        //Debug.Log("regular pathfind");
     //checkClosestPlayer(); // find closest player

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
         if (!Physics.Raycast(transform.position, v, out inf, rayLength, LayerMask.GetMask("wall", "big stuff"))) // for angles that DONT hit any walls in range
         {
             Debug.DrawRay(this.transform.position, v * rayLength, Color.green, 2f);
             if (Vector3.Distance(v.normalized, closestTargetDir.normalized) > bestAngleDist)
             {
                    bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                    newDir = v;
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, inf.transform.position) > minMoveDistance)
                {
                    //Debug.DrawRay(this.transform.position, v * minMoveDistance, Color.yellow, 2f);
                    if (Vector3.Distance(v.normalized, closestTargetDir.normalized) > bestAngleDist)
                    {

                    }
                }
                else
                {
                    Debug.DrawRay(this.transform.position, v * minMoveDistance, Color.red, 2f);
                }

            }
        }
        float targetRange = Random.Range(minTargetRange, maxTargetRange);
        target = transform.position + (newDir * targetRange);
       // targetCDStamp = Time.time + targetCD;
        hasTarget = true;
    }


    public void randomPathfind()
    {
        //Debug.Log("randompathfind");
        Vector3 newDir = new Vector3(0, 0, 0);
        float biggestMove = 0;
        for (int i = 0; i < 16; i++)
        {
            float angleOfRaycast = i * 360 / 16;
            //figure out vector of that angle
            Vector3 v = new Vector3(Mathf.Cos(angleOfRaycast * Mathf.Deg2Rad), 0, Mathf.Sin(angleOfRaycast * Mathf.Deg2Rad));
            RaycastHit inf = new RaycastHit();
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
        float targetRange = Random.Range(minMove, biggestMove);
        target = transform.position + (newDir * targetRange);
        //targetCDStamp = Time.time + targetCD;
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
                {
                    bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                    newDir = v;
                }
            }
            else
            {
                //for angles that hit something
                if (Vector3.Distance(transform.position, inf.transform.position) >= minMoveDistance)
                {
                    if (Vector3.Distance(v.normalized, closestTargetDir.normalized) < bestAngleDist)
                    {
                        bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                        newDir = v;
                    }

                }
            }
        }
        float targetRange = Random.Range(minTargetRange, maxTargetRange);
        target = transform.position + (newDir * targetRange);
        //targetCDStamp = Time.time + targetCD;
        hasTarget = true;
        Debug.DrawRay(this.transform.position, newDir * minMoveDistance, Color.yellow, 2f);

    }

    public void faceTarget() // rotate towards target vector
    {
        Vector3 direction = target - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    public void steadyMove(Vector3 myDir) // move player per frame
    {

        curTargetDir = (myDir - this.transform.position).normalized;
        //Debug.Log("cur target dir: " + curTargetDir);
        curTargetDir = curTargetDir * curMoveSpeed;
        
        if (!beingBumped)
        {
            rb.MovePosition(transform.position + curTargetDir * Time.fixedDeltaTime);
            if (isDashing)
            {
                Debug.DrawLine(this.transform.position, curTargetDir * dashRange, Color.cyan, 2f);
                //agroCDStamp = Time.time + agroCD;
                dashStamp = Time.time + dashTime;
                isAgro = false;
                isDashing = false;
            }
        }
        else
        {
            //Debug.Log("I am bumped");
            rb.MovePosition(transform.position + curTargetDir * Time.fixedDeltaTime);     
        }
    }

    public void becomeAgro(GameObject other)
    {
        isAgro = true;
       // agroTimerStamp = Time.time + agroTime;
        targetEnemy = other.gameObject;
       // slowCDStamp = Time.time + slowCDTime;
        //isSlow = false;
    }
    public void findOthers()
    {
        otherPlayers = GameObject.FindGameObjectsWithTag("Player");

    }

    public void newMoveSpeed(int state)
    {
        if (state == 1) // roam
        {
            curMoveSpeed = Random.Range(minMove, maxMove);
        }
        if (state == 2) //aggro/chase
        {
            curMoveSpeed = Random.Range(agroMinMove, agroMaxMove);
        }
        if (state == 3) //dash forward
        {
            curMoveSpeed = dashForce;
        }
        if (state == 4) //be bumped by something else
        {
            curMoveSpeed = bumpForce;
        }
    }

    public void getBumped(Vector3 newDir, float bumpForce)
    {
        goToBumped();
        target = newDir * bumpForce;
        curMoveSpeed = bumpForce;
        beingBumped = true;
       // bumpStamp = Time.time + bumpTime;
    }

    public void doDash()
    {   
        isDashing = true;
        //isSlow = false;
        curMoveSpeed = dashForce;
    }
}
