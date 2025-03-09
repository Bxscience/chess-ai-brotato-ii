using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CapturedPieceIcon : MonoBehaviour
{
    public Image IconImage;
    public TextMeshProUGUI FallbackText;

    public void Initialize(string pieceType, string color)
    {
        string spritePath = $"Sprites/Chess/{color}{pieceType}";
        Sprite pieceSprite = Resources.Load<Sprite>(spritePath);

        if (pieceSprite != null && IconImage != null)
        {
            IconImage.sprite = pieceSprite;
            IconImage.gameObject.SetActive(true);

            if (FallbackText != null)
                FallbackText.gameObject.SetActive(false);
        }
        else
        {
            if (IconImage != null)
                IconImage.gameObject.SetActive(false);

            if (FallbackText != null)
            {
                FallbackText.gameObject.SetActive(true);
                FallbackText.text = GetPieceSymbol(pieceType);

                if (color == "White")
                    FallbackText.color = Color.white;
                else
                    FallbackText.color = Color.black;
            }
        }
    }

    string GetPieceSymbol(string pieceType)
    {
        switch (pieceType)
        {
            case "Pawn": return "♙";
            case "Knight": return "♘";
            case "Bishop": return "♗";
            case "Rook": return "♖";
            case "Queen": return "♕";
            case "King": return "♔";
            default: return "?";
        }
    }
}