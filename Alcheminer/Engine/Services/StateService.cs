using Alcheminer.Engine.Things;
using System.Collections.Generic;

namespace Alcheminer.Engine.Services
{
	internal static class StateService
	{

		public class State
		{
			public string Name { get; set; }

			public Stack<Thing> Things { get; set; }
		}
	}
}
