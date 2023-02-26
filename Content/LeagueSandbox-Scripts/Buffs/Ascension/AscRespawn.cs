using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
	internal class AscRespawn : IBuffGameScript
	{
		public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
		{
			BuffType = BuffType.INTERNAL,
			BuffAddType = BuffAddType.REPLACE_EXISTING
		};
		public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

		public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
		{
			if (unit is ObjAIBase obj && obj.Inventory != null)
			{
				AddBuff("AscTrinketStartingCD", 0.3f, 1, null, unit, obj);
				ApiEventManager.OnResurrect.AddListener(this, obj, OnRespawn, false);
			}
		}

		public void OnRespawn(ObjAIBase owner)
		{
			owner.Spells[6 + (byte)SpellSlotType.InventorySlots].SetCooldown(0, true);
		}
	}
}