using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Resonant
{
    [Serializable]
    public class ConfigurationProfile
    {
        public string Name;

        public Guid ID;

        public struct HitboxSettings
        {
            public bool Enabled = true;
            public Vector4 Color = ColorPresets.Green;
            public bool Outline = true;
            public Vector4 OutlineColor = ColorPresets.Black;
            public bool UseTargetY = true;
            public bool ShowTargetDeltaY = true;
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

            public bool MeleeAbilityRange = true;
            public int MeleeAbilityThickness = 1;

            public int Thickness = 3;
            public Vector4 ColorFront = ColorPresets.Red;
            public bool FrontSeparate = false;
            public Vector4 ColorRear = ColorPresets.Magenta;
            public bool RearSeparate = false;
            public Vector4 ColorFlank = ColorPresets.Blurple;
            public FlankRegionSetting FlankType = FlankRegionSetting.RearOnly;

            public bool HighlightCurrentRegion = true;
            public float HighlightTransparencyMultiplier = 0.1f;

            public bool ArrowEnabled = true;
            public float ArrowScale = 1f;

            public Brush BrushFront { get { return new(ColorFront, Thickness); } }
            public Brush BrushRear { get { return new(ColorRear, Thickness); } }
            public Brush BrushFlank { get { return new(ColorFlank, Thickness); } }
        }
        public PositionalsSettings Positionals = new();

        public List<string> Jobs = new();

        public ConfigurationProfile(string name) {
            Name = name;
            ID = Guid.NewGuid();
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

    // todo: better name
    public enum FlankRegionSetting
    {
        Full, // draw the full 90deg
        RearOnly, // only draw rear 45deg
        FullSeparated, // draw the full 90deg, but separate the regions
    }

    internal static class FlankRegionSettingExtension
    {
        public static String Description(this FlankRegionSetting setting)
        {
            switch (setting)
            {
                case FlankRegionSetting.Full:
                    return "Full (90 degrees)";
                case FlankRegionSetting.RearOnly:
                    return "Rear Only (45 degrees)";
                case FlankRegionSetting.FullSeparated:
                    return "Separated (90 degrees, separated)";
                default:
                    return "error - unknown setting";
            }
        }
    }
}
