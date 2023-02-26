using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace CharScripts
{
	internal class CharScriptOdinSpeedShrine : ICharScript
	{
		public void OnActivate(ObjAIBase owner, Spell spell = null)
		{
			SetStatus(owner, StatusFlags.Rooted, true);
			SetStatus(owner, StatusFlags.CanMove, false);
			SetStatus(owner, StatusFlags.Ghosted, true);
			SetStatus(owner, StatusFlags.Targetable, false);
			SetStatus(owner, StatusFlags.SuppressCallForHelp, true);
			SetStatus(owner, StatusFlags.IgnoreCallForHelp, true);
			SetStatus(owner, StatusFlags.ForceRenderParticles, true);

			AddBuff("OdinSpeedShrineAura", 25000.0f, 1, null, owner, owner, false);
		}
	}
}
