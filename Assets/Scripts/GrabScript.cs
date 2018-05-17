using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrabScript : MonoBehaviour {

    public Vector3 acceleration;
    public Vector3 velocity;
    public Vector3 speed;
    public float xForce, yForce;
    public float collisionStatus;
    public RoomAliveToolkit.RATUserViewCamera userCam;

    private Vector3 mousePosScreen, currentPos, previousPos;
    private Vector3 previousVel;
    private Rigidbody carRb2d;
    private Rigidbody characterRb2d;
    private Vector3 robotPos;
    private float T11 = 1, T12 = 0, T13 = 1, T21 = 0, T22 = 1, T23 = 1, T31 = 1, T32 = 1, T33 = 1;
    private float a = 0, b = 0, c = 0, d = 0;
    private bool forceFeedback;
    private Transform characterTransform;

    private void Start()
    {
        Calibrate();
        characterRb2d = GetComponent<Rigidbody>();
        characterTransform = GetComponent<Transform>();
        robotPos = Vector3.zero;
        previousPos = transform.position;
        previousVel = characterRb2d.velocity;
       
    }

    public void Calibrate() //Initialize
    {
        try
        {
            T11 = PlayerPrefs.GetFloat("T11");
            T12 = PlayerPrefs.GetFloat("T12");
            T13 = PlayerPrefs.GetFloat("T13");

            T21 = PlayerPrefs.GetFloat("T21");
            T22 = PlayerPrefs.GetFloat("T22");
            T23 = PlayerPrefs.GetFloat("T23");

            T31 = PlayerPrefs.GetFloat("T31");
            T32 = PlayerPrefs.GetFloat("T32");
            T33 = PlayerPrefs.GetFloat("T33");
            /*
            a = PlayerPrefs.GetFloat("a");
            b = PlayerPrefs.GetFloat("b");
            c = PlayerPrefs.GetFloat("c");
            d = PlayerPrefs.GetFloat("d");*/
        }
        catch
        {
            Debug.Log("Please Calibrate");
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
		if (Input.GetKey(KeyCode.UpArrow)) //If using mouse to move 
        {
            characterRb2d.velocity = new Vector3(1,1,1);  
        }
        else if (MatlabServer.instance.serverRunning == true) //If connected to rehab robot
        {
            characterTransform.position = new Vector3(MatlabServer.instance.xMove, MatlabServer.instance.yMove, MatlabServer.instance.zMove);
            //characterRb2d.velocity = (CalibratedMovement() - characterTransform.position) * 30;
        }
        else
        {
            //Do Nothing
        }
        velocity = (transform.position - previousPos) / Time.deltaTime;
        acceleration = (characterRb2d.velocity - previousVel) / Time.deltaTime;
        speed = new Vector3(Mathf.Abs(velocity.x), Mathf.Abs(velocity.y), 0);

        previousPos = transform.position;
        previousVel = characterRb2d.velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 contactNormal = collision.contacts[0].normal.normalized; //Gives unit vector direction normal to point of contact


        //Debug.Log("x: " +xForce + " y: "+ yForce + " Vel: "+ velocity + " UnityVel: "+characterRb2d.velocity+" Accel: " + acceleration);
    }

    private void OnCollisionStay(Collision collision)
    {
        //While in contact with ball
        Vector3 contactNormal = collision.contacts[0].normal.normalized; //Gives unit vector direction normal to point of contact
        if (forceFeedback)
        {
        }
        else
        {
            xForce = 0;
            yForce = 0;
        }
        MatlabServer.instance.collisionStatus = 1f;
        MatlabServer.instance.xForce = xForce;
        MatlabServer.instance.yForce = yForce;

        //Debug.DrawRay(collision.contacts[0].point, contactNormal, Color.red, 2, true);
    }
    private void OnCollisionExit(Collision collision)
    {
        xForce = 0;
        yForce = 0;
        MatlabServer.instance.collisionStatus = 0f;
        MatlabServer.instance.xForce = xForce;
        MatlabServer.instance.yForce = yForce;
    }

    /*public Vector3 CalibratedMovement()
    {
        // Ps = T*Pr
        float xS = MatlabServer.instance.xMove * T11 + MatlabServer.instance.yMove * T12;
        float yS = MatlabServer.instance.xMove * T21 + MatlabServer.instance.yMove * T22;
        float lambda = MatlabServer.instance.xMove * T31 + MatlabServer.instance.yMove * T32 + T33;

        
        float xS = MatlabServer.instance.xMove * a - MatlabServer.instance.yMove * b + c;
        float yS = MatlabServer.instance.xMove * b + MatlabServer.instance.yMove * a + d;
        
        return new Vector3(xS/lambda, yS/lambda, 0f);
    }*/
}
