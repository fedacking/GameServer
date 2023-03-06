using System;
using System.Collections.Generic;
using System.Numerics;
using GameMaths;
using System.Runtime.CompilerServices;
using GameServerCore;
using LeagueSandbox.GameServer.Content.Navigation;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using System.Linq;
using LeagueSandbox.GameServer.Handlers;
using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using System.IO;
using GameServerLib.Handlers;
using System.Activities.Presentation.View;

namespace LeagueSandbox.GameServer.Handlers
{
	/// <summary>
	/// Class which calls path based functions for GameObjects.
	/// </summary>
	public class PathingHandler
	{
		private static ILog _logger = LoggerProvider.GetLogger();
		private MapScriptHandler _map;
		private NavigationGrid navGrid;
		public UnitPathingHandler unitPathing;

		public PathingHandler(MapScriptHandler map)
		{
			_map = map;
			navGrid = _map.NavigationGrid;
			unitPathing = new UnitPathingHandler(navGrid);
		}

		/// <summary>
		/// Updates pathing for the specified object.
		/// </summary>
		/// <param name="obj">GameObject to check for incorrect paths.</param>
		public void UpdatePaths(AttackableUnit obj)
		{
			var path = obj.Waypoints;
			if (path.Count == 0) return;

			var lastWaypoint = path[path.Count - 1];
			if (obj.CurrentWaypoint.Equals(lastWaypoint) && lastWaypoint.Equals(obj.Position)) return;

			var newPath = new List<Vector2> { obj.Position };
			foreach (Vector2 waypoint in path)
			{
				if (IsPathable(waypoint, obj.PathfindingRadius, obj))
					newPath.Add(waypoint);
				else
					break;
			}

			obj.SetWaypoints(newPath);
		}

		/// <summary>
		/// Checks if the given position can be pathed on. We check if there isn't a unit stopeed there
		/// </summary>
		public bool IsPathable(Vector2 pos, float radius, AttackableUnit unit)
		{
			bool pathable = navGrid.IsWalkable(pos, radius);

			if (pathable)
				pathable = !unitPathing.CheckPathing(pos, radius, unit);

			return pathable;
		}

		/// <summary>
		/// Checks if the given position can be moved into. We check if there's a unit there
		/// </summary>
		public bool IsOpen(Vector2 pos, float radius, AttackableUnit unit)
		{
			bool pathable = navGrid.IsWalkable(pos, radius);

			if (pathable)
				pathable = !unitPathing.CheckCollision(pos, radius, unit);

			return pathable;
		}

		/// <summary>
		/// Gets the closest pathable position to the given position. *NOTE*: Computationally heavy, use sparingly.
		/// </summary>
		/// <param name="location">Vector2 position to start the check at.</param>
		/// <param name="distanceThreshold">Amount of distance away from terrain the exit should be.</param>
		/// <returns>Vector2 position which can be pathed on.</returns>
		public Vector2 GetClosestTerrainExit(Vector2 location, AttackableUnit unit, float distanceThreshold = 0)
		{
			double angle = Math.PI / 4;

			// x = r * cos(angle)
			// y = r * sin(angle)
			// r = distance from center
			// Draws spirals until it finds a walkable spot
			for (int r = 1; !IsPathable(location, distanceThreshold, unit); r++)
			{
				location.X += r * (float)Math.Cos(angle);
				location.Y += r * (float)Math.Sin(angle);
				angle += Math.PI / 4;
			}

			return location;
		}

		/// <summary>
		/// Checks if a stadium shape is closed for pathfinding.
		/// </summary>
		private bool CastCirclePathable(Vector2 orig, Vector2 dest, float radius, AttackableUnit unit)
		{
			if(unitPathing.CheckPathing(orig, dest, radius, unit)) return true;
			return false;
		}

		/// <summary>
		/// Checks if a stadium shape is closed for pathfinding.
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="dest"></param>
		/// <param name="radius"></param>
		/// <param name="translate"></param> Do we translate the coordinates to navgrid cells
		/// <returns></returns>
		public bool CastCircle(Vector2 orig, Vector2 dest, float radius, AttackableUnit unit)
		{
			if (CastCirclePathable(orig, dest, radius, unit)) return true;
			orig = navGrid.TranslateToNavGrid(orig);
			dest = navGrid.TranslateToNavGrid(dest);

			float tradius = radius / navGrid.CellSize;
			Vector2 p = (dest - orig).Normalized().Perpendicular() * tradius;

			var cells = navGrid.GetAllCellsInRange(orig, radius, false)
			.Concat(navGrid.GetAllCellsInRange(dest, radius, false))
			.Concat(navGrid.GetAllCellsInLine(orig + p, dest + p))
			.Concat(navGrid.GetAllCellsInLine(orig - p, dest - p));

			foreach (var cell in cells)
			{
				if (!navGrid.IsWalkable(cell))
					return true;
			}

			return false;
		}
		/// <summary>
		/// Remove waypoints (cells) that have a path from one to the other from path.
		/// </summary>
		/// <param name="path"></param>
		public void SmoothPath(List<Vector2> path, AttackableUnit traveler, float checkDistance = 0f)
		{
			if (path.Count < 3)
			{
				return;
			}
			int j = 0;
			// The first point remains untouched.
			for (int i = 2; i < path.Count; i++)
			{
				// If there is something between the last added point and the current one
				if (CastCircle(path[j], path[i], checkDistance, traveler))
				{
					// add previous.
					path[++j] = path[i - 1];
				}
			}
			// Add last.
			path[++j] = path[path.Count - 1];
			j++; // Remove everything after.
			path.RemoveRange(j, path.Count - j);
		}

		/// <summary>
		/// Returns a path to the given target position from the given unit's position.
		/// </summary>
		public List<Vector2> GetPath(AttackableUnit obj, Vector2 target, bool usePathingRadius = true)
		{
			if (usePathingRadius)
			{
				return GetPath(obj.Position, target, obj, obj.PathfindingRadius);
			}
			return GetPath(obj.Position, target, obj, 0);
		}

		/// <summary>
		/// Finds a path of waypoints, which are aligned by the cells of the navgrid (A* method), that lead to a set destination.
		/// </summary>
		/// <param name="from">Point that the path starts at.</param>
		/// <param name="to">Point that the path ends at.</param>
		/// <param name="distanceThreshold">Amount of distance away from terrain that the path should be.</param>
		/// <returns>List of points forming a path in order: from -> to</returns>
		public List<Vector2> GetPath(Vector2 from, Vector2 to, AttackableUnit traveler, float distanceThreshold = 0)
		{
			//_logger.Debug($"unit {traveler}, {traveler.NetId}, {traveler.CharData} is asking for a path");
			if (from == to)
			{
				_logger.Debug("Effed start vs to");
				return null;
			}


			var fromNav = navGrid.TranslateToNavGrid(from);
			var cellFrom = navGrid.GetCell(fromNav, false);
			//var goal = GetClosestWalkableCell(to, distanceThreshold, true);
			to = GetClosestTerrainExit(to, traveler, distanceThreshold);
			var toNav = navGrid.TranslateToNavGrid(to);
			var cellTo = navGrid.GetCell(toNav, false);

			if (cellFrom == null || cellTo == null)
			{
				//_logger.Debug("We didn't find cellFrom cellT");
				return null;
			}
			if (cellFrom.ID == cellTo.ID)
			{
				//_logger.Debug("Start Cell and end Cell are the same");
				return new List<Vector2>(2) { from, to };
			}

			// A size large enough not to relocate the array while playing Summoner's Rift
			var priorityQueue = new PriorityQueue<(List<NavigationGridCell>, float), float>(1024);

			var start = new List<NavigationGridCell>(1);
			start.Add(cellFrom);
			priorityQueue.Enqueue((start, 0), Vector2.Distance(fromNav, toNav));

			var closedList = new HashSet<int>();
			closedList.Add(cellFrom.ID);

			List<NavigationGridCell> path = null;

			// while there are still paths to explore
			while (true)
			{
				if (!priorityQueue.TryDequeue(out var element, out _))
				{
					// no solution
					//_logger.Debug("No A* solution");
					return null;
				}

				float currentCost = element.Item2;
				path = element.Item1;

				NavigationGridCell cell = path[path.Count - 1];

				// found the min solution and return it (path)
				if (cell.ID == cellTo.ID)
				{
					break;
				}

				foreach (NavigationGridCell neighborCell in navGrid.GetCellNeighbors(cell))
				{
					// if the neighbor is in the closed list - skip
					if (closedList.Contains(neighborCell.ID))
					{
						continue;
					}

					Vector2 neighborCellCoord = toNav;
					// The target point is always walkable,
					// we made sure of this at the beginning of the function
					if (neighborCell.ID != cellTo.ID)
					{
						neighborCellCoord = navGrid.TranslateFromNavGrid(neighborCell.GetCenter());

						Vector2 cellCoord = navGrid.TranslateFromNavGrid(fromNav);
						if (cell.ID != cellFrom.ID)
							cellCoord = navGrid.TranslateFromNavGrid(cell.GetCenter());

						// close cell if not walkable or circle LOS check fails (start cell skipped as it always fails)
						if(CastCircle(cellCoord, neighborCellCoord, distanceThreshold, traveler))
						{
							closedList.Add(neighborCell.ID);
							continue;
						}
					}

					// calculate the new path and cost +heuristic and add to the priority queue
					var npath = new List<NavigationGridCell>(path.Count + 1);
					foreach (var pathCell in path)
					{
						npath.Add(pathCell);
					}
					npath.Add(neighborCell);

					// add 1 for every cell used
					float cost = currentCost + 1
						+ neighborCell.ArrivalCost
						+ neighborCell.AdditionalCost;

					priorityQueue.Enqueue(
						(npath, cost), cost
						+ neighborCell.Heuristic
						+ Vector2.Distance(neighborCellCoord, toNav)
					);

					closedList.Add(neighborCell.ID);
				}
			}

			// shouldn't happen usually
			if (path == null)
			{
				_logger.Warn("path got nulled for some reason");
				return null;
			}

			var returnList = new List<Vector2>(path.Count) { from };

			for (int i = 1; i < path.Count - 1; i++)
			{
				var navGridCell = path[i];
				returnList.Add(navGrid.TranslateFromNavGrid(navGridCell.Locator));
			}
			returnList.Add(to);

			SmoothPath(returnList, traveler, distanceThreshold);

			//_logger.Debug("Good path found");
			return returnList;
		}

		internal void AddPathfinder(AttackableUnit attackableUnit)
		{
			unitPathing.AddUnit(attackableUnit);
		}

		internal void RemovePathfinder(AttackableUnit attackableUnit)
		{
			unitPathing.RemoveUnit(attackableUnit);
		}

		internal void LogPathfinding(Champion champion)
		{
			using (StreamWriter sw = File.CreateText("../../../../../HelperScripts/input/navgrid.txt"))
			{
				sw.WriteLine($"{navGrid.CellCountX}");
				sw.WriteLine($"{navGrid.CellCountY}");
				sw.WriteLine($"{navGrid.CellSize}");
				foreach (NavigationGridCell cell in navGrid.Cells)
				{
					sw.WriteLine($"{cell.GetCenter()};{cell.Flags};{cell.IsOpen};{navGrid.IsWalkable(cell)};" +
						$"{IsPathable(navGrid.TranslateFromNavGrid(cell.GetCenter()), champion.PathfindingRadius, champion)};" +
						$"{IsOpen(navGrid.TranslateFromNavGrid(cell.GetCenter()), champion.PathfindingRadius, champion)};"
					);
				}
			}
		}
	}
}
