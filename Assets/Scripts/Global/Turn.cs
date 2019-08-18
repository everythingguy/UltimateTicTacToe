using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : MonoBehaviour
{
    public bool player1Turn { get; private set; }
    public bool gameOverBool { get; private set; }
    //twoPlayerLocal
    //AI
    //Online
    public string mode;
    private int lastAmountWon;
    public bool opponentConnected { get; private set; }
    private bool firstTurn;

    private void Awake()
    {
        player1Turn = true;
        lastAmountWon = 0;
        gameOverBool = false;
        opponentConnected = false;
        //changed for debugging firstTurn = true;
        firstTurn = false;
    }

    public void endTurn(buttonProperties btn)
    {
        if(firstTurn)
        {
            firstTurn = false;
            if (SceneManager.Instance.networkManager.mode == "Failed")
            {
                Debug.Log("back to main menu");
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
                return;
            }
        }
        if (mode == "Online")
        {
            SceneManager.Instance.networkManager.sendMessage(btn.locationBigBoard + ":" + btn.locationSmallBoard + "</MSG>");
        }
        checkSmallWinner(btn);
        checkBigWinner(btn);

        if (!gameOverBool)
        {
            player1Turn = !player1Turn;

            if (mode == "twoPlayerLocal")
            {
                Player player = SceneManager.Instance.player;
                player.circles = !player.circles;
                player.isPlayer1 = !player.isPlayer1;
            }

            bool nextBoardFull = adjustButtons(btn);
            adjustAreas(btn, nextBoardFull);
        }
    }

    private bool adjustButtons(buttonProperties btn)
    {
        //check if board sent to is full
        string bigBoardCheck = btn.locationSmallBoard;
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("Button");
        int used = 0;
        bool nextBoardFull = false;

        foreach (GameObject button in buttons)
        {
            if (button.GetComponent<click>().isSet() && button.GetComponent<buttonProperties>().locationBigBoard == bigBoardCheck) used++;
        }

        if (used == 9) nextBoardFull = true;
        
        foreach(GameObject button in buttons)
        {
            buttonProperties prop = button.GetComponent<buttonProperties>();
            if(prop.locationBigBoard == btn.locationSmallBoard || nextBoardFull)
            {
                button.GetComponent<BoxCollider2D>().enabled = true;
            } else
            {
                button.GetComponent<BoxCollider2D>().enabled = false;
            }
        }

        return nextBoardFull;
    }

    private void adjustAreas(buttonProperties btn, bool nextBoardFull)
    {
        GameObject[] areas = GameObject.FindGameObjectsWithTag("Area");
        foreach(GameObject area in areas)
        {
            if(area.transform.parent.name == btn.locationSmallBoard || nextBoardFull)
            {
                area.GetComponent<SpriteRenderer>().enabled = false;
            } else
            {
                area.GetComponent<SpriteRenderer>().enabled = true;
            }
        }
    }
    /*  
     *  Check the mini board that was just played in for a winner
     */
    private void checkSmallWinner(buttonProperties btn)
    {
        //no use checking for a winner if the mini board is already won
        if (!btn.transform.parent.GetComponent<areaProperties>().areaWon)
        {
            string symbol = "x";
            if (SceneManager.Instance.player.circles) symbol = "o";
            int buttons = btn.transform.parent.childCount;
            //check rows for winner
            for (int i = 0; i < 9; i = i + 3)
            {
                if (btn.transform.parent.GetChild(i).GetComponent<SpriteRenderer>().sprite.name == symbol &&
                    btn.transform.parent.GetChild(i + 1).GetComponent<SpriteRenderer>().sprite.name == symbol &&
                    btn.transform.parent.GetChild(i + 2).GetComponent<SpriteRenderer>().sprite.name == symbol)
                {
                    btn.transform.parent.GetComponent<areaProperties>().setWinner(symbol);
                }
            }
            //check columns for winner
            for (int i = 0; i < 3; i++)
            {
                if (btn.transform.parent.GetChild(i).GetComponent<SpriteRenderer>().sprite.name == symbol &&
                    btn.transform.parent.GetChild(i + 3).GetComponent<SpriteRenderer>().sprite.name == symbol &&
                    btn.transform.parent.GetChild(i + 6).GetComponent<SpriteRenderer>().sprite.name == symbol)
                {
                    btn.transform.parent.GetComponent<areaProperties>().setWinner(symbol);
                }
            }
            //check diagonals for winner
            for (int i = 0, j = 4; i < 3; i = i + 2, j = j - 2)
            {
                if (btn.transform.parent.GetChild(i).GetComponent<SpriteRenderer>().sprite.name == symbol &&
                    btn.transform.parent.GetChild(i + j).GetComponent<SpriteRenderer>().sprite.name == symbol &&
                    btn.transform.parent.GetChild(i + 2 * j).GetComponent<SpriteRenderer>().sprite.name == symbol)
                {
                    btn.transform.parent.GetComponent<areaProperties>().setWinner(symbol);
                }
            }
        }
    }

    private void checkBigWinner(buttonProperties btn)
    {
        //check if a new board has been won, no use checking for a winner if the board hasn't changed
        //and check if at least 3 mini boards are won because you need at least 3 to win
        if(lastAmountWon != areaProperties.amountWon && areaProperties.amountWon >= 3)
        {
            lastAmountWon = areaProperties.amountWon;

            string symbol = "x";
            if (SceneManager.Instance.player.circles) symbol = "o";
            GameObject buttons = GameObject.Find("Buttons");
            //check rows for winner
            for(int i = 0; i < 9; i = i + 3)
            {
                if(buttons.transform.GetChild(i).GetComponent<areaProperties>().winner == symbol &&
                    buttons.transform.GetChild(i + 1).GetComponent<areaProperties>().winner == symbol &&
                    buttons.transform.GetChild(i + 2).GetComponent<areaProperties>().winner == symbol)
                {
                    gameOver(symbol);
                    return;
                }
            }

            for(int i = 0; i < 3; i++)
            {
                if(buttons.transform.GetChild(i).GetComponent<areaProperties>().winner == symbol &&
                    buttons.transform.GetChild(i + 3).GetComponent<areaProperties>().winner == symbol &&
                    buttons.transform.GetChild(i + 6).GetComponent<areaProperties>().winner == symbol)
                {
                    gameOver(symbol);
                    return;
                }
            }

            for(int i = 0, j = 4; i < 3; i = i + 2, j = j - 2)
            {
                if(buttons.transform.GetChild(i).GetComponent<areaProperties>().winner == symbol &&
                    buttons.transform.GetChild(i + j).GetComponent<areaProperties>().winner == symbol &&
                    buttons.transform.GetChild(i + 2 * j).GetComponent<areaProperties>().winner == symbol)
                {
                    gameOver(symbol);
                    return;
                }
            }
        }
    }

    private void gameOver(string symbol)
    {
        gameOverBool = true;

        GameObject[] areas = GameObject.FindGameObjectsWithTag("Area");
        foreach (GameObject area in areas)
            area.GetComponent<SpriteRenderer>().enabled = false;

        SpriteRenderer winnerIcon = GameObject.Find("Winner Icon").GetComponent<SpriteRenderer>();
        winnerIcon.sprite = Resources.Load<Sprite>(symbol);
        winnerIcon.enabled = true;
    }
}
