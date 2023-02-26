using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Players;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
	public class HandleScoreboard : PacketHandlerBase<ScoreboardRequest>
	{
		private readonly PlayerManager _playerManager;
		private static ILog _logger = LoggerProvider.GetLogger();
		private readonly Game _game;

		public HandleScoreboard(Game game)
		{
			_game = game;
			_playerManager = game.PlayerManager;
		}

		public override bool HandlePacket(int userId, ScoreboardRequest req)
		{
			_logger.Debug($"Player {_playerManager.GetPeerInfo(userId).Name} has looked at the scoreboard.");
			// Send to that player stats packet
			var champion = _playerManager.GetPeerInfo(userId).Champion;
			_game.PacketNotifier.NotifyS2C_HeroStats(champion);
			return true;
		}
	}
}
