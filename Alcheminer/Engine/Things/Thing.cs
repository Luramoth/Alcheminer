namespace Alcheminer.Engine.Things
{
    internal abstract class Thing
    {
        public Services.IDService.ThingID ID;
        public Transform Transform { get; set; }

        public abstract void Init();
    }
}
