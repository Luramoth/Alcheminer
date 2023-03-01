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
					Debug.Fail("Chaos Engine Error: ID:0 element not found", "element you were looking for wasent found");
					break;
			}
		}
	}
}
