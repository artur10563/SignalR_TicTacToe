using Microsoft.AspNetCore.SignalR;
using System.Numerics;
using TicTacToe.Enums;
using TicTacToe.Models;

namespace TicTacToe.Hubs
{
	public class TicTacToeHub : Hub
	{
		private static readonly List<Player> _players = new List<Player>();
		private static readonly List<Game> _games = new List<Game>();

		/// <summary>
		/// Removes a player from the list of active players. If the player was in an active game, the other player is notified about the disconnection.
		/// </summary>
		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var player = _players.FirstOrDefault(p => p.Id == Context.ConnectionId);
			if (player == null) return;

			var game = GetGameByPlayerId(player.Id);

			//If player left mid-game => surrender
			if (game != null)
			{
				var opponent =
					player.Id == game.PlayerOne.Id
					? game.PlayerTwo
					: game.PlayerOne;

				if (game.Status == GameStatus.InProgress)
					await Clients.Client(opponent.Id).SendAsync("Surrender", new PlayerDTO(opponent));
				_games.Remove(game);
			}

			_players.Remove(player);
			await Clients.All.SendAsync("RemoveFromList", player.Id);
			await base.OnDisconnectedAsync(exception);
		}

		public async Task OnSearch(string name)
		{
			if (name.Length == 0 || name.Length > 50) return;

			var player = GetPlayerById(Context.ConnectionId);

			if (player == null)
			{
				player = new Player { Id = Context.ConnectionId, Name = name };
				_players.Add(player);
			}
			player.Name = name;
			player.IsSearching = true;

			var opps = _players.Where(p => p.IsSearching).Select(p => new PlayerDTO(p));
			await Clients.All.SendAsync("Search", opps);
		}

		public async Task InviteToPlay(string invitedId)
		{
			var inviter = GetPlayerById(Context.ConnectionId);
			if (inviter == null) return;

			await Clients.Client(invitedId).SendAsync("ReceiveInvitation", new PlayerDTO(inviter));
		}


		//Create a game instance, show field to both players, remove them from searching list
		public async Task AcceptInvitation(string inviterId)
		{
			var inviter = GetPlayerById(inviterId);
			var acceptor = GetPlayerById(Context.ConnectionId);

			if (inviter == null || acceptor == null
				|| inviter.Id == acceptor.Id
				|| !inviter.IsSearching || !acceptor.IsSearching) return;

			_games.Add(new Game(inviter, acceptor));

			await Clients.Client(inviter.Id).SendAsync("InvitationAccepted", inviter.IsReadyToMark, (int)inviter.Marker);
			await Clients.Client(acceptor.Id).SendAsync("InvitationAccepted", acceptor.IsReadyToMark, (int)acceptor.Marker);
			await Clients.All.SendAsync("RemoveFromList", inviter.Id, acceptor.Id);
		}

		public async Task DeclineInvitation(string inviterId)
		{
			await Clients.Client(inviterId).SendAsync("InvitationDeclined");
		}


		public async Task MakeMark(int index)
		{
			//If player modified data-index
			if (index >= Game.TotalMoves) return;

			var player = GetPlayerById(Context.ConnectionId);
			if (player == null) return;
			if (!player.IsReadyToMark) return;

			var game = GetGameByPlayerId(player.Id);
			if (game == null) return;


			int row = index / Game.Rows;
			int col = index % Game.Cols;


			GameStatus status = game.MakeMark(player.Marker, row, col, out Player? winner);

			await Clients.Client(game.PlayerOne.Id).SendAsync("Mark", index, player.Marker, game.PlayerOne.IsReadyToMark);
			await Clients.Client(game.PlayerTwo.Id).SendAsync("Mark", index, player.Marker, game.PlayerTwo.IsReadyToMark);


			if (status == GameStatus.Finished)
			{
				PlayerDTO? playerDTO = null;
				if (winner != null)
				{
					playerDTO = new PlayerDTO(winner);
				}

				await Clients.Client(game.PlayerOne.Id).SendAsync("GameEnd", playerDTO);
				await Clients.Client(game.PlayerTwo.Id).SendAsync("GameEnd", playerDTO);
			}
		}

		public async Task LeaveGame()
		{
			var player = GetPlayerById(Context.ConnectionId);
			if (player == null) return;

			var game = GetGameByPlayerId(player.Id);
			if (game == null) return; //Game won`t exist if other player allready left

			await Clients.Client(player.Opponent.Id).SendAsync("OpponentLeft");

			_games.Remove(game);

		}



		#region private methods
		private static Player? GetPlayerById(string? id)
		{
			return
				_players.FirstOrDefault(p => p.Id == id);
		}

		private static Game? GetGameByPlayerId(string? playerId)
		{
			return
				_games.FirstOrDefault(g => g.PlayerOne.Id == playerId || g.PlayerTwo.Id == playerId);
		}

		#endregion
	}
}
