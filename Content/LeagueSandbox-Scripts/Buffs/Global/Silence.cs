using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
	internal class Silence : IBuffGameScript
	{
		public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
		{
			BuffType = BuffType.SILENCE,
			BuffAddType = BuffAddType.REPLACE_EXISTING,
			IsHidden = true
		};

		public StatsModifier StatsModifier { get; private set; }
	}
}
