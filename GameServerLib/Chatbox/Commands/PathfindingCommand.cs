using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Chatbox;
using LeagueSandbox.GameServer.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerLib.Chatbox.Commands
{
	internal class PathfindingCommand : ChatCommandBase
	{
		Game _game;
		public PathfindingCommand(ChatCommandManager chatCommandManager, Game game) : base(chatCommandManager, game)
		{
			_game = game;
		}

		public override string Command => "pathfinding";

		public override string Syntax => $"{Command}";

		public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
		{
			ApiMapFunctionManager.LogPathfinding(_game.PlayerManager.GetPeerInfo(userId).Champion);
		}
	}
}
