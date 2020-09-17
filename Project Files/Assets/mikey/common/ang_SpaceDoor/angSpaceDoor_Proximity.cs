using UnityEngine;
using System.Collections;

public class angSpaceDoor_Proximity : MonoBehaviour {

    // GameObjects in this door
    private GameObject doorLeft;
    private GameObject doorRight;
    private MeshRenderer keypadLeft;
    private MeshRenderer keypadRight;
    // Vector3's used by the two "door" parts, for positions
    // Right should be negative, Left positive
    private Vector3 vecClosed = new Vector3(1.025f, 0, 0);
    private Vector3 vecOpen = new Vector3(3.5f, 0, 0);
    // Materials stored for the keypad light
    public Material lightRed;
    public Material lightGreen;
    // Used by the opening/closing/lock mechanisms
    private float doorProgression;
    public bool isLocked;
    public bool isOpen;

    // Use this for initialization
    void Start()
    {
        // Initializes GameObjects to their correct objects
        doorRight = transform.FindChild("doorRight").gameObject;
        doorLeft = transform.FindChild("doorLeft").gameObject;
        keypadRight = transform.FindChild("keypadRight").gameObject.GetComponent<MeshRenderer>();
        keypadLeft = transform.FindChild("keypadLeft").gameObject.GetComponent<MeshRenderer>();

        // Sets door positions to their new "shut" states
        doorLeft.transform.localPosition = vecClosed;
        doorRight.transform.localPosition = -vecClosed;

        // Initializes lock state, all doors should be unlocked at gamestart
        // Any locked doors should have their "setLockState" method called
        doorProgression = 0;
    }

    // isLocked controls if the door can be opened or not
    // This allows a new lockstate to be given
    // Should be called for all doors at game-start
    // If it is locked, then it will also force it to close
    public void setLockState(bool newLock)
    {
        if (newLock)
        {
            isLocked = true;
            keypadLeft.materials[0] = lightRed;
            keypadRight.materials[0] = lightRed;
            CloseDoor();
        }
        else
        {
            isLocked = false;
            keypadLeft.materials[0] = lightGreen;
            keypadRight.materials[0] = lightGreen;
        }
    }

    // Calls these when player enters or exits the triggerzone
    // Will call the open/close functions 
    void OnTriggerEnter(Collider other)
    {
        // Only if player enters
        if (other.tag == "Player")
        {
            OpenDoor();
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            CloseDoor();
        }
    }

    public void OpenDoor()
    {
        // Only works if the door is not locked
        if (!isLocked && !isOpen)
        {
            // "Opens" the door, preventing this from being called while its running
            isOpen = true;
            // Sets new vector positions
        }
    }

    public void CloseDoor()
    {
        isOpen = false;
    }

    // Update function is mostly used to control door movement
    void Update()
    {
        // If the door should be open but isn't
        if ((doorProgression < 1) && (isOpen))
        {
            // Should take two seconds to fully transition
            doorProgression = doorProgression + (Time.deltaTime / 2);
            doorLeft.transform.localPosition = Vector3.Lerp(vecClosed, vecOpen, doorProgression);
            doorRight.transform.localPosition = Vector3.Lerp(-vecClosed, -vecOpen, doorProgression);
            // Changing colour shouldn't be used, this is just here to test it actually changes
            keypadLeft.materials[0].Lerp(lightRed, lightGreen, doorProgression);
            keypadRight.materials[0].Lerp(lightRed, lightGreen, doorProgression);
        }
        // If the door should be closed but isn't
        if ((doorProgression > 0) && (!isOpen))
        {
            // Should take two seconds to fully transition
            doorProgression = doorProgression - (Time.deltaTime / 2);
            doorLeft.transform.localPosition = Vector3.Lerp(vecClosed, vecOpen, doorProgression);
            doorRight.transform.localPosition = Vector3.Lerp(-vecClosed, -vecOpen, doorProgression);
            // Changing colour shouldn't be used, this is just here to test it actually changes
            keypadLeft.materials[0].Lerp(lightRed, lightGreen, doorProgression);
            keypadRight.materials[0].Lerp(lightRed, lightGreen, doorProgression);
        }
    }
}
