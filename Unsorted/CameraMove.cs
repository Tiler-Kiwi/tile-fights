using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Control Script/FPS Input")]
public class CameraMove : MonoBehaviour
{
    public float speed = 50.0f;
    public float RotSpeed = 5.0f;
    // public float gravity = -9.8f;
    private CharacterController _charController;
    private GameObject ControlledCamera;
    void Start()
    {
        _charController = GetComponent<CharacterController>();
        if (this.GetComponent<Camera>() != null)
        {
            ControlledCamera = this.gameObject;
        }
        else
        {
            ControlledCamera = this.gameObject.GetComponentInChildren<Camera>().gameObject;
        }
    }
    void Update()
    {
        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;
        float deltaY = Input.GetAxis("Mouse ScrollWheel") * speed * speed;
        float deltaRot = Input.GetAxis("RotateAround") * RotSpeed;
        Vector3 movement = new Vector3(deltaX, deltaY, deltaZ);
        //movement = Vector3.ClampMagnitude(movement, speed);
        // movement.y = gravity;
        movement *= Time.deltaTime;
        movement = transform.TransformDirection(movement);
        _charController.Move(movement);
        this.transform.localPosition = CircularRotation(this.transform.position, deltaRot, ControlledCamera.transform.eulerAngles.y, ControlledCamera.transform.eulerAngles.x);
        this.transform.Rotate(new Vector3(0, deltaRot, 0), Space.World);
    }

    Vector3 CircularRotation(Vector3 position, float deltarotY, float rotY, float rotX) //currentpos, change in rotation, current  Y/X rotation of the camera
    {
        //rotate at point derived from a line from where the camera is facing, and its intersection with the 0 y plane
        //"Orbit" being a circle drawn from the distance between the camera and the point of rotation
        //camera is always in a position oppisite of its own rotation
        float PositionInOrbit = (180 + rotY) * Mathf.Deg2Rad;
        //deltarot is how far the camera is moving in the circle. maybe misnamed.
        //unity math deals in radians so a conversion is needed
        float NewOrbitPosition = ((180 + rotY) + deltarotY) * Mathf.Deg2Rad;
        // https://i.imgur.com/QiFv3sS.png
        float y = this.transform.position.y;
        float RotX =  Mathf.Max(10, 90 - rotX); // lets just keep this value sane, for now
        float radius = Mathf.Abs(Mathf.Tan(RotX * Mathf.Deg2Rad) * y);
        float NewX = position.x - (Mathf.Sin(PositionInOrbit) * radius) + (Mathf.Sin(NewOrbitPosition) * radius);
        float NewZ = position.z - (Mathf.Cos(PositionInOrbit) * radius) + (Mathf.Cos(NewOrbitPosition) * radius);
        return new Vector3(NewX, y, NewZ);
    }
}