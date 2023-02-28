using GameServerCore.Domain;
using GameServerCore.Enums;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map1
{
	public static class LevelScriptObjects
	{
		private static Dictionary<GameObjectTypes, List<MapObject>> _mapObjects;

		public static Dictionary<TeamId, Fountain> FountainList = new Dictionary<TeamId, Fountain>();
		public static Dictionary<string, MapObject> SpawnBarracks = new Dictionary<string, MapObject>();
		public static Dictionary<TeamId, bool> AllInhibitorsAreDead = new Dictionary<TeamId, bool> { { TeamId.TEAM_BLUE, false }, { TeamId.TEAM_PURPLE, false } };
		static Dictionary<TeamId, Dictionary<Inhibitor, float>> DeadInhibitors = new Dictionary<TeamId, Dictionary<Inhibitor, float>> { { TeamId.TEAM_BLUE, new Dictionary<Inhibitor, float>() }, { TeamId.TEAM_PURPLE, new Dictionary<Inhibitor, float>() } };
		static List<Nexus> NexusList = new List<Nexus>();
		public static string LaneTurretAI = "TurretAI";

		static Dictionary<TeamId, Dictionary<Lane, List<LaneTurret>>> TurretList = new Dictionary<TeamId, Dictionary<Lane, List<LaneTurret>>>
		{
			{TeamId.TEAM_BLUE, new Dictionary<Lane, List<LaneTurret>>{
				{ Lane.LANE_Unknown, new List<LaneTurret>()},
				{ Lane.LANE_L, new List<LaneTurret>()},
				{ Lane.LANE_C, new List<LaneTurret>()},
				{ Lane.LANE_R, new List<LaneTurret>()}}
			},
			{TeamId.TEAM_PURPLE, new Dictionary<Lane, List<LaneTurret>>{
				{ Lane.LANE_Unknown, new List<LaneTurret>()},
				{ Lane.LANE_L, new List<LaneTurret>()},
				{ Lane.LANE_C, new List<LaneTurret>()},
				{ Lane.LANE_R, new List<LaneTurret>()}}
			}
		};

		public static Dictionary<TeamId, Dictionary<Lane, Inhibitor>> InhibitorList = new Dictionary<TeamId, Dictionary<Lane, Inhibitor>>
		{
			{TeamId.TEAM_BLUE, new Dictionary<Lane, Inhibitor>{
				{ Lane.LANE_L, null },
				{ Lane.LANE_C, null },
				{ Lane.LANE_R, null }}
			},
			{TeamId.TEAM_PURPLE, new Dictionary<Lane, Inhibitor>{
				{ Lane.LANE_L, null },
				{ Lane.LANE_C, null },
				{ Lane.LANE_R, null }}
			}
		};

		//Nexus models
		static Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
		{
			{TeamId.TEAM_BLUE, "OrderNexus" },
			{TeamId.TEAM_PURPLE, "ChaosNexus" }
		};

		//Inhib models
		static Dictionary<TeamId, string> InhibitorModels { get; set; } = new Dictionary<TeamId, string>
		{
			{TeamId.TEAM_BLUE, "OrderInhibitor" },
			{TeamId.TEAM_PURPLE, "ChaosInhibitor" }
		};

		//Tower Models
		static Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
		{
			{TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
			{
				{TurretType.FOUNTAIN_TURRET, "OrderTurretShrine" },
				{TurretType.NEXUS_TURRET, "OrderTurretAngel" },
				{TurretType.INHIBITOR_TURRET, "OrderTurretDragon" },
				{TurretType.INNER_TURRET, "OrderTurretNormal2" },
				{TurretType.OUTER_TURRET, "OrderTurretNormal" },
			} },
			{TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
			{
				{TurretType.FOUNTAIN_TURRET, "ChaosTurretShrine" },
				{TurretType.NEXUS_TURRET, "ChaosTurretNormal" },
				{TurretType.INHIBITOR_TURRET, "ChaosTurretGiant" },
				{TurretType.INNER_TURRET, "ChaosTurretWorm2" },
				{TurretType.OUTER_TURRET, "ChaosTurretWorm" },
			} }
		};

		//Turret Items
		static Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
		{
			{ TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
			{ TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
			{ TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
			{ TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
		};

		// Ugly Hack, this has coded into the information of the cell size and coordinates, but oh well
		static List<Vector2> TurretCells = new List<Vector2>
		{
			new Vector2(17, 205),
			new Vector2(17, 206),
			new Vector2(17, 207),
			new Vector2(18, 205),
			new Vector2(18, 206),
			new Vector2(18, 207),
			new Vector2(21, 82),
			new Vector2(21, 83),
			new Vector2(21, 84),
			new Vector2(22, 82),
			new Vector2(22, 83),
			new Vector2(22, 84),
			new Vector2(23, 82),
			new Vector2(23, 83),
			new Vector2(23, 84),
			new Vector2(27, 130),
			new Vector2(27, 131),
			new Vector2(27, 132),
			new Vector2(28, 130),
			new Vector2(28, 131),
			new Vector2(28, 132),
			new Vector2(29, 130),
			new Vector2(29, 131),
			new Vector2(29, 132),
			new Vector2(32, 41),
			new Vector2(32, 42),
			new Vector2(32, 43),
			new Vector2(33, 41),
			new Vector2(33, 42),
			new Vector2(33, 43),
			new Vector2(34, 41),
			new Vector2(34, 42),
			new Vector2(34, 43),
			new Vector2(40, 33),
			new Vector2(41, 33),
			new Vector2(41, 34),
			new Vector2(41, 35),
			new Vector2(42, 32),
			new Vector2(42, 33),
			new Vector2(42, 34),
			new Vector2(43, 34),
			new Vector2(70, 70),
			new Vector2(70, 71),
			new Vector2(71, 70),
			new Vector2(71, 71),
			new Vector2(72, 70),
			new Vector2(72, 71),
			new Vector2(80, 22),
			new Vector2(80, 23),
			new Vector2(81, 22),
			new Vector2(81, 23),
			new Vector2(82, 22),
			new Vector2(82, 23),
			new Vector2(84, 274),
			new Vector2(84, 275),
			new Vector2(84, 276),
			new Vector2(85, 274),
			new Vector2(85, 275),
			new Vector2(85, 276),
			new Vector2(98, 93),
			new Vector2(98, 94),
			new Vector2(99, 93),
			new Vector2(99, 94),
			new Vector2(100, 93),
			new Vector2(100, 94),
			new Vector2(114, 124),
			new Vector2(114, 125),
			new Vector2(114, 126),
			new Vector2(115, 124),
			new Vector2(115, 125),
			new Vector2(115, 126),
			new Vector2(116, 124),
			new Vector2(116, 125),
			new Vector2(116, 126),
			new Vector2(136, 26),
			new Vector2(136, 27),
			new Vector2(136, 28),
			new Vector2(137, 26),
			new Vector2(137, 27),
			new Vector2(137, 28),
			new Vector2(156, 265),
			new Vector2(156, 266),
			new Vector2(157, 265),
			new Vector2(157, 266),
			new Vector2(158, 265),
			new Vector2(158, 266),
			new Vector2(176, 167),
			new Vector2(176, 168),
			new Vector2(177, 167),
			new Vector2(177, 168),
			new Vector2(178, 167),
			new Vector2(178, 168),
			new Vector2(192, 199),
			new Vector2(192, 200),
			new Vector2(193, 199),
			new Vector2(193, 200),
			new Vector2(194, 199),
			new Vector2(194, 200),
			new Vector2(207, 17),
			new Vector2(207, 18),
			new Vector2(207, 19),
			new Vector2(208, 17),
			new Vector2(208, 18),
			new Vector2(208, 19),
			new Vector2(209, 17),
			new Vector2(209, 18),
			new Vector2(209, 19),
			new Vector2(211, 270),
			new Vector2(211, 271),
			new Vector2(211, 272),
			new Vector2(212, 270),
			new Vector2(212, 271),
			new Vector2(212, 272),
			new Vector2(220, 221),
			new Vector2(220, 222),
			new Vector2(220, 223),
			new Vector2(221, 221),
			new Vector2(221, 222),
			new Vector2(221, 223),
			new Vector2(222, 221),
			new Vector2(222, 222),
			new Vector2(222, 223),
			new Vector2(248, 258),
			new Vector2(248, 259),
			new Vector2(248, 260),
			new Vector2(249, 258),
			new Vector2(249, 259),
			new Vector2(249, 260),
			new Vector2(259, 250),
			new Vector2(259, 251),
			new Vector2(260, 250),
			new Vector2(260, 251),
			new Vector2(264, 161),
			new Vector2(264, 162),
			new Vector2(264, 163),
			new Vector2(265, 161),
			new Vector2(265, 162),
			new Vector2(265, 163),
			new Vector2(269, 210),
			new Vector2(269, 211),
			new Vector2(269, 212),
			new Vector2(270, 210),
			new Vector2(270, 211),
			new Vector2(270, 212),
			new Vector2(271, 210),
			new Vector2(271, 211),
			new Vector2(271, 212),
			new Vector2(274, 87),
			new Vector2(274, 88),
			new Vector2(275, 87),
			new Vector2(275, 88),
			new Vector2(276, 87),
			new Vector2(276, 88),
		};

		static StatsModifier TurretStatsModifier = new StatsModifier();
		static StatsModifier OuterTurretStatsModifier = new StatsModifier();
		public static void LoadBuildings(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
		{
			_mapObjects = mapObjects;

			CreateBuildings();
			LoadProtection();

			LoadSpawnBarracks();
			LoadFountains();
		}

		public static void OnMatchStart()
		{
			LoadShops();

			Dictionary<TeamId, List<Champion>> Players = new Dictionary<TeamId, List<Champion>>
			{
				{TeamId.TEAM_BLUE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_BLUE) },
				{TeamId.TEAM_PURPLE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_PURPLE) }
			};

			StatsModifier TurretHealthModifier = new StatsModifier();
			foreach (var team in TurretList.Keys)
			{
				TeamId enemyTeam = TeamId.TEAM_BLUE;

				if (team == TeamId.TEAM_BLUE)
				{
					enemyTeam = TeamId.TEAM_PURPLE;
				}

				foreach (var lane in TurretList[team].Keys)
				{
					foreach (var turret in TurretList[team][lane])
					{
						if (turret.Type == TurretType.FOUNTAIN_TURRET)
						{
							continue;
						}
						else if (turret.Type != TurretType.NEXUS_TURRET)
						{
							TurretHealthModifier.HealthPoints.BaseBonus = 250.0f * Players[enemyTeam].Count;
						}
						else
						{
							TurretHealthModifier.HealthPoints.BaseBonus = 125.0f * Players[enemyTeam].Count;
						}

						turret.AddStatModifier(TurretHealthModifier);
						turret.Stats.CurrentHealth += turret.Stats.HealthPoints.Total;
						AddTurretItems(turret, GetTurretItems(TurretItems, turret.Type));
						AddTurret(turret);
					}
				}
				//AddTurretCells(TurretCells);
			}

			TurretStatsModifier.Armor.FlatBonus = 1;
			TurretStatsModifier.MagicResist.FlatBonus = 1;
			TurretStatsModifier.AttackDamage.FlatBonus = 4;

			//Outer turrets dont get armor
			OuterTurretStatsModifier.MagicResist.FlatBonus = 1;
			OuterTurretStatsModifier.AttackDamage.FlatBonus = 4;
		}

		public static void OnUpdate(float diff)
		{
			var gameTime = GameTime();

			if (gameTime >= timeCheck && timesApplied < 30)
			{
				UpdateTowerStats();
			}

			if (gameTime >= outerTurretTimeCheck && outerTurretTimesApplied < 7)
			{
				UpdateOuterTurretStats();
			}

			foreach (var fountain in FountainList.Values)
			{
				fountain.Update(diff);
			}

			foreach (var team in DeadInhibitors.Keys)
			{
				foreach (var inhibitor in DeadInhibitors[team].Keys.ToList())
				{
					DeadInhibitors[team][inhibitor] -= diff;
					if (DeadInhibitors[team][inhibitor] <= 0)
					{
						inhibitor.Stats.CurrentHealth = inhibitor.Stats.HealthPoints.Total;
						inhibitor.NotifyState();
						DeadInhibitors[inhibitor.Team].Remove(inhibitor);
					}
					else if (DeadInhibitors[team][inhibitor] <= 15.0f * 1000)
					{
						inhibitor.SetState(DampenerState.RespawningState);
					}
				}
			}
		}

		public static void OnNexusDeath(DeathData deathaData)
		{
			var nexus = deathaData.Unit;
			EndGame(nexus.Team, new Vector3(nexus.Position.X, nexus.GetHeight(), nexus.Position.Y), deathData: deathaData);
		}

		public static void OnInhibitorDeath(DeathData deathData)
		{
			var inhibitor = deathData.Unit as Inhibitor;

			DeadInhibitors[inhibitor.Team].Add(inhibitor, inhibitor.RespawnTime * 1000);

			if (DeadInhibitors[inhibitor.Team].Count == InhibitorList[inhibitor.Team].Count)
			{
				AllInhibitorsAreDead[inhibitor.Team] = true;
			}
		}

		static float timeCheck = 480.0f * 1000;
		static byte timesApplied = 0;
		static void UpdateTowerStats()
		{
			foreach (var team in TurretList.Keys)
			{
				foreach (var lane in TurretList[team].Keys)
				{
					foreach (var turret in TurretList[team][lane])
					{
						if (turret.Type == TurretType.OUTER_TURRET || turret.Type == TurretType.FOUNTAIN_TURRET || (turret.Type == TurretType.INNER_TURRET && timesApplied >= 20))
						{
							continue;
						}

						turret.AddStatModifier(TurretStatsModifier);
					}
				}
			}

			timesApplied++;
			timeCheck += 60.0f * 1000;
		}

		static float outerTurretTimeCheck = 30.0f * 1000;
		static byte outerTurretTimesApplied = 0;
		static void UpdateOuterTurretStats()
		{
			foreach (var team in TurretList.Keys)
			{
				foreach (var lane in TurretList[team].Keys)
				{
					var turret = TurretList[team][lane].Find(x => x.Type == TurretType.OUTER_TURRET);

					if (turret != null)
					{
						turret.AddStatModifier(OuterTurretStatsModifier);
					}
				}
			}

			outerTurretTimesApplied++;
			outerTurretTimeCheck += 60.0f * 1000;
		}

		static void LoadFountains()
		{
			foreach (var fountain in _mapObjects[GameObjectTypes.ObjBuilding_SpawnPoint])
			{
				var team = fountain.GetTeamID();
				FountainList.Add(team, CreateFountain(team, new Vector2(fountain.CentralPoint.X, fountain.CentralPoint.Z)));
			}
		}

		static void LoadShops()
		{
			foreach (var shop in _mapObjects[GameObjectTypes.ObjBuilding_Shop])
			{
				CreateShop(shop.Name, new Vector2(shop.CentralPoint.X, shop.CentralPoint.Z), shop.GetTeamID());
			}
		}

		static void LoadSpawnBarracks()
		{
			foreach (var spawnBarrack in _mapObjects[GameObjectTypes.ObjBuildingBarracks])
			{
				SpawnBarracks.Add(spawnBarrack.Name, spawnBarrack);
			}
		}

		static void CreateBuildings()
		{
			foreach (var nexusObj in _mapObjects[GameObjectTypes.ObjAnimated_HQ])
			{
				var teamId = nexusObj.GetTeamID();
				var position = new Vector2(nexusObj.CentralPoint.X, nexusObj.CentralPoint.Z);
				var nexusStats = new Stats();
				nexusStats.HealthPoints.BaseValue = 5500.0f;
				nexusStats.CurrentHealth = nexusStats.HealthPoints.BaseValue;

				var nexus = CreateNexus(nexusObj.Name, NexusModels[teamId], position, teamId, 353, 1700, nexusStats);

				ApiEventManager.OnDeath.AddListener(nexus, nexus, OnNexusDeath, true);
				NexusList.Add(nexus);
				AddObject(nexus);
			}

			foreach (var inhibitorObj in _mapObjects[GameObjectTypes.ObjAnimated_BarracksDampener])
			{
				var teamId = inhibitorObj.GetTeamID();
				var lane = inhibitorObj.GetLaneID();
				var position = new Vector2(inhibitorObj.CentralPoint.X, inhibitorObj.CentralPoint.Z);
				var inhibitorStats = new Stats();
				inhibitorStats.HealthPoints.BaseValue = GlobalData.BarrackVariables.MaxHP;
				inhibitorStats.Armor.BaseValue = GlobalData.BarrackVariables.Armor;
				inhibitorStats.CurrentHealth = inhibitorStats.HealthPoints.BaseValue;

				var inhibitor = CreateInhibitor(inhibitorObj.Name, InhibitorModels[teamId], position, teamId, lane, 214, 0, inhibitorStats);
				ApiEventManager.OnDeath.AddListener(inhibitor, inhibitor, OnInhibitorDeath, false);
				inhibitor.RespawnTime = 240.0f;
				InhibitorList[teamId][lane] = inhibitor;
				AddObject(inhibitor);
			}
			foreach (var turretObj in _mapObjects[GameObjectTypes.ObjAIBase_Turret])
			{
				var teamId = turretObj.GetTeamID();
				var lane = turretObj.GetLaneID();
				var position = new Vector2(turretObj.CentralPoint.X, turretObj.CentralPoint.Z);

				if (turretObj.Name.Contains("Shrine"))
				{
					var fountainTurret = CreateLaneTurret(turretObj.Name + "_A", TowerModels[teamId][TurretType.FOUNTAIN_TURRET], position, teamId, TurretType.FOUNTAIN_TURRET, Lane.LANE_Unknown, LaneTurretAI, turretObj);
					TurretList[teamId][lane].Add(fountainTurret);
					AddObject(fountainTurret);
					continue;
				}

				var turretType = GetTurretType(turretObj.ParseIndex(), lane, teamId);

				if (turretType == TurretType.FOUNTAIN_TURRET)
				{
					continue;
				}

				switch (turretObj.Name)
				{
					case "Turret_T1_C_06":
						lane = Lane.LANE_L;
						break;
					case "Turret_T1_C_07":
						lane = Lane.LANE_R;
						break;
				}

				var turret = CreateLaneTurret(turretObj.Name + "_A", TowerModels[teamId][turretType], position, teamId, turretType, lane, LaneTurretAI, turretObj);
				TurretList[teamId][lane].Add(turret);
				AddObject(turret);
			}
		}

		static TurretType GetTurretType(int trueIndex, Lane lane, TeamId teamId)
		{
			TurretType returnType = TurretType.FOUNTAIN_TURRET;

			if (lane == Lane.LANE_C)
			{
				if (trueIndex < 3)
				{
					returnType = TurretType.NEXUS_TURRET;
					return returnType;
				}

				trueIndex -= 2;
			}

			switch (trueIndex)
			{
				case 1:
				case 4:
				case 5:
					returnType = TurretType.INHIBITOR_TURRET;
					break;
				case 2:
					returnType = TurretType.INNER_TURRET;
					break;
				case 3:
					returnType = TurretType.OUTER_TURRET;
					break;
			}

			return returnType;
		}

		static void LoadProtection()
		{
			//I can't help but feel there's a better way to do this
			Dictionary<TeamId, List<Inhibitor>> TeamInhibitors = new Dictionary<TeamId, List<Inhibitor>> { { TeamId.TEAM_BLUE, new List<Inhibitor>() }, { TeamId.TEAM_PURPLE, new List<Inhibitor>() } };
			foreach (var teams in InhibitorList.Keys)
			{
				foreach (var lane in InhibitorList[teams].Keys)
				{
					TeamInhibitors[teams].Add(InhibitorList[teams][lane]);
				}
			}

			foreach (var nexus in NexusList)
			{
				// Adds Protection to Nexus
				AddProtection(nexus, TurretList[nexus.Team][Lane.LANE_C].FindAll(turret => turret.Type == TurretType.NEXUS_TURRET).ToArray(), TeamInhibitors[nexus.Team].ToArray());
			}

			foreach (var InhibTeam in TeamInhibitors.Keys)
			{
				foreach (var inhibitor in TeamInhibitors[InhibTeam])
				{
					var inhibitorTurret = TurretList[inhibitor.Team][inhibitor.Lane].First(turret => turret.Type == TurretType.INHIBITOR_TURRET);

					// Adds Protection to Inhibitors
					if (inhibitorTurret != null)
					{
						// Depends on the first available inhibitor turret.
						AddProtection(inhibitor, false, inhibitorTurret);
					}

					// Adds Protection to Turrets
					foreach (var turret in TurretList[inhibitor.Team][inhibitor.Lane])
					{
						if (turret.Type == TurretType.NEXUS_TURRET)
						{
							AddProtection(turret, false, TeamInhibitors[inhibitor.Team].ToArray());
						}
						else if (turret.Type == TurretType.INHIBITOR_TURRET)
						{
							AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.INNER_TURRET));
						}
						else if (turret.Type == TurretType.INNER_TURRET)
						{
							AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.OUTER_TURRET));
						}
					}
				}
			}
		}
	}
}
