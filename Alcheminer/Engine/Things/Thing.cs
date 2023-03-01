namespace Alcheminer.Engine.Things
{
    internal abstract class Thing : Element
	{
		public Transform Transform { get; set; }

        public abstract void Init();

        public abstract void Update();

        public abstract void Destroy();
    }
}
