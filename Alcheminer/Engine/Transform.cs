using Microsoft.Xna.Framework;

namespace Alcheminer.Engine
{
	internal abstract class Transform
	{
		public Vector2 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public Vector2 Scale { get; set; }
	}
}
