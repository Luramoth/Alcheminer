using System.Collections;
using System.Collections.Generic;

namespace Alcheminer.Engine.Services
{
	internal static class IDService
	{
		public static Stack<ThingID> ThingStack;

		public static Stack<AssetID> AssetStack;

		public static void ClearStacks()
		{
			AssetStack.Clear();
			ThingStack.Clear();
		}

		public class ThingID
		{
			public int ID;
		}

		public class AssetID
		{
			public int ID;
		}
	}
}
