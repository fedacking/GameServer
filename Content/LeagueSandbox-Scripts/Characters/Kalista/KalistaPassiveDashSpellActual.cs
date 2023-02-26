using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
	public class KalistaPassiveDashSpellActual : ISpellScript
	{
		public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
		{
			NotSingleTargetSpell = true,
			DoesntBreakShields = true,
			TriggersSpellCasts = true,
			CastingBreaksStealth = true,
			IsDamagingSpell = true
		};
	}
}

