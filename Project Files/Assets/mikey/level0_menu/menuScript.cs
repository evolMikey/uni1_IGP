using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class menuScript : MonoBehaviour {

    public void startGame()
    {
        SceneManager.LoadSceneAsync("spaceScene");
    }

    public void exitGame()
    {
        Application.Quit();
    }
}
