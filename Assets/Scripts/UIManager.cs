using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject panelPause;
    [SerializeField] private GameObject panelWon;
    [SerializeField] private GameObject panelLose;

    public void PauseOn() 
    {
        panelPause.SetActive(true);
        Time.timeScale = 0;
    }

    public void PauseOff() 
    {
        panelPause.SetActive(false);
        Time.timeScale = 1;
    }

    public void Won() 
    {
        panelWon.SetActive(true);
        Time.timeScale = 0;
    }

    public void DelayWon() 
    {
        Invoke("Won", 2f);
    }

    public void Lose() 
    {
        panelLose.SetActive(true);
        Time.timeScale = 0;
    }

    public void DelayLose() 
    {
        Invoke("Lose", 2f);
    }
}
