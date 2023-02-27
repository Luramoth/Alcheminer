using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alcheminer.Engine
{
	internal abstract class Thing
	{
		public Transform transform;

		public abstract void Init();
	}
}
