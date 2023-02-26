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
	internal class RocketGrab : IBuffGameScript
	{
		public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
		{
			BuffAddType = BuffAddType.REPLACE_EXISTING
		};

		public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

		Particle grab;

		public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
		{
			grab = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "FistReturn_mis", ownerSpell.CastInfo.Owner, buff.Duration, 1, "head", "R_hand");
		}

		public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
		{
			RemoveParticle(grab);
		}
	}
}

