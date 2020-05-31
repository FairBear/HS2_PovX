using AIChara;
using System.Collections.Generic;
using UnityEngine;

namespace HS2_PovX
{
	public static class Controller
	{
		public static bool _toggled = false;

		// Angle offsets are used for situations where the character can't move.
		// The offsets are added to the neck's current rotation.
		// This means that the values can be negative.
		public static float cameraAngleOffsetX = 0f;
		public static float cameraAngleOffsetY = 0f;
		public static float cameraAngleY = 0f;
		public static float cameraFoV = 0f;
		public static Vector3 cameraPosition = Vector3.zero;
		public static Quaternion cameraRotation = Quaternion.identity;
		public static bool cameraDidSet = false;

		// 0 = Player; 1 = 1st Partner; 2 = 2nd Partner; 3 = ...
		public static int focus = 0;
		public static ChaControl[] chaCtrls = new ChaControl[0];
		public static ChaControl chaCtrl = null;

		public static Vector3 prevPosition = Vector3.zero;
		public static Vector3 eyeOffset = Vector3.zero;
		public static float backupFoV = 0f;
		public static Vector3 backupPosition = Vector3.zero;
		public static Quaternion backupRotation = Quaternion.identity;
		public static Vector3 backupHead = Vector3.zero;
		public static Queue<Vector3> seqPositions = new Queue<Vector3>();

		public static bool didHideHead = false;

		public static bool Toggled
		{
			get => _toggled;

			set
			{
				if (_toggled == value)
					return;

				if (value)
				{
					ChaControl[] list = Tools.ChaCtrls;

					if (list.Length == 0)
						return;

					focus = 0;
					chaCtrls = list;

					SetChaControl(ChaCtrl);

					if (chaCtrl == null)
						return;

					cameraAngleOffsetX = cameraAngleOffsetY = 0f;
					cameraAngleY = chaCtrl.neckLookCtrl.neckLookScript.aBones[0].neckBone.eulerAngles.y;
					backupFoV = cameraFoV = Camera.main.fieldOfView;
					backupPosition = cameraPosition = Camera.main.transform.position;
					backupRotation = cameraRotation = Camera.main.transform.rotation;
				}
				else
				{
					SetChaControl(null);

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

				_toggled = value;
			}
		}

		public static ChaControl ChaCtrl
		{
			get
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
				SetChaControl(ChaCtrl);

				if (chaCtrl == null)
				{
					Toggled = false;
					return;
				}
			}

			if (HS2_PovX.CharaCycleKey.Value.IsDown())
			{
				focus = (focus + 1) % chaCtrls.Length;
				SetChaControl(ChaCtrl);
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
				cameraAngleY = Tools.Mod2(cameraAngleY + y, 360f);
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

		public static void SetChaControl(ChaControl next)
		{
			seqPositions.Clear();

			if (chaCtrl != null && didHideHead)
			{
				didHideHead = false;
				//chaCtrl.objHeadBone.SetActive(true);
				chaCtrl.objHeadBone.transform.localScale = backupHead;
			}

			chaCtrl = next;

			if (chaCtrl != null)
			{
				eyeOffset = Tools.GetEyesOffset(chaCtrl);

				if (HS2_PovX.HideHead.Value)
				{
					didHideHead = true;
					//chaCtrl.objHeadBone.SetActive(false);
					backupHead = chaCtrl.objHeadBone.transform.localScale;
					chaCtrl.objHeadBone.transform.localScale = Vector3.zero;
				}
			}
		}

		public static void SetPosition(Transform neck)
		{
			if (cameraPosition != Camera.main.transform.position)
				backupPosition = Camera.main.transform.position;

			/*EyeObject[] eyes = chaCtrl.eyeLookCtrl.eyeLookScript.eyeObjs;
			Vector3 pos = Vector3.Lerp(
				eyes[0].eyeTransform.position,
				eyes[1].eyeTransform.position,
				0.5f
			);

			Vector3 next =
				pos +
				HS2_PovX.OffsetX.Value * neck.right +
				HS2_PovX.OffsetY.Value * neck.up +
				HS2_PovX.OffsetZ.Value * neck.forward;*/

			Vector3 next =
				neck.position +
				(HS2_PovX.OffsetX.Value + eyeOffset.x) * neck.right +
				(HS2_PovX.OffsetY.Value + eyeOffset.y) * neck.up +
				(HS2_PovX.OffsetZ.Value + eyeOffset.z) * neck.forward;

			if (HS2_PovX.CameraStabilize.Value)
			{
				seqPositions.Enqueue(next);

				if (seqPositions.Count > 10)
					seqPositions.Dequeue();

				next = Vector3.zero;

				foreach (Vector3 prev in seqPositions)
					next += prev;

				next /= seqPositions.Count;
			}

			Camera.main.transform.position = cameraPosition = next;
		}

		public static void SetRotation(Transform neck)
		{
			if (cameraRotation != Camera.main.transform.rotation)
				backupRotation = Camera.main.transform.rotation;

			if (HS2_PovX.CameraHeadRotate.Value)
			{
				neck.Rotate(cameraAngleOffsetX, cameraAngleOffsetY, 0f);
				Camera.main.transform.rotation = neck.rotation;
			}
			else
			{
				// Preserve current neck rotation.
				Camera.main.transform.rotation = neck.rotation;
				Camera.main.transform.Rotate(cameraAngleOffsetX, cameraAngleOffsetY, 0f);
			}

			cameraRotation = Camera.main.transform.rotation;
		}

		// Used for scenes where the focused character cannot be controlled.
		public static void ScenePoV()
		{
			NeckObjectVer2[] bones = chaCtrl.neckLookCtrl.neckLookScript.aBones;
			Transform neck = bones[0].neckBone;

			if (cameraFoV != Camera.main.fieldOfView)
				backupFoV = Camera.main.fieldOfView;

			Camera.main.fieldOfView = cameraFoV =
				HS2_PovX.ZoomKey.Value.IsPressed() ?
					HS2_PovX.ZoomFoV.Value :
					HS2_PovX.FoV.Value;

			SetRotation(neck);
			SetPosition(neck);
		}
	}
}
