using TicTacToe.Enums;

namespace TicTacToe.Models
{

	public class Game
	{
		public const int Rows = 3;
		public const int Cols = 3;
		public const int TotalMoves = Rows * Cols;

		public string Id { get; private set; }
		private int[,] Field { get; set; } = new int[Rows, Cols];
		public GameStatus Status { get; private set; } = GameStatus.InProgress;
		public int MovesLeft { get; private set; } = TotalMoves;

		public Player PlayerOne { get; private set; }
		public Player PlayerTwo { get; private set; }

		public Game(Player playerOne, Player playerTwo)
		{
			if (playerOne == playerTwo) throw new ArgumentException("You can't play with yourself.", nameof(playerTwo));

			Id = Guid.NewGuid().ToString();

			PlayerOne = playerOne;
			PlayerTwo = playerTwo;

			PlayerOne.Marker = Marker.Cross;
			PlayerTwo.Marker = Marker.Circle;

			PlayerOne.Opponent = PlayerTwo;
			PlayerTwo.Opponent = PlayerOne;

			PlayerOne.IsReadyToMark = true;
			PlayerTwo.IsReadyToMark = false;

			PlayerOne.IsSearching = PlayerTwo.IsSearching = false;
		}

		/// <summary>
		/// Marks the specified cell on the game board with the provided marker for the given player.
		/// </summary>
		/// <param name="marker">The marker (Cross or Circle) to place on the board.</param>
		/// <param name="row">The row index of the cell to mark.</param>
		/// <param name="col">The column index of the cell to mark.</param>
		/// <param name="winner">An out parameter that returns the winning player, if any, after making the mark.</param>
		/// <returns>The current status of the game after making the mark.</returns>
		/// <exception cref="Exception">Thrown when the specified cell is already assigned.</exception>
		public GameStatus MakeMark(Marker marker, int row, int col, out Player? winner)
		{
			if (Field[row, col] != 0) throw new Exception("Cell is allready assigned");
			if (MovesLeft <= 0 || Status == GameStatus.Finished) throw new Exception("Game is over");

			MovesLeft--;

			Field[row, col] = (int)marker;

			PlayerOne.IsReadyToMark = !PlayerOne.IsReadyToMark;
			PlayerTwo.IsReadyToMark = !PlayerTwo.IsReadyToMark;

			return UpdateGameStatus(out winner);
		}


		#region Win conditions
		private GameStatus UpdateGameStatus(out Player? winner)
		{
			winner = null;
			Marker? winMarker = CheckRowWin() ?? CheckColWin() ?? CheckDiagonalWin();

			if (winMarker.HasValue)
			{
				winner =
					winMarker == PlayerOne.Marker ? PlayerOne : PlayerTwo;
			}
			if (winMarker.HasValue || MovesLeft == 0)
			{
				Status = GameStatus.Finished;
			}

			return Status;
		}

		private Marker? CheckRowWin()
		{
			for (int i = 0; i < Rows; i++)
			{
				var sum = Field[i, 0] + Field[i, 1] + Field[i, 2];

				if (Math.Abs(sum) == 3)
					return (Marker)Field[i, 0];
			}
			return null;
		}

		private Marker? CheckColWin()
		{
			for (int i = 0; i < Cols; i++)
			{
				var sum = Field[0, i] + Field[1, i] + Field[2, i];

				if (Math.Abs(sum) == 3)
					return (Marker)Field[0, i];
			}
			return null;
		}

		private Marker? CheckDiagonalWin()
		{
			//top-left to bottom-right
			var mainDiagonalSum = Field[0, 0] + Field[1, 1] + Field[2, 2];

			//top-right to bottom-left
			var secondaryDiagonalSum = Field[0, 2] + Field[1, 1] + Field[2, 0];

			if (Math.Abs(mainDiagonalSum) == 3)
				return (Marker)Field[0, 0];
			if (Math.Abs(secondaryDiagonalSum) == 3)
				return (Marker)Field[0, 2];

			return null;
		}
		#endregion
	}
}
