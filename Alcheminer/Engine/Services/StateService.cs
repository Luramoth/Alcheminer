using Alcheminer.Engine.Things;
using System.Collections.Generic;
using static Alcheminer.Engine.Services.IDService;

namespace Alcheminer.Engine.Services
{
	internal static class StateService
	{

		public class State
		{
			public string Name { get; set; }

			public Stack<SceneService.Scene> Scenes;

			public State(SceneService.Scene intitialScene)
			{
				intitialScene.ID.IDNum = IDService.AssignID(IDType.SceneID, this);
			}
		}
	}
}
