using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ui_masterScript : MonoBehaviour
{

    // This script controls all the UI elements for the player
    // The objective, score, ammo/clip, and health values are supplied from the player_script
    // The timer itself automatically increases here
    // Ammo and health sprites decrease as the values change
    // The position of the rifle/pistol info will also change when the gun is swapped

    // Default values exist but will be overridden by player_script immediately
    private string objectiveText;
    private int scoreText = 0;
    private int rifleClip = 48; // Clips store the amount of rounds in the gun currently
    private int pistolClip = 12; // Ammo stores the amount of rounds that can be loaded into the clip
    private int rifleAmmo = 192;    // Default max clip for rifle is 48 rounds, max clip for pistol is 12
    private int pistolAmmo = 60;    // Default max ammo for rifle is 192 rounds, max ammo for pistol is 60
    private float timeVal; // Stores time from mission start: won't use Time because this value should be start/stopped/paused when needed
    private bool timePaused = false; // Prevents the timer from progressing if true 
    private float healthLeft = 400; // Amount of health the player has

    private GameObject rifleClipBarTop;
    private GameObject rifleClipBarBot;
    private GameObject pistolClipBar;
    private GameObject rifleAmmoObject;
    private GameObject pistolAmmoObject;
    private GameObject objectiveObject;
    private GameObject timeObject;
    private GameObject scoreObject;
    private GameObject missionSpecsObject;
    private GameObject areaRifle;
    private GameObject areaPistol;
    private GameObject healthBar;
    private GameObject crosshair;
    private Camera camera;

    // Use this for initialization
    void Start()
    {
        // Initializes each UI gameobject so it can be used in code easily
        areaRifle = gameObject.transform.FindChild("area_Rifle").gameObject;
        areaPistol = gameObject.transform.FindChild("area_Pistol").gameObject;
        rifleClipBarTop = areaRifle.transform.FindChild("panel_RifleTop").transform.FindChild("ammoBar_RifleTop").gameObject;
        rifleClipBarBot = areaRifle.transform.FindChild("panel_RifleBot").transform.FindChild("ammoBar_RifleBottom").gameObject;
        pistolClipBar = areaPistol.transform.FindChild("panel_Pistol").transform.FindChild("ammoBar_Pistol").gameObject;
        rifleAmmoObject = areaRifle.transform.FindChild("ammoCounter_Rifle").transform.FindChild("rifle_text").gameObject;
        pistolAmmoObject = areaPistol.transform.FindChild("ammoCounter_Pistol").transform.FindChild("pistol_text").gameObject;
        missionSpecsObject = gameObject.transform.FindChild("missionSpecs_background").gameObject; // Not used except to get children from
        objectiveObject = missionSpecsObject.transform.FindChild("ObjectiveVar").gameObject;
        timeObject = missionSpecsObject.transform.FindChild("TimeVar").gameObject;
        scoreObject = missionSpecsObject.transform.FindChild("ScoreVar").gameObject;
        healthBar = gameObject.transform.FindChild("healthbar").gameObject;
        crosshair = gameObject.transform.FindChild("crosshair").gameObject;
        camera = gameObject.transform.parent.FindChild("fpsCamera").gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // No point updating the timer if it hasn't changed
        if (!timePaused)
        {
            timeVal = timeVal + Time.deltaTime; // Increases time
            // Sets the number of "minutes" and "seconds" to display
            string dispSeconds = "00";
            string dispMinutes = "00";
            dispMinutes = (Mathf.FloorToInt(timeVal / 60)).ToString("00");
            dispSeconds = (Mathf.FloorToInt(timeVal % 60)).ToString("00");
            // Sets the timer on the UI to the appropriate value
            timeObject.GetComponent<Text>().text = (dispMinutes + ":" + dispSeconds);
        }

        // Constantly checking what the crosshair is overlapping
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(new Vector3(566, 272, 0));
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.tag == "npcFriendly")
                crosshair.GetComponent<Image>().color = Color.green;
            else if (hit.transform.tag == "npcEnemy")
                crosshair.GetComponent<Image>().color = Color.red;
            else if (hit.transform.tag == "interact")
                crosshair.GetComponent<Image>().color = Color.blue;
            else
                crosshair.GetComponent<Image>().color = Color.grey;
        }
    }

    // Simply changes the objectiveText to a new one
    public void setObjective(string newObjective)
    {
        objectiveText = newObjective;
        objectiveObject.GetComponent<Text>().text = objectiveText;
    }

    // These exist so the time score can be manipulated if necessary
    public void setTime(int minutes, int seconds)
    {
        // Returns in pure seconds
        timeVal = ((minutes * 60) + (seconds));
    }
    public int getTime()
    {
        return (int)timeVal;
    }
    // Allows time to be paused if true, and unpaused if false
    public void pauseTime(bool pause)
    {
        timePaused = pause;
    }

    // Call this to modify the players score
    // Positive values increase score, negative decrease
    public void setScore(int newScore)
    {
        scoreText = newScore;
        scoreObject.GetComponent<Text>().text = scoreText.ToString();
    }

    public void setBullets(bool rifle, int ammo, int clip)
    {
        /*
        // Allows the ammo (not in current clip) of a gun to be modified
        // A true boolean will affect rifle; a false will affect pistol
        // Error checking should be handled by the player_script but this contains some as well to prevent strange numbers appearing
        */
        if (rifle)
        {
            // Sets clip
            rifleClip = clip;
            adjustClipCounters();
            // Adds ammo
            rifleAmmo = ammo;    // Includes basic error checking to prevent going over max or going below 0
            if (rifleAmmo > 192)
                rifleAmmo = 192;
            if (rifleAmmo < 0)
                rifleAmmo = 0;
            rifleAmmoObject.GetComponent<Text>().text = (rifleAmmo).ToString(); // Sets the appropriate UI element
        }
        else
        {
            // Sets clip
            pistolClip = clip;
            adjustClipCounters();
            // Adds ammo
            pistolAmmo = ammo;  // Includes basic error checking to prevent going over max or going below 0
            if (pistolAmmo > 60)
                pistolAmmo = 60;
            if (pistolAmmo < 0)
                pistolAmmo = 0;
            pistolAmmoObject.GetComponent<Text>().text = (pistolAmmo).ToString();   // Sets the appropriate UI element
        }
    }

    private void adjustClipCounters()
    {
        /*
        I am using a mask to surround the clip bar in the UI. Moving images outside of that will prevent them from being drawn
        This counts the number of bullets missing from a clip, and pushes the image to the right, thus "removing" a bullet icon
        For the rifle, it will check if the number of bullets is even or odd. If odd, then the top bar must be one bullet longer than the bottom
        */

        float rifleTopBullets = 0;
        float rifleBotBullets = 0;
        float pistolBullets = 0;

        // 
        rifleBotBullets = Mathf.Floor(24 - (rifleClip / 2));
        rifleTopBullets = rifleBotBullets;
        if (rifleClip % 2 != 0)
        {
            rifleTopBullets = rifleBotBullets - 1;
        }
        // Pistol is easier, just MaxClip - CurrentClip
        pistolBullets = (12 - pistolClip);

        // Now stretches the masks appropriately
        rifleClipBarTop.GetComponent<RectTransform>().anchoredPosition = new Vector2((rifleTopBullets * 10.66f), 0);
        rifleClipBarBot.GetComponent<RectTransform>().anchoredPosition = new Vector2((rifleBotBullets * 10.66f), 0);
        pistolClipBar.GetComponent<RectTransform>().anchoredPosition = new Vector2((pistolBullets * 16), 0);
    }

    public void switchWeapons(bool rifle)
    {
        /*
        Will swap the canvases containing the rifle/pistol clips and ammo, so the currently active weapon is on top
        Same as other functions, this one uses a boolean to determine which gun is currently being held
        Truth means a rifle is currently being held (and swapped to a pistol) and false means vice-verser
        */
        // Rifle currently held, swaps main weapon to pistol
        if (rifle)
        {
            Debug.Log("UI shows pistol being held!");
            areaPistol.GetComponent<RectTransform>().anchoredPosition = new Vector2(-128, -16);
            areaRifle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-160, -64);
        }
        // Pistol currently held, swaps main weapon to rifle
        else
        {
            Debug.Log("UI shows rifle being held!");
            areaPistol.GetComponent<RectTransform>().anchoredPosition = new Vector2(-128, -80);
            areaRifle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-160, -32);
        }
    }

    // At 100%, the bar will reach both sides of the rect anchor
    // As percentage decreases, it will decrease in width
    // At 0%, the bar should be fully gone, and the player dies
    public void modHealth(float percentageLeft)
    {
        healthLeft = (percentageLeft * 400);
        healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(healthLeft, 0);
    }

    
    // Called by the Pause Screen
    public void onResume()
    {
        // Resumes gameplay
        pauseTime(false);
        Time.timeScale = 1;
        gameObject.transform.parent.FindChild("pause_Canvas").gameObject.SetActive(false);
        gameObject.transform.parent.FindChild("ui_Canvas").GetComponent<Canvas>().enabled = true;
    }
    public void onRestart()
    {
        SceneManager.LoadSceneAsync("spaceScene");
    }
    public void onExit()
    {
        SceneManager.LoadSceneAsync("menuScene");
        SceneManager.UnloadScene("spaceScene");
    }
}