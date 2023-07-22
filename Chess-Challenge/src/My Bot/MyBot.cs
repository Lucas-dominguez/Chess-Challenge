#define DEBUG_TIMER
using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
	//                     .  P    K    B    R    Q    K
	int[] kPieceValues = { 0, 100, 300, 310, 500, 900, 10000 };
	int kBigNum = 9999999;
	int kMassiveNum = 99999999;

#if DEBUG_TIMER
	int dNumMovesMade = 0;
	int dTotalMsElapsed = 0;
#endif

	bool mIsWhite;
	int mTeamMult;
	Timer mTimer;

	public Move Think(Board board, Timer timer)
	{
		Move[] legalMoves = board.GetLegalMoves();
		mIsWhite = board.IsWhiteToMove;
		mTeamMult = mIsWhite ? 1 : -1;
		mTimer = timer;

		Move bestMove = legalMoves.MaxBy(move =>
		{
			board.MakeMove(move);
			int eval = mTeamMult * EvaluateBoardMinimax(board, 4, -kMassiveNum, kMassiveNum, !mIsWhite);
			board.UndoMove(move);
			return eval;
		});

#if DEBUG_TIMER
		dNumMovesMade++;
		dTotalMsElapsed += timer.MillisecondsElapsedThisTurn;
		Console.WriteLine("My bot time average: {0}", (float)dTotalMsElapsed / dNumMovesMade);
#endif
		return bestMove;
	}

	int EvaluateBoardMinimax(Board board, int depth, int alpha, int beta, bool maximise)
	{
		Move[] legalMoves;

		if (board.IsInCheckmate())
			return board.IsWhiteToMove ? -kBigNum : kBigNum;

		if (board.IsDraw())
			return 0;

		if (depth == 0 || (legalMoves = board.GetLegalMoves()).Length == 0)
			return EvaluateBoard(board);

		if(maximise)
		{
			int maxEval = -kBigNum;
			foreach(Move move in legalMoves)
			{
				board.MakeMove(move);
				int evaluation = EvaluateBoardMinimax(board, depth - 1, alpha, beta, !maximise);
				board.UndoMove(move);
				maxEval = Math.Max(maxEval, evaluation);
				alpha = Math.Max(alpha, evaluation);
				if(beta <= alpha)
				{
					break;
				}
			}
			return maxEval;
		}

		int minEval = kBigNum;
		foreach (Move move in legalMoves)
		{
			board.MakeMove(move);
			int evaluation = EvaluateBoardMinimax(board, depth - 1, alpha, beta, !maximise);
			board.UndoMove(move);
			minEval = Math.Min(minEval, evaluation);
			beta = Math.Min(beta, evaluation);
			if (beta <= alpha)
			{
				break;
			}
		}

		return minEval;
	}

	int EvaluateBoard(Board board)
	{
		int sum = 0;

		for (int i = 0; ++i < 7;)
			sum += (board.GetPieceList((PieceType)i, true).Count - board.GetPieceList((PieceType)i, false).Count) * kPieceValues[i];

		return sum;
	}
}