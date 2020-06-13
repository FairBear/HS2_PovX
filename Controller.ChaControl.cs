using AIChara;
using UnityEngine;

namespace HS2_PovX
{
	public static partial class Controller
	{
		public static void SetChaControl(ChaControl next)
		{
			if (chaCtrl != null)
			{
				RestoreBackups();

				if (didHideHead)
				{
					didHideHead = false;
					chaCtrl.objHeadBone.SetActive(true);
				}
			}

			chaCtrl = next;

			if (chaCtrl != null)
			{
				SetBackups();

				//eyeOffset = Tools.GetEyesOffset(chaCtrl);
				prevPosition = GetDesiredPosition(chaCtrl);
				cameraAngleOffsetX = cameraAngleOffsetY = 0f;

				if (HS2_PovX.HideHead.Value)
				{
					didHideHead = true;
					chaCtrl.objHeadBone.SetActive(false);
				}
			}
		}

		public static ChaControl GetChaControl()
		{
			if (chaCtrls.Length == 0)
				return null;

			int length = chaCtrls.Length;

			if (focus >= length)
				focus %= length;

			for (int i = 0; i < length; i++)
			{
				ChaControl target = chaCtrls[focus];

				if (target != null && target.visibleAll)
					return target;

				// Skip invisible or destroyed characters.
				focus = (focus + 1) % length;
			}

			return null;
		}

		public static void RefreshChaControl()
		{
			if (Toggled)
				SetChaControl(GetChaControl());
		}

		public static ChaControl GetChaControlLockOn()
		{
			if (focusLockOn == -1 || chaCtrls.Length < 2)
				return null;

			int length = chaCtrls.Length;

			if (focusLockOn >= length)
				focusLockOn %= length;

			for (; focusLockOn < length; focusLockOn++)
			{
				ChaControl target = chaCtrls[focusLockOn];

				// Skip invisible, destroyed, or focused characters.
				if (target != null &&
					target != chaCtrl &&
					target.visibleAll)
					return target;
			}

			focusLockOn = -1;
			return null;
		}

		public static ChaControl[] GetChaControls()
		{
			return Object.FindObjectsOfType<ChaControl>();
		}

		public static void SetBackups()
		{
			Camera camera = Camera.main;
			//cameraAngleY = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone.eulerAngles.y;
			backupFoV = cameraFoV = camera.fieldOfView;
			backupPosition = cameraPosition = camera.transform.position;
			backupRotation = cameraRotation = camera.transform.rotation;
		}

		public static void RestoreBackups()
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			Camera camera = Camera.main;

			if (camera.fieldOfView == cameraFoV)
				camera.fieldOfView = backupFoV;

			if (camera.transform.position == cameraPosition)
				camera.transform.position = backupPosition;

			if (camera.transform.rotation == cameraRotation)
				camera.transform.rotation = backupRotation;
		}
	}
}
