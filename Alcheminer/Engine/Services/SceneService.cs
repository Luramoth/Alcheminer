using Alcheminer.Engine.Things;
using System.Collections.Generic;

namespace Alcheminer.Engine.Services
{
	internal class SceneService
	{

		public class Scene : Element
		{
			public List<Thing> Things { get; set; }
		}
	}
}
