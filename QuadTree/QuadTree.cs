using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Runtime;
using System.Numerics;
using System.Linq;

// https://referencesource.microsoft.com/#System.Data/cdf/src/NetFx40/Tools/System.Activities.Presentation/System/Activities/Presentation/View/QuadTree.cs
//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.View
{

	/// <summary>
	/// This class efficiently stores and retrieves arbitrarily sized and positioned
	/// objects in a quad-tree data structure.  This can be used to do efficient hit
	/// detection or visiblility checks on objects in a virtualized canvas.
	/// The object does not need to implement any special interface because the Rect Bounds
	/// of those objects is handled as a separate argument to Insert.
	/// </summary>
	public class QuadTree<T> where T : class
	{
		Rect bounds; // overall bounds we are indexing.
		Quadrant? root;
		Dictionary<T, Tuple<Quadrant, QuadNode>> map = new Dictionary<T, Tuple<Quadrant, QuadNode>>();

		public QuadTree(float top, float left, float width, float height)
		{
			bounds = new Rect(top, left, width, height);
		}

		/// <summary>
		/// Insert a node with given bounds into this QuadTree.
		/// </summary>
		/// <param name="node">The node to insert</param>
		/// <param name="bounds">The bounds of this node</param>
		public void Insert(T node, Circle bounds)
		{
			if (this.bounds.Width == 0 || this.bounds.Height == 0)
				throw new ArgumentException("Bounds must be non zero");
			if (bounds.isEmpty)
				throw new ArgumentException("Bounds must be non zero");

			root ??= new Quadrant(null, this.bounds);

			var insertion = root.Insert(node, bounds);
			map.Add(node, insertion);
		}

		public void Remove(T node)
		{
			map[node].Item1.Remove(map[node].Item2);
		}

		/// <summary>
		/// Get a list of the nodes within the specified range of a target position.
		/// </summary>
		/// <param name="position">Vector2 position to check.</param>
		/// <param name="range">Distance to check.</param>
		/// <returns>List of zero or mode nodes found inside the given bounds</returns>
		public IEnumerable<T> GetNodesInside(Vector2 position, float range)
		{
			return GetNodesInside(new Circle(position, range));
		}

		/// <summary>
		/// Get a list of the nodes that intersect the given bounds.
		/// </summary>
		/// <param name="bounds">The bounds to test</param>
		/// <returns>List of zero or mode nodes found inside the given bounds</returns>
		public IEnumerable<T> GetNodesInside(Circle bounds)
		{
			foreach (QuadNode n in GetNodes(bounds))
			{
				yield return n.Node;
			}
		}

		/// <summary>
		/// Get list of nodes that intersect the given bounds.
		/// </summary>
		/// <param name="bounds">The bounds to test</param>
		/// <returns>The list of nodes intersecting the given bounds</returns>
		IEnumerable<QuadNode> GetNodes(Circle bounds)
		{
			HashSet<QuadNode> result = new HashSet<QuadNode>();
			root?.GetIntersectingNodes(result, bounds); // root? checks if root is null
			return result;
		}

		public void Clear()
		{
			root = null;
			map.Clear();
		}

		/// <summary>
		/// Each node stored in the tree has a position, width & height.
		/// </summary>
		internal class QuadNode
		{
			public Circle Bounds;
			public T Node; // the actual visual object being stored here.

			/// <summary>
			/// Construct new QuadNode to wrap the given node with given bounds
			/// </summary>
			/// <param name="node">The node</param>
			/// <param name="bounds">The bounds of that node</param>
			public QuadNode(T node, Circle bounds)
			{
				Node = node;
				Bounds = bounds;
			}
		}


		/// <summary>
		/// The canvas is split up into four Quadrants and objects are stored in the quadrant that contains them
		/// and each quadrant is split up into four child Quadrants recurrsively.  Objects that overlap more than
		/// one quadrant are stored in the this.nodes list for this Quadrant.
		/// </summary>
		internal class Quadrant
		{
			Quadrant? parent; // Null if you are the root quadrant
			Rect bounds; // quadrant bounds.

			HashSet<QuadNode> nodes = new HashSet<QuadNode>(); // nodes that overlap the sub quadrant boundaries.

			// The quadrant is subdivided when nodes are inserted that are 
			// completely contained within those subdivisions.
			Quadrant? topLeft;
			Quadrant? topRight;
			Quadrant? bottomLeft;
			Quadrant? bottomRight;

			/// <summary>
			/// Construct new Quadrant with a given bounds all nodes stored inside this quadrant
			/// will fit inside this bounds.  
			/// </summary>
			/// <param name="parent">The parent quadrant (if any)</param>
			/// <param name="bounds">The bounds of this quadrant</param>
			public Quadrant(Quadrant? parent, Rect bounds)
			{
				this.parent = parent;
				Debug.Assert(bounds.Width != 0 && bounds.Height != 0, "Cannot have empty bound");
				if (bounds.Width == 0 || bounds.Height == 0)
					throw new ArgumentException("Bounds must be non zero");

				this.bounds = bounds;
			}

			/// <summary>
			/// Insert the given node
			/// </summary>
			/// <param name="node">The node </param>
			/// <param name="bounds">The bounds of that node</param>
			/// <returns></returns>
			internal Tuple<Quadrant, QuadNode> Insert(T node, Circle bounds)
			{
				Debug.Assert(!bounds.isEmpty, "Cannot have empty bound");
				if (bounds.isEmpty)
					throw new ArgumentException("Bounds must be non zero");

				Quadrant toInsert = this;
				while (true)
				{
					float w = toInsert.bounds.Width / 2;
					float h = toInsert.bounds.Height / 2;
					if (w < 1) w = 1;
					if (h < 1) h = 1;

					// assumption that the Rect struct is almost as fast as doing the operations
					// manually since Rect is a value type.

					Rect topLeft = new Rect(toInsert.bounds.Left, toInsert.bounds.Top, w, h);
					Rect topRight = new Rect(toInsert.bounds.Left + w, toInsert.bounds.Top, w, h);
					Rect bottomLeft = new Rect(toInsert.bounds.Left, toInsert.bounds.Top + h, w, h);
					Rect bottomRight = new Rect(toInsert.bounds.Left + w, toInsert.bounds.Top + h, w, h);

					Quadrant? child = null;

					// See if any child quadrants completely contain this node.
					if (bounds.ContainedBy(topLeft))
					{
						toInsert.topLeft ??= new Quadrant(toInsert, topLeft);
						child = toInsert.topLeft;
					}
					else if (bounds.ContainedBy(topRight))
					{
						toInsert.topRight ??= new Quadrant(toInsert, topRight);
						child = toInsert.topRight;
					}
					else if (bounds.ContainedBy(bottomLeft))
					{
						toInsert.bottomLeft ??= new Quadrant(toInsert, bottomLeft);
						child = toInsert.bottomLeft;
					}
					else if (bounds.ContainedBy(bottomRight))
					{
						toInsert.bottomRight ??= new Quadrant(toInsert, bottomRight);
						child = toInsert.bottomRight;
					}

					if (child != null)
						toInsert = child;
					else
					{
						QuadNode n = new QuadNode(node, bounds);
						toInsert.nodes.Add(n);
						return new Tuple<Quadrant, QuadNode>(toInsert, n);
					}
				}

			}

			/// <summary>
			/// Returns all nodes in this quadrant that intersect the given bounds.
			/// The nodes are returned in pretty much random order as far as the caller is concerned.
			/// </summary>
			/// <param name="nodes">List of nodes found in the given bounds</param>
			/// <param name="bounds">The bounds that contains the nodes you want returned</param>
			internal void GetIntersectingNodes(HashSet<QuadNode> nodes, Shape bounds, bool doNotCheck = false)
			{

				doNotCheck = doNotCheck || bounds.Contains(this.bounds); // Do not check means that we include everything inside

				float w = this.bounds.Width / 2;
				float h = this.bounds.Height / 2;

				// assumption that the Rect struct is almost as fast as doing the operations manually since Rect is a value type.
				// See if any child quadrants completely contain this node.
				if ((doNotCheck || bounds.IntersectsWith(new Rect(this.bounds.Left, this.bounds.Top, w, h))) && topLeft != null)
					topLeft.GetIntersectingNodes(nodes, bounds, doNotCheck);

				if ((doNotCheck || bounds.IntersectsWith(new Rect(this.bounds.Left + w, this.bounds.Top, w, h))) && topRight != null)
					topRight.GetIntersectingNodes(nodes, bounds, doNotCheck);

				if ((doNotCheck || bounds.IntersectsWith(new Rect(this.bounds.Left, this.bounds.Top + h, w, h))) && bottomLeft != null)
					bottomLeft.GetIntersectingNodes(nodes, bounds, doNotCheck);

				if ((doNotCheck || bounds.IntersectsWith(new Rect(this.bounds.Left + w, this.bounds.Top + h, w, h))) && bottomRight != null)
					bottomRight.GetIntersectingNodes(nodes, bounds, doNotCheck);

				if (doNotCheck)
					nodes.UnionWith(this.nodes);
				else
					nodes.UnionWith(this.nodes.Where(e => bounds.IntersectsWith(e.Bounds)));
			}

			internal void Remove(QuadNode node)
			{
				nodes.Remove(node);
			}
		}
	}
}