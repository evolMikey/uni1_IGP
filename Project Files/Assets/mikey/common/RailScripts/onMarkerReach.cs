using UnityEngine;
using System.Collections;

public class onMarkerReach : MonoBehaviour
{

    private onRails_Space railsScript;
    private bool hasReached = false;

    private void Start()
    {
        railsScript = gameObject.GetComponentInParent<onRails_Space>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Touched!");
        if (!hasReached)
        {
            if (other.tag == "Player")
            {
                StartCoroutine(disableBox());
            }
        }
    }

    // Temporarily disables the rail box
    private IEnumerator disableBox()
    {
        hasReached = true;
        yield return new WaitForSeconds(5);
        hasReached = false;
    }
}