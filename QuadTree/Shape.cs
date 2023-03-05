using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace System.Activities.Presentation.View
{

	public interface Shape
	{
		public bool IntersectsWith(Circle bounds);
		public bool IntersectsWith(Rect rect);
		public bool Contains(Rect rect);
	}

	public struct Circle: Shape
	{
		public Vector2 Position;
		public float Radius;

		public bool isEmpty
		{
			//bounds.Width == 0 || bounds.Height == 0
			get { return Radius <= 0; }
		}

		public Circle(Vector2 position, float radius)
		{
			Position = position;
			Radius = radius;
		}

		// For insertion
		public bool ContainedBy(Rect rect)
		{
			return (
				rect.Left <= (Position.X - Radius) &&
				rect.Top <= (Position.Y - Radius) &&
				rect.Left + rect.Width >= (Position.X + Radius) &&
				rect.Top + rect.Height >= (Position.X + Radius)
			);
		}
		public bool Contains(Vector2 v)
		{
			// The distance to the furthest corner is less than the radius 
			return new Vector2(v.X - Position.X, v.Y - Position.Y).LengthSquared() < (Radius * Radius);
		}
		// The rest is for query 
		public bool Contains(Rect rect)
		{
			// The distance to the furthest corner is less than the radius 
			return new Vector2(
				Math.Max(Math.Abs(Position.X - rect.Left), Math.Abs(Position.X - (rect.Left + rect.Width))),
				Math.Max(Math.Abs(Position.Y - rect.Top), Math.Abs(Position.Y - (rect.Top + rect.Height)))
			).LengthSquared() < (Radius * Radius);
		}

		// https://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection/1879223#1879223
		public bool IntersectsWith(Rect rect)
		{
			return Vector2.DistanceSquared(Position, new Vector2(
				Math.Clamp(Position.X, rect.Left, rect.Left + rect.Width),
				Math.Clamp(Position.Y, rect.Top, rect.Top + rect.Height)
			)) < (Radius * Radius);
		}

		public bool IntersectsWith(Circle circle)
		{
			return Vector2.DistanceSquared(Position, circle.Position) < (Radius + circle.Radius) * (Radius + circle.Radius);
		}
	}

	public struct Rect
	{
		public float Top;
		public float Left;
		public float Width;
		public float Height;

		public Rect(float top, float left, float width, float height)
		{
			Top = top;
			Left = left;
			Width = width;
			Height = height;
		}

		public bool Contains(Vector2 v)
		{
			return (v.X >= Left && v.X <= (Left + Width) && v.Y <= Top && v.Y >= (Top - Height));
		}
	}

	public struct Stadium: Shape
	{
		public Vector2 X1, X2;
		public float radius;


		// We use these shapes to speed up calculations later
		public Circle C1;
		public Circle C2;
		public float rectAngle;
		public Rect rect;
		public List<Vector2> Axes;
		public List<Vector2> rectVertex;

		public static float UnitVectorToAngle(Vector2 v)
		{
			return MathF.Atan2(v.Y, v.X);
		}

		public static Vector2 Rotate(Vector2 v, float angle)
		{
			var x = MathF.Cos(angle) * v.X - MathF.Sin(angle) * v.Y;
			var y = MathF.Sin(angle) * v.X + MathF.Cos(angle) * v.Y;
			return new Vector2(x, y);
		}

		public bool AxisCollision(Vector2 axis, IEnumerable<Vector2> P1, IEnumerable<Vector2> P2)
		{
			float max_p1 = P1.Select(e => Vector2.Dot(e, axis)).Max();
			float min_p1 = P1.Select(e => Vector2.Dot(e, axis)).Min();
			float max_p2 = P2.Select(e => Vector2.Dot(e, axis)).Max();
			float min_p2 = P2.Select(e => Vector2.Dot(e, axis)).Min();
			return min_p1 >= max_p2 || max_p1 >= min_p2;
		}

		public Stadium(Vector2 x1, Vector2 x2, float radius)
		{
			X1 = x1;
			X2 = x2;
			this.radius = radius;
			C1 = new Circle(X1, radius);
			C2 = new Circle(X2, radius);
			var A1 = x2 - x1;
			var A2 = new Vector2(-A1.X, A1.Y);
			rectAngle = -UnitVectorToAngle(A1);
			Vector2 rotatedX1 = Rotate(x1, rectAngle);
			Vector2 rotatedX2 = Rotate(x2, rectAngle);

			rect = new Rect(MathF.Max(rotatedX1.Y, rotatedX2.Y) + radius, MathF.Min(rotatedX1.X, rotatedX1.X), A1.Length(), 2 * radius);

			var width = (A2 / A1.Length()) * radius;
			rectVertex = new List<Vector2>
			{
				X1 + width,
				X2 + width,
				X1 - width,
				X2 - width,
			};

			Axes = new List<Vector2> { 
				x2 - x1,
				new Vector2((x2 - x1).Y, -(x2 - x1).X), 
				new Vector2(1, 0), 
				new Vector2(0, 1) 
			};
		}

		public bool Contains(Vector2 v)
		{
			if (C1.Contains(v) || C2.Contains(v)) return true;
			if (rect.Contains(Rotate(v,rectAngle))) return true;
			return false;
		}

		public bool Contains(Rect rect) // if any of the vertex isn't in the rectangle isn't in
		{
			if (!Contains(new Vector2(rect.Left, rect.Top))) return false;
			if (!Contains(new Vector2(rect.Left + rect.Width, rect.Top))) return false;
			if (!Contains(new Vector2(rect.Left, rect.Top + rect.Height))) return false;
			if (!Contains(new Vector2(rect.Left + rect.Width, rect.Top + rect.Height))) return false;
			return true;
		}

		public bool IntersectsWith(Rect rect)
		{
			if (C1.IntersectsWith(rect)) return true;
			if (C2.IntersectsWith(rect)) return true;

			var vertexP1 = new List<Vector2> {
				new Vector2(rect.Left, rect.Top),
				new Vector2(rect.Left + rect.Width, rect.Top),
				new Vector2(rect.Left, rect.Top - rect.Height),
				new Vector2(rect.Left + rect.Width, rect.Top - rect.Height)
			};
			Stadium stad = this;
			return Axes.All(a => stad.AxisCollision(a, vertexP1, stad.rectVertex));
		}

		public bool IntersectsWith(Circle bounds)
		{
			if (C1.IntersectsWith(rect)) return true;
			if (C2.IntersectsWith(rect)) return true;

			return (new Circle(Rotate(bounds.Position, rectAngle), bounds.Radius)).IntersectsWith(rect);
		}
	}
}
