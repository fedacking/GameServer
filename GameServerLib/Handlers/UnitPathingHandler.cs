using LeagueSandbox.GameServer.Content.Navigation;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
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
		QuadTree<AttackableUnit> quadTreeAll;
		QuadTree<AttackableUnit> quadTreeStopped;
		HashSet<AttackableUnit> units = new HashSet<AttackableUnit>();

		public UnitPathingHandler(NavigationGrid navGrid)
		{
			quadTreeAll = new QuadTree<AttackableUnit>(
				navGrid.MinGridPosition.X, // MIN
				navGrid.MaxGridPosition.Z, // yep, MAX
				navGrid.MaxGridPosition.X - navGrid.MinGridPosition.X,
				navGrid.MaxGridPosition.Z - navGrid.MinGridPosition.Z
			);
			quadTreeStopped = new QuadTree<AttackableUnit>(
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

		public void AddUnit(AttackableUnit unit)
		{
			units.Add(unit);
			InsertUnit(unit);
		}

		public void InsertUnit(AttackableUnit unit)
		{
			if (!unit.Status.HasFlag(GameServerCore.Enums.StatusFlags.Ghosted))
			{
				quadTreeAll.Insert(unit, GetBounds(unit));
				if (!unit.IsMoving())
					quadTreeStopped.Insert(unit, GetBounds(unit));
			}
		}

		public void RemoveUnit(AttackableUnit unit)
		{
			units.Remove(unit);
		}

		public void Update()
		{
			quadTreeAll.Clear();
			quadTreeStopped.Clear();
			foreach(AttackableUnit unit in units)
			{
				InsertUnit(unit);
			}
		}

		public bool CheckPathing(GameObject obj)
		{
			if (quadTreeStopped.GetNodesInside(GetBounds(obj)).Any())
				return true;
			return false;
		}

		public bool CheckPathing(Vector2 position, float radius)
		{
			if (quadTreeStopped.GetNodesInside(GetBounds(position, radius)).Any())
				return true;
			return false;
		}

		public bool CheckCollision(GameObject obj)
		{
			if (quadTreeAll.GetNodesInside(GetBounds(obj)).Any())
				return true;
			return false;
		}

		public bool CheckCollision(Vector2 position, float radius)
		{
			if (quadTreeAll.GetNodesInside(GetBounds(position, radius)).Any())
				return true;
			return false;
		}
	}
}
