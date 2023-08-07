using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot

{
    //r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1 -> -> (with quiencese)
    //Depth=9 move n°0 checks : 16 299 258 Nb positions saved : 3039928 -> Move: 'e2a6' = -47 -> 42.7
    //Depth=8 move n°0 checks : 3 392 607 Nb positions saved : 1327587 -> Move: 'e2a6' = 5 -> 17.2s (19 without init) -> 12.1
    //Depth=8 move n°0 checks : 3 396 320 Nb positions saved : 1329521 -> Move: 'e2a6' = 5 (with one killermove) -> 11.7
    //Depth=7 move n°0 checks : 1 257 329 Nb positions saved : 238708 -> Move: 'e2a6' = 27 -> 5.4 -> 3.1
    //Depth=6 move n°0 checks : 2 453 28 Nb positions saved : 77741 -> Move: 'e2a6' = 42 -> 1.3 (1.4 without init) -> 0.9 (with bitboard)
    //Depth=5 Nb move checks : 107 362 Nb positions saved : 6107 -> Move: 'd5e6' = 315 -> 0.4s
    //Depth=4 Nb move checks : 317 510 Nb positions saved : 68992 -> Move: 'd5e6' = 315 -> 1.4s
    //Depth=3 Nb move checks : 5 906 Nb positions saved : 1796 -> Move: 'e2a6' = 50 -> 0.1s
    //Depth=2 Nb move checks : 4 004 Nb positions saved : 138 -> Move: 'e2a6' = 390 -> 0.1s
    //Depth=1 Nb move checks : 166 Nb positions saved : 49 -> Move: 'e2a6' = 70 -> 0.1
    // 1- 1 -6 contre X, 2-2-12, 3-2-6, 2-4-8
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

    /* PST with simplified tb and king EG
      ulong[] bestPositions = {
        0xfc0cf0dc0cf0, 0xea0ae0ea0ae0, 0xda0ad0ea0ad0, 0xc90ad0f90ad0, 0xc90ad0f90ad0, 0xda0ad0ea0ad0, 0xea0ae0ea0ae0, 0xfc0cf0dc0cf0,
        0xda1ae7da1ae7, 0xc020c7e020c7, 0xa02007e02007, 0x2007f02007, 0x2007f02007, 0xa02007e02007, 0xc020c7e020c7, 0xda1ae7da1ae7,
        0xda9ad2da9ad2, 0xa00002e00002, 0x410124e10124, 0x510235f10235, 0x510235f10235, 0x410124e10124, 0xa00002e00002, 0xda9ad2da9ad2,
        0xd99ad1d99ad1, 0xa00111e00111, 0x510132e10132, 0x610244f10244, 0x610244f10244, 0x510132e10132, 0xa00111e00111, 0xd99ad1d99ad1,
        0xd09ad0c09ad0, 0xa00000d00000, 0x510230d10230, 0x610244e10244, 0x610244e10244, 0x510230d10230, 0xa00000d00000, 0xd99ad0c99ad0,
        0xda9ad1aa9ad1, 0xa10219c10219, 0x41022ac1022a, 0x510230c10230, 0x510230c10230, 0x41022ac1022a, 0xa00219c00219, 0xda9ad1aa9ad1,
        0xda9ae14a9ae1, 0xd001c24001c2, 0x10002010002, 0x1c00001c, 0x1c00001c, 0x2000002, 0xd001c24001c2, 0xda9ae14a9ae1,
        0xfc0cf04c0cf0, 0xda0ae05a0ae0, 0xda0ad02a0ad0, 0xd91ad0091ad0, 0xd91ad0091ad0, 0xda0ad02a0ad0, 0xda0ae05a0ae0, 0xfc0cf04c0cf0
        };*/

    /*PESTO reworked
     * ulong[] bestPositions = {
        0xfa3bf0fd5df0, 0xe42ce04061f0, 0xc44ab0355fd0, 0xc53ad0b27ef0, 0xa529d0f77d70, 0x342ad0d62ef0, 0x122bf00651b0, 0xb41cf0366af0,
        0xab2ad75c5df7, 0x3439a70e53e7, 0x3531d7c97c77, 0x362a07907b67, 0x3799a7ab7547, 0x641bd7977777, 0x4529c7e55415, 0x201bf7d76fba,
        0x2c10c7ab9bf9, 0x311ac74b4671, 0x421027015665, 0x371027b26675, 0x471007c53577, 0x6591a7176777, 0x6490c7477674, 0x3291e7c7306c,
        0xa119b5bdc9ab, 0x441214cda133, 0x443243ab1441, 0x560241db5774, 0x570340d04664, 0x560221d35672, 0x570123b0a143, 0x1600c3e0c04c,
        0xcc19c3fae9bd, 0x9511920dd310, 0x442339daa339, 0x471449ea0532, 0x559139f02553, 0x45923ae99241, 0x26a911d11242, 0xa4aac0f9c1ad,
        0xcb9ac1bbf0cd, 0x9d0991b0d3a9, 0x239209cab329, 0x410230f0b32a, 0x429320e91341, 0x33a199d00531, 0x12a9c0b39445, 0xa1bbcad1d2ba,
        0xdc9be30ee1de, 0xac9cc21ab3f0, 0x1d09a2a2c3ac, 0x3b0092f0a09c, 0x3ba103e2010b, 0x1caac0b32444, 0x9eabc02995b6, 0xbd9de920f0cc,
        0xfdacd0b0cdf0, 0xdd0af06cb9c0, 0xcc1cc02a0bf0, 0xae09b0f23cd0, 0xd99ac02b3bb0, 0xbdbbc0dd1ad0, 0xcc19f04deec0, 0xeecbf03fdcc0
    };*/

    ulong[] bestPositions = {
            0xfa3bf0dc0cf0, 0xe42ce0ea0ae0, 0xc44ab0ea0ad0, 0xc53ad0f90ad0, 0xa529d0f90ad0, 0x342ad0ea0ad0, 0x122bf0ea0ae0, 0xb41cf0dc0cf0,
            0xab2ad7da1ae7, 0x3439a7e020c7, 0x3531d7e02007, 0x362a07f02007, 0x3799a7f02007, 0x641bd7e02007, 0x4529c7e020c7, 0x201bf7da1ae7,
            0x2c10c7da9ad2, 0x311ac7e00002, 0x421027e10124, 0x371027f10235, 0x471007f10235, 0x6591a7e10124, 0x6490c7e00002, 0x3291e7da9ad2,
            0xa119b5d99ad1, 0x441214e00111, 0x443243e10132, 0x560241f10244, 0x570340f10244, 0x560221e10132, 0x570123e00111, 0x1600c3d99ad1,
            0xcc19c3c09ad0, 0x951192d00000, 0x442339d10230, 0x471449e10244, 0x559139e10244, 0x45923ad10230, 0x26a911d00000, 0xa4aac0c99ad0,
            0xcb9ac1aa9ad1, 0x9d0991c10219, 0x239209c1022a, 0x410230c10230, 0x429320c10230, 0x33a199c1022a, 0x12a9c0c00219, 0xa1bbcaaa9ad1,
            0xdc9be34a9ae1, 0xac9cc24001c2, 0x1d09a2010002, 0x3b009200001c, 0x3ba10300001c, 0x1caac0000002, 0x9eabc04001c2, 0xbd9de94a9ae1,
            0xfdacd04c0cf0, 0xdd0af05a0ae0, 0xcc1cc02a0ad0, 0xae09b0091ad0, 0xd99ac0091ad0, 0xbdbbc02a0ad0, 0xcc19f05a0ae0, 0xeecbf04c0cf0,
                };

    int[,,] mg_PST = new int[2, 6, 64];
    int[,,] eg_PST = new int[2, 6, 64];
    public MyBot()
    {
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                (mg_PST[0, i, j], mg_PST[1, i, j]) = (getPSTValue(0, true, i, j), getPSTValue(0, false, i, j));
                (eg_PST[0, i, j], eg_PST[1, i, j]) = (getPSTValue(1, true, i, j), getPSTValue(1, false, i, j));
            }
        }
    }

    Dictionary<ulong, MyMove> transposition = new Dictionary<ulong, MyMove>();
    int MAX_DEPTH = 6; //6 is ideal
    //int INF = 25000;
    int count;
    MyMove[] killerMoves; //keep only 1 instead of 2 because save time and token.
    Board board;
    public Move Think(Board _board, Timer timer)
    {
        board = _board;
        killerMoves = new MyMove[6000];//Max ply
        count = 0;
        if (timer.MillisecondsRemaining < 10000) //If 10s left -> aggressif quick mode
            MAX_DEPTH = 6;
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
            bestMoveValue = color * evaluateBoard();
            if (bestMoveValue > beta) return bestMoveValue;
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
                if (!currentMove.move.IsCapture)// && !currentMove.Equals(killerMoves[board.PlyCount, 0]))
                {
                    //killerMoves[board.PlyCount, 1] = killerMoves[board.PlyCount, 0];
                    killerMoves[board.PlyCount] = currentMove;
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
        else bestMove.flag = (bestMoveValue <= alphaOrigin) ? 1 : 2; //upperbound : exact
        bestMove.value = bestMoveValue;
        bestMove.depth = depth;
        transposition.TryAdd(board.ZobristKey, bestMove);
        return bestMoveValue;
    }
    //int[] gamephaseInc = { 0, 1, 1, 2, 4, 0 };// -> 0x042110

    int getPSTValue(int phase, bool whitePiece, int i, int j)
    {
        ulong pst = (bestPositions[whitePiece ? 63 - j : j] >> (4 * i + 24 * phase)) & 0xF;
        int sign = (pst & 8) == 8 ? -1 : 1;
        return (whitePiece ? 1 : -1) * (int)(
            sign * ((0x23281E140F0A0500 >> ((int)(pst & 7) * 8)) & 0xff) //pst value
            + (0x5A32201F0A >> i * 8 & 0xff) * 10); //piece value
    }
    void evaluateForColor(bool whitePiece, int w)
    {
        //int w = whitePiece ? 0 : 1;
        for (int i = 0; i < 6; i++)
        {
            ulong bitboard = board.GetPieceBitboard((PieceType)i + 1, whitePiece);
            while (bitboard != 0)
            {
                gamePhase += 0x042110 >> i * 4 & 0xF; //gamephaseInc[i];
                int j = BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard);
                mgValue += mg_PST[w, i, j];
                egValue += eg_PST[w, i, j];
            }
        }
    }
    int gamePhase, mgValue, egValue;
    int evaluateBoard()
    {
        count++;
        (gamePhase, mgValue, egValue) = (0,0,0);
        evaluateForColor(true, 0);
        evaluateForColor(false, 1);
        return (mgValue * gamePhase + egValue * (24 - gamePhase)) / 24;
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
            if (ttMove != null && move.move.Equals(ttMove.move)) //TT ordering
                value = MVA_OFFSET + 60;
            else if (move.move.IsCapture) //Capture ordering
                value = MVA_OFFSET + (0xABCDEF0 >> (int)move.move.MovePieceType * 4 & 0xF) + 10 * (int)move.move.CapturePieceType; //MVV_LVA
            else //Killer move ordering
                //for (int n = 0; n < 2; n++)
                if (killerMoves[board.PlyCount] != null && move.move.Equals(killerMoves[board.PlyCount].move))
                    value = MVA_OFFSET - 10; //10 = killerValue
                
            move.sortScore = value;
        }
    }
    class MyMove //also tt entry
    {
        public Move move;
        public int sortScore;
        public int value;
        //0 -> alpha
        //1 -> beta
        //2 -> exact
        public int flag;
        public int depth;
        public MyMove(Move _move) => move = _move;
    }
}