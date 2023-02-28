using System.Diagnostics;

namespace Alcheminer.Engine.Services
{
	internal static class ErrorService
	{
		public static void Error (int id)
		{
			switch (id)
			{
				case 0:
					Debug.Fail("Chaos Engine Error: ID:0 nessesary item unspecified", "there was an item needed for your request but it was not given");
					break;
			}
		}
	}
}
