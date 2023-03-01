using System.Collections.Generic;
using Alcheminer.Engine;

namespace Alcheminer.Engine.Services
{
	internal static class IDService
	{
		// list of assets to keep track of
		private static List<Element> Elements;

		// this function will give an unused ID to a Scene, Thing, or Asset
		public static void Track(Element element)
		{
			if (Elements.Count == 0)
			{
				element.id.IDNum = 0;
			}
			else
			{
				int i = 0;

				foreach (Element ele in Elements)
				{
					if (ele.id.IDNum == i)
					{
						i++;
						continue;
					}
					else
					{
						element.id.IDNum = i;
						break;
					}
				}
				if (i == Elements.Count)
					element.id.IDNum = Elements.Count + 1;
			}

			Elements.Add(element);
		}

		public static void Untrack(Element element)
		{
			foreach (Element ele in Elements)
			{
				if (ele.id.IDNum == element.id.IDNum)
				{
					Elements.Remove(ele);
					break;
				}
				else
				{
					continue;
				}
			}
		}

		public static void UntrackAll()
		{
			Elements.Clear();
		}

		public class ID
		{
			public int IDNum;
		}
	}
}
