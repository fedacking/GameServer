﻿using LeagueSandbox.GameServer.Content.Navigation;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using Roy_T.AStar.Primitives;
using System;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServerLib.Handlers
{
	public class UnitPathingHandler
	{
		QuadTree<AttackableUnit> quadTree;

		public UnitPathingHandler(NavigationGrid navGrid)
		{
			quadTree = new QuadTree<AttackableUnit>(
				navGrid.MinGridPosition.X, // MIN
				navGrid.MaxGridPosition.Z, // yep, MAX
				navGrid.MaxGridPosition.X - navGrid.MinGridPosition.X,
				navGrid.MaxGridPosition.Z - navGrid.MinGridPosition.Z
			);
		}

		Circle GetBounds(GameObject obj)
		{
			return new Circle(obj.Position, Math.Max(0.5f, obj.PathfindingRadius));
		}
		Circle GetBounds(Vector2 position, float radius)
		{
			return new Circle(position, Math.Max(0.5f, radius));
		}

		private Shape GetBounds(Vector2 orig, Vector2 dest, float radius)
		{
			return new Stadium(orig, dest, MathF.Max(0.5f, radius));
		}

		public void AddUnit(AttackableUnit unit)
		{
			InsertUnit(unit);
		}

		public bool ShouldCollide(AttackableUnit unit, AttackableUnit other, bool checkMoving)
		{
			if (other.Status.HasFlag(GameServerCore.Enums.StatusFlags.Ghosted)) return false;
			if (unit != null && unit.Status.HasFlag(GameServerCore.Enums.StatusFlags.Ghosted) && !(other is LaneTurret)) return false;
			if (checkMoving && !other.IsMoving()) return false;
			return true;
		}

		public void InsertUnit(AttackableUnit unit)
		{
			quadTree.Insert(unit, GetBounds(unit));
		}

		public void RemoveUnit(AttackableUnit unit)
		{
			quadTree.Remove(unit);
		}

		public bool CheckPathing(Vector2 position, float radius, AttackableUnit unit=null)
		{
			if (quadTree.GetNodesInside(GetBounds(position, radius)).Any(e => ShouldCollide(unit, e, true)))
				return true;
			return false;
		}

		public bool CheckPathing(Vector2 orig, Vector2 dest, float radius, AttackableUnit unit = null)
		{
			if (quadTree.GetNodesInside(GetBounds(orig, dest, radius)).Any(e => ShouldCollide(unit, e, true)))
				return true;
			return false;
		}

		public bool CheckCollision(Vector2 position, float radius, AttackableUnit unit = null)
		{
			if (quadTree.GetNodesInside(GetBounds(position, radius)).Any(e => ShouldCollide(unit, e, false)))
				return true;
			return false;
		}
	}
}
