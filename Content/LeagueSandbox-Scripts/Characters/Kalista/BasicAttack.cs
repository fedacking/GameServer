using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
	public class KalistaBasicAttack : ISpellScript
	{
		public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
		{
			MissileParameters = new MissileParameters
			{
				Type = MissileType.Target
			}
			// TODO
		};
	}
}

