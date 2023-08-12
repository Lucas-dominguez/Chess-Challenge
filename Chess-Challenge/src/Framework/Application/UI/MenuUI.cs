using Raylib_cs;
using System.Numerics;
using System;
using System.IO;

namespace ChessChallenge.Application
{
    public static class MenuUI
    {
        public static void DrawButtons(ChallengeController controller)
        {
			Vector2 buttonPos = UIHelper.Scale(new Vector2(140, 70));
			Vector2 buttonSize = UIHelper.Scale(new Vector2(240, 55));
            float spacing = buttonSize.Y * 1.2f;
            float breakSpacing = spacing * 0.6f;

			// Game Buttons
			if (NextButtonInRow("Human vs MyBot", ref buttonPos, spacing, buttonSize))
            {
                var whiteType = controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.MyBot : ChallengeController.PlayerType.Human;
                var blackType = !controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.MyBot : ChallengeController.PlayerType.Human;
                controller.StartNewGame(whiteType, blackType);
            }
			if (NextButtonInRow("MyBot vs MyBot", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.MyBot);
            }
			if (NextButtonInRow("MyBot vs MyBotV1", ref buttonPos, spacing, buttonSize))
			{
				controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.MyBotV1);
			}
			if (NextButtonInRow("MyBot vs MyBotV2", ref buttonPos, spacing, buttonSize))
			{
				controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.MyBotV2);
			}
			if (NextButtonInRow("MyBot vs MyBotV3", ref buttonPos, spacing, buttonSize))
			{
				controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.MyBotV3);
			}
			buttonPos = UIHelper.Scale(new Vector2(400, 70));
			if (NextButtonInRow("MyBot vs Stockfish", ref buttonPos, spacing, buttonSize))
			{
				controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.Stockfish);
			}
			if (NextButtonInRow("MyBot vs OB1", ref buttonPos, spacing, buttonSize))
			{
				controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.OtherBot1);
			}
			if (NextButtonInRow("MyBot vs OB2", ref buttonPos, spacing, buttonSize))
			{
				controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.OtherBot2);
			}
			if (NextButtonInRow("MyBot vs OB3", ref buttonPos, spacing, buttonSize))
			{
				controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.OtherBot3);
			}
			if (NextButtonInRow("MyBot vs MBasic", ref buttonPos, spacing, buttonSize))
			{
				controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.NegamaxBasic);
			}

			// Page buttons
			buttonPos.Y += breakSpacing;
            buttonPos.X = UIHelper.Scale(new Vector2(260, 70)).X;

			if (NextButtonInRow("Save Games", ref buttonPos, spacing, buttonSize))
            {
                string pgns = controller.AllPGNs;
                string directoryPath = Path.Combine(FileHelper.AppDataPath, "Games");
                Directory.CreateDirectory(directoryPath);
                string fileName = FileHelper.GetUniqueFileName(directoryPath, "games", ".txt");
                string fullPath = Path.Combine(directoryPath, fileName);
                File.WriteAllText(fullPath, pgns);
                ConsoleHelper.Log("Saved games to " + fullPath, false, ConsoleColor.Blue);
            }
            if (NextButtonInRow("Rules & Help", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
            }
            if (NextButtonInRow("Documentation", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
            }
            if (NextButtonInRow("Submission Page", ref buttonPos, spacing, buttonSize))
            {
                FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");
            }

            // Window and quit buttons
            buttonPos.Y += breakSpacing;

            bool isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
            string windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
            if (NextButtonInRow(windowButtonName, ref buttonPos, spacing, buttonSize))
            {
                Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
            }
            if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacing, buttonSize))
            {
                Environment.Exit(0);
            }

            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size)
            {
                bool pressed = UIHelper.Button(name, pos, size);
                pos.Y += spacingY;
                return pressed;
            }
        }
    }
}