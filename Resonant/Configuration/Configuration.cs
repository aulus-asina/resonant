using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Resonant
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public Guid ActiveProfileID;
        public List<ConfigurationProfile> Profiles = new();

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
        public WindowBoxSettings ViewportWindowBox = new();

        public bool Debug = false;

        internal Configuration()
        {
        }

        internal ConfigurationProfile Active
        {
            get
            {
                return Profiles.Find(p => p.ID == ActiveProfileID)
                    ?? Profiles.FirstOrDefault()
                    ?? FillDefaultProfile();
            }
            set {
                ActiveProfileID = value.ID;
            }
        }

        internal ConfigurationProfile FillDefaultProfile()
        {
            var profile = new ConfigurationProfile("Default");
            Profiles.Add(profile);
            Active = profile;
            return profile;
        }

        internal ConfigurationProfile? ProfileForClassJob(string classJobAbbreviation)
        {
            return Profiles.Find((p) => p.Jobs.Contains(classJobAbbreviation));
        }
    }
}
