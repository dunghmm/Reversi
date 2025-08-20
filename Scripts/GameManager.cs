using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject plane;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Disc discBlackUp;

    [SerializeField]
    private Disc discWhiteUp;

    [SerializeField]
    private GameObject highlightPrefab;

    [SerializeField]
    private UIManager uiManager;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioSource placeDiscSound;

    [SerializeField]
    AudioSource boardStartSound;

    [SerializeField]
    GameObject DirectLight;

    [SerializeField]
    GameObject SpotLight;

    [SerializeField]
    GameObject cpuThinkingOverlay;

    private Dictionary<Player, Disc> discPrefabs = new Dictionary<Player, Disc>();
    private GameState gameState = new GameState();
    private Disc[,] discs = new Disc[8, 8];
    private List<GameObject> highlights = new List<GameObject>();
    private BotLogic botLogic = new BotLogic();
    private string planePictureName;
    private CpuSettings cpuSettings = new CpuSettings();
    private float placeAnimationTime = 0.33f;
    private float flipAnimationTime = 0.83f;
    private bool botTurn;

    private void OnEnable()
    {
        if (PlayerPrefs.GetFloat("AnimationSpeed") != 0)
        {
            placeAnimationTime /= PlayerPrefs.GetFloat("AnimationSpeed");
            flipAnimationTime /= PlayerPrefs.GetFloat("AnimationSpeed");
        }

        if (PlayerPrefs.GetInt("Night") == 1)
        {
            DirectLight.SetActive(false);
            SpotLight.SetActive(true);
        }

        if (cpuSettings.GetPlayer() == Player.None)
        {
            uiManager.ToggleShowUndoRedo();
        }

        if (PlayerPrefs.GetInt("Music") == 1)
        {
            audioSource.Play();
        }

        if (PlayerPrefs.GetInt("IsUsingUserImage") != 1)
        {
            planePictureName = PlayerPrefs.GetString("planePicture");
            Material material = plane.GetComponent<MeshRenderer>().material;
            Sprite sprite = Resources.Load<Sprite>("PlaneTemplates/" + planePictureName);
            material.mainTexture = sprite.texture;
        }
        else
        {
            StartCoroutine(SetPlaneToUserImage());
        }

        if (PlayerPrefs.GetInt("Sound") == 1)
        {
            boardStartSound.Play();
        }
    }

    private IEnumerator SetPlaneToUserImage()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(PlayerPrefs.GetString("userDirectory"));
        yield return www.SendWebRequest();
        if (!(www.result == UnityWebRequest.Result.ConnectionError) && !(www.result == UnityWebRequest.Result.ProtocolError))
        {
            var texture = DownloadHandlerTexture.GetContent(www);
            Material material = plane.GetComponent<MeshRenderer>().material;
            material.mainTexture = texture;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        string save = PlayerPrefs.GetString("GameSave");
        discPrefabs[Player.Black] = discBlackUp;
        discPrefabs[Player.White] = discWhiteUp;
        if (save != null && save.Length == 67)
        {
            gameState = new GameState(cpuSettings.GetBoard(), cpuSettings.GetCurrentPlayer());
            AddStartDiscs();
            ShowLegalMoves();
            uiManager.SetPlayerText(gameState.CurrentPlayer);
        }
        else
        {
            AddStartDiscs();
            if (cpuSettings.GetPlayer() == Player.Black)
            {
                StartCoroutine(FirstBotMove());
            }
            else
            {
                ShowLegalMoves();
            }

            uiManager.SetPlayerText(gameState.CurrentPlayer);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameState.GameOver && !cpuThinkingOverlay.activeSelf)
            {
                uiManager.TogglePauseOverlay();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    Vector3 impact = hitInfo.point;
                    Position boardPos = SceneToBoardPos(impact);
                    OnBoardClicked(boardPos);
                }
            }
        }
    }

    private void ShowLegalMoves()
    {
        foreach (Position boardPos in gameState.LegalMoves.Keys)
        {
            Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.01f;
            GameObject highlight = Instantiate(highlightPrefab, scenePos, Quaternion.identity);
            highlights.Add(highlight);
        }
    }

    private void HideLegalMoves()
    {
        highlights.ForEach(Destroy);
        highlights.Clear();
    }

    private void OnBoardClicked(Position boardPos)
    {
        if (gameState.MakeMove(boardPos, out MoveInfo moveInfo))
        {
            StartCoroutine(OnMoveMade(moveInfo));
        }
    }

    private IEnumerator FirstBotMove()
    {
        yield return new WaitForSeconds(0.5f);
        yield return BotPlayMove(new Position(5, 4));
        ShowLegalMoves();
    }

    private IEnumerator BotPlayMove(Position bestMove)
    {
        if (bestMove.Row != -1)
        {
            gameState.MakeMove(bestMove, out MoveInfo botMoveInfo);
            yield return ShowMove(botMoveInfo);
            yield return ShowTurnOutcome(botMoveInfo);
        }
    }


    private IEnumerator OnMoveMade(MoveInfo moveInfo)
    {
        if (gameState.RedoHistory.Count == 0)
        {
            uiManager.SetRedoInteractable(false);
        }

        if (cpuSettings.GetDepth() == 0)
        {
            StartCoroutine(uiManager.SetUndoRedoInactive(placeAnimationTime + flipAnimationTime));
            HideLegalMoves();
            yield return ShowMove(moveInfo);
            yield return ShowTurnOutcome(moveInfo);
            uiManager.SetUndoInteractable(true);
            ShowLegalMoves();
        }
        else
        {
            //uiManager.SetUndoInteractable(false);
            //uiManager.SetRedoInteractable(false);
            HideLegalMoves();
            yield return ShowMove(moveInfo);
            yield return ShowTurnOutcome(moveInfo);

            if (gameState.CurrentPlayer == cpuSettings.GetPlayer())
            {
                botTurn = true;
            }

            while (botTurn)
            {
                cpuThinkingOverlay.SetActive(true);
                
                botTurn = false;
                yield return null;
                yield return botLogic.Solve(gameState, moveInfo.Player.Opponent(), cpuSettings.GetDepth());
                yield return new WaitForSeconds(0.5f);
                cpuThinkingOverlay.SetActive(false);
                Position bestMove = botLogic.BestMove;
                Debug.Log("Best move is" + bestMove.Row + "and" + bestMove.Col);
                yield return BotPlayMove(bestMove);
            }

            //uiManager.SetUndoInteractable(true);
            //if (gameState.RedoHistory.Count > 0)
            //{
            //    uiManager.SetRedoInteractable(true);
            //}
            ShowLegalMoves();
        }
    }

    private Position SceneToBoardPos(Vector3 scenePos)
    {
        int col = (int)(scenePos.x - 0.25f);
        int row = 7 - (int)(scenePos.z - 0.25f);
        return new Position(row, col);
    }

    private Vector3 BoardToScenePos(Position boardPos)
    {
        return new Vector3(boardPos.Col + 0.75f, 0, 7 - boardPos.Row + 0.75f);
    }

    private void SpawnDisc(Disc prefab, Position boardPos)
    {
        Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.1f;
        discs[boardPos.Row, boardPos.Col] = Instantiate(prefab, scenePos, Quaternion.identity);
    }

    private void AddStartDiscs()
    {
        foreach (Position boardPos in gameState.OccupiedPositions())
        {
            Player player = gameState.Board[boardPos.Row, boardPos.Col];
            SpawnDisc(discPrefabs[player], boardPos);
        }
    }

    private void FlipDiscs(List<Position> positions)
    {
        foreach (Position boardPos in positions)
        {
            discs[boardPos.Row, boardPos.Col].Flip();
        }
    }

    private IEnumerator ShowMove(MoveInfo moveInfo)
    {
        if (PlayerPrefs.GetInt("Sound") == 1)
        {
            placeDiscSound.Play();
        }
        SpawnDisc(discPrefabs[moveInfo.Player], moveInfo.Position);
        yield return new WaitForSeconds(placeAnimationTime);
        FlipDiscs(moveInfo.Outflanked);
        yield return new WaitForSeconds(flipAnimationTime);
    }

    private IEnumerator ShowTurnSkipped(Player skippedPlayer)
    {
        uiManager.SetSkippedText(skippedPlayer);
        yield return uiManager.AnimateTopText();
    }

    private IEnumerator ShowGameOver(Player winner)
    {
        uiManager.SetTopText("Neither Player Can Move");

        yield return uiManager.AnimateTopText();

        yield return uiManager.ShowScoreText();
        yield return new WaitForSeconds(0.5f);

        yield return ShowCounting();

        uiManager.SetWinnerText(winner);
        yield return uiManager.ShowEndScreen();
    }

    private IEnumerator ShowTurnOutcome(MoveInfo moveInfo)
    {
        if (gameState.GameOver)
        {
            PlayerPrefs.SetString("GameSave", null);
            if (cpuSettings.GetPlayer() == Player.None)
            {
                uiManager.ToggleShowUndoRedo();
            }
            yield return ShowGameOver(gameState.Winner);
            yield break;
        }

        Player currentPlayer = gameState.CurrentPlayer;

        if (currentPlayer == moveInfo.Player)
        {
            if (currentPlayer == cpuSettings.GetPlayer())
            {
                botTurn = true;
            }
            yield return ShowTurnSkipped(currentPlayer.Opponent());
        }

        uiManager.SetPlayerText(currentPlayer);
    }

    private IEnumerator ShowCounting()
    {
        int black = 0, white = 0;

        foreach (Position pos in gameState.OccupiedPositions())
        {
            Player player = gameState.Board[pos.Row, pos.Col];

            if (player == Player.Black)
            {
                black++;
                uiManager.SetBlackScoreText(black);
            }
            else
            {
                white++;
                uiManager.SetWhiteScoreText(white);
            }

            discs[pos.Row, pos.Col].Twitch();
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator RestartGame()
    {
        yield return uiManager.HideEndScreen();
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void OnPlayAgainClicked()
    {
        StartCoroutine(RestartGame());
    }

    public void OnUndoClicked()
    {
        if (gameState.UndoMove())
        {
            if (gameState.MoveHistory.Count == 0)
            {
                uiManager.SetUndoInteractable(false);
            }
            StartCoroutine(ShowUndo());
        }
    }

    public IEnumerator ShowUndo()
    {
        StartCoroutine(uiManager.SetUndoRedoInactive(flipAnimationTime));
        MoveInfo moveInfo = gameState.RedoHistory.Peek();
        List<Position> outflanked = moveInfo.Outflanked;
        Position pos = moveInfo.Position;

        HideLegalMoves();
        Destroy(discs[pos.Row, pos.Col].gameObject);
        FlipDiscs(outflanked);
        yield return new WaitForSeconds(flipAnimationTime);
        uiManager.SetPlayerText(moveInfo.Player);
        ShowLegalMoves();
        uiManager.SetRedoInteractable(true);
    }

    public void OnRedoClicked()
    {
        if (gameState.RedoMove())
        {
            MoveInfo moveInfo = gameState.MoveHistory.Peek();
            StartCoroutine(OnMoveMade(moveInfo));
        }
    }

    public void SaveGame()
    {
        string save = null;
        if (!gameState.GameOver)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    switch (gameState.Board[i, j])
                    {
                        case Player.Black:
                            save += "b";
                            break;
                        case Player.White:
                            save += "w";
                            break;
                        case Player.None:
                            save += "n";
                            break;
                    }
                }
            }
        }
        if (gameState.CurrentPlayer == Player.White)
        {
            save += "w";
        }
        else
        {
            save += "b";
        }

        save += cpuSettings.GetDepth().ToString();
        switch (cpuSettings.GetPlayer())
        {
            case Player.Black:
                save += "b";
                break;
            case Player.White:
                save += "w";
                break;
            case Player.None:
                save += "n";
                break;
        }

        PlayerPrefs.SetString("GameSave", save);
    }
}