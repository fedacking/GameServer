using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace CharScripts
{
	internal class CharScriptAscWarpIcon : ICharScript
	{
		public void OnActivate(ObjAIBase owner, Spell spell = null)
		{
			SetStatus(owner, StatusFlags.Targetable, false);
			SetStatus(owner, StatusFlags.Stunned, true);
			SetStatus(owner, StatusFlags.IgnoreCallForHelp, true);
			SetStatus(owner, StatusFlags.Ghosted, true);
			SetStatus(owner, StatusFlags.Invulnerable, true);
			SetStatus(owner, StatusFlags.CanMoveEver, false);
		}
	}
}
