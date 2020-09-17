using UnityEngine;
using System.Collections;

public class bullet_script : MonoBehaviour
{
    private float existTime = 0;
    private int dmgValue = 20;
    public bool isPlayerBullet = false;
    private player_Script playerScript;

    void Start()
    {
        // Same script is used for all three bullets
        // But different bullets use different names
        // So the damage value varies based on the name
        // Different guns also have different spreads
        if (gameObject.name == "rifleProjectile")
        {
            dmgValue = 20;
            gameObject.GetComponent<ConstantForce>().relativeForce = new Vector3(20, (Random.Range(-10f, 10f)), 0);
        }
        else if (gameObject.name == "pistolProjectile")
        {
            dmgValue = 25;
            gameObject.GetComponent<ConstantForce>().relativeForce = new Vector3(20, (Random.Range(-5f, 5f)), 0);
        }
        else if (gameObject.name == "turretProjectile")
        {
            dmgValue = 5;
            gameObject.GetComponent<ConstantForce>().relativeForce = new Vector3(20, (Random.Range(-15f, 15f)), 0);
        }
        else
        {
            dmgValue = 20;
        }

        playerScript = GameObject.Find("playerCollider").GetComponent<player_Script>();
    }

    // Detects collisions
    void OnTriggerEnter(Collider other)
    {
        // Colliding with a gun does nothing, so to prevent accidental despawning (this is common for all characters)
        if (other.tag == "gun") { }
        // If the player shot the bullet
        else if (isPlayerBullet)
        {
            // Does nothing, to prevent players self-harming
            if (other.tag == "Player") { }
            else if (other.tag == "npcFriendly")
            {
 //               Debug.Log("PLAYER SHOT!");
                // Hurts ally first, then checks health for points
                other.GetComponent<npc_Script>().dmgHealth(dmgValue);
                // If player has killed an ally
                if (other.GetComponent<npc_Script>().getHealth() <= 0)
                {
                    switch (other.GetComponent<npc_Script>().getRank())
                    {
                        case "Colonel":
                            playerScript.adjustScore(-500, "friendlycolonel");
                            break;
                        case "Captain":
                            playerScript.adjustScore(-120, "friendlycaptain");
                            break;
                        case "Sergeant":
                            playerScript.adjustScore(-80, "friendlysergeant");
                            break;
                        case "Private":
                            playerScript.adjustScore(-50, "friendlyprivate");
                            break;
                        default:
                            playerScript.adjustScore(-10, "friendlyunkown");
                            break;
                    }
                }
                // Destroys bullet after it has been fired, to prevent damage being applied multiple times
                Destroy(gameObject);
            }
            else if (other.tag == "npcEnemy")
            {
                // Hurts enemy first, then checks health for points
                other.GetComponent<npc_Script>().dmgHealth(dmgValue);
                // If player has killed an enemy
                if (other.GetComponent<npc_Script>().getHealth() <= 0)
                {
                    switch (other.GetComponent<npc_Script>().getRank())
                    {
                        case "Colonel":
                            // GG
                            playerScript.adjustScore(250, "enemycolonel");
                            break;
                        case "Captain":
                            playerScript.adjustScore(60, "enemycaptain");
                            break;
                        case "Sergeant":
                            playerScript.adjustScore(40, "enemysergeant");
                            break;
                        case "Private":
                            playerScript.adjustScore(25, "enemyprivate");
                            break;
                        default:
                            playerScript.adjustScore(5, "enemyunknown");
                            break;
                    }
                }
                // Destroys bullet after it has been fired, to prevent damage being applied multiple times
                Destroy(gameObject);
            }
            // If a light is hit
            else if (other.tag == "light")
            {
                other.GetComponent<light_Script>().destroyLight();
                playerScript.adjustScore(5, "light");
                // Destroys bullet after it has been fired, to prevent damage being applied multiple times
                Destroy(gameObject);
            }
        }
        else // Bullets shot by others (ally or enemy)
        {
            // Unlike player bullets, these CAN harm the player
            if (other.tag == "Player")
            {
                playerScript.modPlayerHealth(-dmgValue);
                Debug.Log("Player shot!");
                // Destroys bullet after it has been fired, to prevent damage being applied multiple times
                Destroy(gameObject);
            }
            else if ((other.tag == "npcFriendly") || (other.tag == "npcEnemy"))
            {
 //               Debug.Log("NPC SHOT!");
                // Since points aren't awarded for NPC bullets, they treat npcEnemy and npcFriendly the exact same way
                other.GetComponent<npc_Script>().dmgHealth(dmgValue);
                // Destroys bullet after it has been fired, to prevent damage being applied multiple times
                Destroy(gameObject);
            }
            // Also no damage to lights, those are unique to player
        }
    }

    void Update()
    {
        // Increases over time
        // If the bullet doesn't collide after 10 seconds, it deletes itself
        existTime = existTime + Time.deltaTime;
        if (existTime > 5)
            Destroy(gameObject);
    }
}