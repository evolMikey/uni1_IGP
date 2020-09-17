using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class deathCam_Script : MonoBehaviour
{
    // Very basic, just rotates around the player's body after they die
    private Vector3 playerbody;
    private List<string> pointsString = new List<string>();
    private List<int> pointsNum = new List<int>();
    private bool panning = true;
    private string timeText;

    private void Start()
    {
        // Positions camera inside body, then adjusts location slightly to be outside and above
        playerbody = GameObject.Find("playerCollider").transform.position;
        Vector3 tempPos = playerbody;
        tempPos.x = tempPos.x + 2;
        tempPos.y = tempPos.y + 2;
        gameObject.transform.position = tempPos;
        gameObject.transform.SetParent(null, true);
        pointsString = GameObject.Find("playerCollider").GetComponent<player_Script>().listOfPointsStr;
        pointsNum = GameObject.Find("playerCollider").GetComponent<player_Script>().listOfPointsNum;
        updateScoreList();
        StartCoroutine(displayScoreboard());

        int timeVal = GameObject.Find("ui_Canvas").GetComponent<ui_masterScript>().getTime();
        string dispSeconds = "00";
        string dispMinutes = "00";
        dispMinutes = (Mathf.FloorToInt(timeVal / 60)).ToString("00");
        dispSeconds = (Mathf.FloorToInt(timeVal % 60)).ToString("00");
        // Sets the timer on the UI to the appropriate value
        timeText = (dispMinutes + ":" + dispSeconds);
        Debug.Log(timeVal);
        Debug.Log(timeText);
    }

    void Update ()
    {
        // Will pan around the player's body after death
        // Will stop panning after 5 seconds to display the deathscreen
        if (panning)
        {
            gameObject.transform.LookAt(playerbody);
            gameObject.transform.Translate(Vector3.right * (Time.deltaTime) / 2);
        }
    }

    private IEnumerator displayScoreboard()
    {
        // Waits for 5 seconds before displaying the 
        yield return new WaitForSeconds(10);
        panning = false;
        gameObject.transform.position = new Vector3(1000, 1000, 1000);
        gameObject.transform.FindChild("death_Canvas").GetComponent<Canvas>().enabled = true;
    }

    private void updateScoreList()
    {

        // Counters for each type of unit killed
        int ecolonel = 0;
        int ecaptain = 0;
        int esergeant = 0;
        int eprivate = 0;
        int eunknown = 0;
        int acolonel = 0;
        int acaptain = 0;
        int asergeant = 0;
        int aprivate = 0;
        int aunknown = 0;
        int light = 0;

        // Counts each reason for points from the scoreboard
        for (int index = 0; index < pointsString.Count; index++)
        {
            if (pointsString[index] == "enemycolonel")
                ecolonel++;
            if (pointsString[index] == "enemycaptain")
                ecaptain++;
            if (pointsString[index] == "enemysergeant")
                esergeant++;
            if (pointsString[index] == "enemyprivate")
                eprivate++;
            if (pointsString[index] == "enemyunkown")
                eunknown++;
            if (pointsString[index] == "friendlycolonel")
                acolonel++;
            if (pointsString[index] == "friendlycaptain")
                acaptain++;
            if (pointsString[index] == "friendlysergeant")
                asergeant++;
            if (pointsString[index] == "friendlyprivate")
                aprivate++;
            if (pointsString[index] == "friendlyunknown")
                aunknown++;
            if (pointsString[index] == "light")
                light++;
        }
        string ecolonelkilled = "Colonels: " + ecolonel;
        string ecaptainkilled = "Captains: " + ecaptain;
        string esergeantkilled = "Sergeants: " + esergeant;
        string eprivatekilled = "Privates: " + eprivate;
        string acolonelkilled = "Colonels: " + acolonel;
        string acaptainkilled = "Captains: " + acaptain;
        string asergeantkilled = "Sergeants: " + asergeant;
        string aprivatekilled = "Privates: " + aprivate;
        string lightskilled = "Lights destroyed: " + light;
        string enemieskilled = "Total enemies killed: " + (ecolonel + ecaptain + esergeant + eprivate);
        string allieskilled = "Total allies killed: " + (acolonel + acaptain + asergeant + aprivate);
        // Basically just combines all the strings together
        gameObject.transform.FindChild("death_Canvas").FindChild("casualtiesList").GetComponent<Text>().text = "Time: " + timeText + "\n" + enemieskilled + "\n" + ecolonelkilled + "\n" + ecaptainkilled + "\n" + esergeantkilled + "\n" + eprivatekilled + "\n \n" + allieskilled + "\n" + acolonelkilled + "\n" + acaptainkilled + "\n" + asergeantkilled + "\n" + aprivatekilled + "\n \n" + lightskilled;
    }
}
