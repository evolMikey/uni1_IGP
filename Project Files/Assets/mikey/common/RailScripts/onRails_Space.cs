using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class onRails_Space : MonoBehaviour
{
    // Lists are used to store the paths the player must take
    public List<Transform> path0 = new List<Transform>();

    private GameObject playerChar;
    private Vector3 lerpStartPoint;
    private Vector3 lerpEndPoint;
    private float lerpTimer;
    private float lerpLength;
    public int curMarker;

	// Use this for initialization
	void Start ()
    {
        curMarker = 1;
        lerpStartPoint = path0[0].position;
        lerpEndPoint = path0[1].position;
        playerChar = GameObject.Find("playerCollider");
        lerpTimer = 0;
        lerpLength = Vector3.Distance(lerpStartPoint, lerpEndPoint);
	}

    // Update is called once per frame
    void Update()
    {
        lerpTimer = lerpTimer + Time.deltaTime * 3f;
        playerChar.transform.position = Vector3.Lerp(lerpStartPoint, lerpEndPoint, (lerpTimer / lerpLength));
        playerChar.transform.LookAt(lerpEndPoint);
        if (Vector3.Distance(playerChar.transform.position, lerpEndPoint) <= 0.5f)
        {
            nextMarker();
        }
    }

    public void nextMarker()
    {
        lerpTimer = 0;
        curMarker++;
        if (curMarker == 12)
        {
            curMarker = 2;
            lerpStartPoint = path0[11].position;
            lerpEndPoint = path0[2].position;
        }
        else
        {
            lerpStartPoint = path0[(curMarker - 1)].position;
            lerpEndPoint = path0[(curMarker)].position;
        }
        lerpLength = Vector3.Distance(lerpStartPoint, lerpEndPoint);
    }
}
