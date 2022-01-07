using Dalamud.Configuration;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace Resonant
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public struct WindowBoxSettings
        {
            // relative to the viewport's topleft/bottomright
            public Vector2 TopLeft = new(0, 0);
            public Vector2 BottomRight = new(0, 0);
            public Vector2 SizeWith(Vector2 viewportSize)
            {
                return viewportSize - TopLeft - BottomRight;
            }
        }
        public WindowBoxSettings WindowBox = new();

        public struct HitboxSettings
        {
            public bool Enabled = true;
            public Vector4 Color = ColorPresets.Green;
            public bool Outline = true;
            public Vector4 OutlineColor = ColorPresets.Black;
            public bool ShowDeltaY = true;
        };
        public HitboxSettings Hitbox = new();

        public struct RingSettings
        {
            public bool Enabled = false;
            public float Radius = 5f;
            public Brush Brush = new(ColorPresets.Green, 1);
        };
        public RingSettings TargetRing = new();
        public RingSettings PlayerRing = new();

        public struct ConeSettings
        {
            public bool Enabled = false;
            public float Radius = 7f;
            public int Angle = 90;
            public Brush Brush = new(ColorPresets.Blurple, 3);
        }
        public ConeSettings Cone = new();

        public struct PositionalsSettings
        {
            public bool Enabled = true;
            public bool ShowAbilityRegions = true;

            public int Thickness = 3;
            public Vector4 ColorFront = ColorPresets.Red;
            public bool FrontSeparate = true;
            public Vector4 ColorRear = ColorPresets.Magenta;
            public bool RearSeparate = true;
            public Vector4 ColorFlank = ColorPresets.Blurple;

            public bool HighlightCurrentRegion = true;
            public float HighlightTransparencyMultiplier = 0.2f;

            public bool ArrowEnabled = true;
            public float ArrowScale = 1f;

            public Brush BrushFront { get { return new(ColorFront, Thickness); } }
            public Brush BrushRear { get { return new(ColorRear, Thickness); } }
            public Brush BrushFlank { get { return new(ColorFlank, Thickness); } }
        }
        public PositionalsSettings Positionals = new();

        public bool DebugUIVisible = false;

        // XXX: if adding values here be sure to add to the copy constructor
        //      below and to ConfigurationUI. when i'm smarter reflection might
        //      handle those chores instead.

        [NonSerialized]
        public bool ConfigUIVisible = false;

        [NonSerialized]
        public bool DrawUIVisible = false;

        [NonSerialized]
        private Configuration? backup = null;

        public Configuration()
        {
        }

        // copy constructor
        public Configuration(Configuration config)
        {
            Version = config.Version;

            WindowBox = config.WindowBox;

            Hitbox = config.Hitbox;
            TargetRing = config.TargetRing;
            PlayerRing = config.PlayerRing;
            Cone = config.Cone;
            Positionals = config.Positionals;

            DebugUIVisible = config.DebugUIVisible;

            backup = config;
        }

        internal Configuration getBackup()
        {
            return backup != null
                ? new Configuration(backup)
                : new Configuration();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    internal static class ColorPresets
    {
        public static readonly Vector4 Red = new(1, 0, 0, 1.0f);
        public static readonly Vector4 Black = new(0, 0, 0, 1.0f);
        public static readonly Vector4 Green = new(0.34f, 0.92f, 0.05f, 1.0f);

        public static readonly Vector4 Blurple = new(0.275f, 0.05f, 0.92f, 1.0f);
        public static readonly Vector4 Magenta = new(0.92f, 0.05f, 0.829f, 1.0f);
    }
}
