﻿using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
	internal class CaitlynEntrapment : IBuffGameScript
	{
		public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
		{
			BuffAddType = BuffAddType.REPLACE_EXISTING,
			IsHidden = true
		};

		public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

		public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
		{
			OverrideAnimation(unit, "Spell3b", "Run");
		}

		public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
		{
			ClearOverrideAnimation(unit, "Run");
		}
	}
}
