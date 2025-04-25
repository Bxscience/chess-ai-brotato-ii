using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public GameController GameController;
    public GameObject BlackPieces;
    public GameObject WhitePieces;
    public GameObject Board;
    public float MoveDelay = 1.0f;
    public bool UseRandomMoves = false;
    public int SearchDepth;

    private bool aiTurnPending = false;
    private float moveTimer;
    private int nodesEvaluated = 0;
    private bool isExecutingMove = false;
    private static readonly Dictionary<string, int> PieceValues = new Dictionary<string, int>
    {
        {"Pawn", 100}, {"Knight", 320}, {"Bishop", 330},
        {"Rook", 500}, {"Queen", 900}, {"King", 20000}, {"None", 0}
    };

    // Enhanced Piece-Square Tables (midgame)
    private static readonly int[] PawnTable = {
         0,   0,   0,   0,   0,   0,  0,   0,
     98, 134,  61,  95,  68, 126, 34, -11,
     -6,   7,  26,  31,  65,  56, 25, -20,
    -14,  13,   6,  21,  23,  12, 17, -23,
    -27,  -2,  -5,  12,  17,   6, 10, -25,
    -26,  -4,  -4, -10,   3,   3, 33, -12,
    -35,  -1, -20, -23, -15,  24, 38, -22,
      0,   0,   0,   0,   0,   0,  0,   0,
    };

    private static readonly int[] KnightTable = {
      -167, -89, -34, -49,  61, -97, -15, -107,
     -73, -41,  72,  36,  23,  62,   7,  -17,
     -47,  60,  37,  65,  84, 129,  73,   44,
      -9,  17,  19,  53,  37,  69,  18,   22,
     -13,   4,  16,  13,  28,  19,  21,   -8,
     -23,  -9,  12,  10,  19,  17,  25,  -16,
     -29, -53, -12,  -3,  -1,  18, -14,  -19, 
    -105, -21, -58, -33, -17, -28, -19,  -23,
    };

    private static readonly int[] BishopTable = {
       -29,   4, -82, -37, -25, -42,   7,  -8,
    -26,  16, -18, -13,  30,  59,  18, -47,
    -16,  37,  43,  40,  35,  50,  37,  -2,
     -4,   5,  19,  50,  37,  37,   7,  -2,
     -6,  13,  13,  26,  34,  12,  10,   4,
      0,  15,  15,  15,  14,  27,  18,  10,
      4,  15,  16,   0,   7,  21,  33,   1,
    -33,  -3, -14, -21, -13, -12, -39, -21,
    };

    private static readonly int[] RookTable = {
        32,  42,  32,  51, 63,  9,  31,  43,
     27,  32,  58,  62, 80, 67,  26,  44,
     -5,  19,  26,  36, 17, 45,  61,  16,
    -24, -11,   7,  26, 24, 35,  -8, -20,
    -36, -26, -12,  -1,  9, -7,   6, -23,
    -45, -25, -16, -17,  3,  0,  -5, -33,
    -44, -16, -20,  -9, -1, 11,  -6, -71,
    -19, -13,   1,  17, 16,  7, -37, -26,
    };

    private static readonly int[] QueenTable = {
        -28,   0,  29,  12,  59,  44,  43,  45,
    -24, -39,  -5,   1, -16,  57,  28,  54,
    -13, -17,   7,   8,  29,  56,  47,  57,
    -27, -27, -16, -16,  -1,  17,  -2,   1,
     -9, -26,  -9, -10,  -2,  -4,   3,  -3,
    -14,   2, -11,  -2,  -5,   2,  14,   5,
    -35,  -8,  11,   2,   8,  15,  -3,   1,
     -1, -18,  -9,  10, -15, -25, -31, -50,
    };

    private static readonly int[] KingTable = {
        -65,  23,  16, -15, -56, -34,   2,  13,
     29,  -1, -20,  -7,  -8,  -4, -38, -29,
     -9,  24,   2, -16, -20,   6,  22, -22,
    -17, -20, -12, -27, -30, -25, -14, -36,
    -49,  -1, -27, -39, -46, -44, -33, -51,
    -14, -14, -22, -46, -44, -30, -15, -27,
      1,   7,  -8, -64, -43, -16,   9,   8,
    -15,  36,  12, -54,   8, -28,  24,  14,
    };

    // Endgame piece-square tables
    private static readonly int[] PawnTableEndgame = {
        0, 0, 0, 0, 0, 0, 0, 0,
    178, 173, 158, 134, 147, 132, 165, 187,
    94, 100, 85, 67, 56, 53, 82, 84,
    32, 24, 13, 5, -2, 4, 17, 17,
    13, 9, -3, -7, -7, -8, 3, -1,
    4, 7, -6, 1, 0, -5, -1, -8,
    13, 8, 8, 10, 13, 0, 2, -7,
    0, 0, 0, 0, 0, 0, 0, 0
    };

    private static readonly int[] KingTableEndgame = {
       -74, -35, -18, -18, -11, 15, 4, -17,
    -12, 17, 14, 17, 17, 38, 23, 11,
    10, 17, 23, 15, 20, 45, 44, 13,
    -8, 22, 24, 27, 26, 33, 26, 3,
    -18, -4, 21, 24, 27, 23, 9, -11,
    -19, -3, 11, 21, 23, 16, 7, -9,
    -27, -11, 4, 13, 14, 4, -5, -17,
    -53, -34, -21, -11, -28, -14, -24, -43
    };

    private const int TempoBonus = 10;

    private static readonly Dictionary<string, int> MobilityWeights = new Dictionary<string, int>
    {
        {"Pawn", 0}, {"Knight", 4}, {"Bishop", 5},
        {"Rook", 2}, {"Queen", 1}, {"King", 0}
    };

    private static readonly Dictionary<string, Dictionary<string, int>> ThreatWeights = 
        new Dictionary<string, Dictionary<string, int>>
    {
        {"Pawn", new Dictionary<string, int> 
            {{"Pawn", 0}, {"Knight", 50}, {"Bishop", 50}, {"Rook", 50}, {"Queen", 50}, {"King", 0}}},
        {"Knight", new Dictionary<string, int> 
            {{"Pawn", 5}, {"Knight", 5}, {"Bishop", 5}, {"Rook", 10}, {"Queen", 10}, {"King", 5}}},
        {"Bishop", new Dictionary<string, int> 
            {{"Pawn", 5}, {"Knight", 5}, {"Bishop", 5}, {"Rook", 10}, {"Queen", 10}, {"King", 5}}},
        {"Rook", new Dictionary<string, int> 
            {{"Pawn", 10}, {"Knight", 10}, {"Bishop", 10}, {"Rook", 20}, {"Queen", 20}, {"King", 10}}},
        {"Queen", new Dictionary<string, int> 
            {{"Pawn", 20}, {"Knight", 20}, {"Bishop", 20}, {"Rook", 30}, {"Queen", 30}, {"King", 20}}},
        {"King", new Dictionary<string, int> 
            {{"Pawn", 0}, {"Knight", 0}, {"Bishop", 0}, {"Rook", 0}, {"Queen", 0}, {"King", 0}}}
    };

    void Start()
    {
        InitializeReferences();
    }

    void InitializeReferences()
    {
        if (GameController == null) GameController = FindObjectOfType<GameController>();
        if (BlackPieces == null) BlackPieces = GameObject.Find("Black Pieces");
        if (WhitePieces == null) WhitePieces = GameObject.Find("White Pieces");
        if (Board == null) Board = GameObject.Find("Board");
    }

    void Update()
    {
        if (ShouldStartAITurn())
        {
            aiTurnPending = true;
            moveTimer = MoveDelay;
        }

        if (aiTurnPending)
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0)
            {
                StartCoroutine(ExecuteAITurn(UseRandomMoves));
                aiTurnPending = false;
            }
        }
    }

    bool ShouldStartAITurn()
    {
        return !GameController.WhiteTurn && !aiTurnPending && !IsPieceMoving() && !isExecutingMove;
    }

    bool IsPieceMoving()
    {
        foreach (Transform piece in BlackPieces.transform)
        {
            if (piece.GetComponent<PieceController>().IsMoving())
                return true;
        }
        return false;
    }

    IEnumerator ExecuteAITurn(bool randomMove)
    {
        isExecutingMove = true;
        
        if (randomMove)
        {
            MakeRandomMove();
        }
        else
        {
            yield return StartCoroutine(CalculateBestMove());
        }

        isExecutingMove = false;
    }

    IEnumerator CalculateBestMove()
    {
        nodesEvaluated = 0;
        MoveOption bestMove = null;
        List<MoveOption> allMoves = GetValidMoves(BlackPieces);

        // Boost non-pawn moves in initial ordering

        if (allMoves.Count == 1)
        {
            ExecuteMove(allMoves[0]);
            yield break;
        }
        allMoves.Sort((a, b) => {
    bool aIsPawn = a.Piece.GetComponent<PieceController>().PieceType == "Pawn";
    bool bIsPawn = b.Piece.GetComponent<PieceController>().PieceType == "Pawn";
    return aIsPawn.CompareTo(bIsPawn); // Non-pawns first
});

        int alpha = int.MinValue;
        int beta = int.MaxValue;
        int bestValue = int.MinValue;

        // Sort moves to improve alpha-beta pruning efficiency
        allMoves.Sort((a, b) => CompareMoves(a, b));

        foreach (MoveOption move in allMoves)
        {
            SimulateMove(move, true);
            int moveValue = Minimax(SearchDepth - 1, alpha, beta, false);
            SimulateMove(move, false);
            
            if (moveValue > bestValue)
            {
                bestValue = moveValue;
                bestMove = move;
                alpha = Mathf.Max(alpha, bestValue);
            }

            if (beta <= alpha)
                break;

            if (nodesEvaluated % 20 == 0) // More frequent yielding
                yield return null;
        }

        Debug.Log($"AI evaluated {nodesEvaluated} nodes. Best move value: {bestValue}");

        if(nodesEvaluated==0) {
            Debug.Log("Checkmate!");
            //add checkmate here
        }
        
        if (bestMove != null)
        {
            ExecuteMove(bestMove);
        }
    }

   int CompareMoves(MoveOption a, MoveOption b)
{
    if (a == null) return b == null ? 0 : 1;
    if (b == null) return -1;
    
    // Safe piece type access
    string aPieceType = a.Piece?.GetComponent<PieceController>()?.PieceType ?? "None";
    string bPieceType = b.Piece?.GetComponent<PieceController>()?.PieceType ?? "None";

    // Captures first (MVV-LVA)
    string aCapturedType = GetCapturedPieceType(a);
    string bCapturedType = GetCapturedPieceType(b);
    
    bool aCaptures = aCapturedType != "None";
    bool bCaptures = bCapturedType != "None";
    
    if (aCaptures && bCaptures)
    {
        int aValue = PieceValues.TryGetValue(aCapturedType, out int aVal) ? aVal : 0;
        int bValue = PieceValues.TryGetValue(bCapturedType, out int bVal) ? bVal : 0;
        return bValue.CompareTo(aValue);
    }
    if (aCaptures) return -1;
    if (bCaptures) return 1;
    
    // Checks next
    SimulateMove(a, true);
    bool aChecks = IsInCheck(a.Piece.tag == "Black" ? WhitePieces : BlackPieces);
    SimulateMove(a, false);
    
    SimulateMove(b, true);
    bool bChecks = IsInCheck(b.Piece.tag == "Black" ? WhitePieces : BlackPieces);
    SimulateMove(b, false);
    
    if (aChecks && !bChecks) return -1;
    if (!aChecks && bChecks) return 1;
    
    // 4. Prioritize central pawn moves over edge pawn moves
//     if (aPieceType == "Pawn" && bPieceType == "Pawn")
// {
//     // Calculate centrality - higher values for more central files
//     float aCentrality = 3.5f - Mathf.Abs(a.TargetPosition.x - 3.5f);
//     float bCentrality = 3.5f - Mathf.Abs(b.TargetPosition.x - 3.5f);
    
//     // Also consider pawn advancement - more advanced pawns get slightly higher priority
//     float aRank = a.Piece.tag == "Black" ? a.TargetPosition.y + 4 : 3.5f - a.TargetPosition.y;
//     float bRank = b.Piece.tag == "Black" ? b.TargetPosition.y + 4 : 3.5f - b.TargetPosition.y;
    
//     // Combined score (60% centrality, 40% rank)
//     float aScore = (aCentrality * 0.6f) + (aRank * 0.4f);
//     float bScore = (bCentrality * 0.6f) + (bRank * 0.4f);
    
//     return bScore.CompareTo(aScore);
// }
    
    //Prioritize piece development (non-pawn moves)
    bool aIsPawn = aPieceType == "Pawn";
    bool bIsPawn = bPieceType == "Pawn";
    
    if (!aIsPawn && bIsPawn) return -1;
    if (aIsPawn && !bIsPawn) return 1;
    
    //Default: prioritize moves that advance the piece forward
    float aProgress = a.Piece.tag == "Black" ? a.TargetPosition.y - a.OriginalPosition.y 
                                           : a.OriginalPosition.y - a.TargetPosition.y;
    float bProgress = b.Piece.tag == "Black" ? b.TargetPosition.y - b.OriginalPosition.y 
                                           : b.OriginalPosition.y - b.TargetPosition.y;
    return bProgress.CompareTo(aProgress);
}

string GetCapturedPieceType(MoveOption move)
{
    GameObject captured = GetPieceAtPosition(move.TargetPosition);
    return captured?.GetComponent<PieceController>()?.PieceType ?? "None";
}

    int Minimax(int depth, int alpha, int beta, bool isMaximizingPlayer)
    {
        nodesEvaluated++;

        if (depth == 0 || IsTerminalNode())
            return EvaluateBoard();

        List<MoveOption> moves = GetValidMoves(isMaximizingPlayer ? BlackPieces : WhitePieces);
        if (moves.Count == 0)
            return EvaluateBoard();

        if (isMaximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (MoveOption move in moves)
            {
                SimulateMove(move, true);
                int eval = Minimax(depth - 1, alpha, beta, false);
                SimulateMove(move, false);
                
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha)
                    break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (MoveOption move in moves)
            {
                SimulateMove(move, true);
                int eval = Minimax(depth - 1, alpha, beta, true);
                SimulateMove(move, false);
                
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha)
                    break;
            }
            return minEval;
        }
    }
    bool IsTerminalNode()
    {
        return GetValidMoves(BlackPieces).Count == 0 || GetValidMoves(WhitePieces).Count == 0;
    }
    int EvaluateBoard()
{
    int score = 0;
    bool endgame = IsEndgame();
    // Material with tapered evaluation
    score += TaperedEval(EvaluateMaterial(BlackPieces), EvaluateMaterial(WhitePieces), endgame);
    // Piece activity
    score += EvaluatePieceActivity(BlackPieces, endgame) - EvaluatePieceActivity(WhitePieces, endgame);
    // King safety - more important in middlegame
    if (!endgame){score += EvaluateKingSafety(BlackPieces) - EvaluateKingSafety(WhitePieces);}
    // Pawn structure
    score += EvaluatePawnStructure(BlackPieces) - EvaluatePawnStructure(WhitePieces);
    // Space control
    score += EvaluateSpaceControl(BlackPieces) - EvaluateSpaceControl(WhitePieces);
    // Tempo
    score += TempoBonus;
    return score;
}

int TaperedEval(int mgScore, int egScore, bool endgame)
{
    if (endgame) return egScore;
    return mgScore;
}

int EvaluatePieceActivity(GameObject pieces, bool endgame)
{
    int activity = 0;
    
    foreach (Transform piece in pieces.transform)
    {
        if (!piece.gameObject.activeSelf) continue;
        
        var pc = piece.GetComponent<PieceController>();
        string type = pc.PieceType;
        Vector3 pos = piece.position;
        
        // Central control bonus
        float centrality = (3.5f - Mathf.Abs(pos.x - 3.5f)) * (3.5f - Mathf.Abs(pos.y - 3.5f));
        activity += (int)(centrality * 0.5f);
        
        // Mobility
        int mobility = GetPieceMobility(piece.gameObject);
        activity += mobility * (type == "Knight" ? 3 : type == "Bishop" ? 2 : 1);
        
        // Outpost squares for knights/bishops
        if ((type == "Knight" || type == "Bishop") && IsOutpost(piece.gameObject))
        {
            activity += 25;
        }
    }
    
    return activity;
}

bool IsOutpost(GameObject piece)
{
    // Check if piece is on a square that can't be attacked by enemy pawns
    return false; // Implement proper outpost detection
}

int EvaluateSpaceControl(GameObject pieces)
{
    int space = 0;
    int[] controlMap = new int[64];
    
    foreach (Transform piece in pieces.transform)
    {
        if (!piece.gameObject.activeSelf) continue;
        
        var pc = piece.GetComponent<PieceController>();
        foreach (Transform square in Board.transform)
        {
            Vector3 target = new Vector3(square.position.x, square.position.y, piece.position.z);
            if (pc.ValidateMovement(piece.position, target, out _))
            {
                int file = Mathf.FloorToInt(target.x + 4);
                int rank = Mathf.FloorToInt(target.y + 4);
                controlMap[rank*8 + file]++;
            }
        }
    }
    
    // Reward controlling central squares
    for (int rank = 2; rank <= 5; rank++)
    {
        for (int file = 2; file <= 5; file++)
        {
            space += controlMap[rank*8 + file] * 2;
        }
    }
    
    return space;
}

    bool IsEndgame()
    {
        int blackMajors = 0;
        int whiteMajors = 0;
        
        foreach (Transform piece in BlackPieces.transform)
        {
            string type = piece.GetComponent<PieceController>().PieceType;
            if (type == "Queen" || type == "Rook") blackMajors++;
        }
        
        foreach (Transform piece in WhitePieces.transform)
        {
            string type = piece.GetComponent<PieceController>().PieceType;
            if (type == "Queen" || type == "Rook") whiteMajors++;
        }
        
        // Consider it endgame if both sides have no queens or 
        // one side has no queens and the other has at most one rook
        return (blackMajors == 0 && whiteMajors <= 1) || 
               (whiteMajors == 0 && blackMajors <= 1);
    }

    // int EvaluatePieceSquareTables(GameObject pieces, bool isWhite, bool endgame)
    // {
    //     int score = 0;
    //     foreach (Transform piece in pieces.transform)
    //     {
    //         if (!piece.gameObject.activeSelf) continue;
            
    //         Vector3 pos = piece.transform.position;
    //         int file = Mathf.FloorToInt(pos.x + 4); // Convert to 0-7
    //         int rank = Mathf.FloorToInt(pos.y + 4); // Convert to 0-7
            
    //         if (isWhite) rank = 7 - rank; // Flip for white
            
    //         int index = rank * 8 + file;
    //         string type = piece.GetComponent<PieceController>().PieceType;

    //         switch (type)
    //         {
    //             case "Pawn":
    //                 score += endgame ? PawnTableEndgame[index] : PawnTable[index];
    //                 break;
    //             case "Knight":
    //                 score += KnightTable[index];
    //                 break;
    //             case "Bishop":
    //                 score += BishopTable[index];
    //                 break;
    //             case "Rook":
    //                 score += RookTable[index];
    //                 break;
    //             case "Queen":
    //                 score += QueenTable[index];
    //                 break;
    //             case "King":
    //                 score += endgame ? KingTableEndgame[index] : KingTable[index];
    //                 break;
    //         }
    //     }
    //     return score;
    // }

    // int EvaluatePassedPawns(GameObject pieces)
    // {
    //     int score = 0;
    //     string color = pieces == WhitePieces ? "White" : "Black";
        
    //     foreach (Transform pawn in pieces.transform)
    //     {
    //         var pc = pawn.GetComponent<PieceController>();
    //         if (pc.PieceType != "Pawn") continue;
            
    //         if (IsPassedPawn(pawn.gameObject, color))
    //         {
    //             // Bonus increases as pawn advances
    //             int rank = Mathf.FloorToInt(pawn.position.y + 4);
    //             if (color == "White") rank = 7 - rank;
                
    //             score += 20 * rank; // 0-140 bonus based on rank
    //         }
    //     }
    //     return score;
    // }

    // bool IsPassedPawn(GameObject pawn, string color)
    // {
    //     int file = Mathf.FloorToInt(pawn.transform.position.x + 4);
    //     int rank = Mathf.FloorToInt(pawn.transform.position.y + 4);
    //     if (color == "White") rank = 7 - rank;
        
    //     GameObject opponentPieces = color == "White" ? BlackPieces : WhitePieces;
        
    //     // Check if there are any opposing pawns that can intercept this pawn
    //     foreach (Transform opponentPawn in opponentPieces.transform)
    //     {
    //         var pc = opponentPawn.GetComponent<PieceController>();
    //         if (pc.PieceType != "Pawn") continue;
            
    //         int oppFile = Mathf.FloorToInt(opponentPawn.position.x + 4);
    //         int oppRank = Mathf.FloorToInt(opponentPawn.position.y + 4);
    //         if (color == "Black") oppRank = 7 - oppRank;
            
    //         // Opponent pawn is on adjacent file and not behind our pawn
    //         if (Mathf.Abs(oppFile - file) <= 1 && oppRank < rank)
    //         {
    //             return false;
    //         }
    //     }
    //     return true;
    // }

    // int EvaluateThreats(GameObject attackers, GameObject defenders)
    // {
    //     int score = 0;
        
    //     foreach (Transform attacker in attackers.transform)
    //     {
    //         if (!attacker.gameObject.activeSelf) continue;
            
    //         string attackerType = attacker.GetComponent<PieceController>().PieceType;
            
    //         foreach (Transform defender in defenders.transform)
    //         {
    //             if (!defender.gameObject.activeSelf) continue;
                
    //             string defenderType = defender.GetComponent<PieceController>().PieceType;
                
    //             // Check if attacker threatens defender
    //             GameObject dummy;
    //             if (attacker.GetComponent<PieceController>().ValidateMovement(
    //                 attacker.position, defender.position, out dummy, true))
    //             {
    //                 // Add threat bonus based on piece types
    //                 if (ThreatWeights.ContainsKey(attackerType) && 
    //                     ThreatWeights[attackerType].ContainsKey(defenderType))
    //                 {
    //                     score += ThreatWeights[attackerType][defenderType];
    //                 }
    //             }
    //         }
    //     }
    //     return score;
    // }

    // int EvaluateKingSafety(GameObject pieces, bool endgame)
    // {
    //     if (endgame) return 0; // King safety less important in endgame
        
    //     int safety = 0;
    //     Transform king = null;
    //     string color = pieces == WhitePieces ? "White" : "Black";
        
    //     // Find the king
    //     foreach (Transform piece in pieces.transform)
    //     {
    //         if (piece.GetComponent<PieceController>().PieceType == "King")
    //         {
    //             king = piece;
    //             break;
    //         }
    //     }
    //     if (king == null) return 0;

    //     Vector3 kingPos = king.position;
    //     int kingFile = Mathf.FloorToInt(kingPos.x + 4);
    //     int kingRank = Mathf.FloorToInt(kingPos.y + 4);
    //     if (color == "White") kingRank = 7 - kingRank;

    //     // Penalty for open files near king
    //     for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
    //     {
    //         int file = kingFile + fileOffset;
    //         if (file < 0 || file > 7) continue;
            
    //         bool openFile = true;
    //         foreach (Transform pawn in pieces.transform)
    //         {
    //             var pc = pawn.GetComponent<PieceController>();
    //             if (pc.PieceType == "Pawn" && 
    //                 Mathf.FloorToInt(pawn.position.x + 4) == file)
    //             {
    //                 openFile = false;
    //                 break;
    //             }
    //         }
    //         if (openFile) safety -= 20;
    //     }

    //     // Pawn shelter bonus
    //     int shelter = 0;
    //     for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
    //     {
    //         int file = kingFile + fileOffset;
    //         if (file < 0 || file > 7) continue;
            
    //         for (int rankOffset = 1; rankOffset <= 2; rankOffset++)
    //         {
    //             int rank = kingRank + rankOffset;
    //             if (rank > 7) continue;
                
    //             foreach (Transform pawn in pieces.transform)
    //             {
    //                 var pc = pawn.GetComponent<PieceController>();
    //                 if (pc.PieceType == "Pawn")
    //                 {
    //                     int pawnFile = Mathf.FloorToInt(pawn.position.x + 4);
    //                     int pawnRank = Mathf.FloorToInt(pawn.position.y + 4);
    //                     if (color == "White") pawnRank = 7 - pawnRank;
                        
    //                     if (pawnFile == file && pawnRank == rank)
    //                     {
    //                         shelter += rank == 1 ? 10 : 5;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     safety += shelter;

    //     // Attackers near king penalty
    //     GameObject opponentPieces = color == "White" ? BlackPieces : WhitePieces;
    //     int attackerCount = 0;
    //     int attackerWeight = 0;
        
    //     foreach (Transform attacker in opponentPieces.transform)
    //     {
    //         if (!attacker.gameObject.activeSelf) continue;
            
    //         string attackerType = attacker.GetComponent<PieceController>().PieceType;
    //         if (attackerType == "King") continue;
            
    //         float dist = Vector3.Distance(attacker.position, kingPos);
    //         if (dist < 3.5f) // Pieces within 3 squares
    //         {
    //             attackerCount++;
    //             // More dangerous pieces get higher weight
    //             attackerWeight += PieceValues[attackerType] / 100;
    //         }
    //     }
        
    //     safety -= attackerCount * 5 + attackerWeight * 10;

    //     return safety;
    // }

    int EvaluateMaterial(GameObject pieces)
    {
        int material = 0;
        foreach (Transform piece in pieces.transform)
        {
            if (!piece.gameObject.activeSelf) continue;
            
            string type = piece.GetComponent<PieceController>().PieceType;
            if (PieceValues.ContainsKey(type))
                material += PieceValues[type];
        }
        return material;
    }
//     int EvaluatePieceDevelopment(GameObject pieces)
// {
//     int development = 0;
//     int pieceCount = 0;
    
//     foreach (Transform piece in pieces.transform)
//     {
//         var pc = piece.GetComponent<PieceController>();
//         if (pc.PieceType != "Pawn" && pc.PieceType != "King")
//         {
//             pieceCount++;
//             // Bonus for moving from starting position
//             if (pc.moved) development += 10; 
            
//             // Extra bonus for central control
//             Vector3 pos = piece.position;
//             float centrality = (3.5f - Mathf.Abs(pos.x - 3.5f)) * 5;
//             development += (int)centrality;
//         }
//     }
    
//     // Penalty for undeveloped pieces
//     development -= (4 - pieceCount) * 15; // Assuming 4 non-pawn pieces
    
//     return development;
// }

//     int EvaluatePieceSquareTables(GameObject pieces, bool isWhite)
//     {
//         int score = 0;
//         foreach (Transform piece in pieces.transform)
//         {
//             if (!piece.gameObject.activeSelf) continue;
            
//             Vector3 pos = piece.transform.position;
//             int file = Mathf.FloorToInt(pos.x + 4); // Convert to 0-7
//             int rank = Mathf.FloorToInt(pos.y + 4); // Convert to 0-7
            
//             if (isWhite) rank = 7 - rank; // Flip for white
            
//             int index = rank * 8 + file;
//             string type = piece.GetComponent<PieceController>().PieceType;

//             switch (type)
//             {
//                 case "Pawn":
//                     score += PawnTable[index];
//                     break;
//                 case "Knight":
//                     score += KnightTable[index];
//                     break;
//                 case "Bishop":
//                     score += BishopTable[index];
//                     break;
//                 case "Rook":
//                     score += RookTable[index];
//                     break;
//                 case "Queen":
//                     score += QueenTable[index];
//                     break;
//                 case "King":
//                     score += KingTable[index];
//                     break;
//             }
//         }
//         return score;
//     }

//     int EvaluateMobility(GameObject pieces)
//     {
//         int mobility = 0;
//         foreach (Transform piece in pieces.transform)
//         {
//             if (!piece.gameObject.activeSelf) continue;
            
//             string type = piece.GetComponent<PieceController>().PieceType;
//             int moves = GetPieceMobility(piece.gameObject);

//             // Stockfish-style mobility bonuses
//             switch (type)
//             {
//                 case "Knight":
//                     mobility += moves * 2;
//                     break;
//                 case "Bishop":
//                     mobility += moves * 3;
//                     break;
//                 case "Rook":
//                     mobility += moves * 2;
//                     break;
//                 case "Queen":
//                     mobility += moves;
//                     break;
//             }
//         }
//         return mobility;
//     }

    int GetPieceMobility(GameObject piece)
    {
        int count = 0;
        PieceController pc = piece.GetComponent<PieceController>();
        foreach (Transform square in Board.transform)
        {
            Vector3 target = new Vector3(square.position.x, square.position.y, piece.transform.position.z);
            GameObject enemy;
            if (pc.ValidateMovement(piece.transform.position, target, out enemy))
            {
                count++;
            }
        }
        return count;
    }

    int EvaluatePawnStructure(GameObject pieces)
{
    int score = 0;
    int[] pawnFiles = new int[8]; // Count pawns per file (0-7)
    
    // Count pawns
    foreach (Transform piece in pieces.transform)
    {
        var pc = piece.GetComponent<PieceController>();
        if (pc.PieceType == "Pawn")
        {
            int file = Mathf.FloorToInt(piece.position.x + 4);
            pawnFiles[file]++;
        }
    }
    
    // Evaluate structure
    for (int file = 0; file < 8; file++)
    {
        if (pawnFiles[file] == 0) continue;
        
        // Penalize doubled/tripled pawns
        if (pawnFiles[file] > 1) score -= 20 * (pawnFiles[file] - 1);
        
        // Bonus for central pawns
        if (file >= 3 && file <= 4) score += 15;
    }
    
    return score;
}

    int EvaluateKingSafety(GameObject pieces)
{
    int safety = 0;
    Transform king = null;
    
    // Find the king
    foreach (Transform piece in pieces.transform)
    {
        if (piece.GetComponent<PieceController>().PieceType == "King")
        {
            king = piece;
            break;
        }
    }
    if (king == null) return 0;

    Vector3 kingPos = king.position;
    int pawnShield = 0;
    int attackerCount = 0;

    // Pawn shield evaluation
    for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
    {
        float checkX = kingPos.x + fileOffset;
        
        // Check for friendly pawns in front of king
        foreach (Transform p in pieces.transform)
        {
            var pc = p.GetComponent<PieceController>();
            if (pc.PieceType == "Pawn" && 
                Mathf.Abs(p.position.x - checkX) < 0.1f && 
                Mathf.Abs(p.position.y - kingPos.y) < 2f)
            {
                pawnShield += 10;
            }
        }
    }
    safety += pawnShield;

    // Open file penalty
    bool openFile = true;
    foreach (Transform p in pieces.transform)
    {
        var pc = p.GetComponent<PieceController>();
        if (pc.PieceType == "Pawn" && 
            Mathf.Abs(p.position.x - kingPos.x) < 0.1f)
        {
            openFile = false;
            break;
        }
    }
    if (openFile) safety -= 20;

    // Attacker count penalty
    GameObject opponentPieces = pieces == WhitePieces ? BlackPieces : WhitePieces;
    foreach (Transform piece in opponentPieces.transform)
    {
        if (Vector3.Distance(piece.position, kingPos) < 3f)
        {
            attackerCount++;
        }
    }
    safety -= attackerCount * 5;

    return safety;
}

    // bool HasBishopPair(GameObject pieces)
    // {
    //     int bishopCount = 0;
    //     foreach (Transform piece in pieces.transform)
    //     {
    //         if (!piece.gameObject.activeSelf) continue;
    //         if (piece.GetComponent<PieceController>().PieceType == "Bishop")
    //         {
    //             bishopCount++;
    //             if (bishopCount >= 2) return true;
    //         }
    //     }
    //     return false;
    // }

    bool IsInCheck(GameObject kingSide)
    {
        Transform king = null;
        foreach (Transform piece in kingSide.transform)
        {
            if (piece.GetComponent<PieceController>().PieceType == "King")
            {
                king = piece;
                break;
            }
        }
        if (king == null) return false;

        GameObject attackerSide = kingSide == WhitePieces ? BlackPieces : WhitePieces;
        foreach (Transform piece in attackerSide.transform)
        {
            GameObject encounteredEnemy;
            if (piece.GetComponent<PieceController>().ValidateMovement(
                piece.position, king.position, out encounteredEnemy, true))
            {
                return true;
            }
        }
        return false;
    }

    void SimulateMove(MoveOption move, bool execute)
    {
        if (execute)
        {
            move.OriginalPosition = move.Piece.transform.position;
            move.Piece.transform.position = move.TargetPosition;
            
            GameObject targetPiece = GetPieceAtPosition(move.TargetPosition);
            if (targetPiece != null && targetPiece.tag != move.Piece.tag)
            {
                move.CapturedPiece = targetPiece;
                targetPiece.SetActive(false);
            }
        }
        else
        {
            move.Piece.transform.position = move.OriginalPosition;
            if (move.CapturedPiece != null)
            {
                move.CapturedPiece.SetActive(true);
                move.CapturedPiece = null;
            }
        }
    }

    GameObject GetPieceAtPosition(Vector3 position)
    {
        foreach (Transform piece in WhitePieces.transform)
        {
            if (piece.position.x == position.x && piece.position.y == position.y)
                return piece.gameObject;
        }
        foreach (Transform piece in BlackPieces.transform)
        {
            if (piece.position.x == position.x && piece.position.y == position.y)
                return piece.gameObject;
        }
        return null;
    }

    List<MoveOption> GetValidMoves(GameObject pieces)
    {
        List<MoveOption> validMoves = new List<MoveOption>();

        foreach (Transform piece in pieces.transform)
        {
            if (!piece.gameObject.activeSelf) continue;

            PieceController pc = piece.GetComponent<PieceController>();
            foreach (Transform square in Board.transform)
            {
                Vector3 target = new Vector3(square.position.x, square.position.y, piece.position.z);
                GameObject enemy;
                if (pc.ValidateMovement(piece.position, target, out enemy))
                {
                    validMoves.Add(new MoveOption(piece.gameObject, target));
                }
            }
        }
        return validMoves;
    }

    void ExecuteMove(MoveOption move)
    {
        GameController.SelectPiece(move.Piece);
        move.Piece.GetComponent<PieceController>().MovePiece(move.TargetPosition);
    }

    void MakeRandomMove()
    {
        List<MoveOption> moves = GetValidMoves(BlackPieces);
        if (moves.Count > 0)
        {
            ExecuteMove(moves[Random.Range(0, moves.Count)]);
        }
    }

    private class MoveOption
    {
        public GameObject Piece { get; }
        public Vector3 TargetPosition { get; }
        public Vector3 OriginalPosition { get; set; }
        public GameObject CapturedPiece { get; set; }

        public MoveOption(GameObject piece, Vector3 targetPosition)
        {
            Piece = piece;
            TargetPosition = targetPosition;
        }
    }
}