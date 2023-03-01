﻿using LeagueSandbox.GameServer.Content.Navigation;
using LeagueSandbox.GameServer.GameObjects;
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
	public class TurretPathingHandler
	{
		QuadTree<LaneTurret> quadTree;

		public TurretPathingHandler(NavigationGrid navGrid)
		{
			quadTree = new QuadTree<LaneTurret>(
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

		public void InsertTurret(LaneTurret turret)
		{
			quadTree.Insert(turret, GetBounds(turret));
		}

		public bool CheckCollision(GameObject obj)
		{
			if (quadTree.GetNodesInside(GetBounds(obj)).Any())
				return true;
			return false;
		}

		public bool CheckCollision(Vector2 position, float radius)
		{
			if (quadTree.GetNodesInside(GetBounds(position, radius)).Any())
				return true;
			return false;
		}
	}
}
