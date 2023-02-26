using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
	internal class HealCheck : IBuffGameScript
	{
		public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
		{
			BuffType = BuffType.COMBAT_DEHANCER,
			BuffAddType = BuffAddType.REPLACE_EXISTING
		};

		public StatsModifier StatsModifier { get; private set; }
	}
}
