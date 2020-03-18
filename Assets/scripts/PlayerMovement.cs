using System.Collections;
using System.Collections.Generic;
using Tastypill.Debug;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;

    public DebugFloat maxDragDistance = 175f;
    public DebugFloat maxSpeed = 40f, accelerationSpeed = 225f, decelerationSpeed = 30f, rotationSpeed = 10f, dashForce = 80f, dashSwipeLength = 225f, kickCD = .25f;


    private Vector2 tapStartPosition;

    private float moveSpeed;

    public Vector3 lastPlayerInput;
    private float touchStartTime, touchEndTime;
    public DebugFloat swipeTouchTime = .2f;

    public bool canKick = true;
    private float kickCDStamp;
    public bool beingBumped;
    public float bumpTime, bumpStamp;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        DebugMenu.instance.CreateDebugSlider("Max Drag Dist", Color.black, maxDragDistance, 400, 0);
        DebugMenu.instance.CreateDebugSlider("Max Speed", Color.black, maxSpeed, 80, 1);
        DebugMenu.instance.CreateDebugSlider("Acceleration Speed", Color.black, accelerationSpeed, 300, 1);
        DebugMenu.instance.CreateDebugSlider("Deceleration Speed", Color.black, decelerationSpeed, 100, 1);
        DebugMenu.instance.CreateDebugSlider("Rotation Speed", Color.black, rotationSpeed, 10, 0);
        DebugMenu.instance.CreateDebugSlider("dash Force", Color.black, dashForce, 100, 0);
        DebugMenu.instance.CreateDebugSlider("dash swipe length", Color.black, dashSwipeLength, 500, 100);
        DebugMenu.instance.CreateDebugSlider("dash cooldown", Color.black, kickCD, 1, 0);
        DebugMenu.instance.CreateDebugSlider("swipe time", Color.black, swipeTouchTime, 1, 0);

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
        

        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                tapStartPosition = Input.GetTouch(0).position;
                touchStartTime = Time.fixedTime;

            }

            Vector2 currentTapLocation = Input.GetTouch(0).position;
            Vector2 inputDiff = currentTapLocation - tapStartPosition; // difference between original touch and current touch, based on screen location

            if (inputDiff.magnitude > maxDragDistance) //if the player moves their finger too far away from original touch
            {
                tapStartPosition = currentTapLocation - inputDiff.normalized * maxDragDistance; //change the new "start" touch location for more accurate measurements
                touchStartTime = Time.fixedTime;
            }

            if (inputDiff.magnitude >= dashSwipeLength)
            {
                if (canKick)
                {
                    //doKick(new Vector3(inputDiff.x, 0, inputDiff.y).normalized);
                }
            }
            // check to see if player swipes to do a big kick in that direction
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                touchEndTime = Time.fixedTime;
                if (canKick)
                {
                    if(touchEndTime - touchStartTime <= swipeTouchTime)
                    {
                        if(inputDiff.magnitude >= dashSwipeLength / 2)
                        {
                            doKick(new Vector3(inputDiff.x, 0, inputDiff.y).normalized);    
                        }
                        
                    }
                    
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
        else
        {

            //decelerate if not touching
            if (canKick && !beingBumped)
            {

                moveSpeed = Mathf.MoveTowards(moveSpeed, 0f, Time.fixedDeltaTime * decelerationSpeed);
                Vector3 targetMoveVector = lastPlayerInput.normalized * moveSpeed;
                lastPlayerInput = Vector3.MoveTowards(lastPlayerInput, targetMoveVector, accelerationSpeed * Time.fixedDeltaTime);
            }
        }

        // move player every frame some amount
        rb.MovePosition(transform.position + lastPlayerInput * Time.fixedDeltaTime);

        if (lastPlayerInput.magnitude > 0.2f)
        {
            Quaternion newLook = Quaternion.LookRotation(lastPlayerInput);
            rb.MoveRotation(Quaternion.Lerp(transform.rotation, newLook, rotationSpeed * Time.deltaTime));
        }
    }

    public void doKick(Vector3 newDir)
    {
        //Debug.Log("kick");
        //rb.AddForce(newDir * dashForce, ForceMode.VelocityChange);
        lastPlayerInput = newDir * dashForce;
        kickCDStamp = Time.fixedTime + kickCD;
        Quaternion newLook = Quaternion.LookRotation(newDir);
        rb.MoveRotation(Quaternion.Lerp(transform.rotation, newLook, rotationSpeed * Time.deltaTime));
        canKick = false;
    }

    public void getBumped(Vector3 newDir, float bumpForce)
    {
        lastPlayerInput = newDir * bumpForce;
        beingBumped = true;
        bumpStamp = Time.time + bumpTime;
        //Debug.Log("player got bumped");
        //Quaternion newLook = Quaternion.LookRotation(newDir);
        //rb.MoveRotation(Quaternion.Lerp(transform.rotation, newLook, rotationSpeed * Time.deltaTime));
        //canKick = false;
    }
}
