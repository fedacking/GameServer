using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static GameServerCore.Content.HashFunctions;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Spells
{
	public class EzrealMysticShot : ISpellScript
	{
		public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
		{
			TriggersSpellCasts = true,
			IsDamagingSpell = true
		};

		private ObjAIBase _owner;
		private Spell _spell;
		private float _bonusAd = 0;

		public void OnActivate(ObjAIBase owner, Spell spell)
		{
			_owner = owner;
			_spell = spell;
			ApiEventManager.OnUpdateStats.AddListener(this, owner, OnStatsUpdate, false);
		}

		public void OnSpellCast(Spell spell)
		{
			var owner = spell.CastInfo.Owner;
			AddParticleTarget(owner, owner, "ezreal_bow", owner, bone: "L_HAND");
		}

		public void OnSpellPostCast(Spell spell)
		{
			var owner = spell.CastInfo.Owner as Champion;
			var targetPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
			var distance = Vector2.Distance(owner.Position, targetPos);
			FaceDirection(targetPos, owner);

			if (distance > 1200.0)
			{
				targetPos = GetPointFromUnit(owner, 1150.0f);
			}

			if (owner.SkinID == 5)
			{
				SpellCast(owner, 3, SpellSlotType.ExtraSlots, targetPos, targetPos, false, Vector2.Zero);
			}
			else
			{
				SpellCast(owner, 0, SpellSlotType.ExtraSlots, targetPos, targetPos, false, Vector2.Zero);
			}
		}

		private void OnStatsUpdate(AttackableUnit _unit, float diff)
		{
			float bonusAd = _owner.Stats.AttackDamage.Total * _spell.SpellData.AttackDamageCoefficient;
			if (_bonusAd != bonusAd)
			{
				_bonusAd = bonusAd;
				SetSpellToolTipVar(_owner, 2, bonusAd, SpellbookType.SPELLBOOK_CHAMPION, 0, SpellSlotType.SpellSlots);
			}
		}
	}

	public class EzrealMysticShotPulseMissile : EzrealMysticShotMissile
	{
	}

	public class EzrealMysticShotMissile : ISpellScript
	{
		public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
		{
			MissileParameters = new MissileParameters
			{
				Type = MissileType.Circle
			},
			IsDamagingSpell = true
			// TODO
		};

		//Vector2 direction;

		public void OnActivate(ObjAIBase owner, Spell spell)
		{
			ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
		}

		public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
		{
			var owner = spell.CastInfo.Owner;
			var ad = owner.Stats.AttackDamage.Total * spell.SpellData.AttackDamageCoefficient;
			var ap = owner.Stats.AbilityPower.Total * spell.SpellData.MagicDamageCoefficient;
			var damage = 15 + spell.CastInfo.SpellLevel * 20 + ad + ap;

			IEventSource source; // The hash of the current script name does not match the replays.
								 //                      But this is not a problem as long as the parent skill name hash matches.
								 //IEventSource source = new EventSource(spell.ScriptNameHash, HashString("EzrealMysticShot"));
			if (owner.SkinID == 5)
			{
				source = new AbilityInfo(266740993, HashString("EzrealMysticShot"));
			}
			else
			{
				source = new AbilityInfo(3693728257, HashString("EzrealMysticShot"));
			}
			target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false, source);

			for (byte i = 0; i < 4; i++)
			{
				owner.Spells[i].LowerCooldown(1);
			}

			AddParticleTarget(owner, target, "Ezreal_mysticshot_tar", target);
			missile.SetToRemove();

			// SpellBuffAdd EzrealRisingSpellForce
		}
	}
}
