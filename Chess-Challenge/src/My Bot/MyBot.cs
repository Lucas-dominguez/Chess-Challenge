using System;
using System.Collections.Generic;
using System.Numerics;
using ChessChallenge.API;

public class MyBot : IChessBot

{
    //r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1 -> -> (with quiencese)
    //Depth=9 Nb move checks : 25 765 376 Nb positions saved : 1 791 464 -> Move: 'e2a6' = 225 -> 60s
    //Depth=8 Nb move checks : 2 411 306 Nb positions saved : 709179 -> Move: 'e2a6' = -110 -> 12.2s
    //Depth=7 Nb move checks : 1 890 098 Nb positions saved : 107476 -> Move: 'e2a6' = 225 -> 5.6s -> 6.3
    //Depth=6 Nb move checks : 153 626 Nb positions saved : 42034 -> Move: 'e2a6' = -100 -> 1s -> 1.4
    //Depth=5 Nb move checks : 107 362 Nb positions saved : 6107 -> Move: 'd5e6' = 315 -> 0.4s
    //Depth=4 Nb move checks : 317 510 Nb positions saved : 68992 -> Move: 'd5e6' = 315 -> 1.4s
    //Depth=3 Nb move checks : 5 906 Nb positions saved : 1796 -> Move: 'e2a6' = 50 -> 0.1s
    //Depth=2 Nb move checks : 4 004 Nb positions saved : 138 -> Move: 'e2a6' = 390 -> 0.1s
    //Depth=1 Nb move checks : 166 Nb positions saved : 49 -> Move: 'e2a6' = 70 -> 0.1
    // 1- 1 -6 contre X, 2-2-12
    int MAX_DEPTH = 6; //6 is ideal
    //best = 537 token -> 853 -> 959
    //int[] positionValues = { -50, -40, -30, -20, -10, -5, 0, 5, 10, 15, 20, 25, 30, 40, 50 };
    //int[] positionValues = { 0, 5, 10, 15, 20, 30, 40, 50 };
    //TODO make symetric, in ulong and modify bestPos to include -1/+1
    // ->  positionValues = 0x23281E140F0A0500
    //int[] pieceValues = { 100, 310, 320, 500, 900, 0 }; 
    //               King|Queen|Rook|Bishop|Knight|Pawn piece value /10 coded on 8 bits
    //->  pieceValue = 0x5A32201F0A;

    // List of all prefered possition : index of value above coded in an 6*4 bit int for each case
    // King|Queen|Rook|Bishop|Knight|Pawn
    // square piece from https://github.com/lhartikk/simple-chess-ai/blob/master/script.js 
    /*ulong[] bestPositions =
                       {0x236306, 0x146416, 0x146426, 0x56426, 0x56426, 0x146426, 0x146416, 0x236306,
                        0x24741e, 0x16863e, 0x16866e, 0x6866e, 0x6866e, 0x16866e, 0x16863e, 0x24741e,
                        0x245428, 0x166668, 0x17678a, 0x7689c, 0x7689c, 0x17678a, 0x166668, 0x245428,
                        0x255427, 0x166777, 0x176798, 0x768ab, 0x768ab, 0x176798, 0x166777, 0x255427,
                        0x365426, 0x266666, 0x276896, 0x1768aa, 0x1768aa, 0x276896, 0x266666, 0x355426,
                        0x445427, 0x376875, 0x376884, 0x376896, 0x376896, 0x376884, 0x366875, 0x445427,
                        0xa45417, 0xa66738, 0x676668, 0x666673, 0x666673, 0x666668, 0xa66738, 0xa45417,
                        0xa36306, 0xc46416, 0x846426, 0x657426, 0x657426, 0x846426, 0xc46416, 0xa36306
    };*/
    /* With EG = MG
    ulong[] bestPositions = {
    0xdc0cf0dc0cf0, 0xea0ae0ea0ae0, 0xea0ad0ea0ad0, 0xf90ad0f90ad0, 0xf90ad0f90ad0, 0xea0ad0ea0ad0, 0xea0ae0ea0ae0, 0xdc0cf0dc0cf0,
    0xda1ae7da1ae7, 0xe020c7e020c7, 0xe02007e02007, 0xf02007f02007, 0xf02007f02007, 0xe02007e02007, 0xe020c7e020c7, 0xda1ae7da1ae7,
    0xda9ad2da9ad2, 0xe00002e00002, 0xe10124e10124, 0xf10235f10235, 0xf10235f10235, 0xe10124e10124, 0xe00002e00002, 0xda9ad2da9ad2,
    0xd99ad1d99ad1, 0xe00111e00111, 0xe10132e10132, 0xf10244f10244, 0xf10244f10244, 0xe10132e10132, 0xe00111e00111, 0xd99ad1d99ad1,
    0xc09ad0c09ad0, 0xd00000d00000, 0xd10230d10230, 0xe10244e10244, 0xe10244e10244, 0xd10230d10230, 0xd00000d00000, 0xc99ad0c99ad0,
    0xaa9ad1aa9ad1, 0xc10219c10219, 0xc1022ac1022a, 0xc10230c10230, 0xc10230c10230, 0xc1022ac1022a, 0xc00219c00219, 0xaa9ad1aa9ad1,
    0x4a9ae14a9ae1, 0x4001c24001c2, 0x010002010002, 0x000001c00001c, 0x00001c00001c, 0x000002000002, 0x4001c24001c2, 0x4a9ae14a9ae1,
    0x4c0cf04c0cf0, 0x5a0ae05a0ae0, 0x2a0ad02a0ad0, 0x0091ad0091ad0, 0x091ad0091ad0, 0x2a0ad02a0ad0, 0x5a0ae05a0ae0, 0x4c0cf04c0cf0
    };*/

    ulong[] bestPositions = {
        0xfc0cf0dc0cf0, 0xea0ae0ea0ae0, 0xda0ad0ea0ad0, 0xc90ad0f90ad0, 0xc90ad0f90ad0, 0xda0ad0ea0ad0, 0xea0ae0ea0ae0, 0xfc0cf0dc0cf0, 0xda1ae7da1ae7, 0xc020c7e020c7, 0xa02007e02007, 0x2007f02007, 0x2007f02007, 0xa02007e02007, 0xc020c7e020c7, 0xda1ae7da1ae7, 0xda9ad2da9ad2, 0xa00002e00002, 0x410124e10124, 0x510235f10235, 0x510235f10235, 0x410124e10124, 0xa00002e00002, 0xda9ad2da9ad2, 0xd99ad1d99ad1, 0xa00111e00111, 0x510132e10132, 0x610244f10244, 0x610244f10244, 0x510132e10132, 0xa00111e00111, 0xd99ad1d99ad1, 0xd09ad0c09ad0, 0xa00000d00000, 0x510230d10230, 0x610244e10244, 0x610244e10244, 0x510230d10230, 0xa00000d00000, 0xd99ad0c99ad0, 0xda9ad1aa9ad1, 0xa10219c10219, 0x41022ac1022a, 0x510230c10230, 0x510230c10230, 0x41022ac1022a, 0xa00219c00219, 0xda9ad1aa9ad1, 0xda9ae14a9ae1, 0xd001c24001c2, 0x10002010002, 0x1c00001c, 0x1c00001c, 0x2000002, 0xd001c24001c2, 0xda9ae14a9ae1, 0xfc0cf04c0cf0, 0xda0ae05a0ae0, 0xda0ad02a0ad0, 0xd91ad0091ad0, 0xd91ad0091ad0, 0xda0ad02a0ad0, 0xda0ae05a0ae0, 0xfc0cf04c0cf0
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
        int bestScore = applyNegascoutOnmoves(MAX_DEPTH, -25000, 25000, board.IsWhiteToMove ? 1 : -1, ref bestMove);//- INF, +INF
        //Console.WriteLine("Depth=" + MAX_DEPTH + " move n°" + board.PlyCount + " checks : " + count + " Nb positions saved : " + transposition.Count + " -> " + bestMove + " = " + bestScore);
        return bestMove;
    }

    //Test with minimax, negamaw -> negascout was the most quick and strong.
    //inspired by https://rustic-chess.org/search/ordering/how.html
    int applyNegascoutOnmoves(int depth, int alpha, int beta, int color, ref Move pv)
    {
        bool quiencense = depth <= 0;
        int alphaOrigin = alpha;
        bool dopv = false;

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
        int bestMoveValue = -25000;
        if (quiencense)
        {
            bestMoveValue = color * (evaluateBoardForColor(true) - evaluateBoardForColor(false));
            if(bestMoveValue > beta) return bestMoveValue;
            alpha = Math.Max(alpha, bestMoveValue);
        }

        //Span<Move> stackMove = stackalloc Move[400];//generate moves, 400=upper max movement possible for a turn
        //board.GetLegalMovesNonAlloc(ref stackMove);
        var stackMove = board.GetLegalMoves(quiencense);
        var moves = new List<MyMove>();
        foreach (Move m in stackMove) moves.Add(new MyMove(m));
        scoreMoves(moves, ttMove);//Apply a sort score to all moves

        MyMove bestMove = new MyMove(Move.NullMove);

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
                if (!dopv) currentScore = -applyNegascoutOnmoves(depth - 1, -beta, -alpha, -color, ref node_pv);
                else
                {
                    currentScore = -applyNegascoutOnmoves(depth - 1, -alpha - 1, -alpha, -color, ref node_pv);
                    if (currentScore > alpha && currentScore < beta) currentScore = -applyNegascoutOnmoves(depth - 1, -beta, -alpha, -color, ref node_pv);
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
        if (!quiencense && moves.Count == 0)
        {
            if (board.IsInCheck())
                return -24000 + board.PlyCount; //we are checkmate
            return 0; //stalemate
        }
        if (bestMoveValue >= beta) bestMove.flag = 0;//lowerbound
        else if (bestMoveValue <= alphaOrigin) bestMove.flag = 1;//upperbound
        else bestMove.flag = 2;//exact
        bestMove.value = bestMoveValue;
        bestMove.depth = depth;
        transposition.TryAdd(board.ZobristKey, bestMove);
        return bestMoveValue;
    }
    //int[] gamephaseInc = { 0, 1, 1, 2, 4, 0 }; -> 0x042110

    int getPSTValue(int phase, bool whitePiece, int i, int j)
    {
        ulong pst = (bestPositions[whitePiece ? 63 - j : j] >> (4 * i + 24 * phase)) & 0xF;
        int sign = (pst & 8) == 8 ? -1 : 1;
        return (int) (sign*((0x23281E140F0A0500 >> ((int)(pst & 7) * 8)) & 0xff) + (0x5A32201F0A >> i * 8 & 0xff) * 10);
    }
    int evaluateBoardForColor(bool whitePiece)
    {
        count++;
        //int boardValue = 0;
        int gamePhase = 0;
        int mgValue = 0;
        int egValue = 0;
        for (int i = 0; i < 6; i++)
        {
            ulong bitboard = board.GetPieceBitboard((PieceType) i + 1, whitePiece);
            for (int j = 0; j < 64; j++)
                if ((bitboard & 1UL << j) != 0)
                {
                    gamePhase += 0x042110 >> i * 4 & 0xF;
                    mgValue += getPSTValue(0, whitePiece, i, j);
                    egValue += getPSTValue(1, whitePiece, i, j);
                }
        }
        return (mgValue * gamePhase + egValue * (12 - gamePhase)) / 12;
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
    class MyMove //also tt entry
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