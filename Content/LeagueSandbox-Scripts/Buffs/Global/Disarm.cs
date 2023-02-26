using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
	internal class Disarm : IBuffGameScript
	{
		public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
		{
			BuffType = BuffType.DISARM,
			BuffAddType = BuffAddType.REPLACE_EXISTING
		};

		public StatsModifier StatsModifier { get; private set; }
	}
}

