using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
	internal class TT_RelicHeal : IBuffGameScript
	{
		public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
		{
			BuffType = BuffType.HEAL,
			BuffAddType = BuffAddType.REPLACE_EXISTING,
			IsHidden = true
		};

		public BuffType BuffType => BuffType.HEAL;
		public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
		public int MaxStacks => 1;
		public bool IsHidden => true;

		public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

		public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
		{
			unit.Stats.CurrentHealth += 94 + 13 * (unit.Stats.Level - 1);
			unit.Stats.CurrentMana += 90 + 14 * (unit.Stats.Level - 1);
			AddParticleTarget(unit, unit, "odin_healthpackheal", unit);
			AddParticleTarget(unit, unit, "summoner_mana", unit);
		}
	}
}

