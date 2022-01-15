using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;

namespace Resonant
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Resonant";

        private DalamudPluginInterface DalamudInterface { get; }
        private CommandManager CommandManager { get; }
        private ConfigurationManager ConfigManager { get; }

        private ConfigurationUI ConfigUI { get; }
        private DebugUI DebugUI { get; }
        private ResonantCore ResonantCore { get; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface dalamudInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            ClientState clientState,
            GameGui gameGui
        )
        {
            DalamudInterface = dalamudInterface;
            CommandManager = commandManager;

            ConfigManager = new ConfigurationManager(DalamudInterface);

            ConfigUI = new ConfigurationUI(ConfigManager);
            DebugUI = new DebugUI(ConfigManager, clientState);

            ResonantCore = new ResonantCore(ConfigManager, clientState, gameGui);

            Initialize();
        }

        internal void Initialize()
        {
            CommandManager.AddHandler("/resonant", new CommandInfo(HandleSlashCommand)
            {
                HelpMessage = "Toggle configuration",
            });

            DalamudInterface.UiBuilder.Draw += Draw;

            DalamudInterface.UiBuilder.OpenConfigUi += () =>
            {
                ConfigManager.ConfigUIVisible = true;
            };

            // hack: show config by default if debug is enabled
            if (ConfigManager.DebugUIVisible)
            {
                ConfigManager.ConfigUIVisible = true;
            }
        }

        internal void Draw()
        {
            ConfigUI.Draw();
            ResonantCore.Draw();
            DebugUI.Draw();
        }

        internal void HandleSlashCommand(string command, string args)
        {
            ConfigManager.ConfigUIVisible = !ConfigManager.ConfigUIVisible;
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler("/resonant");
        }
    }
}
