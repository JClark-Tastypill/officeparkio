using System.Collections;
using System.Collections.Generic;
using Tastypill.Debug;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public bool isAlive = true;
    public DebugFloat maxDragDistance = 175f;
    public DebugFloat maxSpeed = 40f, accelerationSpeed = 450f, decelerationSpeed = 30f, rotationSpeed = 10f, dashForce = 140f, dashSwipeLength = 225f, kickCD = .06f, kickTime = .15f;
    //public DebugFloat activeDecelerationRate

    private Vector2 tapStartPosition;

    private float moveSpeed;

    public Vector3 lastPlayerInput;
    private float touchStartTime, touchEndTime;
    public DebugFloat swipeTouchTime = .2f;

    public bool canKick = true;
    public bool isKicking = true;
    private float kickCDStamp;
    public bool beingBumped;
    public float bumpTime, bumpStamp;
    public DebugFloat maxDashSwipeLength = 50, minDashForce = 20f, sameTouchSwipeDist = 175;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        /*  
          DebugMenu.instance.CreateDebugSlider("Max Speed", Color.black, maxSpeed, 80, 1);
          
          DebugMenu.instance.CreateDebugSlider("Rotation Speed", Color.black, rotationSpeed, 10, 0);
          DebugMenu.instance.CreateDebugSlider("max dash Swipe Length", Color.black, maxDashSwipeLength, 500, 200);
          DebugMenu.instance.CreateDebugSlider("min dash Force", Color.black, minDashForce, 200, 0);
          DebugMenu.instance.CreateDebugSlider("dash swipe length", Color.black, dashSwipeLength, 300, 1);
          DebugMenu.instance.CreateDebugSlider("swipe time", Color.black, swipeTouchTime, 1, 0);
          DebugMenu.instance.CreateDebugSlider("same touch swipe length", Color.black, swipeTouchTime, 400, 100);*/
        DebugMenu.instance.CreateDebugSlider("dash force", Color.black, dashForce, 200, 1);
        DebugMenu.instance.CreateDebugSlider("dash Swipe Length", Color.black, maxDashSwipeLength, 300, 1);
        DebugMenu.instance.CreateDebugSlider("dash cooldown", Color.black, kickCD, 1, 0);
        DebugMenu.instance.CreateDebugSlider("Deceleration Speed", Color.black, decelerationSpeed, 100, 1);
        DebugMenu.instance.CreateDebugSlider("Max Drag Dist", Color.black, maxDragDistance, 400, 0);
        DebugMenu.instance.CreateDebugSlider("Acceleration Speed", Color.black, accelerationSpeed, 500, 1);
        DebugMenu.instance.CreateDebugSlider("how long dash lasts", Color.black, kickTime, .5f, 0);
        //DebugMenu.instance.CreateDebugSlider("same touch swipe mag", Color.black, sameTouchSwipeDist, 300, 100);
    }



    private void FixedUpdate()
    {
       // Debug.Log("last player input mag: " + lastPlayerInput.magnitude);
       // Debug.Log("rb vel: " + rb.velocity.magnitude);
       if(!canKick)
        {
            if (Time.fixedTime >= kickCDStamp)
            {
                canKick = true;
            }
        }
        
        if(beingBumped)
        {
            if (Time.fixedTime >= bumpStamp)
            {
                beingBumped = false;
            }
        }
        

        if (Input.touchCount > 0) // touching screen
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                tapStartPosition = Input.GetTouch(0).position;
            }

            Vector2 currentTapLocation = Input.GetTouch(0).position;
            Vector2 inputDiff = currentTapLocation - tapStartPosition; // difference between original touch and current touch, based on screen location

            if (inputDiff.magnitude > maxDragDistance) //if the player moves their finger too far away from original touch
            {
                tapStartPosition = currentTapLocation - inputDiff.normalized * maxDragDistance; //change the new "start" touch location for more accurate measurements
                inputDiff = currentTapLocation - tapStartPosition;
            }

            //check to see if a big swipe was made without letting go of screen //////////////
            float bigSwipeMag = Input.GetTouch(0).deltaPosition.magnitude;
            //Debug.Log("swipe magnitude: " + bigSwipeMag + ", max swipe: " + sameTouchSwipeDist);
            if (bigSwipeMag >= sameTouchSwipeDist && canKick)
            {
                //doKick(new Vector3(inputDiff.x, 0, inputDiff.y));
            }

            // check to see if player swipes to do a big kick in that direction//////////////////
            if (Input.GetTouch(0).phase == TouchPhase.Ended) // touch end
            {
                float swipeMag = Input.GetTouch(0).deltaPosition.magnitude;
                if (swipeMag >= maxDashSwipeLength && canKick)
                {
                    doKick(new Vector3(inputDiff.x, 0, inputDiff.y));
                }
            }


            if(canKick && !beingBumped)
            {
                float t = inputDiff.magnitude / maxDragDistance; //percentage of max speed, max drag distance is edge of joystick
                float targetMoveSpeed = Mathf.Lerp(0f, maxSpeed, t); // target speed is based on the above percentage and maxspeed
                moveSpeed = targetMoveSpeed;
                //moveSpeed = Mathf.MoveTowards(moveSpeed, targetMoveSpeed, accelerationSpeed * Time.fixedDeltaTime); //start to acceleratate towards target speed, but not faster than acceration speed * time
                Vector3 targetMoveVector = new Vector3(inputDiff.x, 0f, inputDiff.y).normalized * moveSpeed; //targetmovevector is the movement vector based on direction and speed;
                lastPlayerInput = Vector3.MoveTowards(lastPlayerInput, targetMoveVector, accelerationSpeed * Time.fixedDeltaTime); // lerps to target vector based on current, at acceleraton
            }
        }
        else // not touching
        {
            //decelerate if not touching
            if (!beingBumped)
            {
                if(canKick)
                {
                    //moveSpeed = Mathf.MoveTowards(moveSpeed, 0f, Time.fixedDeltaTime * decelerationSpeed);
                }
                else
                {
                    //moveSpeed = Mathf.MoveTowards(moveSpeed, 0f, Time.fixedDeltaTime * (decelerationSpeed * 2)); // slow down less when dashing
                }
                moveSpeed = Mathf.MoveTowards(moveSpeed, 0f, Time.fixedDeltaTime * decelerationSpeed); // slow down less when dashing
                Vector3 targetMoveVector = lastPlayerInput.normalized * moveSpeed;
                lastPlayerInput = Vector3.MoveTowards(lastPlayerInput, targetMoveVector, accelerationSpeed * Time.fixedDeltaTime);
            }
        }

        // move player every frame some amount
        if(isAlive)
        {
            rb.MovePosition(transform.position + lastPlayerInput * Time.fixedDeltaTime);

            if (lastPlayerInput.magnitude > 0.1f)
            {
                Quaternion newLook = Quaternion.LookRotation(lastPlayerInput);
                rb.MoveRotation(Quaternion.Lerp(transform.rotation, newLook, rotationSpeed * Time.deltaTime));
            }
        }
        
    }

    public void doKick(Vector3 newDir)
    {
        newDir = newDir.normalized * dashForce;

        lastPlayerInput = newDir;//.normalized * dashForce;
        Debug.Log("dash force: " + lastPlayerInput.magnitude);
        kickCDStamp = Time.fixedTime + kickCD;
        Quaternion newLook = Quaternion.LookRotation(newDir);
        rb.MoveRotation(Quaternion.Lerp(transform.rotation, newLook, rotationSpeed * Time.deltaTime));
        canKick = false;
    }

    public void getBumped(Vector3 newDir, float bumpForce)
    {
        Debug.Log("player bumped");
        lastPlayerInput = newDir * bumpForce;
        beingBumped = true;
        bumpStamp = Time.time + bumpTime;
    }
}
