using ChessChallenge.API;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class OtherBot1 : IChessBot
{
	private readonly int[] _pieceValues = { 0, 100, 300, 300, 500, 1000, 1073741823 };
	private readonly Dictionary<ulong, int> _transpositionTable = new Dictionary<ulong, int>();
	private bool _side;
	public Move Think(Board board, Timer timer)
	{
		List<Move> moves = OrderMoves(board, board.GetLegalMoves());
		if (board.PlyCount <= 1 && moves.Count == 20)
		{
			_side = board.IsWhiteToMove;
		}
		int maxEval = -int.MaxValue;
		Move maxMove = Move.NullMove;
		int currDepth = 1;
		int time = timer.MillisecondsRemaining / 50;
		while (true)
		{
			foreach (Move move in moves)
			{
				board.MakeMove(move);
				int currEval = -NegaMax(board, currDepth, int.MinValue, int.MaxValue);
				if (maxEval < currEval)
				{
					maxEval = currEval;
					maxMove = move;
				}
				board.UndoMove(move);
			}

			if (timer.MillisecondsElapsedThisTurn > time)
			{
				if (maxMove != Move.NullMove)
				{
					return maxMove;
				}
				Random rng = new();
				return moves[rng.Next(moves.Count)];
			}
			currDepth++;
		}

	}
	List<Move> OrderMoves(Board board, Move[] moves)
	{
		List<Move> orderedMoves = new List<Move>();
		foreach (Move move in moves)
		{
			int seeValue = See(board, move);
			if (seeValue > 0)
			{
				orderedMoves.Insert(0, move); // good capture
			}
			else if (seeValue == 0)
			{
				orderedMoves.Insert(orderedMoves.Count / 2, move); // equal capture
			}
			else
			{
				orderedMoves.Add(move); // bad capture or quiet move
			}
		}
		return orderedMoves;
	}
	int EvaluateBoard(Board board)
	{
		if (board.IsDraw())
		{
			return 0;
		}

		if (board.IsInCheckmate())
		{
			return int.MaxValue * (_side ? 1 : -1);
		}
		PieceList[] pieceList = board.GetAllPieceLists();
		int material = 0;
		foreach (PieceList list in pieceList)
		{
			material += _pieceValues[(int)list.TypeOfPieceInList] * list.Count * (list.IsWhitePieceList ? 1 : -1);
		}
		return ((material) * 10 + board.GetLegalMoves().Length) * (_side ? 1 : -1);
	}
	int NegaMax(Board board, int depth, int alpha, int beta)
	{
		if (depth == 0)
		{
			if (_transpositionTable.ContainsKey(board.ZobristKey))
			{
				return _transpositionTable[board.ZobristKey];
			}
			int eval = EvaluateBoard(board);
			_transpositionTable.Add(board.ZobristKey, eval);
			return eval;
		}
		Move[] moves = board.GetLegalMoves();
		List<Move> orderedMoves = OrderMoves(board, moves);
		foreach (Move move in orderedMoves)
		{
			board.MakeMove(move);
			int currEval = -NegaMax(board, depth - 1, -beta, -alpha);
			board.UndoMove(move);
			if (currEval >= beta)
			{
				return beta;
			}
			alpha = Math.Max(alpha, currEval);
		}
		return alpha;
	}

	int See(Board board, Move move)
	{
		if (!move.IsCapture) return 0;
		int captureValue = _pieceValues[(int)board.GetPiece(move.TargetSquare).PieceType];
		int pieceValue = _pieceValues[(int)board.GetPiece(move.StartSquare).PieceType];
		return pieceValue - captureValue;
	}

}
