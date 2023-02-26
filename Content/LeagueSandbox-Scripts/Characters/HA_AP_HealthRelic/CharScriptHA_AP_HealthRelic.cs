﻿using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace CharScripts
{
	internal class CharScriptHA_AP_HealthRelic : ICharScript
	{
		public void OnActivate(ObjAIBase owner, Spell spell = null)
		{
			SetStatus(owner, StatusFlags.CanMove, false);
			SetStatus(owner, StatusFlags.Ghosted, true);
			SetStatus(owner, StatusFlags.SuppressCallForHelp, true);
			SetStatus(owner, StatusFlags.IgnoreCallForHelp, true);
			SetStatus(owner, StatusFlags.ForceRenderParticles, true);

			AddBuff("HowlingAbyssRelicAura", 25000.0f, 1, null, owner, owner, false);
		}
	}
}
