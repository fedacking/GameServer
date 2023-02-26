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
	internal class AscWarpReappear : IBuffGameScript
	{
		public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
		{
			BuffType = BuffType.INTERNAL,
			BuffAddType = BuffAddType.REPLACE_EXISTING,
		};

		public StatsModifier StatsModifier { get; private set; }

		public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
		{
			AddParticleTarget(unit, unit, "Global_Asc_Teleport_reappear", unit, buff.Duration);
		}
	}
}