using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Frameworks.DriverSystem
{
	// Make sure script execution timing is after fk/ik and other stuff
	[ExecuteAlways]
	public class DriverController : MonoBehaviour
	{
		[SerializeReference]
		public List<BaseDriver> Drivers = new ();

		public void OnEnable()
		{
			for (var i = 0; i < Drivers.Count; i++)
			{
				try
				{
					Drivers[i].OnEnable();
				}
				catch (Exception e)
				{
					Debug.LogError($"[DriverController {gameObject.name}] Exception initializing driver at index {i} {e}");
				}
			}
		}

		public void OnDisable()
		{
			for (var i = 0; i < Drivers.Count; i++)
			{
				try
				{
					Drivers[i].OnDisable();
				}
				catch (Exception e)
				{
					Debug.LogError($"[DriverController {gameObject.name}] Exception resetting driver at index {i} {e}");
				}
			}
		}

		public void LateUpdate()
		{
			for (var i = 0; i < Drivers.Count; i++)
			{
				try
				{
					var driver = Drivers[i];
					if (!driver.Enabled)
						continue;
					
					driver.Run();
				}
				catch (Exception e)
				{
					Debug.LogError($"[DriverController {gameObject.name}] Exception running driver at index {i} {e}");
				}
			}
		}
	}
}