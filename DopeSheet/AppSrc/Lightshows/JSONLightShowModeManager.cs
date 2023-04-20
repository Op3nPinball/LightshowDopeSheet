using System.IO;
using System.Collections.Generic;

using Multimorphic.P3;
using Multimorphic.P3App.Modes;

namespace Op3nPinball.DopeSheet
{
    public class JSONLightShowModeManager : GameMode
    {       
        Dictionary<string, JSONLightShowMode> JSONLightShows;
        List<string> ActiveModes;
        public const string EvtStartLightMode = "Op3n_StartLightMode";
        public const string EvtStopLightMode = "Op3n_StopLightMode";
        
        public JSONLightShowModeManager (P3Controller controller, int priority)
            : base(controller, priority)
        {
            JSONLightShows = new Dictionary<string, JSONLightShowMode>();
            ActiveModes = new List<string>();
            ConstructLightShows();
            AddModeEventHandler(EvtStartLightMode, StartLightModeEventHandler, priority);
            AddModeEventHandler(EvtStopLightMode, StopLightModeEventHandler, priority);
        }

        public override void mode_stopped ()
        {
            if (ActiveModes.Count > 0)
            {
                foreach(var mode in ActiveModes)
                    p3.RemoveMode( JSONLightShows[mode]);
                ActiveModes.Clear();
            }
        }

        public void ConstructLightShows()
        {
            string lightshowPath = Path.Combine(Path.GetFullPath(UnityEngine.Application.streamingAssetsPath), "LightShowConfigs");
            string [] fileEntries = Directory.GetFiles(lightshowPath, "*.json");
            foreach (string jsonFile in fileEntries) {
                string jsonStr = File.ReadAllText(jsonFile);
                string lightshow = Path.GetFileNameWithoutExtension(jsonFile);
                JSONLightShows[lightshow] = new JSONLightShowMode(p3, Priority, jsonStr);
            }
        }

        public bool StartLightModeEventHandler(string eventName, object eventData)
        {
            string modeName = (string)eventData;
            if (JSONLightShows.ContainsKey(modeName) && !ActiveModes.Contains(modeName))
            {
                p3.AddMode(JSONLightShows[modeName]);
                ActiveModes.Add(modeName);
            }
            return EVENT_STOP;
        }
        
        public bool StopLightModeEventHandler(string eventName, object eventData)
        {
            string modeName = (string)eventData;
            if (JSONLightShows.ContainsKey(modeName) && ActiveModes.Contains(modeName))
            {
                p3.RemoveMode(JSONLightShows[modeName]);
                ActiveModes.Remove(modeName);
            }
            return EVENT_STOP;
        }
    }
}
