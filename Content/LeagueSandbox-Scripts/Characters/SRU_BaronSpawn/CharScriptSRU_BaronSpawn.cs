using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace CharScripts
{
	internal class CharScriptSRU_BaronSpawn : ICharScript
	{
		public void OnActivate(ObjAIBase owner, Spell spell = null)
		{
			SetStatus(owner, StatusFlags.CanMove, false);
			SetStatus(owner, StatusFlags.Ghosted, true);
			SetStatus(owner, StatusFlags.Targetable, false);
			SetStatus(owner, StatusFlags.SuppressCallForHelp, true);
			SetStatus(owner, StatusFlags.IgnoreCallForHelp, true);
			SetStatus(owner, StatusFlags.CanAttack, false);
		}
	}
}
