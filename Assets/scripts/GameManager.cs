using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject thePlayer;
    public int numEnemies;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetButtonDown("Fire3"))// && !thePlayer.GetComponent<swipeToMove>().isAlive)
        {
            Application.LoadLevel("office test");
            
        }
    }

    public void updateEnemyCount()
    {
        Debug.Log("enemy count changed");
        numEnemies--;
        if(numEnemies <= 0)
        {
            thePlayer.GetComponent<PlayerMovement>().startWin();
        }
    }


    
}
