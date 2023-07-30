using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot

{
    //r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1
    //Depth=9 Nb move checks : 25 765 376 Nb positions saved : 1 791 464 -> Move: 'e2a6' = 225 -> 60s
    //Depth=8 Nb move checks : 2 411 306 Nb positions saved : 709179 -> Move: 'e2a6' = -110 -> 15.9s
    //Depth=7 Nb move checks : 1 890 098 Nb positions saved : 107476 -> Move: 'e2a6' = 225 -> 5.6s
    //Depth=6 Nb move checks : 153 626 Nb positions saved : 42034 -> Move: 'e2a6' = -100 -> 1s
    //Depth=5 Nb move checks : 107 362 Nb positions saved : 6107 -> Move: 'd5e6' = 315 -> 0.4s
    //Depth=4 Nb move checks : 317 510 Nb positions saved : 68992 -> Move: 'd5e6' = 315 -> 1.4s
    //Depth=3 Nb move checks : 5 906 Nb positions saved : 1796 -> Move: 'e2a6' = 50 -> 0.1s
    //Depth=2 Nb move checks : 4 004 Nb positions saved : 138 -> Move: 'e2a6' = 390 -> 0.1s
    //Depth=1 Nb move checks : 166 Nb positions saved : 49 -> Move: 'e2a6' = 70 -> 0.1
    int MAX_DEPTH = 6; //6 is ideal
    //best = 537 token -> 853 -> 939
    int[] positionValues = { -50, -40, -30, -20, -10, -5, 0, 5, 10, 15, 20, 25, 30, 40, 50 };
    //TODO make symetric, in ulong and modify bestPos to include -1/+1
    // ->  positionValues = 0x645a504b46413
    //int[] pieceValues = { 100, 300, 300, 450, 950, 960 }; 
    //               King|Queen|Rook|Bishop|Knight|Pawn piece value /100 coded on 8 bits
    //->  pieceValue = 0x605f2d1e1e0a;

    // List of all prefered possition : index of value above coded in an 6*4 bit int for each case
    // King|Queen|Rook|Bishop|Knight|Pawn
    int[] bestPositions =
                       {0x236306, 0x146416, 0x146426, 0x56426, 0x56426, 0x146426, 0x146416, 0x236306,
                        0x24741e, 0x16863e, 0x16866e, 0x6866e, 0x6866e, 0x16866e, 0x16863e, 0x24741e,
                        0x245428, 0x166668, 0x17678a, 0x7689c, 0x7689c, 0x17678a, 0x166668, 0x245428,
                        0x255427, 0x166777, 0x176798, 0x768ab, 0x768ab, 0x176798, 0x166777, 0x255427,
                        0x365426, 0x266666, 0x276896, 0x1768aa, 0x1768aa, 0x276896, 0x266666, 0x355426,
                        0x445427, 0x376875, 0x376884, 0x376896, 0x376896, 0x376884, 0x366875, 0x445427,
                        0xa45417, 0xa66738, 0x676668, 0x666673, 0x666673, 0x666668, 0xa66738, 0xa45417,
                        0xa36306, 0xc46416, 0x846426, 0x657426, 0x657426, 0x846426, 0xc46416, 0xa36306
    };

    Dictionary<ulong, MyMove> transposition = new Dictionary<ulong, MyMove>();
    //int INF = 25000;
    int count;
    MyMove[,] killerMoves;
    Board board;
    public Move Think(Board _board, Timer timer)
    {
        board = _board;
        killerMoves = new MyMove[6000, 2];//Max ply
        count = 0;
        if (timer.MillisecondsRemaining < 10000) //If 10s left -> aggressif quick mode
            MAX_DEPTH = 5;
        Move bestMove = Move.NullMove;
        int bestScore = applyNegamaxOnmoves(MAX_DEPTH, -25000, 25000, board.IsWhiteToMove ? 1 : -1, ref bestMove);//- INF, +INF
        Console.WriteLine("Depth=" + MAX_DEPTH + " move n°" + board.PlyCount + " checks : " + count + " Nb positions saved : " + transposition.Count + " -> " + bestMove + " = " + bestScore);
        return bestMove;
    }

    //Test with minimax, negamax, negascout
    int applyNegamaxOnmoves(int depth, int alpha, int beta, int color, ref Move pv)
    {
        int alphaOrigin = alpha;
        bool dopv = false;
        if (board.PlyCount >= 6000 || depth <= 0) //max ply
            return color * (evaluateBoardForColor(true) - evaluateBoardForColor(false));

        if (transposition.TryGetValue(board.ZobristKey, out MyMove? ttMove))
        {
            if (ttMove.depth >= depth)
            {
                switch (ttMove.flag)
                {
                    case 0:
                        alpha = Math.Max(alpha, ttMove.value);
                        break;
                    case 1:
                        beta = Math.Min(beta, ttMove.value);
                        break;
                    case 2:
                        return ttMove.value;
                }
                if (alpha >= beta) return ttMove.value;
            }
        }

        Span<Move> stackMove = stackalloc Move[400];//generate moves, 400=upper max movement possible for a turn
        board.GetLegalMovesNonAlloc(ref stackMove);
        var moves = new List<MyMove>();
        foreach (Move m in stackMove) moves.Add(new MyMove(m));
        scoreMoves(moves, ttMove);//Apply a sort score to all moves

        MyMove bestMove = new MyMove(Move.NullMove);
        int bestMoveValue = -25000;

        for (int i = 0; i < moves.Count; i++)
        {
            //PickMove -> put the best sort score at the first index
            for (int j = i + 1; j < moves.Count; j++)
                if (moves[j].sortScore > moves[i].sortScore)
                    (moves[i], moves[j]) = (moves[j], moves[i]); //swap
            MyMove currentMove = moves[i];

            board.MakeMove(currentMove.move);
            Move node_pv = pv;
            int currentScore = 0; //DRAW score
            if (!board.IsDraw())
            {
                if (!dopv) currentScore = -applyNegamaxOnmoves(depth - 1, -beta, -alpha, -color, ref node_pv);
                else
                {
                    currentScore = -applyNegamaxOnmoves(depth - 1, -alpha - 1, -alpha, -color, ref node_pv);
                    if (currentScore > alpha && currentScore < beta) currentScore = -applyNegamaxOnmoves(depth - 1, -beta, -alpha, -color, ref node_pv);
                }
            }
            board.UndoMove(currentMove.move);
            if (currentScore > bestMoveValue)
            {
                bestMoveValue = currentScore;
                bestMove = currentMove;
            }
            if (bestMoveValue > alpha)
            {
                alpha = bestMoveValue;
                dopv = true;
                pv = bestMove.move;
            }
            if (alpha >= beta)
            {
                if (!currentMove.move.IsCapture && !currentMove.Equals(killerMoves[board.PlyCount, 0]))
                {
                    killerMoves[board.PlyCount, 1] = killerMoves[board.PlyCount, 0];
                    killerMoves[board.PlyCount, 0] = currentMove;
                }
                break;
            }
        }
        if (moves.Count == 0)
        {
            if (board.IsInCheck())
                return -24000 + board.PlyCount; //we are checkmate
            return -1000;//-24000; //stalemate
        }
        if (bestMoveValue >= beta) bestMove.flag = 0;//lowerbound
        else if (bestMoveValue <= alphaOrigin) bestMove.flag = 1;//upperbound
        else bestMove.flag = 2;//exact
        bestMove.value = bestMoveValue;
        bestMove.depth = depth;
        transposition.TryAdd(board.ZobristKey, bestMove);
        return bestMoveValue;
    }

    int evaluateBoardForColor(bool whitePiece)
    {
        count++;
        int boardValue = 0;
        for (int i = 0; i < 6; i++)
        {
            ulong bitboard = board.GetPieceBitboard((PieceType)i + 1, whitePiece);
            for (int j = 0; j < 64; j++)
                if ((bitboard & 1UL << j) != 0)
                    boardValue += (int)
                        (
                            positionValues[(bestPositions[whitePiece ? 63 - j : j] >> i * 4) & 0xF]
                            + (0x5A32201F0A >> i * 8 & 0xff) * 10
                        );
        }
        return boardValue;
    }

    /*int[][] MVV_LVA = { -> 
        new int[]{15, 14, 13, 12, 11, 10 }, // victim P, attacker P, N, B, R, Q, K
        new int[]{25, 24, 23, 22 ,21, 20 }, // victim N, attacker P, N, B, R, Q, K
        new int[]{35, 34, 33, 32, 31, 30 }, // victim B, attacker P, N, B, R, Q, K
        new int[]{45, 44, 43, 42, 41, 40 }, // victim R, attacker P, N, B, R, Q, K
        new int[]{55, 54, 53, 52, 51, 50 }, // victim Q, attacker P, N, B, R, Q, K
     //value = MVA_OFFSET + MVV_LVA[(int)move.move.CapturePieceType - 1][(int)move.move.MovePieceType - 1] -10;
    };*/


    int MVA_OFFSET = int.MaxValue - 256;
    void scoreMoves(List<MyMove> moves, MyMove? ttMove)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            int value = 0;
            MyMove move = moves[i];
            if (ttMove != null && move.move.Equals(ttMove.move))
                value = MVA_OFFSET + 60;
            else if (move.move.IsCapture)
                value = MVA_OFFSET + (0xABCDEF0 >> (int)move.move.MovePieceType * 4 & 0xF) + 10 * (int)move.move.CapturePieceType; //MVV_LVA
            else
                for (int n = 0; n < 2; n++)
                    if (killerMoves[board.PlyCount, n] != null && move.move.Equals(killerMoves[board.PlyCount, n].move))
                    {
                        value = MVA_OFFSET - (n + 1) * 10; //10 = killerValue
                        break;
                    }
            move.sortScore = value;
        }
    }
    class MyMove
    {
        public Move move;
        public long sortScore;
        public int value;
        //0 -> alpha
        //1 -> beta
        //2 -> exact
        public int flag;
        public int depth;
        public MyMove(Move _move) => move = _move;
    }
}