﻿using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace CharScripts
{
	internal class CharScriptAscRelic : ICharScript
	{
		public void OnActivate(ObjAIBase owner, Spell spell = null)
		{
			AddBuff("AscRelicBombBuff", 25000.0f, 1, null, owner, owner);
		}
	}
}
