using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class rangeFinder_Script : MonoBehaviour
{
    /*
     * The purpose of this script is to log all characters (player or NPC) who enters the sphere radius
     * It can return allies or enemies. This is in reference to the player (allies won't shoot player, enemies will) not the NPC itself
     */

    // One list for allies, one for enemies    
    public List<Transform> EnemiesList = new List<Transform>();
    public List<Transform> AlliesList = new List<Transform>();

    // Detects collisions, checks if they're ally/enemy, adds to appropriate list
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != gameObject.transform.parent.gameObject)
        {
            Debug.Log("FOUND!");
            // Ally of player, or player themselves
            if ((other.tag == "npcFriendly") || (other.tag == "Player"))
            {
                Debug.Log("ALLY!");
                AlliesList.Add(other.transform);
            }
            // Enemy
            if (other.tag == "npcEnemy")
            {
                Debug.Log("ENEMY!");
                EnemiesList.Add(other.transform);
            }
        }
    }

    // Returns a specific object from the defined list
    // "isAlly true" will pull from AlliesList
    // "isAlly false" will pull from EnemiesList
    public Transform getObject(bool isAlly, int elementNum)
    {
        if ((isAlly) && (elementNum <= AlliesList.Count))
            return AlliesList[elementNum];
        if ((!isAlly) && (elementNum <= EnemiesList.Count))
            return EnemiesList[elementNum];
        return gameObject.transform;
    }
    // Gets how many elements there are in the lists
    public int countObjects(bool isAlly)
    {
        if (isAlly)
            return AlliesList.Count;
        if (!isAlly)
            return EnemiesList.Count;
        return 0;
    }

    /*
     * Called by NPC "FindTarget" function
     * First asks if the NPC is an ally or enemy, to dictate which list it searches
     * Checks if the list actually has an element in it
     * Will then return a random element from the list between 0 (first) and count (last)
     * IsAlly and else both do same thing but for different lists
    */
    public Transform returnRandom(bool isAlly)
    {
        if (isAlly)
        {
            if (EnemiesList.Count > 0)
            {
                return (EnemiesList[(Random.Range(0, EnemiesList.Count))]);
            }
        }
        if (!isAlly)
        {
            if (AlliesList.Count > 0)
            {
                return (AlliesList[(Random.Range(0, AlliesList.Count))]);
            }
        }
        // Fallback in case either list returns null, will point at itself... somehow.
        return GameObject.Find("playerCollider").transform;
    }
}