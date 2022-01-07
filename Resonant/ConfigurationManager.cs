using Dalamud.Plugin;
using System;

namespace Resonant
{
    [Serializable]
    public class ConfigurationManager
    {
        private DalamudPluginInterface? dalamud;

        public ConfigurationManager(DalamudPluginInterface dalamud)
        {
            this.dalamud = dalamud;
        }

        public Configuration GetSavedConfig()
        {
            return this.dalamud!.GetPluginConfig() as Configuration ?? new Configuration();
        }

        public void Save(Configuration configuration)
        {
            this.dalamud!.SavePluginConfig(configuration);
        }
    }
}
