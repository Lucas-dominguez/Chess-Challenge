using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    public class EvilBot : IChessBot


    {
        int MAX_DEPTH = 4; //6 is ideal
                           //best = 537 token -> 853 -> 939
        int[] positionValues = { -50, -40, -30, -20, -10, -5, 0, 5, 10, 15, 20, 25, 30, 40, 50 };
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
                return 0;//-24000; //stalemate
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

        /*
         //611 tokens
        int[] positionValues = { -50, -40, -30, -20, -10, -5, 0, 5, 10, 15, 20, 25, 30, 40, 50 };
        int[] pieceValues = { 100, 300, 300, 500, 900, 9000 };

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

        // Piece values: pawn, knight, bishop, rook, queen, king

        int MAX_DEPTH = 3; //4 is ideal, 3 is fast
        bool isPlayWhite;
        Dictionary<ulong, double> transposition = new Dictionary<ulong, double>();
        double boardValue = 0, MIN = double.MinValue, MAX = double.MaxValue;
        int count;
        // {} = 1, () = 1
        public Move Think(Board board, Timer timer)
        {
            if (timer.MillisecondsRemaining < 10000) //If 10s left -> aggressif quick mode
                MAX_DEPTH = 3;
            isPlayWhite = board.IsWhiteToMove;
            count = 0;
            return getBestMoveScore(board, MAX_DEPTH, true);
        }

        Move getBestMoveScore(Board board, int depth, bool isMaxPlayer)
        {
            //count = 0;
            double bestScore = MIN; //~ double.Min
            Move bestMove = Move.NullMove;
            //Console.WriteLine("\n==========My turn=========\n");
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                double score = miniMax(depth - 1, board, MIN, MAX, !isMaxPlayer);
                board.UndoMove(move);
                if (score >= bestScore)
                {
                    //Console.WriteLine("New best move : " + move + " = " + score);
                    bestScore = score;
                    bestMove = move;
                }
            }
            //Console.WriteLine("Nb move checks : " + count + " Nb positions saved : " + transposition.Count);
            return bestMove;

        }

        double applyMinimaxOnmoves(int depth, Board board, bool isMaxPlayer, double alpha, double beta)
        {
            double bestMove = isMaxPlayer ? MIN : MAX;
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                double tmp = miniMax(depth - 1, board, alpha, beta, !isMaxPlayer);
                if (isMaxPlayer)
                {
                    bestMove = Math.Max(bestMove, tmp);
                    alpha = Math.Max(alpha, bestMove);
                }
                else
                {
                    bestMove = Math.Min(bestMove, tmp);
                    beta = Math.Min(beta, bestMove);
                }
                board.UndoMove(move);
                if (beta <= alpha) break;
            }
            return bestMove;
        }

        double miniMax(int depth, Board board, double alpha, double beta, bool isMaxPlayer)
        {
            if (depth == 0) return (isPlayWhite ? 1 : -1) * evaluateBoard(board);
            return applyMinimaxOnmoves(depth, board, isMaxPlayer, alpha, beta);
        }

        void evaluateBoardForColor(Board board, bool whitePiece)
        {
            count++;
            for (int i = 0; i < 6; i++)
            {
                ulong bitboard = board.GetPieceBitboard((PieceType)i + 1, whitePiece);
                for (int j = 0; j < 64; j++)
                    if ((bitboard & (1UL << j)) != 0)
                        boardValue += (positionValues[(bestPositions[(whitePiece ? (63 - j) : j)] >> i * 4) & 0xF] + pieceValues[i]) * (whitePiece ? 1 : -1);
            }
        }

        double evaluateBoard(Board board)
        {
            boardValue = 0;
            if (!transposition.TryGetValue(board.ZobristKey, out boardValue))
            {
                evaluateBoardForColor(board, true);
                evaluateBoardForColor(board, false);
                if (board.IsDraw()) boardValue = MIN;
                else if (board.IsInCheckmate()) boardValue = MAX;
                transposition.Add(board.ZobristKey, boardValue);
            }
            return boardValue;
        }*/


        //283 tokens
        /*
        Random rng = new Random();
        // Piece values: null, pawn, knight, bishop, rook, queen
        double[] pieceValues = { 0, 100, 300, 300, 500, 900 };
        // none, pawn, knight, bishop, rook, queen, king
        double[] movePiecesIncentive = { 0, 6.01, 6, 6, 3, 6, 1 };
        int MAX_DEPTH = 1; //DO NOT CHANGE 1 is best
        // {} = 1, () = 1
        public Move Think(Board board, Timer timer)
        {            
            return getBestMoveScore(board, MAX_DEPTH).Key;
        }

        double evaluateMove(Move move, Board board, int depth)
        {
            double score = rng.NextDouble() //random seed
                    + toInt(move.IsCastles) * 302
                    + toInt(move.IsEnPassant) * 2
                    + pieceValues[(int)move.PromotionPieceType]         //always choose Q with 900
                    + movePiecesIncentive[(int)move.MovePieceType]
                    + pieceValues[(int)move.CapturePieceType]
                    - pieceValues[toInt(board.SquareIsAttackedByOpponent(move.TargetSquare)) * (int)move.MovePieceType];
            board.MakeMove(move);
            score += toInt(board.IsInCheckmate()) * 1000000000
                    + toInt(board.IsInCheck()) * 10
                    - toInt(board.IsDraw()) * 10000;
            if (depth != 0)
            {
                KeyValuePair<Move, double> opponentScore = getBestMoveScore(board, depth - 1);
                score += opponentScore.Value * Math.Pow(-1, depth);

            }
            board.UndoMove(move);
            //Console.WriteLine(score);
            return score;
        }

        KeyValuePair<Move, double> getBestMoveScore(Board board, int depth)
        {
            Dictionary<Move, double> scoreByMove = new Dictionary<Move, double>();
            //Console.WriteLine("DEPTH = " + depth);
            foreach (Move move in board.GetLegalMoves())
            {
                //Console.WriteLine("Checking for move " + move);
                scoreByMove.Add(move, evaluateMove(move, board, depth));
                //Console.WriteLine("Score is " + scoreByMove[move]);
            }
            return scoreByMove.FirstOrDefault(x => x.Value == scoreByMove.Values.Max());
        }


        int toInt(bool b) => b ? 1 : 0;
        



        ////////////////////////////////////////////////////////////////////////////////////////
        /*public Move Think(Board board, Timer timer)
        {
            Move[] moves = board.GetLegalMoves();
            return moves[(new Random()).Next(moves.Length)];
        }*/

        ///////////////////////////////////////////////////////////////////////////////////////


        /*
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();

            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
            int highestValueCapture = 0;

            foreach (Move move in allMoves)
            {
                // Always play checkmate in one
                if (MoveIsCheckmate(board, move))
                {
                    moveToPlay = move;
                    break;
                }

                // Find highest value capture
                Piece capturedPiece = board.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                if (capturedPieceValue > highestValueCapture)
                {
                    moveToPlay = move;
                    highestValueCapture = capturedPieceValue;
                }
            }

            return moveToPlay;
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }*/



    }
}