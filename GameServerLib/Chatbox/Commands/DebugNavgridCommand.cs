using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Chatbox;

namespace GameServerLib.Chatbox.Commands
{
	internal class DebugNavgridCommand : ChatCommandBase
	{
		private Game _game;

		public DebugNavgridCommand(ChatCommandManager chatCommandManager, Game game) : base(chatCommandManager, game)
		{
			_game = game;
		}

		public override string Command => "debugnavgrid";
		public override string Syntax => $"{Command}";

		public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
		{
			_game.Map.NavigationGrid.DebugmodeLogNavgrid();
		}
	}
}
