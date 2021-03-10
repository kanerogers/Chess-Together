using UnityEngine;
using System.Collections;

public class Square : MonoBehaviour {
    Color MyColor;
    Color Transparent;
    Color currentColor;
    Color Valid = new Color(0f, 1f, 0f, 1f);
    Color Invalid = new Color(1f, 0f, 0f, 1f);
    string ColorString = "_BaseColor";
    public int Row;
    public int Column;
    Renderer m_renderer;
    WaitForSeconds flashWait = new WaitForSeconds(0.05f);
    static int flashes = 3;
    void Start() {
        m_renderer = GetComponent<Renderer>();
        MyColor = m_renderer.material.GetColor(ColorString);
        Transparent = new Color(MyColor.r, MyColor.g, MyColor.b, 0f);
        m_renderer.material.SetColor(ColorString, Transparent);
    }

    void OnSquareHovered(int row, int column) {
        if (row == Row && column == Column) {
            ToggleHighlight(true);
        }

    }
    void OnSquareDehovered(int row, int column) {
        if (row == Row && column == Column) {
            ToggleHighlight(false);
        }

    }

    void OnSquareSelected(int row, int column) {
        if (row == Row && column == Column) {
            ToggleHighlight(true);
        }

    }
    void OnSquareDeselected(int row, int column) {
        if (row == Row && column == Column) {
            ToggleHighlight(false);
        }
    }

    public void ToggleHighlight(bool on, ChessPiece piece = null) {
        if (on && piece != null) {
            if (piece.Row == Row && piece.Column == Column) {
                currentColor = MyColor;
            } else {
                if (ChessBoard.Inst.IsValidMove(piece, Row, Column)) {
                    currentColor = Valid;
                } else {
                    currentColor = Invalid;
                }
            }
        } else {
            currentColor = Transparent;
        }

        m_renderer.material.SetColor(ColorString, currentColor);
    }

    public void FlashOff() {
        StartCoroutine(RunFlash());
    }

    IEnumerator RunFlash() {
        for (int i = 0; i < flashes; i++) {
            m_renderer.material.SetColor(ColorString, Transparent);
            yield return flashWait;
            m_renderer.material.SetColor(ColorString, currentColor);
            yield return flashWait;
        }

        m_renderer.material.SetColor(ColorString, Transparent);

    }
}