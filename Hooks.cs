using HarmonyLib;

namespace HS2_PovX
{
	public partial class HS2_PovX
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
		public static bool Prefix_NeckLookControllerVer2_LateUpdate(NeckLookControllerVer2 __instance)
		{
			if (!Controller.Toggled)
				return true;

			bool flag = __instance != Controller.chaCtrl.neckLookCtrl;

			if (Controller.cameraDidSet)
				return flag;

			Controller.cameraDidSet = true;
			Controller.ScenePoV();

			return flag;
		}
	}
}
