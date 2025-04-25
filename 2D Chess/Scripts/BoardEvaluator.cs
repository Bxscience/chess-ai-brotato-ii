using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardEvaluator : MonoBehaviour
{
    public static readonly Dictionary<string, int> PieceValues = new Dictionary<string, int>
    {
        { "Pawn", 1 },
        { "Knight", 3 },
        { "Bishop", 3 },
        { "Rook", 5 },
        { "Queen", 9 },
        { "King", 2000000 }
    };

    public static int EvaluateBoard(GameObject whitePieces, GameObject blackPieces)
    {
        int whiteScore = CalculateMaterialScore(whitePieces);
        int blackScore = CalculateMaterialScore(blackPieces);

        return blackScore - whiteScore;
    }

    private static int CalculateMaterialScore(GameObject pieces)
    {
        int totalScore = 0;

        foreach (Transform piece in pieces.transform)
        {
            string pieceName = piece.name;
            foreach (var pieceType in PieceValues.Keys)
            {
                if (pieceName.Contains(pieceType))
                {
                    totalScore += PieceValues[pieceType];
                    break;
                }
            }
        }

        return totalScore;
    }
    public static int Evaluate(string boardState)
    {
        int score = 0;
        foreach (char piece in boardState)
        {
            if (PieceValues.ContainsKey(piece.ToString()))
            {
                score += PieceValues[piece.ToString()];
            }
        }
        return score;
    }

    public static int EvaluateMove(GameObject piece, Vector3 targetPosition, GameObject whitePieces, GameObject blackPieces)
    {
        Vector3 originalPosition = piece.transform.position;

        PieceController pieceController = piece.GetComponent<PieceController>();
        GameObject capturedPiece = null;

        // Find if there's a piece at the target position
        if (piece.tag == "White")
        {
            foreach (Transform blackPiece in blackPieces.transform)
            {
                if (blackPiece.position.x == targetPosition.x && blackPiece.position.y == targetPosition.y)
                {
                    capturedPiece = blackPiece.gameObject;
                    break;
                }
            }
        }
        else // Black piece
        {
            foreach (Transform whitePiece in whitePieces.transform)
            {
                if (whitePiece.position.x == targetPosition.x && whitePiece.position.y == targetPosition.y)
                {
                    capturedPiece = whitePiece.gameObject;
                    break;
                }
            }
        }

        bool wasCapturedPieceActive = false;
        if (capturedPiece != null)
        {
            wasCapturedPieceActive = capturedPiece.activeSelf;
            capturedPiece.SetActive(false);
        }

        piece.transform.position = targetPosition;

        int evaluation = EvaluateBoard(whitePieces, blackPieces);

        piece.transform.position = originalPosition;

        if (capturedPiece != null && wasCapturedPieceActive)
        {
            capturedPiece.SetActive(true);
        }

        return evaluation;
    }
}