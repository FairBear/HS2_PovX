using AIChara;
using UnityEngine;

namespace HS2_PovX
{
	public static partial class Controller
	{
		// Angle offsets are used for situations where the character can't move.
		// The offsets are added to the neck's current rotation.
		// This means that the values can be negative.
		public static float cameraAngleOffsetX = 0f;
		public static float cameraAngleOffsetY = 0f;
		public static float cameraFoV = 0f;
		public static Vector3 cameraPosition = Vector3.zero;
		public static Quaternion cameraRotation = Quaternion.identity;
		public static bool cameraDidSet = false;
		public static float cameraSmoothness = 0f;

		// 0 = Player; 1 = 1st Partner; 2 = 2nd Partner; 3 = ...
		public static int focus = 0;
		public static int focusLockOn = -1;
		public static ChaControl[] chaCtrls = new ChaControl[0];
		public static ChaControl chaCtrl = null;

		public static Vector3 prevPosition = Vector3.zero;
		public static Vector3 eyeOffset = Vector3.zero;
		public static float backupFoV = 0f;
		public static Vector3 backupPosition = Vector3.zero;
		public static Quaternion backupRotation = Quaternion.identity;

		public static bool didHideHead = false;

		public static bool Toggled
		{
			get => chaCtrl != null;

			set
			{
				if (Toggled == value)
					return;

				if (value)
				{
					focus = 0;
					chaCtrls = GetChaControls();

					if (chaCtrls.Length == 0)
						return;

					SetChaControl(GetChaControl());
				}
				else
				{
					SetChaControl(null);
					focusLockOn = -1;
				}
			}
		}

		public static void Update()
		{
			if (cameraDidSet)
				cameraDidSet = false;

			if (HS2_PovX.PoVKey.Value.IsDown())
				Toggled = !Toggled;

			if (!Toggled)
				return;

			if (chaCtrl == null || !chaCtrl.visibleAll)
			{
				SetChaControl(GetChaControl());

				if (chaCtrl == null)
				{
					Toggled = false;
					return;
				}
			}

			if (HS2_PovX.CharaCycleKey.Value.IsDown())
			{
				int prev = focus;
				focus = (focus + 1) % chaCtrls.Length;

				// Swap lock-on.
				if (focusLockOn == focus)
				{
					focusLockOn = prev;
					cameraAngleOffsetX = cameraAngleOffsetY = 0f;
				}

				SetChaControl(GetChaControl());
				return;
			}

			if (HS2_PovX.LockOnKey.Value.IsDown())
			{
				focusLockOn = (focusLockOn + 2) % (chaCtrls.Length + 1) - 1;
				cameraAngleOffsetX = cameraAngleOffsetY = 0f;
				return;
			}

			float sensitivity = HS2_PovX.Sensitivity.Value;
			bool didZoom = HS2_PovX.ZoomKey.Value.IsPressed();

			if (didZoom)
				sensitivity *= HS2_PovX.ZoomFoV.Value / HS2_PovX.FoV.Value;

			float x = Input.GetAxis("Mouse Y") * sensitivity;
			float y = Input.GetAxis("Mouse X") * sensitivity;

			if (Cursor.lockState != CursorLockMode.None || HS2_PovX.CameraDragKey.Value.IsPressed())
			{
				float max = HS2_PovX.CameraMaxX.Value;
				float min = HS2_PovX.CameraMinX.Value;
				float span = HS2_PovX.CameraSpanY.Value;

				cameraAngleOffsetX = Mathf.Clamp(cameraAngleOffsetX - x, -max, min);
				cameraAngleOffsetY = Mathf.Clamp(cameraAngleOffsetY + y, -span, span);
			}

			if (HS2_PovX.ToggleCursorKey.Value.IsDown())
			{
				Cursor.visible = !Cursor.visible;
				Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
			}
			else if (!didZoom && !Cursor.visible && Input.anyKeyDown)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
		}

		public static Vector3 GetDesiredPosition(ChaControl chaCtrl)
		{
			Transform head = chaCtrl.objHeadBone.transform;
			EyeObject[] eyes = chaCtrl.eyeLookCtrl.eyeLookScript.eyeObjs;
			Vector3 pos = Vector3.Lerp(
				eyes[0].eyeTransform.position,
				eyes[1].eyeTransform.position,
				0.5f
			);

			return pos +
				HS2_PovX.OffsetX.Value * head.right +
				HS2_PovX.OffsetY.Value * head.up +
				HS2_PovX.OffsetZ.Value * head.forward;
		}

		public static void SetPosition()
		{
			if (cameraPosition != Camera.main.transform.position)
				backupPosition = Camera.main.transform.position;

			Vector3 next = GetDesiredPosition(chaCtrl);

			if (cameraSmoothness > 0f)
				next = prevPosition = Vector3.Lerp(next, prevPosition, cameraSmoothness);

			Camera.main.transform.position = cameraPosition = next;
		}

		public static void SetRotation()
		{
			Transform head = chaCtrl.objHeadBone.transform;
			Transform camTransform = Camera.main.transform;
			ChaControl lockOn = GetChaControlLockOn();

			if (cameraRotation != Camera.main.transform.rotation)
				backupRotation = Camera.main.transform.rotation;

			if (lockOn != null)
			{
				Vector3 position = camTransform.position;
				camTransform.position = GetDesiredPosition(chaCtrl);
				{
					camTransform.LookAt(GetDesiredPosition(lockOn), Vector3.up);
				}
				camTransform.position = position;
			}
			else if (HS2_PovX.CameraNormalize.Value)
				camTransform.rotation = Quaternion.Euler(head.rotation.eulerAngles.x, head.rotation.eulerAngles.y, 0f);
			else
				camTransform.rotation = head.rotation;

			Camera.main.transform.Rotate(cameraAngleOffsetX, cameraAngleOffsetY, 0f);

			if (HS2_PovX.CameraHeadRotate.Value)
			{
				NeckObjectVer2[] bones = chaCtrl.neckLookCtrl.neckLookScript.aBones;
				bones[0].neckBone.rotation = Camera.main.transform.rotation;
			}

			cameraRotation = Camera.main.transform.rotation;
		}

		public static void SetFoV()
		{
			if (cameraFoV != Camera.main.fieldOfView)
				backupFoV = Camera.main.fieldOfView;

			Camera.main.fieldOfView = cameraFoV =
				HS2_PovX.ZoomKey.Value.IsPressed() ?
					HS2_PovX.ZoomFoV.Value :
					HS2_PovX.FoV.Value;
		}

		public static void ScenePoV()
		{
			SetRotation();
			SetPosition();
			SetFoV();
		}
	}
}
