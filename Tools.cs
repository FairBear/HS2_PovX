using AIChara;
using System;

namespace HS2_PovX
{
	public static class Tools
	{
		// Return the offset of the eyes in the neck's object space.
		/*public static Vector3 GetEyesOffset(ChaControl chaCtrl)
		{
			Transform neck = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone;
			EyeObject[] eyes = chaCtrl.eyeLookCtrl.eyeLookScript.eyeObjs;

			return Vector3.Lerp(
				GetEyesOffset_Internal(neck, eyes[0].eyeTransform),
				GetEyesOffset_Internal(neck, eyes[1].eyeTransform),
				0.5f
			);
		}

		public static Vector3 GetEyesOffset_Internal(Transform neck, Transform eye)
		{
			Vector3 offset = Vector3.zero;

			for (int i = 0; i < 50; i++)
			{
				if (eye == null || eye == neck)
					break;

				offset += eye.localPosition;
				eye = eye.parent;
			}

			return offset;
		}*/

		// Modulo without negative.
		public static float Mod2(float value, float mod)
		{
			if (value < 0)
				value = mod + (value % mod);

			return value % mod;
		}
	}
}
