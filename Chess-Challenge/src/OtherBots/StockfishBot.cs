using System;
using Stockfish;
using ChessChallenge.API;
using Stockfish.NET;

public class StockfishBot : IChessBot
{
	const int STOCKFISH_LEVEL = 3;

	IStockfish mStockFish;

	public StockfishBot()
	{
		
	}
	
	public Move Think(Board board, Timer timer)
	{

	}
}