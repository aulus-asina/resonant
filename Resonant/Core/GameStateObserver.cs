using System;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Plugin.Services;

namespace Resonant
{
    internal class GameStateObserver
    {
        IClientState ClientState { get; }
        IDataManager DataManager { get; }

        // todo: use enum value from Lumina instead of string abbreviation
        string? CurrentJobAbbrev;

        public event EventHandler<string>? JobChangedEvent;

        internal GameStateObserver(IClientState clientState, IDataManager dataManager)
        {
            ClientState = clientState;
            DataManager = dataManager;

            CurrentJobAbbrev = CurrentJob();
        }

        internal void Observe()
        {
            var observedClassJob = CurrentJob();

            if (observedClassJob != CurrentJobAbbrev) {
                CurrentJobAbbrev = observedClassJob;
                JobChangedEvent?.Invoke(this, observedClassJob);
            }
        }

        private string CurrentJob()
        {
            return ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation ?? "UNKNOWN";
        }
    }
}