using System.Collections.Generic;

namespace Alcheminer.Engine.Services
{
	internal static class StateService
	{

		public class State
		{
			public string Name { get; set; }

			public List<SceneService.Scene> Scenes;
		}
	}
}
