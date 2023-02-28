namespace Alcheminer.Engine
{
	internal abstract class Thing
	{
		public Transform Transform { get; set; }

		public abstract void Init();
	}
}
