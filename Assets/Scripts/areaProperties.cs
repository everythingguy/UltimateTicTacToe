using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class areaProperties : MonoBehaviour
{
    public bool areaWon { get; private set; }
    public string winner { get; private set; }
    public static int amountWon { get; private set; }

    private void Awake()
    {
        areaWon = false;
        winner = "";
        amountWon = 0;
    }

    public void setWinner(string symbol)
    {
        if(!areaWon)
        {
            amountWon++;
            areaWon = true;
            winner = symbol;

            this.transform.Find("winner symbol").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(symbol);
            this.transform.Find("winner symbol").GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
