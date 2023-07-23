using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;

public class OtherBot1 : IChessBot
{
	int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

	double materialWeight = 1.0,
		mobilityWeight = 0.5,
		kingSafetyWeight = 0.3,
		capturingWeight = 0.6,
		distanceWeight = 0.4,
		repetitionWeight = 0.5;


	Queue<Move> previousMoves = new Queue<Move>();
	Dictionary<ulong, (int score, Move bestMove, int depth)> transpositionTable = new Dictionary<ulong, (int score, Move bestMove, int depth)>();
	public void AddMove(Move move)
	{
		previousMoves.Enqueue(move);
		if (previousMoves.Count > 2) previousMoves.Dequeue();
	}

	public Move Think(Board board, Timer timer)
	{
		int maxDepth = 500;

		Move bestMove = Move.NullMove;
		int bestScore = int.MinValue;

		List<Move> orderedMoves = OrderMoves(board.GetLegalMoves());

		for (int depth = 1; depth <= maxDepth; depth++)
		{
			foreach (Move move in orderedMoves)
			{
				if (board.IsInCheckmate())
				{
					AddMove(move);
					return move;
				}

				double weight = board.GetAllPieceLists().Sum(pl => pl.Count) <= 12 ? kingSafetyWeight : (board.GetAllPieceLists().Sum(pl => pl.Count) <= 24 ? mobilityWeight : materialWeight);

				int score;
				board.MakeMove(move);
				if (transpositionTable.ContainsKey(board.ZobristKey))
				{
					score = transpositionTable[board.ZobristKey].score;
				}
				else
				{
					score = (int)(EvaluatePiece(board, board.GetPiece(move.TargetSquare)) * weight) + NegaMax(board, maxDepth, int.MinValue, int.MinValue).Item2; // Use minimax for deeper evaluations
				}
				board.UndoMove(move);

				if (score > bestScore)
				{
					bestScore = score;
					bestMove = move;
				}
			}
		}
		AddMove(bestMove);
		if (!orderedMoves.Contains(bestMove))
		{
			bestMove = orderedMoves[Random.Shared.Next(orderedMoves.Count)];
		}
		return bestMove;
	}
	private (Move, int) NegaMax(Board board, int depth, int alpha, int beta)
	{
		if (transpositionTable.ContainsKey(board.ZobristKey))
		{
			var entry = transpositionTable[board.ZobristKey];
			if (entry.depth >= depth)
			{
				return (entry.bestMove, entry.score);
			}
		}
		if (depth == 0 || board.IsInCheckmate())
		{
			int score = 0;

			foreach (Piece piece in GetPieces(board))
			{
				score += EvaluatePiece(board, piece);
			}

			return (Move.NullMove, score);
		}

		Move bestMove = Move.NullMove;
		int bestScore = int.MinValue;
		List<Move> moves = OrderMoves(board.GetLegalMoves());

		foreach (Move move in moves)
		{
			board.MakeMove(move);
			int score = -NegaMax(board, depth - 1, -beta, -alpha).Item2; // Negate score and swap alpha and beta
			board.UndoMove(move);

			if (score > bestScore)
			{
				bestScore = score;
				bestMove = move;
			}

			alpha = Math.Max(alpha, score); // Update alpha with max value

			if (alpha >= beta)
				break;
		}

		transpositionTable[board.ZobristKey] = (bestScore, bestMove, depth);

		return (bestMove, bestScore);
	}

	private List<Move> OrderMoves(Move[] moves)
	{
		List<Move> orderedMoves = new List<Move>();
		foreach (Move move in moves)
		{
			if (move.IsCapture)
			{
				orderedMoves.Insert(0, move);
			}
			else
			{
				orderedMoves.Add(move);
			}
		}

		return orderedMoves;
	}

	private int EvaluatePiece(Board board, Piece piece)
	{
		int materialScore = 0;
		int mobilityScore = 0;
		int kingSafetyScore = 0;
		int capturingScore = 0;
		int distanceScore = 0;
		int repetitionScore = 0;

		int pieceValue = pieceValues[(int)piece.PieceType];
		materialScore += GetPieceCount(board, piece.PieceType, true) * pieceValue;
		materialScore -= GetPieceCount(board, piece.PieceType, false) * pieceValue;

		foreach (Move move in board.GetLegalMoves())
		{
			if (move.StartSquare == piece.Square)
			{
				mobilityScore += (piece.IsWhite ? 1 : -1) * pieceValue;
			}
		}

		if (board.IsInCheck())
		{
			kingSafetyScore += board.IsWhiteToMove ? -1 : 1;
		}

		foreach (Move move in board.GetLegalMoves())
		{
			if (move.IsCapture && move.StartSquare == piece.Square)
			{
				Piece capturedPiece = board.GetPiece(move.TargetSquare);
				int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];
				capturingScore += capturedPieceValue;
			}
		}

		foreach (Move move in board.GetLegalMoves())
		{
			if (move.StartSquare == piece.Square)
			{
				int distance = Math.Abs(move.StartSquare.File - move.TargetSquare.File) + Math.Abs(move.StartSquare.Rank - move.TargetSquare.Rank);
				distanceScore += distance;
			}
		}

		foreach (Move previousMove in previousMoves)
		{
			if (previousMove.StartSquare == piece.Square)
			{
				repetitionScore++;
			}
		}

		int finalScore = (int)(materialWeight * materialScore +
							  mobilityWeight * mobilityScore +
							  kingSafetyWeight * kingSafetyScore +
							  capturingWeight * capturingScore +
							  distanceWeight * distanceScore +
							  repetitionWeight * repetitionScore);

		return finalScore;
	}

	private int GetPieceCount(Board board, PieceType pieceType, bool isWhite)
	{
		int count = 0;
		foreach (var pieceList in board.GetAllPieceLists())
		{
			foreach (var piece in pieceList)
			{
				if (piece.IsWhite == isWhite && piece.PieceType == pieceType)
				{
					count++;
				}
			}
		}
		return count;
	}

	private IEnumerable<Piece> GetPieces(Board board)
	{
		return board.GetAllPieceLists()
					.SelectMany(pieceList => pieceList)
					.Where(piece => true);
	}
}