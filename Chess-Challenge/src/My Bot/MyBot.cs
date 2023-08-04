//#define DEBUG_TIMER
//#define DEBUG_TREE_SEARCH




using ChessChallenge.API;
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;





/// <summary>
/// Main bot class that does the thinking.
/// </summary>
public class MyBot : IChessBot
{
	#region rTypes

	/// <summary>
	/// Transposition table entry. Stores best move and evaluation for a board.
	/// </summary>
	struct TEntry
	{
		public ulong mKey;
		public Move mBestMove;
		public int mDepth, mEval, mEvalType;
		public TEntry(ulong key, Move move, int depth, int eval, int evalType)
		{
			mKey = key;
			mBestMove = move;
			mDepth = depth;
			mEval = eval;
			mEvalType = evalType;
		}
	}

	#endregion rTypes





	#region rConstants

	UInt64[] kCompPTables = {
		 0x7878787878787878, 0x7091D3A9BDA4D9BF, 0x6A8AA0A78E8B7D74, 0x67848189877C816E, 0x667F7C8481747765, 0x6F907A7A71757565, 0x6893896D676A775F, 0x7878787878787878,
		 0x2B6D32A4555F3800, 0x6C7DA58992AC5A43, 0x98ADD5B5A793A356, 0x8885AA939E868472, 0x7287868C81847B6F, 0x6C8A84867F817267, 0x6A6E8577766F5263, 0x676A646C604E692C,
		 0x727D5A665D3D7B63, 0x5685A38E6F6B8465, 0x77939C919597936C, 0x777D93939C867C75, 0x7B7F81918B818174, 0x7F858B8283838378, 0x7990877D7884837B, 0x695C6F6F696E7660,
		 0x978E7EA59D8F968F, 0x988BA8B2A5A28F8B, 0x84A49884928B8674, 0x6A7291898B7D7067, 0x677C737E776F655E, 0x6074787A6C6C6658, 0x45748077726A6C58, 0x655D7D8484796F6A,
		 0x989798A3818D7864, 0x9F8CA16C79745C67, 0xA19AA08D7E7D6C6F, 0x797784776C6C6565, 0x767A757771726572, 0x7C8279747770796E, 0x7976837E7980725F, 0x5462666D7F726B77,
		 0x81795F506D848949, 0x635D7572736A778D, 0x68887C6A6C798972, 0x5E6E6662656F6A6C, 0x536058575C657755, 0x656D625857686E6E, 0x7E7E6C594A727D79, 0x8289647E5181926D,
		 0x7878787878787878, 0xFFEFD7E2D9EAF5F8, 0xB5B39EA0A8B5C0BC, 0x84847B777C81898F, 0x777A727373767E81, 0x7277747879747D7B, 0x737978817F7E7E81, 0x7878787878787878,
		 0x314B6562646F5D4E, 0x5367667277667266, 0x5A6A72777E7F6A67, 0x6B7E808888887A6C, 0x6B7B84848A84746B, 0x686A767F83777667, 0x58676A7774716A5A, 0x4A546B686D675363,
		 0x676C72737270696E, 0x6E756F766F7D7572, 0x7B787C7777787279, 0x797A7F827E817E76, 0x72767F7D86817A74, 0x6D737A817F7E766F, 0x656D727B77736B6E, 0x6C746C7274677267,
		 0x7C7E818183857F81, 0x7A7E7A7680818180, 0x7674767B7C7D7D7D, 0x7977797979817A7B, 0x707274747B7E7C7A, 0x6C726F7377747875, 0x7670727279787474, 0x6A7B6F74777A7972,
		 0x867F868B8B888872, 0x788E8AA2968F866C, 0x7E86919A9B7E7C6A, 0x92A195A19889887A, 0x8994918E9A868C6B, 0x7C7F847E7C83656C, 0x615E676C6C626768, 0x5A6A617459686460,
		 0x6C7B83706B6B5F43, 0x808993848482846F, 0x819898868389847F, 0x7A8B908B8B898872, 0x707E898B8987756B, 0x727D84898780766A, 0x6C747B82817B7065, 0x59676E6470695F52
	};


	//                     P    N    B    R    Q    K
	int[] kPieceValues = { 100, 300, 310, 500, 900,   0,
						   110, 270, 290, 550,1000,   0 };
	int[] kPiecePhase = {    0,   1,   1,   2,   4,   0 };

	int kMassiveNum = 99999999;
	const int kTTSize = 8333329;

	#endregion rConstants





	#region rDebug

#if DEBUG_TIMER
	int dNumMovesMade = 0;
	int dTotalMsElapsed = 0;
#endif
#if DEBUG_TREE_SEARCH
	int dNumPositionsEvaluated;
#endif

	#endregion rDebug





	#region rMembers

	Board mBoard;
	Move mBestMove;
	TEntry[] mTranspositionTable = new TEntry[kTTSize];
	uint[][] mPTables = new uint[12][];

	#endregion rMembers





	#region rInitialise

	/// <summary>
	/// Create bot
	/// </summary>
	public MyBot()
	{
		for (int i = 0; i < 12; ++i)
		{
			mPTables[i] = new uint[64];
			for (int j = 0; j < 8; ++j)
			{
				UInt64 val = kCompPTables[j];
				for (int k = 0; k < 8; ++k)
				{
					mPTables[i][j * 8 + k] = (uint)((val >> 8 * k) & 0xFF);
				}
			}
		}
	}

	#endregion rInitialise





	#region rThinking

	/// <summary>
	/// Top level thinking function.
	/// </summary>
	public Move Think(Board board, Timer timer)
	{
		mBoard = board;

#if DEBUG_TREE_SEARCH
		dNumPositionsEvaluated = 0;
#endif
		int msRemain = timer.MillisecondsRemaining;
		if (msRemain < 200)
			return mBoard.GetLegalMoves()[0];
		int depth = 1;
		while (timer.MillisecondsElapsedThisTurn < (msRemain / 200))
			EvaluateBoardNegaMax(++depth, -kMassiveNum, kMassiveNum, true);
		

#if DEBUG_TIMER
		dNumMovesMade++;
		dTotalMsElapsed += timer.MillisecondsElapsedThisTurn;
		Console.WriteLine("My bot time average: {0}", (float)dTotalMsElapsed / dNumMovesMade);
#endif
#if DEBUG_TREE_SEARCH
		int msElapsed = timer.MillisecondsElapsedThisTurn;
		Console.WriteLine("Num positions evaluated {0} in {1}ms | Depth {2}", dNumPositionsEvaluated, msElapsed, mDepth);
#endif
		return mBestMove;
	}





	/// <summary>
	/// Recursive search of given board position.
	/// </summary>
	int EvaluateBoardNegaMax(int depth, int alpha, int beta, bool top, int totalExtension = 0)
	{
		ulong boardKey = mBoard.ZobristKey;
		Move[] legalMoves = mBoard.GetLegalMoves();
		float alphaOrig = alpha;
		Move move, bestMove = Move.NullMove;
		int recordEval = int.MinValue;

		// Check for definite evaluations.
		if (mBoard.IsRepeatedPosition() || mBoard.IsInsufficientMaterial() || mBoard.FiftyMoveCounter >= 100)
			return 0;

		if (legalMoves.Length == 0)
			return mBoard.IsInCheck() ? -depth-9999999 : 0;

		// Search transposition table for this board.
		TEntry entry = mTranspositionTable[boardKey % kTTSize];
		if(entry.mKey == boardKey && entry.mDepth >= depth)
		{
			if (entry.mEvalType == 0) return entry.mEval; // Exact
			else if (entry.mEvalType == 1) alpha = Math.Max(alpha, entry.mEval); // Lower bound
			else if (entry.mEvalType == 2) beta = Math.Min(beta, entry.mEval); // Upper bound
			if (alpha >= beta) return entry.mEval;
		}

		// Heuristic evaluation
		if (depth <= 0)
		{
#if DEBUG_TREE_SEARCH
			dNumPositionsEvaluated++;
#endif
			recordEval = (mBoard.IsWhiteToMove ? 1 : -1) * (EvalColor(true) - EvalColor(false));
			if (recordEval >= beta || depth <= -4) return recordEval;
			alpha = Math.Max(alpha, recordEval);
		}

		// Sort Moves
		int[] moveScores = new int[legalMoves.Length];
		for (int i = 0; i < legalMoves.Length; ++i)
		{
			move = legalMoves[i];
			moveScores[i] = -(move == entry.mBestMove ? 1000000 :
									move.IsCapture ? 100 * (int)move.CapturePieceType - (int)move.MovePieceType : 
									move.IsPromotion ? (int)move.PromotionPieceType : 0);
		}
		Array.Sort(moveScores, legalMoves);

		// Tree search
		for (int i = 0; i < legalMoves.Length; ++i)
		{
			move = legalMoves[i];
			if (depth <= 0 && !move.IsCapture) continue; // Only search captures in qsearch
			mBoard.MakeMove(move);
			int extension = totalExtension < 6 && mBoard.IsInCheck() ? 1 : 0;
			int evaluation = -EvaluateBoardNegaMax(depth - 1 + extension, -beta, -alpha, false, totalExtension + extension);
			mBoard.UndoMove(move);

			if (recordEval < evaluation)
			{
				recordEval = evaluation;
				bestMove = move;
				if(top)
					mBestMove = move;
			}
			alpha = Math.Max(alpha, recordEval);
			if (alpha >= beta) break;
		}

		// Store in transposition table
		mTranspositionTable[boardKey % kTTSize] = new TEntry(boardKey, bestMove, depth, recordEval, 
			recordEval <= alphaOrig ? 2 :
			recordEval >= beta ? 1 :
			0);

		return recordEval;
	}

	/// <summary>
	/// Evaluate the board for a given color.
	/// </summary>
	int EvalColor(bool isWhite)
	{
		UInt64[] PTable = isWhite ? kWhitePTables : kBlackPTables;

		int phase = 0;
		int sumMg = 0;
		int sumEg = 0;

		for(int i = 0; i < 6; ++i)
		{
			ulong pieceBitBoard = mBoard.GetPieceBitboard((PieceType)(i+1), isWhite);
			int popcount = BitOperations.PopCount(pieceBitBoard);
			phase += kPiecePhase[i] * popcount;
			sumMg += (kPieceValues[i] - 121) * popcount;
			sumEg += (kPieceValues[i + 6] - 121) * popcount;
			for (int b = 0; b < 8; ++b)
			{
				sumMg += BitOperations.PopCount(pieceBitBoard & PTable[i * 8 + b]) * (1 << b);
				sumEg += BitOperations.PopCount(pieceBitBoard & PTable[i * 8 + b + 48]) * (1 << b);
			}
		}

		return sumMg * phase + sumEg * (12-phase);
	}

	#endregion rThinking
}