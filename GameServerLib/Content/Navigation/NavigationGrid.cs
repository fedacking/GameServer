using System;
using System.Collections.Generic;
using System.IO;
using GameServerCore;
using Vector2 = System.Numerics.Vector2;
using System.Numerics;
using GameServerLib.Extensions;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationGrid
    {
        /// <summary>
        /// The minimum position on the NavigationGrid in normal coordinate space (bottom left in 2D).
        /// NavigationGridCells are undefined below these minimums.
        /// </summary>
        public Vector3 MinGridPosition { get; private set; }
        /// <summary>
        /// The maximum position on the NavigationGrid in normal coordinate space (top right in 2D).
        /// NavigationGridCells are undefined beyond these maximums.
        /// </summary>
        public Vector3 MaxGridPosition { get; private set; }
        /// <summary>
        /// Calculated resolution of the Navigation Grid (percentage of a cell 1 normal unit takes up, not to be confused with 1/CellSize).
        /// Multiple used to convert cell-based coordinates back into normal coordinates (CellCountX/Z / TranslationMaxGridPosition).
        /// </summary>
        public Vector3 TranslationMaxGridPosition { get; private set; }
        /// <summary>
        /// Ideal number of normal units a cell takes up (not fully accurate, but mostly, refer to TranslationMaxGridPosition for true size).
        /// </summary>
        public float CellSize { get; private set; }
        /// <summary>
        /// Width of the Navigation Grid in cells.
        /// </summary>
        public uint CellCountX { get; private set; }
        /// <summary>
        /// Height of the Navigation Grid in cells.
        /// </summary>
        public uint CellCountY { get; private set; }
        /// <summary>
        /// Array of all cells contained in this Navigation Grid.
        /// </summary>
        public NavigationGridCell[] Cells { get; private set; }
        /// <summary>
        /// Array of region tags where each index represents a cell's index.
        /// </summary>
        public uint[] RegionTags { get; private set; }
        /// <summary>
        /// Table of regions possible in the current Navigation Grid.
        /// Regions are the areas representing key points on a map. In the case of OldSR, this could be lanes top, middle, or bot, and the last region being jungle.
        /// *NOTE*: Regions only exist in Navigation Grids with a version of 5 or higher. OldSR is version 3.
        /// </summary>
        public NavigationRegionTagTable RegionTagTable { get; private set; }
        /// <summary>
        /// Number of sampled heights in the X coordinate plane.
        /// </summary>
        public uint SampledHeightsCountX { get; private set; }
        /// <summary>
        /// Number of sampled heights in the Y coordinate plane (Z coordinate in 3D space).
        /// </summary>
        public uint SampledHeightsCountY { get; private set; }
        /// <summary>
        /// Multiple used to convert from normal coordinates to an index format used to get sampled heights from the Navigation Grid.
        /// </summary>
        /// TODO: Seems to be volatile. If there ever comes a time when Navigation Grid editing becomes easy, that'd be the perfect time to rework the methods for getting sampled heights.
        public Vector2 SampledHeightsDistance { get; private set; }
        /// <summary>
        /// Array of sampled heights where each index represents a cell's index (depends on SampledHeightsCountX/Y).
        /// </summary>
        public float[] SampledHeights { get; private set; }
        /// <summary>
        /// Grid of hints.
        /// Function likely related to pathfinding.
        /// Currently Unused.
        /// </summary>
        public NavigationHintGrid HintGrid { get; private set; }
        /// <summary>
        /// Width of the map in normal coordinate space, where the origin is at (0, 0).
        /// *NOTE*: Not to be confused with MaxGridPosition.X, whos origin is at MinGridPosition.
        /// </summary>
        public float MapWidth { get; private set; }
        /// <summary>
        /// Height of the map in normal coordinate space, where the origin is at (0, 0).
        /// *NOTE*: Not to be confused with MaxGridPosition.Z, whos origin is at MinGridPosition.
        /// </summary>
        public float MapHeight { get; private set; }
        /// <summary>
        /// Center of the map in normal coordinate space.
        /// </summary>
        public Vector2 MiddleOfMap { get; private set; }

        public NavigationGrid(string fileLocation) : this(File.OpenRead(fileLocation)) { }
        public NavigationGrid(byte[] buffer) : this(new MemoryStream(buffer)) { }
        public NavigationGrid(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                byte major = br.ReadByte();
                ushort minor = major != 2 ? br.ReadUInt16() : (ushort)0;
                if (major != 2 && major != 3 && major != 5 && major != 7)
                {
                    throw new Exception(string.Format("Unsupported Navigation Grid Version: {0}.{1}", major, minor));
                }

                MinGridPosition = br.ReadVector3();
                MaxGridPosition = br.ReadVector3();

                CellSize = br.ReadSingle();
                CellCountX = br.ReadUInt32();
                CellCountY = br.ReadUInt32();

                Cells = new NavigationGridCell[CellCountX * CellCountY];
                RegionTags = new uint[CellCountX * CellCountY];

                if (major == 2 || major == 3 || major == 5)
                {
                    for (int i = 0; i < Cells.Length; i++)
                    {
                        Cells[i] = NavigationGridCell.ReadVersion5(br, i);
                    }

                    if (major == 5)
                    {
                        for (int i = 0; i < RegionTags.Length; i++)
                        {
                            RegionTags[i] = br.ReadUInt16();
                        }
                    }
                }
                else if (major == 7)
                {
                    for (int i = 0; i < Cells.Length; i++)
                    {
                        Cells[i] = NavigationGridCell.ReadVersion7(br, i);
                    }
                    for (int i = 0; i < Cells.Length; i++)
                    {
                        Cells[i].SetFlags((NavigationGridCellFlags)br.ReadUInt16());
                    }

                    for (int i = 0; i < RegionTags.Length; i++)
                    {
                        RegionTags[i] = br.ReadUInt32();
                    }
                }

                if(major >= 5)
                {
                    uint groupCount = major == 5 ? 4u : 8u;
                    RegionTagTable = new NavigationRegionTagTable(br, groupCount);
                }

                SampledHeightsCountX = br.ReadUInt32();
                SampledHeightsCountY = br.ReadUInt32();
                SampledHeightsDistance = br.ReadVector2();
                SampledHeights = new float[SampledHeightsCountX * SampledHeightsCountY];
                for (int i = 0; i < SampledHeights.Length; i++)
                {
                    SampledHeights[i] = br.ReadSingle();
                }

                HintGrid = new NavigationHintGrid(br);

                MapWidth = MaxGridPosition.X + MinGridPosition.X;
                MapHeight = MaxGridPosition.Z + MinGridPosition.Z;
                MiddleOfMap = new Vector2(MapWidth / 2, MapHeight / 2);
            }
        }



        /// <summary>
        /// Translates the given Vector2 into cell format where each unit is a cell.
        /// This is to simplify the calculations required to get cells.
        /// </summary>
        /// <param name="vector">Vector2 to translate.</param>
        /// <returns>Cell formatted Vector2.</returns>
        public Vector2 TranslateToNavGrid(Vector2 vector)
        {
            return new Vector2
            (
                (vector.X - MinGridPosition.X) / CellSize,
                (vector.Y - MinGridPosition.Z) / CellSize
            );
        }

        /// <summary>
        /// Translates the given cell locator position back into normal coordinate space as a Vector2.
        /// *NOTE*: Returns the coordinates of the center of the cell.
        /// </summary>
        /// <param name="locator">Cell locator.</param>
        /// <returns>Normal coordinate space Vector2.</returns>
        public Vector2 TranslateFromNavGrid(NavigationGridLocator locator)
        {
            return TranslateFromNavGrid(new Vector2(locator.X, locator.Y)) + Vector2.One * 0.5f * CellSize;
        }

        /// <summary>
        /// Translates the given cell formatted Vector2 back into normal coordinate space.
        /// </summary>
        /// <param name="vector">Vector2 to translate.</param>
        /// <returns>Normal coordinate space Vector2.</returns>
        public Vector2 TranslateFromNavGrid(Vector2 vector)
        {
            return new Vector2
            (
                vector.X * CellSize + MinGridPosition.X,
                vector.Y * CellSize + MinGridPosition.Z
            );
        }

        public NavigationGridCell GetCell(Vector2 coords, bool translate = true)
        {
            if(translate)
            {
                coords = TranslateToNavGrid(coords);
            }
            return GetCell((short)coords.X, (short)coords.Y);
        }

        /// <summary>
        /// Gets the cell at the given cell based coordinates.
        /// </summary>
        /// <param name="x">cell based X coordinate</param>
        /// <param name="y">cell based Y coordinate.</param>
        /// <returns>Cell instance.</returns>
        public NavigationGridCell GetCell(short x, short y)
        {
            long index = y * CellCountX + x;
            if (x < 0 || x > CellCountX || y < 0 || y > CellCountY || index >= Cells.Length)
            {
                return null;
            }
            return Cells[index];
        }

        /// <summary>
        /// Gets a list of all cells within 8 cardinal directions of the given cell.
        /// </summary>
        /// <param name="cell">Cell to start the check at.</param>
        /// <returns>List of neighboring cells.</returns>
        public List<NavigationGridCell> GetCellNeighbors(NavigationGridCell cell)
        {
            List<NavigationGridCell> neighbors = new List<NavigationGridCell>(9);
            for (short dirY = -1; dirY <= 1; dirY++)
            {
                for (short dirX = -1; dirX <= 1; dirX++)
                {
                    short nx = (short)(cell.Locator.X + dirX);
                    short ny = (short)(cell.Locator.Y + dirY);
                    NavigationGridCell neighborCell = GetCell(nx, ny);
                    if (neighborCell != null)
                    {
                        neighbors.Add(neighborCell);
                    }
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Gets the index of a cell that is closest to the given 2D point.
        /// Usually used when the given point is outside the boundaries of the Navigation Grid.
        /// </summary>
        /// <param name="x">X coordinate to check.</param>
        /// <param name="y">Y coordinate to check.</param>
        /// <param name="translate">Whether or not the given coordinates are in LS form.</param>
        /// <returns>Index of a valid cell.</returns>
        public NavigationGridCell GetClosestValidCell(Vector2 coords, bool translate = true)
        {
            Vector2 minGridPos = Vector2.Zero;
            Vector2 maxGridPos = TranslateToNavGrid(
                new Vector2(MaxGridPosition.X, MaxGridPosition.Z)
            );

            if (translate)
            {
                coords = TranslateToNavGrid(coords);
            }

            return GetCell(
                new Vector2(
                    Math.Clamp(coords.X, minGridPos.X, maxGridPos.X),
                    Math.Clamp(coords.Y, minGridPos.Y, maxGridPos.Y)
                ),
                false
            );
        }

        /// <summary>
        /// Gets a list of cells within the specified range of a specified point.
        /// </summary>
        /// <param name="origin">Vector2 with normal coordinates to start the check.</param>
        /// <param name="radius">Range to check around the origin.</param>
        /// <returns>List of all cells in range. Null if range extends outside of NavigationGrid boundaries.</returns>
        public IEnumerable<NavigationGridCell> GetAllCellsInRange(Vector2 origin, float radius, bool translate = true)
        {
            radius /= CellSize;
            if(translate)
            {
                origin = TranslateToNavGrid(origin);
            }

            short fx = (short)(origin.X - radius);
            short lx = (short)(origin.X + radius);
            short fy = (short)(origin.Y - radius);
            short ly = (short)(origin.Y + radius);

            for(short x = fx; x <= lx; x++)
            {
                for(short y = fy; y <= ly; y++)
                {
                    float distSquared = Extensions.DistanceSquaredToRectangle(
                        new Vector2(x + 0.5f, y + 0.5f), 1f, 1f, origin
                    );
                    if(distSquared <= radius*radius)
                    {
                        var cell = GetCell(x, y);
                        if(cell != null)
                        {
                            yield return cell;
                        }
                    }
                }
            }
        }

        public bool IsWalkable(NavigationGridCell cell)
        {
            return cell != null
                && !cell.HasFlag(NavigationGridCellFlags.NOT_PASSABLE)
                && !cell.HasFlag(NavigationGridCellFlags.SEE_THROUGH);
        }

		public bool IsWalkable(NavigationGridCell cell, float checkRadius)
        {
            Vector2 cellCenter = cell.GetCenter();
            return IsWalkable(cellCenter, checkRadius, false);
        }

        /// <summary>
        /// Whether or not the cell at the given position can be pathed on.
        /// </summary>
        /// <param name="coords">Vector2 position to check.</param>
        /// <param name="checkRadius">Radius around the given point to check for walkability.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        public bool IsWalkable(Vector2 coords, float checkRadius = 0, bool translate = true)
        {
            if (checkRadius == 0)
            {
                NavigationGridCell cell = GetCell(coords, translate);
                return IsWalkable(cell);
            }

            var cells = GetAllCellsInRange(coords, checkRadius, translate);            
            foreach (NavigationGridCell c in cells)
            {
                if (!IsWalkable(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Whether or not the given position is see-through. In other words, if it does not block vision.
        /// </summary>
        /// <param name="coords">Vector2 position to check.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        public bool IsVisible(Vector2 coords, bool translate = true)
        {
            NavigationGridCell cell = GetCell(coords, translate);
            return IsVisible(cell); //TODO: implement bush logic here
        }

        bool IsVisible(NavigationGridCell cell)
        {
            return cell != null
                && (!cell.HasFlag(NavigationGridCellFlags.NOT_PASSABLE)
                || cell.HasFlag(NavigationGridCellFlags.SEE_THROUGH)
                || cell.HasFlag(NavigationGridCellFlags.HAS_GLOBAL_VISION));
        }

        /// <summary>
        /// Whether or not the given position has the specified flags.
        /// </summary>
        /// <param name="coords">Vector2 position to check.</param>
        /// <param name="translate">Whether or not to translate the given position to cell-based format.</param>
        /// <returns>True/False.</returns>
        public bool HasFlag(Vector2 coords, NavigationGridCellFlags flag, bool translate = true)
        {
            NavigationGridCell cell = GetCell(coords, translate);
            return cell != null && cell.HasFlag(flag);
        }

        /// <summary>
        /// Gets the height of the ground at the given position. Used purely for packets.
        /// </summary>
        /// <param name="location">Vector2 position to check.</param>
        /// <returns>Height (3D Y coordinate) at the given position.</returns>
        public float GetHeightAtLocation(Vector2 location)
        {
            // Uses SampledHeights to get the height of a given location on the Navigation Grid
            // This is the method the game uses to get height data

            if (location.X >= MinGridPosition.X && location.Y >= MinGridPosition.Z &&
                location.X <= MaxGridPosition.X && location.Y <= MaxGridPosition.Z)
            {
                float reguestedHeightX = (location.X - MinGridPosition.X) / SampledHeightsDistance.X;
                float requestedHeightY = (location.Y - MinGridPosition.Z) / SampledHeightsDistance.Y;

                int sampledHeight1IndexX = (int)reguestedHeightX;
                int sampledHeight1IndexY = (int)requestedHeightY;
                int sampledHeight2IndexX;
                int sampledHeight2IndexY;

                float v13;
                float v15;

                if (reguestedHeightX >= SampledHeightsCountX - 1)
                {
                    v13 = 1.0f;
                    sampledHeight2IndexX = sampledHeight1IndexX--;
                }
                else
                {
                    v13 = 0.0f;
                    sampledHeight2IndexX = sampledHeight1IndexX + 1;
                }
                if (requestedHeightY >= SampledHeightsCountY - 1)
                {
                    v15 = 1.0f;
                    sampledHeight2IndexY = sampledHeight1IndexY--;
                }
                else
                {
                    v15 = 0.0f;
                    sampledHeight2IndexY = sampledHeight1IndexY + 1;
                }

                uint sampledHeightsCount = SampledHeightsCountX * SampledHeightsCountY;
                int v1 = (int)SampledHeightsCountX * sampledHeight1IndexY;
                int x0y0 = v1 + sampledHeight1IndexX;

                if (v1 + sampledHeight1IndexX < sampledHeightsCount)
                {
                    int v19 = sampledHeight2IndexX + v1;
                    if (v19 < sampledHeightsCount)
                    {
                        int v20 = sampledHeight2IndexY * (int)SampledHeightsCountX;
                        int v21 = v20 + sampledHeight1IndexX;

                        if (v21 < sampledHeightsCount)
                        {
                            int v22 = sampledHeight2IndexX + v20;
                            if (v22 < sampledHeightsCount)
                            {
                                float height = ((1.0f - v13) * SampledHeights[x0y0])
                                          + (v13 * SampledHeights[v19])
                                          + (((SampledHeights[v21] * (1.0f - v13))
                                          + (SampledHeights[v22] * v13)) * v15);

                                return (1.0f - v15) * height;
                            }
                        }
                    }
                }

            }

            return 0.0f;
        }

        /// <summary>
        /// Casts a ray and returns false when failed, with a stopping position, or true on success with the given destination.
        /// </summary>
        /// <param name="origin">Vector position to start the ray cast from.</param>
        /// <param name="destination">Vector2 position to end the ray cast at.</param>
        /// <param name="checkWalkable">Whether or not the ray stops when hitting a position which blocks pathing.</param>
        /// <param name="checkVisible">Whether or not the ray stops when hitting a position which blocks vision.</param>
        /// <returns>True = Reached destination. True = Failed.</returns>
        public bool CastRay(Vector2 origin, Vector2 destination, bool checkWalkable = false, bool checkVisible = false, bool translate = true)
        {
            // Out of bounds
            if (origin.X < MinGridPosition.X || origin.X >= MaxGridPosition.X || origin.Y < MinGridPosition.Z || origin.Y >= MaxGridPosition.Z)
            {
                return true;
            }

            if(translate)
            {
                origin = TranslateToNavGrid(origin);
                destination = TranslateToNavGrid(destination);
            }

            var cells = GetAllCellsInLine(origin, destination).GetEnumerator();

            bool prevPosHadBush = HasFlag(origin, NavigationGridCellFlags.HAS_GRASS, false);
            bool destinationHasGrass = HasFlag(destination, NavigationGridCellFlags.HAS_GRASS, false);

            bool hasNext;
            while (hasNext = cells.MoveNext())
            {
                var cell = cells.Current;

                //TODO: Implement methods for maps whose NavGrids don't use SEE_THROUGH flags for buildings
                if (checkWalkable)
                {
                    if(!IsWalkable(cell))
                    {
                        break;
                    }
                }

                if (checkVisible)
                {
                    if (!IsVisible(cell))
                    {
                        break;
                    }

                    bool isGrass = cell.HasFlag(NavigationGridCellFlags.HAS_GRASS);

                    // If you are outside of a bush
                    if (!prevPosHadBush && isGrass)
                    {
                        break;
                    }

                    // If you are in a different bush
                    if (prevPosHadBush && destinationHasGrass && !isGrass)
                    {
                        break;
                    }
                }
            }
            
            return hasNext;
        }

        // https://playtechs.blogspot.com/2007/03/raytracing-on-grid.html
        public IEnumerable<NavigationGridCell> GetAllCellsInLine(Vector2 v0, Vector2 v1)
        {
            //Why does this not work???
            yield break;
            
            double dx = Math.Abs(v1.X - v0.X);
            double dy = Math.Abs(v1.Y - v0.Y);

            short x = (short)(Math.Floor(v0.X));
            short y = (short)(Math.Floor(v0.Y));

            int n = 1;
            short x_inc, y_inc;
            double error;

            if (dx == 0)
            {
                x_inc = 0;
                error = float.PositiveInfinity;
            }
            else if (v1.X > v0.X)
            {
                x_inc = 1;
                n += (int)(Math.Floor(v1.X)) - x;
                error = (Math.Floor(v0.X) + 1 - v0.X) * dy;
            }
            else
            {
                x_inc = -1;
                n += x - (int)(Math.Floor(v1.X));
                error = (v0.X - Math.Floor(v0.X)) * dy;
            }

            if (dy == 0)
            {
                y_inc = 0;
                error = float.NegativeInfinity;
            }
            else if (v1.Y > v0.Y)
            {
                y_inc = 1;
                n += (int)(Math.Floor(v1.Y)) - y;
                error -= (Math.Floor(v0.Y) + 1 - v0.Y) * dx;
            }
            else
            {
                y_inc = -1;
                n += y - (int)(Math.Floor(v1.Y));
                error -= (v0.Y - Math.Floor(v0.Y)) * dx;
            }

            for (; n > 0; --n)
            {
                yield return GetCell(x, y);

                if (error > 0)
                {
                    y += y_inc;
                    error -= dx;
                }
                else if(error < 0)
                {
                    x += x_inc;
                    error += dy;
                }
                else //if (error == 0)
                {
                    yield return GetCell((short)(x + x_inc), y);
                    yield return GetCell(x, (short)(y + y_inc));

                    x += x_inc;
                    y += y_inc;
                    error += dy - dx;
                    n--;
                }
            }
        }

        /// <summary>
        /// Casts a ray in the given direction and returns false when failed, with a stopping position, or true on success with the given destination.
        /// *NOTE*: Is not actually infinite, just travels (direction * 1024) units ahead of the given origin.
        /// </summary>
        /// <param name="origin">Vector position to start the ray cast from.</param>
        /// <param name="direction">Ray cast direction.</param>
        /// <param name="checkWalkable">Whether or not the ray stops when hitting a position which blocks pathing.</param>
        /// <param name="checkVisible">Whether or not the ray stops when hitting a position which blocks vision. *NOTE*: Does not apply if checkWalkable is also true.</param>
        /// <returns>False = Reached destination. True = Failed.</returns>
        public bool CastInfiniteRay(Vector2 origin, Vector2 direction, bool checkWalkable = true, bool checkVisible = false)
        {
            return CastRay(origin, origin + direction * 1024, checkWalkable, checkVisible);
        }

        /// <summary>
        /// Whether or not there is anything blocking the two given GameObjects from either seeing eachother or pathing straight towards eachother (depending on checkVision).
        /// </summary>
        /// <param name="a">GameObject to start the check from.</param>
        /// <param name="b">GameObject to end the check at.</param>
        /// <param name="checkVision">True = Check for positions that block vision. False = Check for positions that block pathing.</param>
        /// <returns>True/False.</returns>
        public bool IsAnythingBetween(GameObject a, GameObject b, bool checkVision = false)
        {
            var d = Vector2.Normalize(b.Position - a.Position);

            Vector2 origin = a.Position + d * a.PathfindingRadius;
            Vector2 destination = b.Position - d * b.PathfindingRadius;

            return CastRay(origin, destination, !checkVision, checkVision);
        }

        public NavigationGridCell GetClosestWalkableCell(Vector2 coords, float distanceThreshold = 0, bool translate = true)
        {
            if(translate)
            {
                coords = TranslateToNavGrid(coords);
            }
            float closestDist = 0;
            NavigationGridCell closestCell = null;
            foreach(var cell in Cells)
            {
                if(IsWalkable(cell, distanceThreshold))
                {
                    float dist = Vector2.DistanceSquared(cell.GetCenter(), coords);
                    if(closestCell == null || dist < closestDist)
                    {
                        closestCell = cell;
                        closestDist = dist;
                    }
                }
            }
            return closestCell;
        }
    }
}
