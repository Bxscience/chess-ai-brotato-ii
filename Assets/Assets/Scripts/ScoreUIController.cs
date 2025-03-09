using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreUIController : MonoBehaviour
{
    public GameController GameController;
    public GameObject WhitePieces;
    public GameObject BlackPieces;

    public TextMeshProUGUI WhiteScoreText;
    public TextMeshProUGUI BlackScoreText;
    public TextMeshProUGUI LeadText;
    public Transform WhiteCapturedPiecesContainer;
    public Transform BlackCapturedPiecesContainer;

    public GameObject CapturedPiecePrefab;

    private List<GameObject> whiteCapturedPieces = new List<GameObject>();
    private List<GameObject> blackCapturedPieces = new List<GameObject>();

    private Dictionary<string, int> originalWhitePieceCount = new Dictionary<string, int>();
    private Dictionary<string, int> originalBlackPieceCount = new Dictionary<string, int>();

    void Start()
    {
        if (GameController == null) GameController = FindObjectOfType<GameController>();
        if (WhitePieces == null) WhitePieces = GameObject.Find("White Pieces");
        if (BlackPieces == null) BlackPieces = GameObject.Find("Black Pieces");

        CountOriginalPieces();

        UpdateScoreUI();
    }

    void Update()
    {
        UpdateScoreUI();
    }

    void CountOriginalPieces()
    {
        // Count white pieces
        originalWhitePieceCount.Clear();
        foreach (Transform piece in WhitePieces.transform)
        {
            string pieceType = GetPieceType(piece.name);
            if (originalWhitePieceCount.ContainsKey(pieceType))
                originalWhitePieceCount[pieceType]++;
            else
                originalWhitePieceCount[pieceType] = 1;
        }

        // Count black pieces
        originalBlackPieceCount.Clear();
        foreach (Transform piece in BlackPieces.transform)
        {
            string pieceType = GetPieceType(piece.name);
            if (originalBlackPieceCount.ContainsKey(pieceType))
                originalBlackPieceCount[pieceType]++;
            else
                originalBlackPieceCount[pieceType] = 1;
        }
    }
    void UpdateScoreUI()
    {
        // Calculate current piece values
        int whiteScore = CalculateScore(WhitePieces);
        int blackScore = CalculateScore(BlackPieces);

        // Update score texts
        WhiteScoreText.text = $"White: {whiteScore}";
        BlackScoreText.text = $"Black: {blackScore}";

        // Update lead text
        if (whiteScore > blackScore)
        {
            LeadText.text = $"White leads by {whiteScore - blackScore}";
            LeadText.color = Color.green;
        }
        else if (blackScore > whiteScore)
        {
            LeadText.text = $"Black leads by {blackScore - whiteScore}";
            LeadText.color = Color.green;
        }
        else
        {
            LeadText.text = "Even";
            LeadText.color = Color.yellow;
        }

        UpdateCapturedPiecesDisplay();
    }

    int CalculateScore(GameObject pieces)
    {
        int totalScore = 0;

        foreach (Transform piece in pieces.transform)
        {
            string pieceType = GetPieceType(piece.name);
            totalScore += GetPieceValue(pieceType);
        }

        return totalScore;
    }
    
    void UpdateCapturedPiecesDisplay()
    {
        // Create dictionaries for current piece counts
        Dictionary<string, int> currentWhitePieceCount = new Dictionary<string, int>();
        Dictionary<string, int> currentBlackPieceCount = new Dictionary<string, int>();

        // Count current pieces
        foreach (Transform piece in WhitePieces.transform)
        {
            string pieceType = GetPieceType(piece.name);
            if (currentWhitePieceCount.ContainsKey(pieceType))
                currentWhitePieceCount[pieceType]++;
            else
                currentWhitePieceCount[pieceType] = 1;
        }

        foreach (Transform piece in BlackPieces.transform)
        {
            string pieceType = GetPieceType(piece.name);
            if (currentBlackPieceCount.ContainsKey(pieceType))
                currentBlackPieceCount[pieceType]++;
            else
                currentBlackPieceCount[pieceType] = 1;
        }

        // Clear existing captured piece displays
        foreach (Transform child in WhiteCapturedPiecesContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in BlackCapturedPiecesContainer)
        {
            Destroy(child.gameObject);
        }

        // Display captured white pieces (pieces black has taken)
        foreach (var pieceType in originalWhitePieceCount.Keys)
        {
            int originalCount = originalWhitePieceCount[pieceType];
            int currentCount = currentWhitePieceCount.ContainsKey(pieceType) ? currentWhitePieceCount[pieceType] : 0;
            int capturedCount = originalCount - currentCount;

            for (int i = 0; i < capturedCount; i++)
            {
                CreateCapturedPieceIcon(pieceType, "White", BlackCapturedPiecesContainer);
            }
        }

        // Display captured black pieces (pieces white has taken)
        foreach (var pieceType in originalBlackPieceCount.Keys)
        {
            int originalCount = originalBlackPieceCount[pieceType];
            int currentCount = currentBlackPieceCount.ContainsKey(pieceType) ? currentBlackPieceCount[pieceType] : 0;
            int capturedCount = originalCount - currentCount;

            for (int i = 0; i < capturedCount; i++)
            {
                CreateCapturedPieceIcon(pieceType, "Black", WhiteCapturedPiecesContainer);
            }
        }
    }
    void CreateCapturedPieceIcon(string pieceType, string color, Transform container)
    {
        if (CapturedPiecePrefab != null)
        {
            GameObject icon = Instantiate(CapturedPiecePrefab, container);
            Image iconImage = icon.GetComponent<Image>();

            string spritePath = $"Sprites/Chess/{color}{pieceType}";
            Sprite pieceSprite = Resources.Load<Sprite>(spritePath);

            if (pieceSprite != null)
            {
                iconImage.sprite = pieceSprite;
            }
            else
            {
                // If no sprite is found, use text as fallback
                TextMeshProUGUI iconText = icon.GetComponentInChildren<TextMeshProUGUI>();
                if (iconText != null)
                {
                    iconText.text = GetPieceSymbol(pieceType);
                }
            }
        }
    }

    string GetPieceType(string pieceName)
    {
        if (pieceName.Contains("Pawn")) return "Pawn";
        if (pieceName.Contains("Knight")) return "Knight";
        if (pieceName.Contains("Bishop")) return "Bishop";
        if (pieceName.Contains("Rook")) return "Rook";
        if (pieceName.Contains("Queen")) return "Queen";
        if (pieceName.Contains("King")) return "King";
        return "Unknown";
    }

    int GetPieceValue(string pieceType)
    {
        switch (pieceType)
        {
            case "Pawn": return 1;
            case "Knight": return 3;
            case "Bishop": return 3;
            case "Rook": return 5;
            case "Queen": return 9;
            case "King": return 0; 
            default: return 0;
        }
    }

    string GetPieceSymbol(string pieceType)
    {
        switch (pieceType)
        {
            case "Pawn": return "P";
            case "Knight": return "N";
            case "Bishop": return "B";
            case "Rook": return "R";
            case "Queen": return "Q";
            case "King": return "K";
            default: return "?";
        }
    }
}