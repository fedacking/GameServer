using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
	internal class InfernalGuardian : IBuffGameScript
	{
		public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
		{
			BuffType = BuffType.INTERNAL
		};

		public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

		Buff thisBuff;
		public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
		{
			thisBuff = buff;
			ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, true);
		}

		public void OnDeath(DeathData data)
		{
			thisBuff.DeactivateBuff();
		}

		public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
		{
			SetSpell(buff.SourceUnit, "InfernalGuardian", SpellSlotType.SpellSlots, 3);
		}
	}
}