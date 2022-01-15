using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;

namespace Resonant
{
    public sealed class Plugin : IDalamudPlugin, IDisposable
    {
        public string Name => "Resonant";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private ConfigurationManager ConfigManager { get; init; }
        private ConfigurationProfile ActiveConfig { get; init; }
        private ConfigurationUI ConfigUI { get; init; }
        private DebugUI DebugUI { get; init; }
        private ResonantCore ResonantCore { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            ClientState clientState,
            GameGui gameGui
        )
        {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;

            ConfigManager = new ConfigurationManager(this.PluginInterface);
            ActiveConfig = ConfigManager.GetSavedConfig();

            ConfigUI = new ConfigurationUI(ConfigManager, ActiveConfig);
            DebugUI = new DebugUI(ActiveConfig, clientState);

            ResonantCore = new ResonantCore(ActiveConfig, clientState, gameGui);

            Initialize();
        }

        internal void Initialize()
        {
            CommandManager.AddHandler("/resonant", new CommandInfo(this.HandleSlashCommand)
            {
                HelpMessage = "Toggle configuration",
            });

            PluginInterface.UiBuilder.Draw += () =>
            {
                ConfigUI.Draw();
                ResonantCore.Draw();
                DebugUI.Draw();
            };

            PluginInterface.UiBuilder.OpenConfigUi += () =>
            {
                ActiveConfig.ConfigUIVisible = true;
            };

            // HACK: show config by default if debug is enabled
            if (ActiveConfig.DebugUIVisible)
            {
                ActiveConfig.ConfigUIVisible = true;
            }
        }

        internal void HandleSlashCommand(string command, string args)
        {
            ActiveConfig.ConfigUIVisible = !ActiveConfig.ConfigUIVisible;
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler("/resonant");
            CommandManager.RemoveHandler("/resdbg");

            ResonantCore.Dispose();
            ConfigUI.Dispose();
            DebugUI.Dispose();
        }
    }
}
