using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class BoardInterfaceManager : MonoBehaviour {
    #region Editor Variables
    public Sprite AIIcon;
    public Sprite PersonIcon;
    public Sprite JoinIcon;
    public Sprite StartIcon;
    public Sprite BackIcon;
    public Sprite ForwardIcon;
    public Sprite ConfirmIcon;
    public SpriteRenderer BackButton;
    public SpriteRenderer LeftButton;
    public SpriteRenderer RightButton;
    public TextMeshProUGUI ChessBoardScreen;
    public float typingSpeed;
    public GameManager GameManager;
    #endregion

    #region Public
    public State currentState;
    #endregion

    #region Private
    static Dictionary<State, (Sprite, Sprite, Sprite)> icons;
    static Dictionary<State, string> screenText = new Dictionary<State, string> {
        { State.SplashScreen, "Chess Together" },
        { State.ChoosingGameType, "Who would you like to play with?" },
        { State.StartingMultiplayerGame, "Would you like to create or join a game?" },
        { State.JoiningMultiplayerGame, "Enter the code your friend gave you" },
        { State.ChoosingPieceToPromoteTo, "Promote this Pawn to a Queen?" },
    };
    string textToBeTyped;
    WaitForSeconds characterWaitTime;
    WaitForSeconds typingWaitTime = new WaitForSeconds(1);
    private float backbuttonLastPressedTime;
    private float leftButtonLastPressedTime;
    private float rightButtonLastPressedTime;
    private int promotionOptionsIndex = 0;
    private ChessPiece.EName[] PromotionOptions = new ChessPiece.EName[4] {
        ChessPiece.EName.Queen,
        ChessPiece.EName.Bishop,
        ChessPiece.EName.Rook,
        ChessPiece.EName.Knight,
    };
    private State stateBeforePromotion;
    #endregion

    #region Unity Lifecycle

    void OnEnable() {
        characterWaitTime = new WaitForSeconds(typingSpeed);
        icons = new Dictionary<State, (Sprite, Sprite, Sprite)> {
            { State.ChoosingGameType, (null, AIIcon, PersonIcon) },
            { State.StartingMultiplayerGame, (BackIcon, StartIcon, JoinIcon) },
            { State.ChoosingPieceToPromoteTo, (BackIcon, ForwardIcon, ConfirmIcon) }
        };
    }

    void Start() {
        StartCoroutine(SplashScreen());
    }

    #endregion

    #region Public Methods
    public void PlayingMultiplayerGame() {
        ChangeState(State.PlayingMultiplayerGame);
    }

    public void BackButtonPressed() {
        if (backbuttonLastPressedTime > 0 && Time.fixedTime - backbuttonLastPressedTime < 1) {
            Debug.Log($"Waiting a second for next button pressed");
            return;
        }
        backbuttonLastPressedTime = Time.fixedTime;

        State nextState;

        if (currentState == State.PlayingAIGame) {
            nextState = State.ChoosingGameType;
            GameManager.Reset();
        } else if (currentState == State.PlayingMultiplayerGame) {
            GameManager.StopFirebase();
            GameManager.Reset();
            nextState = State.StartingMultiplayerGame;
        } else if (currentState == State.StartingMultiplayerGame) {
            nextState = State.ChoosingGameType;
        } else if (currentState == State.CreatingMultiplayerGame || currentState == State.JoiningMultiplayerGame) {
            GameManager.StopFirebase();
            nextState = State.StartingMultiplayerGame;
        } else if (currentState == State.ChoosingPieceToPromoteTo) {
            DecrementPromotionIndex();
            return;
        } else {
            Debug.LogError($"Invalid state: {currentState}");
            return;
        }

        ChangeState(nextState);
    }

    public void LeftButtonPressed() {
        State nextState;
        if (leftButtonLastPressedTime > 0 && Time.fixedTime - leftButtonLastPressedTime < 1) {
            Debug.Log($"Waiting a second for next button pressed");
            return;
        }

        leftButtonLastPressedTime = Time.fixedTime;

        if (currentState == State.ChoosingGameType) {
            GameManager.StartAIGame();
            nextState = State.PlayingAIGame;
        } else if (currentState == State.StartingMultiplayerGame) {
            GameManager.CreateMultiplayerGame();
            nextState = State.CreatingMultiplayerGame;
        } else if (currentState == State.ChoosingPieceToPromoteTo) {
            IncrementPromotionIndex();
            return;
        } else {
            Debug.LogError($"Invalid state: {currentState}");
            return;
        }

        ChangeState(nextState);
    }

    public void RightButtonPressed() {
        State nextState;
        if (rightButtonLastPressedTime > 0 && Time.fixedTime - rightButtonLastPressedTime < 1) {
            Debug.Log($"Waiting a second for next button pressed");
            return;
        }

        rightButtonLastPressedTime = Time.fixedTime;

        if (currentState == State.ChoosingGameType) {
            nextState = State.StartingMultiplayerGame;
        } else if (currentState == State.StartingMultiplayerGame) {
            GameManager.JoinMultiplayerGame();
            nextState = State.JoiningMultiplayerGame;
        } else if (currentState == State.ChoosingPieceToPromoteTo) {
            PromotePiece();
            nextState = stateBeforePromotion;
        } else {
            Debug.LogError($"Invalid state: {currentState}");
            return;
        }

        ChangeState(nextState);
    }
    public void SetText(string text) {
        StartCoroutine(TypeWriterText(text));
    }

    public void AskForPieceToPromoteTo() {
        promotionOptionsIndex = 0;
        stateBeforePromotion = currentState;
        ChangeState(State.ChoosingPieceToPromoteTo);
    }

    #endregion

    #region Internal Methods
    private void ChangeState(State newState) {
        currentState = newState;
        UpdateState();
    }
    private void UpdateState() {
        SetIcons();
        SetScreenText();
    }

    private void SetIcons() {
        if (!icons.ContainsKey(currentState)) {
            BackButton.sprite = BackIcon;
            LeftButton.sprite = null;
            RightButton.sprite = null;
            return;
        }

        var (backButton, leftButton, rightButton) = icons[currentState];
        BackButton.sprite = backButton;
        LeftButton.sprite = leftButton;
        RightButton.sprite = rightButton;
    }
    private void DecrementPromotionIndex() {
        if (promotionOptionsIndex == 0) promotionOptionsIndex = 3;
        else promotionOptionsIndex--;

        SetScreenText();
    }
    private void IncrementPromotionIndex() {
        if (promotionOptionsIndex == 3) promotionOptionsIndex = 0;
        else promotionOptionsIndex++;

        SetScreenText();
    }
    private void PromotePiece() {
        var name = PromotionOptions[promotionOptionsIndex];
        var text = $"Promoting pawn to {name}..";
        var board = GetComponent<SceneChessBoard>();
        board.PromotePawnTo(name);
    }


    private void SetScreenText() {
        string text;
        if (currentState == State.ChoosingPieceToPromoteTo) {
            var name = PromotionOptions[promotionOptionsIndex];
            text = $"Promote this Pawn to a {name}?";
        } else {
            if (!screenText.ContainsKey(currentState)) return;

            text = screenText[currentState];
        }

        SetText(text);
    }
    IEnumerator TypeWriterText(string text) {
        textToBeTyped = text;
        for (int i = 1; i < text.Length; i++) {
            if (textToBeTyped != text) {
                Debug.Log($"Text to write was {text} but textToBeTyped is {textToBeTyped} - stopping.. ");
                yield break;
            };
            var s = text.Substring(0, i);
            ChessBoardScreen.SetText($"{s} |");
            yield return characterWaitTime;
        }

        ChessBoardScreen.SetText(text);
        yield return typingWaitTime;
    }

    IEnumerator SplashScreen() {
        yield return StartCoroutine(TypeWriterText("Chess Together"));
        ChangeState(State.ChoosingGameType);
    }
    #endregion

    public enum State {
        SplashScreen,
        ChoosingGameType,
        StartingMultiplayerGame,
        CreatingMultiplayerGame,
        JoiningMultiplayerGame,
        PlayingAIGame,
        PlayingMultiplayerGame,
        ChoosingPieceToPromoteTo,
    }
}