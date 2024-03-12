namespace TicTacToe.Models
{
	public class PlayerDTO
	{
		public string Id { get; set; }
		public string Name { get; set; }

		public PlayerDTO() { }
		public PlayerDTO(Player player)
		{
			Id = player.Id;
			Name = player.Name;
		}
	}
}
