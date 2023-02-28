using Alcheminer.Engine.Services;

namespace Alcheminer.Engine.Assets
{
    internal class Asset
    {
        public IDService.AssetID ID;

		public Asset(IDService.AssetID iD) => ID = iD;
	}
}
