using UnityEngine;
using System.Collections;

public class npc_Script : MonoBehaviour {

    public bool npcIsEnemy;
    public string npcRank = "Captain";
    public Transform npcTarget;
    public float npcCurHealth = 1f;
    private float npcMaxHealth = 1f;
    private GameObject npcRifle;
    public bool isReloading = false;
    private rangeFinder_Script rangeScript;
    private bool aiEnabled = true;
    private bool isActing = false;
    
    private Material MatNPCNormal;
    private Material MatNPCDead;
    private Material MatNPCReload;

    // Used for movement towards new location
    private float timeTraveling = 0;    // Counts upwards while traveling
    private Transform currentLocation;  // Starting location of a journey (where character currently is)
    public Transform targetLocation;   // Target location of a journey (where the character is going)
    private float locationDistance = 0; // Distance between the two locations
    private float npcSpeed = 3.5f; // Used to boost the NPC's movement speed if its too slow


    // Use this for initialization
    void Start ()
    {
        rangeScript = gameObject.transform.FindChild("targetRange").GetComponent<rangeFinder_Script>();
        MatNPCNormal = Resources.Load("npc/mat_npcNormal") as Material;
        MatNPCReload = Resources.Load("npc/mat_npcReloading") as Material;
        MatNPCDead = Resources.Load("npc/mat_npcDead") as Material;

        npcRifle = gameObject.transform.FindChild("rifle_prop").gameObject;

        currentLocation = gameObject.transform;
        targetLocation = gameObject.transform;
        // Changes the NPC's editor tag to match script, allowing the crosshair to correctly identify friend from foe
        // Also checks NPC name so their rank can be identified, correct weapons given, health adjusted, etc
        if (npcIsEnemy)
        {
            gameObject.tag = "npcEnemy";
            gameObject.transform.FindChild("npcBody").tag = "npcEnemy";
            gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text = "Enemy ";
        }
        else
        {
            gameObject.tag = "npcFriendly";
            gameObject.transform.FindChild("npcBody").tag = "npcFriendly";
            gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text = "Ally ";
        }

        // Sets name and health based on rank
        if (npcRank == "Private")
        {
            npcMaxHealth = 25f;
            gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text = gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text + "Private";
        }
        if (npcRank == "Sergeant")
        {
            npcMaxHealth = 80f;
            gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text = gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text + "Sergeant";
        }
        if (npcRank == "Captain")
        {
            npcMaxHealth = 100f;
            gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text = gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text + "Captain";
        }
        if (npcRank == "Colonel")
        {
            npcMaxHealth = 150f;
            gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text = gameObject.transform.FindChild("npcName").GetComponent<GUIText>().text + "Colonel";
        }

        // Sets current health to max
        npcCurHealth = npcMaxHealth;

        // Creates debug log to inform when an NPC has been made, as well as its allegiance and rank
        if (npcIsEnemy)
        Debug.Log("Creating NPC: Enemy " + npcRank);
        else
        Debug.Log("Creating NPC: Ally " + npcRank);;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Checks if NPC is still alive, runs kill-script if not
        if (npcCurHealth <= 0)
        {
            // Prevents multiple calls
            npcCurHealth = npcMaxHealth;
            npcDeath();
        }
        // If the NPC doesn't currently have a target, they will search for a new one
        if (npcTarget == null)
        {
            FindTarget();
        }
        // Adds to the movement timer, so the NPC constantly walks to their destination
        timeTraveling = timeTraveling + (Time.deltaTime * npcSpeed);
        transform.position = Vector3.Lerp(currentLocation.position, targetLocation.position, (timeTraveling / locationDistance));
        // If the AI has gotten near its position, search for an appropriate target
        if (Vector3.Distance((gameObject.transform.position), (targetLocation.position)) < 5)
            FindTarget();

        // Random-number-generator AI
        if ((aiEnabled) && (!isActing))
        {
            if (Vector3.Distance(gameObject.transform.position, npcTarget.position) < 17)
            {
                StartCoroutine(rngAI());
            }
        }
    }

    private IEnumerator rngAI()
    {
        isActing = true;
        int rng = Random.Range(0, 10);
        // AI will always prioritize reloading if their gun is near-empty
        if (4 > npcRifle.GetComponent<gun_script>().getClip())
        {
            reloadWeapon();
        }
        else
        {
            if (((npcIsEnemy) && ((npcTarget.tag == "Player") || (npcTarget.tag == "npcFriendly"))) || ((!npcIsEnemy) && (npcTarget.tag == "npcEnemy")))
            {
                // 1
                if (rng < 2)
                    reloadWeapon(); // Reloads the gun
                // 2, 3
                else if (rng < 4)
                    StartCoroutine(sprayBurst()); // Fires a prolonged spray of bullets
                // 4, 5, 6
                else if (rng < 7)
                    StartCoroutine(shortBurst()); // Fires many short bursts
                // 7, 8, 9
                else if (rng < 10)
                    StartCoroutine(longBurst()); // Fires few longer bursts
                // 10
                else
                    FindTarget(); // Finds a new target to attack
            }
        }
        // To end, will wait a random amount of time between 1.5 and 2 seconds
        yield return new WaitForSeconds(Random.Range(1.5f, 2f));
        isActing = false;
    }

    // Returns health
    public float getHealth()
    {
        return npcCurHealth;
    }
    // Modifies health (decreases it with each hit)
    public void dmgHealth(int changeBy)
    {
        // Modifies health
        npcCurHealth = npcCurHealth - changeBy;
        // Changes appearance (lerp between green and red) to let player know how hurt NPC is
        gameObject.transform.FindChild("npcBody").GetComponent<MeshRenderer>().material.Lerp(MatNPCDead, MatNPCNormal, (npcCurHealth / npcMaxHealth));
    }

    public string getRank()
    {
        // Rank is used to determin number of points gained by the player for killing this NPC
        return npcRank;
    }
    
    // Sets target to random element from RangeFinder lists, then points towards the target
    private void FindTarget()
    {
        if (npcIsEnemy)
            npcTarget = rangeScript.returnRandom(false);
        else
            npcTarget = rangeScript.returnRandom(true);
        gameObject.transform.LookAt(npcTarget);
    }
    // New location is assigned to the NPC, and they will automatically move towards it
    private void MoveLocation(Transform newLocation)
    {
        currentLocation = gameObject.transform;
        targetLocation = newLocation;
        timeTraveling = 0;
        locationDistance = Vector3.Distance(currentLocation.position, targetLocation.position);
    }

    // Fires four short bursts of gunfire
    private IEnumerator shortBurst()
    {
        npcRifle.GetComponent<gun_script>().changeFiring(true);
        yield return new WaitForSeconds(Random.Range(0.15f, 0.4f));
        npcRifle.GetComponent<gun_script>().changeFiring(false);
        yield return new WaitForSeconds(0.8f);
        npcRifle.GetComponent<gun_script>().changeFiring(true);
        yield return new WaitForSeconds(Random.Range(0.15f, 0.4f));
        npcRifle.GetComponent<gun_script>().changeFiring(false);
        yield return new WaitForSeconds(0.8f);
        npcRifle.GetComponent<gun_script>().changeFiring(true);
        yield return new WaitForSeconds(Random.Range(0.15f, 0.4f));
        npcRifle.GetComponent<gun_script>().changeFiring(false);
        yield return new WaitForSeconds(0.8f);
        npcRifle.GetComponent<gun_script>().changeFiring(true);
        yield return new WaitForSeconds(Random.Range(0.15f, 0.4f));
        npcRifle.GetComponent<gun_script>().changeFiring(false);
    }
    // Fires two longer bursts of gunfire
    private IEnumerator longBurst()
    {
        npcRifle.GetComponent<gun_script>().changeFiring(true);
        yield return new WaitForSeconds(Random.Range(0.5f, 0.8f));
        npcRifle.GetComponent<gun_script>().changeFiring(false);
        yield return new WaitForSeconds(0.8f);
        npcRifle.GetComponent<gun_script>().changeFiring(true);
        yield return new WaitForSeconds(Random.Range(0.5f, 0.8f));
        npcRifle.GetComponent<gun_script>().changeFiring(false);
    }
    // Fires one long burst of gunfire
    private IEnumerator sprayBurst()
    {
        npcRifle.GetComponent<gun_script>().changeFiring(true);
        yield return new WaitForSeconds(Random.Range(1.5f, 2f));
        npcRifle.GetComponent<gun_script>().changeFiring(false);
    }
    

    // Function that makes AI wait a short while before firing again, to simulate reloading
    // AI prioritises this over firing/targeting if they run out of ammo
    public void reloadWeapon()
    {
        isReloading = true;
        gameObject.transform.FindChild("npcBody").GetComponent<MeshRenderer>().material = MatNPCReload;
        StartCoroutine(npcRifle.GetComponent<gun_script>().ReloadGun(48));
    }


    // Death script! Will kill the NPC! Points are awarded to the player via bullet_script so that's not handled here
    private void npcDeath()
    {
        Debug.Log("NPC died!");
        // Disables the AI, so it will no longer make decisions
        aiEnabled = false;
        // Runs a particle emitter once, also disables the body's renderer
        gameObject.GetComponent<ParticleSystem>().Emit(100);
        gameObject.transform.FindChild("npcBody").GetComponent<MeshRenderer>().enabled = false;
        npcRifle.SetActive(false);
        // Creates fake rifle to drop
        GameObject fakeRifle = (Instantiate((Resources.Load("guns/rifle_fake") as GameObject), npcRifle.transform) as GameObject);
        fakeRifle.transform.position = npcRifle.transform.position; // Sets position of gun
        fakeRifle.transform.rotation = npcRifle.transform.rotation; // Sets rotation of gun
        fakeRifle.transform.SetParent(null, true); // Removes parent to prevent it from being deleted with the NPC, tells worldposition to remain the same
        Destroy(fakeRifle, 10f); // Deletes rifle after 10 seconds
        Destroy(gameObject, 1f); // Fully deletes the NPC
    }
}
