using TicTacToe.Enums;

namespace TicTacToe.Models
{
	public class Player
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public bool IsSearching { get; set; }
		public bool IsReadyToMark { get; set; }
		public Marker Marker { get; set; }
		public Player Opponent { get; set; }
	}
}
