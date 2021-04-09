using System.Collections;
using UnityEngine;

public class SceneChessPiece : MonoBehaviour {
    [SerializeField]
    public AnimationCurve MoveCurve;
    [SerializeField]
    LayerMask SquareMask;

    public GameManager gameManager;

    bool Selected;
    public Color selectedEmissionColor = new Color(0f, 0.3235294f, 0.5f);
    public Color hoverEmissionColor = new Color(0f, 0.2437035f, 0.3804151f);
    public float MovementDuration = 0.5f;
    AudioSource audioSource;
    public ChessPiece Piece;
    public SceneChessBoard Board;
    Vector3 previousLocation;
    public bool HumanControlled = true;
    Square squareBeneath;
    public bool isGrabbed;
    float height;

    #region Unity Lifecycle Events

    void Start() {
        audioSource = GetComponent<AudioSource>();
        SquareMask = LayerMask.GetMask("Square");
        var renderer = GetComponent<Renderer>();
        height = renderer.bounds.size.y;
    }

    void FixedUpdate() {
        if (!isGrabbed) return;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, SquareMask)) {
            var square = hit.transform.GetComponent<Square>();
            if (!square) return;
            if (squareBeneath != square) {
                squareBeneath?.ToggleHighlight(false);
                squareBeneath = square;
                squareBeneath.ToggleHighlight(true, Piece);
            }
        } else {
            squareBeneath?.ToggleHighlight(false);
        }
    }

    #endregion

    #region VREvents
    public void Grabbed() {
        isGrabbed = true;
        previousLocation = transform.localPosition;
    }

    public void Released() {
        isGrabbed = false;

        if (!gameManager.MyTurn(this) || !squareBeneath) {
            RevertPosition();
            return;
        }

        var (toRow, toColumn) = (squareBeneath.Row, squareBeneath.Column);
        if (!Board.Move(this, toRow, toColumn)) {
            RevertPosition();
        }
    }
    #endregion

    void ToggleHover(bool hover) {
        if (Selected) return;

        if (Piece != null) {
            if (hover) EventManager.HoveredOverPiece(Piece.Row, Piece.Column);
            else EventManager.UnhoveredOverPiece(Piece.Row, Piece.Column);
        }

        Color color = hover ? hoverEmissionColor : Color.black;
        SetEmissionColor(color);
    }

    void SetEmissionColor(Color color) {

        Material material = GetComponent<Renderer>().material;
        if (color != Color.black) {
            material.EnableKeyword("_EMISSION");
        } else {
            material.DisableKeyword("_EMISSION");
        }

        GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
    }

    void OnSquareHovered(int row, int column) {
        if (Piece == null) return;
        if (row == Piece.Row && column == Piece.Column) {
            ToggleHover(true);
        }
    }
    void OnSquareUnhovered(int row, int column) {
        if (Piece == null) return;
        if (row == Piece.Row && column == Piece.Column) {
            ToggleHover(false);
        }
    }
    public void ToggleSelected(bool selected) {
        Selected = selected;
        if (selected) EventManager.SelectedPiece(Piece.Row, Piece.Column);
        else EventManager.DeselectedPiece(Piece.Row, Piece.Column);
        Color color = selected ? selectedEmissionColor : Color.black;
        SetEmissionColor(color);
    }

    public void Move(Vector3 endPosition, bool valid = true) {
        if (Piece.Name == ChessPiece.EName.Knight && !HumanControlled) {
            StartCoroutine(KnightMoveTo(endPosition, valid));
        } else {
            StartCoroutine(MoveTo(endPosition, valid));
        }
    }

    IEnumerator MoveTo(Vector3 endPosition, bool valid = true) {
        // Always set the height back to height so that it'll be back on the board.
        endPosition.y = 0;
        Vector3 startPosition = transform.localPosition;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(Vector3.zero);
        float timeElapsed = 0f;

        while (timeElapsed < MovementDuration) {
            float lerpPercent = MoveCurve.Evaluate(timeElapsed / MovementDuration);
            Vector3 position = Vector3.Lerp(startPosition, endPosition, lerpPercent);
            Quaternion rotation = Quaternion.Lerp(startRotation, endRotation, lerpPercent);

            transform.localPosition = position;
            transform.localRotation = rotation;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = endPosition;
        transform.localRotation = endRotation;

        MoveComplete(valid);
    }

    IEnumerator KnightMoveTo(Vector3 endPosition, bool valid = true) {
        endPosition.y = 0;

        var startPosition = transform.localPosition;
        var center = startPosition + (endPosition - startPosition) / 2;
        center += Vector3.up * (height * 2);

        float timeElapsed = 0f;

        while (timeElapsed < MovementDuration) {
            var fracComplete = timeElapsed / MovementDuration;
            Vector3 start = Vector3.Lerp(startPosition, center, MoveCurve.Evaluate(fracComplete / 1f));
            Vector3 end = Vector3.Lerp(center, endPosition, MoveCurve.Evaluate(fracComplete / 1f));
            transform.localPosition = Vector3.Lerp(start, end, MoveCurve.Evaluate(fracComplete / 1f));

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = endPosition;
        MoveComplete(valid);
    }

    void MoveComplete(bool valid) {
        audioSource?.PlayOneShot(audioSource.clip);
        squareBeneath?.FlashOff();

        if (valid) {
            EventManager.EndMove(Piece.Colour);
        }
        squareBeneath = null;
    }

    void RevertPosition() {
        Move(previousLocation, false);
    }

}
