using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;

namespace Resonant
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Resonant";

        private IDalamudPluginInterface DalamudInterface { get; }
        private ICommandManager CommandManager { get; }
        private ConfigurationManager ConfigManager { get; }

        private ConfigurationUI ConfigUI { get; }
        private DebugUI DebugUI { get; }
        private ResonantCore ResonantCore { get; }

        public Plugin(
            IDalamudPluginInterface dalamudInterface,
            ICommandManager commandManager,
            IClientState clientState,
            IGameGui gameGui,
            IDataManager dataManager,
            IPluginLog logger
        )
        {
            DalamudInterface = dalamudInterface;
            CommandManager = commandManager;

            ConfigManager = new ConfigurationManager(DalamudInterface);

            ConfigUI = new ConfigurationUI(ConfigManager, dataManager);
            DebugUI = new DebugUI(ConfigManager, clientState);

            ResonantCore = new ResonantCore(ConfigManager, clientState, gameGui, dataManager, logger);

            Initialize();
        }

        internal void Initialize()
        {
            CommandManager.AddHandler("/resonant", new CommandInfo(HandleSlashCommand)
            {
                HelpMessage = "Toggle configuration",
            });

            DalamudInterface.UiBuilder.Draw += Draw;

            DalamudInterface.UiBuilder.OpenMainUi += () =>
            {
                ConfigManager.ConfigUIVisible = true;
            };

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
