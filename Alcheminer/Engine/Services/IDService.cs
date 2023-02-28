using Alcheminer.Engine.Assets;
using Alcheminer.Engine.Things;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Alcheminer.Engine.Services.StateService;

namespace Alcheminer.Engine.Services
{
	internal static class IDService
	{
		public enum IDType
		{
			AssetID,
			ThingID,
			SceneID
		}

		// list of assets to keep track of
		public static Stack<Asset> Assets;

		// this function will give an unused ID to a Scene, Thing, or Asset
		public static int AssignID(IDType type, StateService.State state = null, SceneService.Scene scene = null)
		{
			switch (type)
			{
				case IDType.AssetID:
					{
						if (Assets.Count == 0)
							return 0;

						int i = 0;

						foreach (Asset asset in Assets)
						{
							if (asset.ID.IDNum == i)
							{
								i++;
								continue;
							}
							else
							{
								return i;
							}
						}
					}
					break;
				case IDType.ThingID:
					if (scene != null)
					{
						if (scene.Things.Count == 0)
							return 0;

						int i = 0;

						foreach (Thing thing in scene.Things)
						{
							if (thing.ID.IDNum == i)
							{
								i++;
								continue;
							}
							else
							{
								return i;
							}
						}
					}
					else
					{
						ErrorService.Error(0);
					}
					break;
				case IDType.SceneID:
					if (state != null)
					{
						if (state.Scenes.Count == 0)
							return 0;

						int i = 0;

						foreach (SceneService.Scene sc in state.Scenes)
						{
							if (sc.ID.IDNum == i)
							{
								i++;
								continue;
							}
							else
							{
								return i;
							}
						}
					}
					else
					{
						ErrorService.Error(0);
					}
					break;
			}
			return 0;
		}

		public class ID
		{
			public int IDNum;
		}

		public class ThingID : ID
		{
			
		}

		public class AssetID : ID
		{
			
		}

		public class SceneID : ID
		{
			
		}
	}
}
