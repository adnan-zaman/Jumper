using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerController : MonoBehaviour
{
    public InputActionAsset playerControls;
    public Transform camTransform;
    [Header("Ground Movement")]
    public float movementSpeed;
    public float rotateSpeed = 0.01f;

 
    [Header("Jump Attributes")]
    //velocities of each jump
    public float[] jumpVelocities;
    //how fast to jump from landing to start next jump in sequence
    public float jumpThreshold;
    public float gravity;

    //input info
    private InputAction movement;
    private InputAction jump;
    private CharacterController charController;

    //cant rely on transform.eulerAngles being consistent
    //so track y world rotation ourselves
    private float yWorldRot;

    /*jump info*/

    //y velocity
    private float yVel;
    //whether jump input event was just performed
    private bool jumpPressed;
    //time since landing from a jump
    private float timeSinceLanded;
    //current jump were on
    private int jumpIndex;

    //previous value of charController.isGrounded
    private bool prevIsGrounded;

    void Start()
    {
        InputActionMap gameplayActionMap = playerControls.FindActionMap("Player");
        gameplayActionMap.Enable();
        movement = gameplayActionMap.FindAction("Move");
        jump = gameplayActionMap.FindAction("Jump");
        jump.performed +=  context => { if (charController.isGrounded) jumpPressed = true; };

        gameplayActionMap.FindAction("Quit").performed += context => Application.Quit();

        charController = GetComponent<CharacterController>();
        prevIsGrounded = charController.isGrounded;

        yWorldRot = transform.eulerAngles.y;
    }

    void Update()
    {
        timeSinceLanded += Time.deltaTime;

        Vector3 totalMovement = Vector3.zero;
        totalMovement += GroundMovement();
        totalMovement += VerticalMovement();


        prevIsGrounded = charController.isGrounded;
        charController.Move(totalMovement * Time.deltaTime);
    }

    /*
     * All logic pertaining to movement on the ground.
     * 
     * <returns> vector containing total movement on the ground for this frame </returns>
     */
   
    private Vector3 GroundMovement()
    {
        Vector3 groundMovement = Vector3.zero;
        Vector2 move = movement.ReadValue<Vector2>().normalized;
        Vector3 targetDir = new Vector3(move.x, 0, move.y);

        if (targetDir.magnitude > 0.01f)
        {
            float vel = 0;

            //target angle relative to world z+ axis
            float targetAngle = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;

            Vector3 forwardFromCamera = camTransform.forward;
            //we only care about direction on xz plane
            forwardFromCamera.y = 0;
            float angleToNewForward = Vector3.SignedAngle(Vector3.forward, forwardFromCamera, Vector3.up);
            float angleRad = angleToNewForward * Mathf.Deg2Rad;

            //we want angle to be relative to the forward direction from camera
            targetAngle += angleToNewForward;
  
            float nextAngle = Mathf.SmoothDampAngle(yWorldRot, targetAngle, ref vel, rotateSpeed);

            //rotate targetDir by angleRad
            //rotation formula may look swapped but regular rotation formula is relative to x+ axis
            //we want relative to z+
            //point (x,z) relative to x+ is (z,x)
            //so z = (x rot formula), x = (z rot formula)
            targetDir = new Vector3(
                targetDir.x * Mathf.Cos(angleRad) + targetDir.z * Mathf.Sin(angleRad),
                0,
                targetDir.z * Mathf.Cos(angleRad) - targetDir.x * Mathf.Sin(angleRad));
       
            transform.Rotate(new Vector3(0, nextAngle - yWorldRot, 0), Space.World);
            yWorldRot = nextAngle;

              
            groundMovement = targetDir * movementSpeed;
        }

        return groundMovement;
    }


    /*
     * All logic pertaining to vertical movement. Including jumping
     * and handling gravity.
     * 
     * <returns> vector containing total vertical movement frame </returns>
     */
    private Vector3 VerticalMovement()
    {
        //restart counter from the moment character lands
        //only for jumpIndex > 0 so that that if you jump within the threshold
        //after landing from falling from a height (without jumping), it won't
        //go into the second jump
        if (charController.isGrounded && !prevIsGrounded && jumpIndex > 0)
            timeSinceLanded = 0f;

        if (yVel < 0f && charController.isGrounded)
            yVel = 0f;

        
        if (jumpPressed && charController.isGrounded)
        {
            //jump wasnt fast enough, reset jump sequence to first jump
            if (timeSinceLanded > jumpThreshold)
                jumpIndex = 0;
            
            yVel = jumpVelocities[jumpIndex];
            if (jumpIndex == jumpVelocities.Length - 1)
                StartCoroutine("Flip");
            jumpPressed = false;
            jumpIndex = (jumpIndex + 1) % jumpVelocities.Length;
        }
            
       
        yVel +=gravity * Time.deltaTime;


        return new Vector3(0f, yVel, 0f);
    }
    
    private IEnumerator Flip()
    {
        Vector3 rotate = new Vector3(360f, 0f, 0f);
        float rotatedSoFar = 0f;
        while (rotatedSoFar < 360f)
        {
            rotatedSoFar += rotate.x * Time.deltaTime;
            transform.Rotate(rotate * Time.deltaTime, Space.Self);
            yield return null;
        }

        //ensure flip ends with player level with ground and not tilted
        Vector3 forward = transform.forward;
        forward.y = 0;
        transform.forward = forward;
    }
    

}
