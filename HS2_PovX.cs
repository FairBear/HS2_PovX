using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using UnityEngine;

namespace HS2_PovX
{
	[BepInProcess("HoneySelect2")]
	[BepInPlugin(GUID, Name, Version)]
	public partial class HS2_PovX : BaseUnityPlugin
    {
		const string GUID = "com.2155x.fairbair.hs2_povx";
		const string Name = "HS2 PoV X";
		const string Version = "1.0.1";

		const string SECTION_GENERAL = "General";
		const string SECTION_CAMERA = "Camera";
		const string SECTION_HOTKEYS = "Hotkeys";

		const string DESCRIPTION_HIDE_HEAD =
			"Should the head be invisible in PoV mode?";

		const string DESCRIPTION_OFFSET_X =
			"Sideway offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Y =
			"Vertical offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Z =
			"Forward offset from the character's eyes.";
		const string DESCRIPTION_CAMERA_MIN_X =
			"Highest downward and leftward angle the camera can rotate.";
		const string DESCRIPTION_CAMERA_MAX_X =
			"Highest upward and rightware angle the camera can rotate.";
		const string DESCRIPTION_CAMERA_SPAN_Y =
			"How far can the camera be rotated horizontally?";
		const string DESCRIPTION_CAMERA_SMOOTHNESS =
			"The higher the number, the smoother the camera becomes.";
		const string DESCRIPTION_CAMERA_HEAD_ROTATE =
			"When enabled, the head rotates along with the camera. " +
			"This may ruin some animations when selecting the girls, " +
			"such as blowjobs.";

		const string DESCRIPTION_CHARA_CYCLE_KEY =
			"Switch between characters during PoV mode.";
		const string DESCRIPTION_CAMERA_DRAG_KEY =
			"During PoV mode, holding down this key will move the camera if the mouse isn't locked.";
		const string DESCRIPTION_TOGGLE_CURSOR_KEY =
			"Pressing this key will toggle the cursor during PoV mode. " +
			"Pressing any other keys will reveal the cursor.";

		public static ConfigEntry<bool> HideHead { get; set; }

		public static ConfigEntry<float> Sensitivity { get; set; }
		public static ConfigEntry<float> FoV { get; set; }
		public static ConfigEntry<float> ZoomFoV { get; set; }
		public static ConfigEntry<float> OffsetX { get; set; }
		public static ConfigEntry<float> OffsetY { get; set; }
		public static ConfigEntry<float> OffsetZ { get; set; }
		public static ConfigEntry<float> CameraMinX { get; set; }
		public static ConfigEntry<float> CameraMaxX { get; set; }
		public static ConfigEntry<float> CameraSpanY { get; set; }
		public static ConfigEntry<bool> CameraStabilize { get; set; }
		public static ConfigEntry<int> CameraSmoothness { get; set; }
		public static ConfigEntry<bool> CameraHeadRotate { get; set; }

		public static ConfigEntry<KeyboardShortcut> PoVKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> CharaCycleKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> CameraDragKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> ToggleCursorKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> ZoomKey { get; set; }

		private void Awake()
		{
			HideHead = Config.Bind(SECTION_GENERAL, "Hide Head", true, DESCRIPTION_HIDE_HEAD);

			Sensitivity = Config.Bind(SECTION_CAMERA, "Camera Sensitivity", 2f);
			FoV = Config.Bind(SECTION_CAMERA, "Field of View", 70f);
			ZoomFoV = Config.Bind(SECTION_CAMERA, "Zoom Field of View", 5f);
			OffsetX = Config.Bind(SECTION_CAMERA, "Offset X", 0f, DESCRIPTION_OFFSET_X);
			OffsetY = Config.Bind(SECTION_CAMERA, "Offset Y", 0f, DESCRIPTION_OFFSET_Y);
			OffsetZ = Config.Bind(SECTION_CAMERA, "Offset Z", 0.15f, DESCRIPTION_OFFSET_Z);
			CameraMinX = Config.Bind(SECTION_CAMERA, "Min Camera Angle X", 80f, DESCRIPTION_CAMERA_MIN_X);
			CameraMaxX = Config.Bind(SECTION_CAMERA, "Max Camera Angle X", 80f, DESCRIPTION_CAMERA_MAX_X);
			CameraSpanY = Config.Bind(SECTION_CAMERA, "Camera Angle Span Y", 70f, DESCRIPTION_CAMERA_SPAN_Y);
			CameraSmoothness = Config.Bind(SECTION_CAMERA, "Camera Smoothness", 0, new ConfigDescription(DESCRIPTION_CAMERA_SMOOTHNESS, new AcceptableValueRange<int>(0, 90)));
			CameraHeadRotate = Config.Bind(SECTION_CAMERA, "Rotate Head to Camera", false, DESCRIPTION_CAMERA_HEAD_ROTATE);

			PoVKey = Config.Bind(SECTION_HOTKEYS, "PoV Toggle Key", new KeyboardShortcut(KeyCode.Comma));
			CharaCycleKey = Config.Bind(SECTION_HOTKEYS, "Character Cycle Key", new KeyboardShortcut(KeyCode.Period), DESCRIPTION_CHARA_CYCLE_KEY);
			CameraDragKey = Config.Bind(SECTION_HOTKEYS, "Camera Drag Key", new KeyboardShortcut(KeyCode.Mouse0), DESCRIPTION_CAMERA_DRAG_KEY);
			ToggleCursorKey = Config.Bind(SECTION_HOTKEYS, "Toggle Cursor Key", new KeyboardShortcut(KeyCode.LeftControl), DESCRIPTION_TOGGLE_CURSOR_KEY);
			ZoomKey = Config.Bind(SECTION_HOTKEYS, "Zoom Key", new KeyboardShortcut(KeyCode.X));

			HideHead.SettingChanged += (sender, args) =>
				Controller.SetChaControl(Controller.ChaCtrl);

			CameraSmoothness.SettingChanged += (sender, args) =>
				Controller.cameraSmoothness = CameraSmoothness.Value / 100f;

			Controller.cameraSmoothness = CameraSmoothness.Value / 100f;

			HarmonyWrapper.PatchAll(typeof(HS2_PovX));
		}

		public void Update()
		{
			Controller.Update();
		}
	}
}
