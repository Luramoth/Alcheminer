using Alcheminer.Engine.Things;
using System.Collections.Generic;
using static Alcheminer.Engine.Services.IDService;
using static Alcheminer.Engine.Services.StateService;

namespace Alcheminer.Engine.Services
{
	internal class SceneService
	{

		public class Scene
		{
			public Stack<Thing> Things { get; set; }
			
			public SceneID ID;
		}
	}
}
