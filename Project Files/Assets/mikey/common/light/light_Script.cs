using UnityEngine;
using System.Collections;

public class light_Script : MonoBehaviour {

    private Light lLight;
    private Color redColor;
    private Color whiteColor;
    private ParticleSystem lParticle;
    // fTimer is used to count time, bTimer is used to check if it should be increasing or decreasing
    private float fTimer;
    private bool bTimer;
    private bool isFlashing;
    private float timer = 0;
    private bool gravglitch = false;

    // Use this for initialization
    void Start()
    {
        lLight = gameObject.GetComponent<Light>();
        redColor = new Color(140, 30, 30, 0);
        whiteColor = new Color(0, 0, 0, 0);
        lParticle = gameObject.GetComponent<ParticleSystem>();
        fTimer = 0;
        bTimer = true;
        isFlashing = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Is the light giving a warning flash?
        if (isFlashing)
        {
            // Timer will increase for three seconds, then decrease for 3 seconds
            if (bTimer)
                fTimer = fTimer + Time.deltaTime;
            else
                fTimer = fTimer - Time.deltaTime;
            // Boolean is changed depending on if fTimer goes above 3 or below 0
            if (fTimer > 3)
                bTimer = false;
            else if (fTimer < 0)
                bTimer = true;
            // Will lerp colour between white and red
            lLight.color = Color.Lerp(whiteColor, redColor, fTimer);
        }

        // If isFlashing is disabled but colour is not pure white, will decrease fTimer
        // This should blend in with the above section but not turn red once finished
        if ((!isFlashing) && (fTimer > 0))
        {
            fTimer = fTimer - Time.deltaTime;
            lLight.color = Color.Lerp(whiteColor, redColor, fTimer);
        }

        // Destroying the light will cause gravity to temporarily glitch
        // After 2 seconds, it resets and gravity returns to normal
        if (gravglitch)
        {
            timer = timer + Time.deltaTime;
        }
        if (timer >= 2)
        {
            gravglitch = false;
            timer = 0;
            Physics.gravity = new Vector3(0, -1f, 0);
        }
    }

    public void alarmActive(bool enable)
    {
        // Will stop the light flashing
        isFlashing = enable;
        // Will control which direction the light flashes in when state changes
        // Meaning if the alarm becomes ACTIVE, it will start turning red right away
        bTimer = enable;
    }

    // Will be called if the light is hit by a bullet
    // Effectively "destroys" the light
    public void destroyLight()
    {
        // Disables collider to prevent the light from being shot multiple times
        gameObject.GetComponent<SphereCollider>().enabled = false;
        // Turns off light
        lLight.enabled = false;
        // Emits particles
        lParticle.Emit(50);

        // Destroying a light causes timer to start, which will glitch gravity for a short period of time
        gravglitch = true;
        Physics.gravity = new Vector3((Random.Range(-0.1f, 0.1f)), 1, (Random.Range(-0.1f, 0.1f)));
    }
}
