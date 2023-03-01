using System.Collections.Generic;

namespace Alcheminer.Engine.Services
{
	internal static class IDService
	{
		// list of assets to keep track of
		public static List<Element> Elements;

		// this function will give an unused ID to a Scene, Thing, or Asset
		public static void Track(Element element)
		{

		}

		public class ID
		{
			public int IDNum;
		}
	}
}
