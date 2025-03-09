using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public GameController GameController;
    public GameObject BlackPieces;
    public GameObject WhitePieces;
    public GameObject Board;
    public float MoveDelay = 1.0f; // Delay before AI makes a move
    public bool UseRandomMoves = true; // Set to false to use evaluation-based moves

    private bool aiTurnPending = false;
    private float moveTimer = 0f;

    // Use this for initialization
    void Start()
    {
        if (GameController == null) GameController = FindObjectOfType<GameController>();
        if (BlackPieces == null) BlackPieces = GameObject.Find("Black Pieces");
        if (WhitePieces == null) WhitePieces = GameObject.Find("White Pieces");
        if (Board == null) Board = GameObject.Find("Board");
    }

    // Update is called once per frame
    void Update()
    {
        // Check if it's AI's turn to move
        if (!GameController.WhiteTurn && !aiTurnPending && !IsPieceMoving())
        {
            aiTurnPending = true;
            moveTimer = MoveDelay;
        }

        // Wait for delay before making a move
        if (aiTurnPending)
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0)
            {
                if (UseRandomMoves)
                {
                    MakeRandomMove();
                }
                else
                {
                    MakeBestMove();
                }
                aiTurnPending = false;
            }
        }
    }

    // Check if any piece is currently moving
    bool IsPieceMoving()
    {
        foreach (Transform piece in BlackPieces.transform)
        {
            if (piece.GetComponent<PieceController>().IsMoving())
            {
                return true;
            }
        }
        foreach (Transform piece in WhitePieces.transform)
        {
            if (piece.GetComponent<PieceController>().IsMoving())
            {
                return true;
            }
        }
        return false;
    }

    // Make a move based on board evaluation
    void MakeBestMove()
    {
        List<MoveOption> validMoves = new List<MoveOption>();

        // Find all valid moves for all black pieces
        foreach (Transform piece in BlackPieces.transform)
        {
            PieceController pieceController = piece.GetComponent<PieceController>();
            GameObject encounteredEnemy;

            // Check every board square as a potential destination
            foreach (Transform square in Board.transform)
            {
                Vector3 targetPosition = new Vector3(square.position.x, square.position.y, piece.position.z);

                if (pieceController.ValidateMovement(piece.position, targetPosition, out encounteredEnemy))
                {
                    // Evaluate the position after this move
                    int evaluation = BoardEvaluator.EvaluateMove(piece.gameObject, targetPosition, WhitePieces, BlackPieces);
                    validMoves.Add(new MoveOption(piece.gameObject, targetPosition, evaluation));
                }
            }
        }

        // If there are valid moves, select the best one
        if (validMoves.Count > 0)
        {
            // Sort moves by evaluation (highest score first)
            validMoves.Sort((a, b) => b.Evaluation.CompareTo(a.Evaluation));

            // Select from top 3 moves randomly (to add some variety)
            int moveIndex = 0;
            if (validMoves.Count >= 3)
            {
                moveIndex = Random.Range(0, 3);
            }

            MoveOption selectedMove = validMoves[moveIndex];

            // Debug output to see evaluations
            Debug.Log($"AI chose move with evaluation: {selectedMove.Evaluation}");

            // Execute the move
            GameController.SelectPiece(selectedMove.Piece);
            selectedMove.Piece.GetComponent<PieceController>().MovePiece(selectedMove.TargetPosition);
        }
        else
        {
            Debug.Log("AI has no valid moves.");
        }
    }

    // Make a random valid move (keeping this for comparison)
    void MakeRandomMove()
    {
        List<MoveOption> validMoves = new List<MoveOption>();

        // Find all valid moves for all black pieces
        foreach (Transform piece in BlackPieces.transform)
        {
            PieceController pieceController = piece.GetComponent<PieceController>();
            GameObject encounteredEnemy;

            // Check every board square as a potential destination
            foreach (Transform square in Board.transform)
            {
                if (pieceController.ValidateMovement(piece.position, new Vector3(square.position.x, square.position.y, piece.position.z), out encounteredEnemy))
                {
                    validMoves.Add(new MoveOption(piece.gameObject, new Vector3(square.position.x, square.position.y, piece.position.z), 0));
                }
            }
        }

        // If there are valid moves, select one randomly
        if (validMoves.Count > 0)
        {
            MoveOption selectedMove = validMoves[Random.Range(0, validMoves.Count)];
            GameController.SelectPiece(selectedMove.Piece);
            selectedMove.Piece.GetComponent<PieceController>().MovePiece(selectedMove.TargetPosition);
        }
        else
        {
            Debug.Log("AI has no valid moves.");
        }
    }

    // Class to store move options with evaluation
    private class MoveOption
    {
        public GameObject Piece { get; private set; }
        public Vector3 TargetPosition { get; private set; }
        public int Evaluation { get; private set; }

        public MoveOption(GameObject piece, Vector3 targetPosition, int evaluation)
        {
            Piece = piece;
            TargetPosition = targetPosition;
            Evaluation = evaluation;
        }
    }
}   