using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
	//                     .  P    K    B    R    Q    K
	int[] kPieceValues = { 0, 100, 300, 310, 500, 900, 10000 };
	int kBigNum = 9999999;

	bool mIsWhite;

	public Move Think(Board board, Timer timer)
	{
		Move[] legalMoves = board.GetLegalMoves();
		mIsWhite = board.IsWhiteToMove;

		return legalMoves.MaxBy(x => EvaluateMoveMinMax(board, timer, x));
	}

	int EvaluateMoveMinMax(Board board, Timer timer, Move move, int depth = 4)
	{
		bool isUs = board.IsWhiteToMove == mIsWhite;
		int finalScore;

		int moveBonus = (move.IsCapture ? 5 : -5) - (int)move.MovePieceType + timer.MillisecondsElapsedThisTurn % 10;
		board.MakeMove(move);
		depth--;

		if (depth == 0)
			finalScore = Evaluate(board);
		else
		{
			Move[] legalMoves = board.GetLegalMoves();
			if (legalMoves.Length == 0)
				finalScore = Evaluate(board);
			else
			{
				finalScore = isUs ? kBigNum : -kBigNum;
				foreach (Move candidateMove in legalMoves)
				{
					int eval = EvaluateMoveMinMax(board, timer, candidateMove, depth);
					if (eval > finalScore != isUs)
						finalScore = eval;
				}
			}
		}
		board.UndoMove(move);

		return finalScore + moveBonus;
	}


	int Evaluate(Board board)
	{
		int sum = 0;

		if(board.IsInCheckmate())
			return mIsWhite == board.IsWhiteToMove ? -kBigNum : kBigNum;

		if (board.IsDraw())
			return 0;

		for (int i = 0; ++i < 7;)
			sum += (board.GetPieceList((PieceType)i, mIsWhite).Count - board.GetPieceList((PieceType)i, !mIsWhite).Count) * kPieceValues[i];

		return sum;
	}
}