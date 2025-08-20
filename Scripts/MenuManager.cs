using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    GameObject MainMenu;

    [SerializeField]
    GameObject OptionMenu;

    [SerializeField]
    GameObject GameModeMenu;

    [SerializeField]
    GameObject DifficultyMenu;

    [SerializeField]
    GameObject ChooseColorMenu;

    [SerializeField]
    GameObject Rule;

    [SerializeField]
    GameObject SaveAlert;

    [SerializeField]
    GameObject ConfirmQuit;

    CpuSettings cpuSettings = new CpuSettings();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MainMenu.activeSelf && !ConfirmQuit.activeSelf)
            {
                Debug.Log("pressed esc in main menu");
                ConfirmQuit.SetActive(true);
                Debug.Log(ConfirmQuit.activeSelf);
            }
            else if (ConfirmQuit.activeSelf)
            {
                OnNoQuitClicked();
            }
            if (OptionMenu.activeSelf)
            {
                OnOptionsBackClicked();
            }
            if (Rule.activeSelf)
            {
                OnRuleBackClicked();
            }
            if (GameModeMenu.activeSelf)
            {
                OnGameModeBackClicked();
            }
            if (DifficultyMenu.activeSelf)
            {
                OnDifficultyBackClicked();
            }
            if (ChooseColorMenu.activeSelf)
            {
                OnChooseColorBackClicked();
            }
            if (SaveAlert.activeSelf)
            {
                OnSaveAlertBackClicked();
            }
        }
    }

    public void OnNoQuitClicked()
    {
        ConfirmQuit.SetActive(false);
    }

    public void OnEasyClicked()
    {
        cpuSettings.Easy();
        DifficultyMenu.SetActive(false);
        ChooseColorMenu.SetActive(true);
    }

    public void OnMediumClicked()
    {
        cpuSettings.Medium();
        DifficultyMenu.SetActive(false);
        ChooseColorMenu.SetActive(true);
    }

    public void OnHardClicked()
    {
        cpuSettings.Hard();
        DifficultyMenu.SetActive(false);
        ChooseColorMenu.SetActive(true);
    }

    public void OnHvHClicked()
    {
        cpuSettings.None();
        SceneManager.LoadScene("MainGame");
    }

    public void OnOptionClicked()
    {
        MainMenu.SetActive(false);
        OptionMenu.SetActive(true);
    }

    public void OnOptionsBackClicked()
    {
        OptionMenu.SetActive(false);
        MainMenu.SetActive(true);
    }
    public void OnPlayClicked()
    {
        if (PlayerPrefs.GetString("GameSave") != null && PlayerPrefs.GetString("GameSave").Length == 67)
        {
            SaveAlert.SetActive(true);
        }
        else
        {
            GameModeMenu.SetActive(true);
            MainMenu.SetActive(false);
        }
    }

    public void OnYesClicked()
    {
        string save = PlayerPrefs.GetString("GameSave");
        Player[,] Board = new Player[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                switch (save[i * 8 + j])
                {
                    case 'b':
                        Board[i, j] = Player.Black;
                        break;
                    case 'w':
                        Board[i, j] = Player.White;
                        break;
                    case 'n':
                        Board[i, j] = Player.None;
                        break;
                }
            }
        }
        cpuSettings.SetBoard(Board);

        Player currentPlayer;

        if (save[64] == 'b')
        {
            currentPlayer = Player.Black;
        }
        else
        {
            currentPlayer = Player.White;
        }

        cpuSettings.SetCurrentPlayer(currentPlayer);

        cpuSettings.SetDepth((int)char.GetNumericValue(save[65]));
        switch (save[66])
        {
            case 'b':
                cpuSettings.Black();
                break;
            case 'w':
                cpuSettings.White();
                break;
            case 'n':
                cpuSettings.None();
                break;
        }

        SceneManager.LoadScene("MainGame");
    }

    public void OnStartNewClicked()
    {
        SaveAlert.SetActive(false);
        GameModeMenu.SetActive(true);
        MainMenu.SetActive(false);
        PlayerPrefs.SetString("GameSave", null);
    }

    public void OnGameModeBackClicked()
    {
        MainMenu.SetActive(true);
        GameModeMenu.SetActive(false);
    }

    public void OnHvEClicked()
    {
        GameModeMenu.SetActive(false);
        DifficultyMenu.SetActive(true);
    }

    public void OnDifficultyBackClicked()
    {
        DifficultyMenu.SetActive(false);
        GameModeMenu.SetActive(true);
    }

    public void OnRuleClicked()
    {
        MainMenu.SetActive(false);
        Rule.SetActive(true);
    }
    public void OnRuleBackClicked()
    {
        Rule.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void OnChooseColorBackClicked()
    {
        ChooseColorMenu.SetActive(false);
        DifficultyMenu.SetActive(true);
    }

    public void OnBlackClicked()
    {
        cpuSettings.White();
        SceneManager.LoadScene("MainGame");
    }
    
    public void OnWhiteClicked()
    {
        cpuSettings.Black();
        SceneManager.LoadScene("MainGame");
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }

    public void OnSaveAlertBackClicked()
    {
        SaveAlert.SetActive(false);
    }
}
