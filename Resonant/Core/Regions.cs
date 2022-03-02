using Dalamud.Game.ClientState.Objects.Types;
using System.Collections.Generic;
using System.Numerics;

namespace Resonant
{
    internal struct Positional
    {
        internal float StartRads;
        internal float EndRads;
        internal Positional(float startRads, float endRads)
        {
            StartRads = startRads;
            EndRads = endRads;
        }

        internal static Positional FromDegrees(float start, float end)
        {
            return new(Maths.Radians(start), Maths.Radians(end));
        }
    }

    internal struct Region
    {
        internal Positional Positional;
        internal (float Inner, float Outer) Radius;
        internal Region(Positional positional, float innerRadius, float outerRadius)
        {
            Positional = positional;
            Radius = (innerRadius, outerRadius);
        }
    }

    internal static class Positionals
    {
        internal static Positional FrontLeft90 = Positional.FromDegrees(0, 90);
        internal static Positional FrontRight90 = Positional.FromDegrees(-90, 0);
        internal static Positional FrontLeft45 = Positional.FromDegrees(0, 45);
        internal static Positional FrontRight45 = Positional.FromDegrees(-45, 0);
        internal static Positional Front90 = Positional.FromDegrees(-45, 45);
        internal static Positional Front180 = Positional.FromDegrees(-90, 90);

        internal static Positional FlankLeft90 = Positional.FromDegrees(45, 135);
        internal static Positional FlankRight90 = Positional.FromDegrees(225, 315);
        internal static Positional FlankLeftFront = Positional.FromDegrees(45, 90);
        internal static Positional FlankLeftRear = Positional.FromDegrees(90, 135);
        internal static Positional FlankRightFront = Positional.FromDegrees(270, 315);
        internal static Positional FlankRightRear = Positional.FromDegrees(225, 270);

        internal static Positional Rear = Positional.FromDegrees(135, 225);
        internal static Positional RearLeft = Positional.FromDegrees(135, 180);
        internal static Positional RearRight = Positional.FromDegrees(180, 225);

        internal static List<Positional> FrontPositionals(ConfigurationProfile.PositionalsSettings c)
        {
            // todo: make logic cleaner
            if (c.FlankType == FlankRegionSetting.RearOnly) {
                if (c.FrontSeparate) {
                    return new List<Positional> { FrontLeft90, FrontRight90 };
                } else {
                    return new List<Positional> { Front180 };
                }
            } else {
                if (c.FrontSeparate) {
                    return new List<Positional> { FrontLeft45, FrontRight45 };
                } else {
                    return new List<Positional> { Front90 };
                }
            }
        }

        internal static List<Positional> FlankPositionals(ConfigurationProfile.PositionalsSettings c)
        {
            switch (c.FlankType)
            {
                case FlankRegionSetting.Full:
                    return new List<Positional> { FlankLeft90, FlankRight90 };
                case FlankRegionSetting.FullSeparated:
                    return new List<Positional> { FlankLeftFront, FlankLeftRear, FlankRightFront, FlankRightRear };
                case FlankRegionSetting.RearOnly:
                default:
                    return new List<Positional> { FlankLeftRear, FlankRightRear };
            }
        }

        internal static List<Positional> RearPositionals(ConfigurationProfile.PositionalsSettings c)
        {
            return c.RearSeparate
                ? new List<Positional> { RearLeft, RearRight }
                : new List<Positional> { Rear };
        }

        internal static List<(Positional, Brush)> FromConfig(ConfigurationProfile.PositionalsSettings c)
        {
            List<(Positional, Brush)> positionals = new();

            FrontPositionals(c)
                .ForEach(p =>
                {
                    positionals.Add((p, c.BrushFront));
                });

            FlankPositionals(c)
                .ForEach(p =>
                {
                    positionals.Add((p, c.BrushFlank));
                });

            RearPositionals(c)
                .ForEach(p =>
                {
                    positionals.Add((p, c.BrushRear));
                });

            return positionals;
        }
    }

    internal static class Regions
    {
        internal static List<(Region Region, Brush Brush)> FromConfig(ConfigurationProfile.PositionalsSettings c, float meleeRange, float abilityRange, float abilityRangeFar)
        {
            var regions = new List<(Region, Brush)>();

            foreach (var (positional, brush) in Positionals.FromConfig(c))
            {
                regions.Add(new(new Region(positional, 0, meleeRange), brush));

                if (c.MeleeAbilityRange)
                {
                    var abBrush = brush with
                    {
                        Thickness = c.MeleeAbilityThickness
                    };

                    regions.Add(new(new Region(positional, meleeRange, abilityRange), abBrush));
                }
                if (c.MeleeAbilityRangeFar)
                {
                    var abBrush = brush with
                    {
                        Thickness = c.FarAbilityThickness
                    };

                    regions.Add(new(new Region(positional, 0, abilityRangeFar), abBrush));
                }
            }

            return regions;
        }
    }

    internal struct Actor
    {
        internal GameObject GameObject;
        public Actor(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public bool regionContains(Region region, Vector3 pos)
        {
            var range = Maths.DistanceXZ(GameObject.Position, pos);
            var angle = Maths.AngleXZ(GameObject.Position, pos) - GameObject.Rotation;

            return
                region.Radius.Inner < range && range < region.Radius.Outer &&
                Maths.BetweenAngles(angle, region.Positional.StartRads, region.Positional.EndRads);
        }
    }
}
