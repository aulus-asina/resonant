using System;
using Dalamud.Data;
using Dalamud.Game.ClientState;

namespace Resonant
{
    internal class GameStateObserver
    {
        ClientState ClientState { get; }
        DataManager DataManager { get; }

        // todo: use enum value from Lumina instead of string abbreviation
        string? CurrentJobAbbrev;

        public event EventHandler<string> JobChangedEvent;

        // todo: figure out why c# is giving a warning about non-nullable event
        internal GameStateObserver(ClientState clientState, DataManager dataManager)
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