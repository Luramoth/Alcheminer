using Alcheminer.Engine.Services;

namespace Alcheminer.Engine.Things
{
    internal abstract class Thing
    {
        public Services.IDService.ThingID ID;

		protected Thing(IDService.ThingID iD)
		{
			ID = iD;
		}

		public Transform Transform { get; set; }

        public abstract void Init();

        public abstract void Update();

        public abstract void Destroy();
    }
}
