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
    private float bumpForce;
    private int lastKindofBump;
    
    public enum EnemyState
    {
        None,
        Roam,
        Aggro,
        Dash,
        Bumped,
        Hide
    };
    public EnemyState currentState = EnemyState.None;

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
            case EnemyState.Hide:
                moveDirection = HandleHide();
                break;

        }

        steadyMove(moveDirection);
 }
    public void gotoRoam()
    {
        //Debug.Log("go to roam");
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
        //Debug.Log("go to aggro");
        checkClosestPlayer();
        pathFind();
        newMoveSpeed(2);
        moveStamp = Time.time + agroTime;
        currentState = EnemyState.Aggro;
    }

    public void goToDash()
    {
        //Debug.Log("go to dash");
        checkClosestPlayer();
        pathFind();
        newMoveSpeed(3);
        moveStamp = Time.time + dashTime;        
        isDashing = true;
        currentState = EnemyState.Dash;
    }

    public void goToBumped(Vector3 bumpDir, float bForce)
    {
        target = bumpDir * bForce;
        curMoveSpeed = bForce;
        beingBumped = true;
        moveStamp = Time.time + bumpTime;
        currentState = EnemyState.Bumped;
    }

    public void goToHide()
    {
        //Debug.Log("go to hide");
        hidePathFind();
        newMoveSpeed(2);
        moveStamp = Time.time + roamTime;
        currentState = EnemyState.Hide;
    }

    public Vector3 HandleRoam() 
    {
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
        //if in range of enemy, make a dash
        //Debug.Log("dash range: " + dashRange);
        if(Vector3.Distance(transform.position, closestTarget.transform.position) <= dashRange) 
        {
            if(Time.fixedTime >= dashStamp)
            {
                goToDash();
            }
            else
            {
                //determineState();
            }
            
        }

        //add in chance to hide instead of dash

        faceTarget();
        return target;
    }
    public Vector3 HandleDash()
    {
        //quick dash forward, cooldown on aggro, next move

        faceTarget();
        target = closestTarget.transform.position;
        if (Time.fixedTime >= moveStamp)
        {
            dashStamp = Time.fixedTime + dashTime;
            isDashing = false;
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
            if(lastKindofBump == 1 || lastKindofBump == 2)
            {
                dashStamp = Time.time + dashCDTime;
            }            
            beingBumped = false;
            determineState();
        }
        return target;
    }

    public Vector3 HandleHide()
    {
        if (Vector3.Distance(transform.position, target) <= reachTargetRange || Time.fixedTime >= moveStamp)
        {
            determineState();
        }
        faceTarget();
        return target;
    }

    public void determineState() //see which state enemy should go in based on cooldowns and position/visability of other players
    {
        isDashing = false;
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
                gotoRoam();
            }
        }
        else
        {
            checkClosestPlayer(); // find closest player, and if they are visable
            if (seesPlayer)
            {
                goToHide();
            }
            else
            {
                gotoRoam();
            }
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

 public void pathFind() // determines the best vector to move in towards closest enemy
 {
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
             //Debug.DrawRay(this.transform.position, v * rayLength, Color.green, 2f);
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

                }
                else
                {
                    //Debug.DrawRay(this.transform.position, v * minMoveDistance, Color.red, 2f);
                }

            }
        }
        float targetRange = Random.Range(minTargetRange, maxTargetRange);
        target = transform.position + (newDir * targetRange);
       // targetCDStamp = Time.time + targetCD;
        hasTarget = true;
    }

    public void hidePathFind() // find path furthest away from closest player
    {
        Vector3 newDir = new Vector3(0, 0, 0);
        bestAngleDist = 360;

        for (int i = 0; i < 16; i++)
        {
            float angleOfRaycast = i * 360 / 16;
            //figure out vector of that angle
            Vector3 v = new Vector3(Mathf.Cos(angleOfRaycast * Mathf.Deg2Rad), 0, Mathf.Sin(angleOfRaycast * Mathf.Deg2Rad));
            RaycastHit inf = new RaycastHit();
            if (!Physics.Raycast(transform.position, v, out inf, rayLength, LayerMask.GetMask("wall", "big stuff"))) // for angles that DONT hit any walls in range
            {
                //Debug.DrawRay(this.transform.position, v * rayLength, Color.green, 2f);
                if (Vector3.Distance(v.normalized, closestTargetDir.normalized) < bestAngleDist)
                {
                    bestAngleDist = Vector3.Distance(v.normalized, closestTargetDir.normalized);
                    newDir = v;
                }
            }            
        }
        float targetRange = Random.Range(minTargetRange, maxTargetRange);
        target = transform.position + (newDir * targetRange);
        hasTarget = true;
    }

    public void randomPathfind() // move in most open direction
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
        hasTarget = true;
        //Debug.DrawRay(this.transform.position, newDir * minMoveDistance, Color.yellow, 2f);
    }

    public void faceTarget() // rotate towards target vector
    {
        Vector3 direction = target - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    public void steadyMove(Vector3 myTarget) // move player per frame
    {
        if (!beingBumped)
        {
            curTargetDir = (myTarget - this.transform.position).normalized;
            curTargetDir = curTargetDir * curMoveSpeed;
            rb.MovePosition(transform.position + curTargetDir * Time.fixedDeltaTime);
            if (isDashing)
            {
                //Debug.DrawLine(this.transform.position, curTargetDir * dashRange, Color.cyan, 2f);
            }
        }
        else
        {
            rb.MovePosition(transform.position + myTarget * Time.fixedDeltaTime);     
        }
    }

    public void becomeAgro(GameObject other)
    {
        isAgro = true;
        targetEnemy = other.gameObject;
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

    public void doDash()
    {   
        isDashing = true;
        //isSlow = false;
        curMoveSpeed = dashForce;
    }
}
