using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Oculus.Platform;
using SimpleFirebaseUnity;
using Oculus.Platform.Models;

public class GameManager : MonoBehaviour {
    #region Editor variables
    public GameObject Avatar;
    public PauseHandler pauseHandler;
    public BoardInterfaceManager boardInterfaceManager;
    public bool IsSideQuestBuild;
    public float AIWaitTime;
    #endregion

    #region Public variables
    public SceneChessBoard SceneBoard;
    public ChessBoard LogicBoard;
    public OpponentType opponentType;
    public ChessPiece.EColour CanMove;
    public int Turn;
    #endregion

    #region Internal State
    private SceneChessPiece SelectedPiece;
    ChessPiece.EColour Player;
    ChessPiece.EColour Opponent;
    AIManager.MoveType AIMoveType;
    string playerId;
    static string RIFT_APP_ID = "4000641133331067";
    static string QUEST_APP_ID = "3416125671847353";
    string APP_ID;
    bool askingForCode;
    TouchScreenKeyboard keyboard;
    int code;
    FirebaseObserver movesObserver;
    FirebaseObserver readyObserver;
    Firebase firebase;
    AudioSource audioSource;
    TimeSpan waitForAI;
    #endregion

    #region Unity Lifecycle
    void Start() {
        Debug.Log("Start!");
        SetAppID();
        audioSource = GetComponent<AudioSource>();
        EventManager.MoveComplete += () => { TurnComplete(); };
        playerId = SystemInfo.deviceUniqueIdentifier;
        SceneBoard.scale = 1;
        SceneBoard.GameManager = this;
        waitForAI = TimeSpan.FromSeconds(AIWaitTime);

        if (!IsSideQuestBuild) OculusSetup();
    }

    void LateUpdate() {
        if (askingForCode) { CodeInput(); }
    }
    #endregion

    #region Public
    public void StartAIGame() {
        opponentType = OpponentType.AI;
        Player = ChessPiece.EColour.White;
        Opponent = ChessPiece.EColour.Black;
        CanMove = Player;
        StartGame();
    }

    public void StopFirebase() {
        code = 0;
        if (readyObserver != null) readyObserver.Stop();
        if (movesObserver != null) movesObserver.Stop();

        readyObserver = null;
        movesObserver = null;
    }

    public void CreateMultiplayerGame() {
        InitFirebase();
        // avoid duplicating
        if (code > 0) return;

        code = UnityEngine.Random.Range(1000, 10000);
        boardInterfaceManager.SetText($"Give your friend the code {code}");

        var child = firebase.Child($"games/{code}");
        child.SetValue("{\"playerID\": \"" + playerId + "\", \"ready\": false }", true);
        readyObserver = new FirebaseObserver(child, 1f);
        readyObserver.OnChange += OnChangeHandler;
        readyObserver.Start();
    }

    public void JoinMultiplayerGame() {
        InitFirebase();
        AskForCode();
    }

    public bool MyTurn(SceneChessPiece piece) {
        return piece.Piece.Colour == CanMove;
    }

    public void Reset() {
        Avatar.SetActive(false);
        SceneBoard.DestroyBoard();
    }
    #endregion

    #region 
    private void StartMultiplayerGame() {
        boardInterfaceManager.PlayingMultiplayerGame();
        opponentType = OpponentType.Human;
        CanMove = ChessPiece.EColour.White;
        StartGame();
        StartListeningForMoves();
    }
    private void StartGame() {
        Avatar.SetActive(true);
        Turn = 0;

        if (Player == ChessPiece.EColour.White) {
            Debug.Log("player is white, setting pivot");
            var whitePivot = Quaternion.Euler(0f, 180f, 0f);
            SceneBoard.Pivot.localRotation = whitePivot;
        }

        LogicBoard = new ChessBoard();
        SceneBoard.InitializeBoard(LogicBoard);

        // TODO: Use player name
        var moveText = Player == CanMove ? "Your move" : "Waiting for your opponent";
        boardInterfaceManager.SetText(moveText);

        // Because PoolManager re-uses game objects it's important to go through each piece and make sure
        // it's been configured correctly.
        foreach (var p in SceneBoard.Pieces) {
            if (!p) continue;
            var piece = p.GetComponent<SceneChessPiece>();
            if (piece.Piece.Colour == Opponent) {
                var grabbable = p.GetComponent<OVRGrabbable>();
                if (grabbable) {
                    grabbable.enabled = false;
                } else {
                    Debug.Log($"No Grabbable found on {piece}");
                }
                piece.HumanControlled = false;
            } else {
                var grabbable = p.GetComponent<OVRGrabbable>();
                if (grabbable) {
                    grabbable.enabled = true;
                } else {
                    Debug.Log($"No Grabbable found on {piece}");
                }
                piece.HumanControlled = true;
            }
        }
    }
    private void SetAppID() {
        if (UnityEngine.Application.platform == RuntimePlatform.Android) {
            APP_ID = QUEST_APP_ID;
        } else {
            APP_ID = RIFT_APP_ID;
        }
    }

    private void OnGetUser(Message<User> message) {
        if (message.IsError) {
            Debug.LogError($"Error getting user: {message.GetError()}");
        } else {
            User user = message.Data;
            Debug.Log($"User is {user}");
        }
    }
    #endregion

    private void AskForCode() {
        askingForCode = true;
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad);
        pauseHandler.keyboardShowing = true;
    }


    private void OnChangeHandler(Firebase firebase, DataSnapshot snapshot) {
        try {
            var dict = (Dictionary<string, object>)snapshot.RawValue;
            var ready = (bool)dict["ready"];
            if (ready) {
                readyObserver.OnChange -= OnChangeHandler;
                readyObserver.Stop();
                readyObserver = null;
                Player = ChessPiece.EColour.White;
                Opponent = ChessPiece.EColour.Black;
                StartMultiplayerGame();
            }
        } catch {
            Debug.LogError("bad object in change handler");
        }
    }

    void InitFirebase() {
        firebase = Firebase.CreateNew("https://chess-together-default-rtdb.firebaseio.com/");
    }

    private void StartListeningForMoves() {
        var child = firebase.Child($"games/{code}/moves");
        movesObserver = new FirebaseObserver(child, 1f);
        movesObserver.OnChange += OnMoveReceived;
        movesObserver.Start();
    }

    private void OnMoveReceived(Firebase firebase, DataSnapshot snapshot) {
        try {
            var moves = (Dictionary<string, object>)snapshot.RawValue;
            foreach (var entry in moves) {
                try {
                    var move = new Move(entry.Value);
                    Debug.Log($"Received move: {move}");

                    if (move.Sequence < Turn) {
                        Debug.Log($"Turn is at sequence {move.Sequence} but we are on turn {Turn} - ignoring");
                        continue;
                    }

                    if (move.Player == playerId) {
                        Debug.Log($"Move player id is {move.Player} and our playerid is {playerId}- ignoring");
                        continue;

                    }

                    var valid = SceneBoard.Move(move);
                    return;
                } catch (Exception e) {
                    Debug.LogError($"Error parsing move: {e}");
                }
            }

        } catch (Exception e) {
            Debug.LogError($"Error receiving child: {e}");
        }
    }

    void CodeInput() {
        var keyboardText = keyboard.text;
        if (keyboardText == "") return;

        var currentCode = Int32.Parse(keyboardText);
        if (currentCode == code) return;

        code = currentCode;
        Debug.Log($"code is now {code}");
        boardInterfaceManager.SetText($"Code: {code}");

        if (code <= 999) return;

        askingForCode = false;
        keyboard.active = false;
        keyboard = null;
        pauseHandler.keyboardShowing = false;
        JoinGame();
    }

    private void JoinGame() {
        boardInterfaceManager.SetText($"Finding game {code}..");
        firebase.OnGetSuccess += OnGetSuccess;
        firebase.GetValue($"games/{code}");
    }

    private void OnGetSuccess(Firebase firebase, DataSnapshot snapshot) {
        Debug.Log($"get: {snapshot}");
        if (snapshot == null) {
            boardInterfaceManager.SetText($"No game with code {code} found.");
        } else {
            var child = firebase.Child($"games/{code}");
            child.SetValue("{\"otherPlayerID\": \"" + playerId + "\", \"ready\": true }", true);
            firebase.OnGetSuccess -= OnGetSuccess;
            Player = ChessPiece.EColour.Black;
            Opponent = ChessPiece.EColour.White;
            StartMultiplayerGame();
        }
    }

    void TurnComplete() {
        Debug.Log($"Turn complete");
        if (opponentType == OpponentType.Human && CanMove == Player) {
            PushMoveToFirebase();
        }

        Turn++;

        ToggleCanMove();
        SelectedPiece?.ToggleSelected(false);
        var state = LogicBoard.State[CanMove];

        if (state == ChessBoard.BoardStatus.Check) {
            boardInterfaceManager.SetText($"{CanMove} is in check");
        } else if (state == ChessBoard.BoardStatus.Checkmate || state == ChessBoard.BoardStatus.Stalemate) {
            EndGame(state);
            return;
        } else {
            boardInterfaceManager.SetText($"{CanMove}'s turn.");
        }

        SelectedPiece = null;

        if (CanMove == Opponent && opponentType == OpponentType.AI) AITurn();
    }

    private void PushMoveToFirebase() {
        var move = LogicBoard.GetLastMove();
        move.Player = playerId;
        move.Sequence = Turn;
        firebase.Child($"games/{code}/moves").Push(move.ToDictionary());
    }

    async void AITurn() {
        await Task.Delay(waitForAI);

        // Use our "AI" to pick a move.
        var move = await Task.Run<Move>(() => {
            return AIManager.GetMove(LogicBoard, Opponent, AIMoveType);
        });

        Debug.Log($"[AI] AI has decided to move {move}");
        SceneBoard.Move(move);

        AIMoveType = AIManager.MoveType.Standard;
    }


    void EndGame(ChessBoard.BoardStatus state) {
        string message;
        if (state == ChessBoard.BoardStatus.Checkmate) {
            var winner = CanMove.Inverse();
            if (winner == Player) message = "Checkmate! You win!";
            else message = "Checkmate! Your opponent won.";
        } else {
            message = "Stalemate! It's a draw.";
        }

        boardInterfaceManager.SetText(message);

        // No need to do anything further. The game (shouldn't) be playable.
    }

    private string PlayerForColour(ChessPiece.EColour colour) {
        if (colour == Player) return "You";
        if (opponentType == OpponentType.AI) return "AI";
        return "Your opponent";
    }

    void ToggleCanMove() {
        if (CanMove == ChessPiece.EColour.Black) {
            CanMove = ChessPiece.EColour.White;
        } else {
            CanMove = ChessPiece.EColour.Black;
        }

    }

    #region Oculus
    private void OculusSetup() {
        try {
            Core.Initialize();
            Oculus.Platform.Core.AsyncInitialize(APP_ID);
            Oculus.Platform.Entitlements.IsUserEntitledToApplication().OnComplete(entitlementCheck);
        } catch (Exception e) {
            Debug.LogError($"Error checking entitlement: {e}");
        }
    }

    private void entitlementCheck(Message message) {
        if (message.IsError) {
            UnityEngine.Application.Quit();
        }
    }
    #endregion


    public enum OpponentType {
        Human,
        AI
    }

}