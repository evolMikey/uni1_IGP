using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class player_Script : MonoBehaviour {

    public float pHealth = 400f; // 400 is max, divide this by 400 to get percentage left
    private int rClip = 48;
    private int pClip = 12;
    private int rAmmo = 192;
    private int pAmmo = 60;
    private bool mainWeapon = true;
    private int pScore = 0;
    private string pObjective = "N/A";
    private ui_masterScript uiScript;
    private Vector3 camFPSRotation;
    private bool isReloading = false;
    private bool gamePaused = false;

    private GameObject rifleHolding;
    private GameObject rifleHolstered;
    private GameObject pistolHolding;
    private GameObject pistolHolstered;
    private GameObject camFirstPerson;
    private GameObject camDeath;

    private List<GameObject> rifleParts = new List<GameObject>();
    private List<GameObject> pistolParts = new List<GameObject>();

    // Used in the win screen
    public List<string> listOfPointsStr = new List<string>();
    public List<int> listOfPointsNum = new List<int>();

    // Use this for initialization
    void Start ()
    {
        // Initializes each variable
        uiScript = gameObject.transform.FindChild("ui_Canvas").GetComponent<ui_masterScript>();
        camFirstPerson = gameObject.transform.FindChild("fpsCamera").gameObject;
        camDeath = gameObject.transform.FindChild("deathCam").gameObject;
        rifleHolding = gameObject.transform.FindChild("rifle_prop").gameObject;
        rifleHolstered = gameObject.transform.FindChild("rifle_fake").gameObject;
        pistolHolding = gameObject.transform.FindChild("pistol_prop").gameObject;
        pistolHolstered = gameObject.transform.FindChild("pistol_fake").gameObject;
        camFPSRotation = new Vector3(0, 0, 0);
        pistolHolding.GetComponent<gun_script>().playerWeapon = true;
        rifleHolding.GetComponent<gun_script>().playerWeapon = true;

        // Fills the gunParts lists with all the GameObjects in both boths
        /*
         * Adds gameObject[parts] to the list, where parts is the index of the child in the weapon
         * Simply put: will loop through every part in the gun and add it to a list
         * Debugs were included to make sure all parts were added
         */
        for (int parts = 0; parts < (rifleHolding.transform.childCount); parts++)
        {
            rifleParts.Add(rifleHolding.transform.GetChild(parts).gameObject);
            rifleHolding.transform.GetChild(parts).gameObject.SetActive(true); // Shows each rifle piece at game start
//            Debug.Log("Added " + rifleHolding.transform.GetChild(parts).name);
        }
//        Debug.Log("Finished adding rifle parts");
        for (int parts = 0; parts < (pistolHolding.transform.childCount); parts++)
        {
            pistolParts.Add(pistolHolding.transform.GetChild(parts).gameObject);
            pistolHolding.transform.GetChild(parts).gameObject.SetActive(false); // Hides each pistol piece at game start
//            Debug.Log("Added " + pistolHolding.transform.GetChild(parts).name);
        }
//        Debug.Log("Finished adding pistol parts");
    }

    // Update is called once per frame
    void Update () {
        // Player can only use controls while the game is unpaused
        if (!gamePaused)
        {
            if (!isReloading) //Will not do if the gun is reloading
            {
                // Various elements to check if the player is pressing specific keys
                if (Input.GetMouseButtonDown(0)) // Left mousebutton being held
                {
                    // Checks if player is currently wielding pistol or rifle, fires appropriate gun
                    if (mainWeapon)
                        rifleHolding.GetComponent<gun_script>().isFiring = true;
                    else
                        pistolHolding.GetComponent<gun_script>().isFiring = true;
                }
                if (Input.GetMouseButtonUp(0)) // Left mousebutton released
                {
                    // Stops the firing of rifles
                    rifleHolding.GetComponent<gun_script>().isFiring = false;
                    pistolHolding.GetComponent<gun_script>().isFiring = false;
                }
                if (Input.GetMouseButton(0))
                {
                    // Sets the current clips (according to player) to the ones the guns say they have
                    rClip = rifleHolding.GetComponent<gun_script>().getClip();
                    uiScript.setBullets(true, rAmmo, rClip);
                    pClip = pistolHolding.GetComponent<gun_script>().getClip();
                    uiScript.setBullets(false, pAmmo, pClip);
                }
            }
            /*
             * I chose "down" and "up" instead of continuous holding because of how the gun script itself works
             * If the pistol is firing, it will release "isFiring" on its own, while the rifle will continue firing until changed
             * If I used continuous holding, then the pistol would keep firing despite the automatic release
             */

            if (Input.GetKeyDown(KeyCode.F)) // F key swaps weapons
            {
                // Stops current gun firing
                pistolHolding.GetComponent<gun_script>().isFiring = false;
                rifleHolding.GetComponent<gun_script>().isFiring = false;
                // Swaps weapons
                switchWeapons();
            }

            if (Input.GetKeyDown(KeyCode.R)) // R key reloads guns
            {
                Debug.Log("Player is reloading (R pressed)");
                // Refills clip, tells gun to make noise, etc
                // More details can be found in the function and gun_script
                reloadGun();
            }

            // Records movement of the mouse and applies it to a new Vector3
            // Clamps included to prevent the player turning too far away from the path they're supposed to follow
            if (Input.GetAxis("Mouse Y") != 0)
            {
                camFPSRotation.x -= (Input.GetAxis("Mouse Y") * 5);
                camFPSRotation.x = Mathf.Clamp(camFPSRotation.x, -40, 40);
            }
            if (Input.GetAxis("Mouse X") != 0)
            {
                camFPSRotation.y += (Input.GetAxis("Mouse X") * 5);
                camFPSRotation.y = Mathf.Clamp(camFPSRotation.y, -50, 50);
            }
            // Applies said Vector3 to the camera, as well as guns (aim offsets slightly when shooting near the edges)
            camFirstPerson.transform.localRotation = Quaternion.Euler(camFPSRotation);
            rifleHolding.transform.localRotation = Quaternion.Euler(new Vector3(0, camFPSRotation.y - 90, -camFPSRotation.x));
            pistolHolding.transform.localRotation = Quaternion.Euler(new Vector3(0, camFPSRotation.y - 90, -camFPSRotation.x));
        }

        // Escape button is pressed aka pause button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Pausing game!");
            gameObject.transform.FindChild("pause_Canvas").gameObject.SetActive(true);
            gameObject.transform.FindChild("ui_Canvas").GetComponent<Canvas>().enabled = false;
            uiScript.pauseTime(true);
            Time.timeScale = 0;
        }
    }

    // Modifies the players health: negative values harm player, positive heal
    public void modPlayerHealth(int changeBy)
    {
        pHealth = pHealth + changeBy;
        uiScript.modHealth(pHealth / 400);
        if (pHealth <= 0)
        {
            killPlayer();
        }
    }

    // Runs when player is killed, semi-copied from the NPC script
    public void killPlayer()
    {
        camDeath.SetActive(true);
        camFirstPerson.SetActive(false);

        // Disables collider to prevent additional calls
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        // Runs a particle emitter once, also disables the body's renderer
        gameObject.GetComponent<ParticleSystem>().Emit(100);
        gameObject.transform.FindChild("playerBody").GetComponent<MeshRenderer>().enabled = false;
        gameObject.transform.FindChild("ui_Canvas").GetComponent<Canvas>().enabled = false;
        if (mainWeapon)
        {
            rifleHolding.SetActive(false);
            // Creates fake rifle to drop
            GameObject fakeRifle = (Instantiate((Resources.Load("guns/rifle_fake") as GameObject), rifleHolding.transform) as GameObject);
            fakeRifle.transform.position = rifleHolding.transform.position; // Sets position of gun
            fakeRifle.transform.rotation = rifleHolding.transform.rotation; // Sets rotation of gun
            fakeRifle.transform.SetParent(null, true); // Removes parent to prevent it from being deleted with the NPC, tells worldposition to remain the same
            // Creates fake pistol to drop
            GameObject fakePistol = (Instantiate((Resources.Load("guns/pistol_fake") as GameObject), pistolHolding.transform) as GameObject);
            fakePistol.transform.position = pistolHolstered.transform.position; // Sets position of gun
            fakePistol.transform.rotation = pistolHolstered.transform.rotation; // Sets rotation of gun
            fakePistol.transform.SetParent(null, true); // Removes parent to prevent it from being deleted with the NPC, tells worldposition to remain the same
            Destroy(rifleHolding);
            Destroy(pistolHolstered);
            Destroy(fakeRifle, 10f); // Deletes rifle after 10 seconds
            Destroy(fakePistol, 10f); // Deletes rifle after 10 seconds
        }
        else
        {
            pistolHolding.SetActive(false);
            // Creates fake rifle to drop
            GameObject fakeRifle = (Instantiate((Resources.Load("guns/rifle_fake") as GameObject), rifleHolding.transform) as GameObject);
            fakeRifle.transform.position = rifleHolstered.transform.position; // Sets position of gun
            fakeRifle.transform.rotation = rifleHolstered.transform.rotation; // Sets rotation of gun
            fakeRifle.transform.SetParent(null, true); // Removes parent to prevent it from being deleted with the NPC, tells worldposition to remain the same
            // Creates fake pistol to drop
            GameObject fakePistol = (Instantiate((Resources.Load("guns/pistol_fake") as GameObject), pistolHolding.transform) as GameObject);
            fakePistol.transform.position = pistolHolding.transform.position; // Sets position of gun
            fakePistol.transform.rotation = pistolHolding.transform.rotation; // Sets rotation of gun
            fakePistol.transform.SetParent(null, true); // Removes parent to prevent it from being deleted with the NPC, tells worldposition to remain the same
            Destroy(rifleHolstered);
            Destroy(pistolHolding);
            Destroy(fakePistol, 10f); // Deletes rifle after 10 seconds
            Destroy(fakeRifle, 10f); // Deletes rifle after 10 seconds
        }
        Destroy(gameObject, 1f); // Fully deletes the NPC
    }

    private void reloadGun()
    {
        Debug.Log("Player is reloading (function pre check)");
        if (!isReloading) //Will not do if the gun is reloading
        {
            isReloading = true;
            Debug.Log("Player is reloading (function post check)");
            /*
            * Gets true ammo-count from the guns
            * Returns the current clip to the ammo
            * Fills clip to max (if possible) or fills with all remaining ammo
            * Adjusts UI as necessary
            * Tells gun to reload (stop firing, make sound, adjust its own internal clip)
            */
            if (mainWeapon)
            {
                rClip = rifleHolding.GetComponent<gun_script>().getClip();
                // Checks if any reloading is actually needed
                if (rClip != 48)
                {
                    rAmmo = rAmmo + rClip;  // Returns current clip to the total ammo
                    if (rAmmo >= 48)    // Checks if a full clip can be given back
                    {
                        rClip = 48; // A full clip is placed, and removed from ammo counter
                        rAmmo = rAmmo - 48;
                    }
                    else
                    {
                        rClip = rAmmo;  // What ammo remains is used as a single clip, ammo is now empty
                        rAmmo = 0;
                    }
                    StartCoroutine(rifleHolding.GetComponent<gun_script>().ReloadGun(rClip)); //Contains a WaitForSeconds function so must be called via Coroutine
                }
            }
            else
            {
                pClip = pistolHolding.GetComponent<gun_script>().getClip();
                if (pClip != 12)   // Checks if any reloading is actually needed
                {
                    pAmmo = pAmmo + pClip;   // Returns current clip to the total ammo
                    if (pAmmo >= 12)   // Checks if a full clip can be given back
                    {
                        pClip = 12;    // A full clip is placed, and removed from ammo counter
                        pAmmo = pAmmo - 12;
                    }
                    else
                    {
                        pClip = pAmmo;    // What ammo remains is used as a single clip, ammo is now empty
                        pAmmo = 0;
                    }
                    StartCoroutine(pistolHolding.GetComponent<gun_script>().ReloadGun(pClip));
                }
            }
            isReloading = false;
        }
    }
    public void updateAmmoUI()
    {
        uiScript.setBullets(true, rAmmo, rClip);
        uiScript.setBullets(false, pAmmo, pClip);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Touching");
        if (other.name == "rifle_fake")
            rAmmo = rAmmo + Random.Range(3, 24);
        if (other.name == "rifle_fake(clone)")
            rAmmo = rAmmo + Random.Range(3, 24);
        if (other.name == "pistol_fake")
            pAmmo = pAmmo + Random.Range(3, 24);
        if (other.name == "pistol_fake(clone)")
            pAmmo = pAmmo + Random.Range(3, 24);
        updateAmmoUI();
    }

    private void switchWeapons()
    {
        if (!isReloading) //Will not do if the gun is reloading
        {
            /*
             The reason I am using for-loops and lists here:
             Attempting to hide the whole gun seemed to mess up the amount of ammo it had left
             Instead of hiding the ENTIRE gun, I decided instead to hide all its parts
             The script containing ammo is an empty gameobject that still exists but is not visible
             The fake guns are still shown/hidden through their parent because they don't have scripts attached and are purely cosmetic
             */

            // If the player currently wields the rifle, switch to pistol
            if (mainWeapon)
            {
                mainWeapon = false; // Tells player_script they are not using main weapon anymore
                uiScript.switchWeapons(true); // Tells UI to switch ammo bars (true means pistol is held)
                                              // Show/hide real guns
                for (int parts = 0; parts < (pistolParts.Count); parts++)
                {
                    pistolParts[parts].gameObject.SetActive(true);
                    //                Debug.Log("Showing: " + pistolParts[parts].gameObject.name);
                }
                for (int parts = 0; parts < (rifleParts.Count); parts++)
                {
                    rifleParts[parts].gameObject.SetActive(false);
                    //                Debug.Log("Hiding: " + rifleParts[parts].gameObject.name);
                }
                // Show/hide fake guns
                rifleHolstered.SetActive(true);
                pistolHolstered.SetActive(false);
            }
            else // If the player currently wields the pistol, switch to rifle
            {
                mainWeapon = true; // Rifle is now being held
                uiScript.switchWeapons(false); // Tells UI to switch ammo bars (false means rifle is held)
                for (int parts = 0; parts < (rifleParts.Count); parts++)
                {
                    rifleParts[parts].gameObject.SetActive(true);
                    //                Debug.Log("Showing: " + rifleParts[parts].gameObject.name);
                }
                for (int parts = 0; parts < (pistolParts.Count); parts++)
                {
                    pistolParts[parts].gameObject.SetActive(false);
                    //                Debug.Log("Hiding: " + pistolParts[parts].gameObject.name);
                }
                // Show/hide fake guns
                rifleHolstered.SetActive(false);
                pistolHolstered.SetActive(true);
            }
        }
    }

    // Called to change the player's score
    public void adjustScore(int changeBy, string changeReason)
    {
        // Adds score to the player's internal counter
        pScore = pScore + changeBy;
        // Adds score to the 
        listOfPointsNum.Add(changeBy);
        listOfPointsStr.Add(changeReason);
        uiScript.setScore(pScore);
    }
}