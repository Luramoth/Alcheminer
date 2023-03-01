using Alcheminer.Engine.Things;
using System;
using System.Collections.Generic;
using static Alcheminer.Engine.Services.IDService;

namespace Alcheminer.Engine.Services
{
	internal class SceneService
	{

		public class Scene : Element
		{
			private List<Thing> things;

			private void TrackThings()
			{
				foreach (Thing thing in things)
				{
					IDService.Track(thing);
				}
			}

			private void UntrackThings()
			{
				
			}
		}
	}
}
