using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndgameScreen : MonoBehaviour
{
    [SerializeField] private GameObject endgameScreen;
    [SerializeField] private TMP_Text gameResult;
    [SerializeField] private TMP_Text turnsPlayedResult;
    [SerializeField] private TMP_Text whoWonResult;
    [SerializeField] private string victoryTitle = "Ganaste!";
    [SerializeField] private string defeatTitle = "Perdiste!";
    [SerializeField] private string enemiesName = "Enemies";

    public void ShowEndgameScreen(bool isVictory, int turnsPlayed, string playerName)
    {
        if(isVictory)
        {
            gameResult.text = victoryTitle;
            whoWonResult.text = playerName;
        }
        else
        {
            gameResult.text = defeatTitle;
            whoWonResult.text = enemiesName;
        }

        turnsPlayedResult.text = turnsPlayed.ToString() + " turnos.";

        endgameScreen.SetActive(true);
    }
}
