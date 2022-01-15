using Dalamud.Plugin;
using System;

namespace Resonant
{
    [Serializable]
    public class ConfigurationManager
    {
        private DalamudPluginInterface DalamudPlugin;

        public ConfigurationManager(DalamudPluginInterface dalamudPlugin)
        {
            DalamudPlugin = dalamudPlugin;
        }

        public Configuration GetSavedConfig()
        {
            return DalamudPlugin.GetPluginConfig() as Configuration ?? new Configuration();
        }

        public void Save(Configuration configuration)
        {
            DalamudPlugin.SavePluginConfig(configuration);
        }
    }
}
