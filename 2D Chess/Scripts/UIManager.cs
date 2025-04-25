using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject promotionPanel;
    public Button knightButton;
    public Button rookButton;
    public Button bishopButton;
    public Button queenButton;

    private Action<string> onPromotionSelected;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Hide promotion panel at start
        promotionPanel.SetActive(false);

        // Add listeners to buttons
        knightButton.onClick.AddListener(() => SelectPromotion("Knight"));
        rookButton.onClick.AddListener(() => SelectPromotion("Rook"));
        bishopButton.onClick.AddListener(() => SelectPromotion("Bishop"));
        queenButton.onClick.AddListener(() => SelectPromotion("Queen"));
    }

    public void ShowPromotionOptions(Action<string> callback)
    {
        // Show the promotion panel and store the callback
        promotionPanel.SetActive(true);
        onPromotionSelected = callback;
    }

    private void SelectPromotion(string chosenPiece)
    {
        // Hide the panel
        promotionPanel.SetActive(false);

        // Call the stored callback function with the chosen piece
        onPromotionSelected?.Invoke(chosenPiece);
    }
}
