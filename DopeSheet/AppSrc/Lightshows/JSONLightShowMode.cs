using System.Collections.Generic;

using Multimorphic.NetProcMachine.LEDs;
using Multimorphic.P3;
using Multimorphic.P3App.Modes;
using Newtonsoft.Json;

namespace Op3nPinball.DopeSheet
{
    public class JSONLightShowMode : GameMode
    {       
        List<LEDScript> LocalLEDScripts;
        Dictionary<LEDScript, LEDScriptInfo> ledScriptInfoDict;
        public JSONLightShowMode (P3Controller controller, int priority, string json)
            : base(controller, priority)
        {
            LightshowClip lightShowClip = JsonConvert.DeserializeObject<LightshowClip>(json);
            
            LocalLEDScripts = new List<LEDScript>();
            ledScriptInfoDict = new Dictionary<LEDScript, LEDScriptInfo>();

            foreach (LightshowLEDCurve curve in lightShowClip.LEDCurves)
            {
                LEDScript script = new LEDScript(p3.LEDs[curve.Name], priority);
                foreach (var keyframe in curve.Keyframes) 
                {
                    script.AddCommand(keyframe.LightColor, keyframe.FadeTime, keyframe.Duration);
                }
                LEDScriptInfo info = new LEDScriptInfo();
                info.Name = curve.Name;
                info.delay = curve.delay;
                info.duration = -1;
                ledScriptInfoDict[script] = info;
                LocalLEDScripts.Add(script);
            }
        }

        public override void mode_started ()
        {
            for (int i=0; i<LocalLEDScripts.Count; i++)
            {
                p3.LEDController.AddScript(
                        LocalLEDScripts[i],
                        ledScriptInfoDict[LocalLEDScripts[i]].duration,
                        ledScriptInfoDict[LocalLEDScripts[i]].delay);
            }
            base.mode_stopped();
        }

        public override void mode_stopped ()
        {
            for (int i=0; i<LocalLEDScripts.Count; i++)
            {
                p3.LEDController.RemoveScript(LocalLEDScripts[i]);
            }
            base.mode_stopped();
        }
    }

    public class LEDScriptInfo
    {
        public string Name;
        public double delay;
        public double duration;
    }
}
