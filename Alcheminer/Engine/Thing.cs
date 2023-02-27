namespace Alcheminer.Engine
{
	internal abstract class Thing
	{
		public Transform transform { get; set; }

		public abstract void Init();
	}
}
