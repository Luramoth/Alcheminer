namespace Alcheminer.Engine
{
	internal abstract class Thing
	{
		public IDService.ThingID ID;
		public Transform Transform { get; set; }

		public abstract void Init();
	}
}
