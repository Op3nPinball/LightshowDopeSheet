using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace Op3nPinball.DopeSheet
{
    public class ModuleLEDConfigParser : MonoBehaviour
    {
        private ModuleLEDConfig _moduleLEDConfig;
        public void ParseJSON(string jsonPath)
        {
            string json = File.ReadAllText(jsonPath);
            _moduleLEDConfig = JsonConvert.DeserializeObject<ModuleLEDConfig>(json);
        }

        public void CreateObjectsFromJSON(string name)
        {
            if (_moduleLEDConfig != null)
            {
                GameObject LEDs = GameObject.Find("LEDAnimator/LEDs");
                GameObject LEDsFromJson = new GameObject();
                LEDsFromJson.name = name;
                LEDsFromJson.transform.parent=null;
                if (LEDs != null)
                {
                    LEDsFromJson.transform.SetParent(LEDs.transform, false);
                }
                LEDsFromJson.transform.position = new Vector3(280,1260,0);

                LEDsFromJson.AddComponent<AnimationExporter>();

                Dictionary<string, GameObject> groups = new Dictionary<string, GameObject>();
                foreach (LEDConfig ledConf in _moduleLEDConfig.LEDs)
                {
                    GameObject led = new GameObject();
                    led.name = ledConf.Name;
                    
                    Match m = Regex.Match(led.name, @"(^[A-Za-z]+)\d+$");
                    if (m.Success)
                    {
                        if (!groups.ContainsKey(m.Groups[1].Value))
                        {
                            GameObject groupGameObject = new GameObject();
                            groupGameObject.name = m.Groups[1].Value;
                            groupGameObject.transform.SetParent(LEDsFromJson.transform, false);
                            groups[m.Groups[1].Value] = groupGameObject;
                        }
                        led.transform.SetParent(groups[m.Groups[1].Value].transform, false);
                    } 
                    else
                    {
                        led.transform.SetParent(LEDsFromJson.transform,false);
                    }
                    
                    //Op3nUnityLight light = led.AddComponent<Op3nUnityLight>();
                    //light.Label = (string)ledInfo["Label"];
                    RawImage img = led.AddComponent<RawImage>();
                    img.color = new Color(0f, 0f, 0f);
                    RectTransform rt = led.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(10, 10);
                    if (ledConf.Location != null )
                    {
                        Vector3 loc = new Vector3(0,0,0);
                        if (ledConf.Location.Length > 1)
                        {
                            loc.x = ledConf.Location[0];
                            loc.y = ledConf.Location[1];
                        }
                        if (ledConf.Location.Length > 2 )
                        {
                            loc.z = ledConf.Location[2];
                        }
                        rt.anchoredPosition = loc*24;
                    }
                }
            }
            else
            {
                Debug.LogError("_moduleLEDConfig is null");
            }
        }
    }

    // I want this to run without need for the SDK, so we are going to define a custom data class to store the LED info.
    // This is a subset of what the P3 SDK defines for the LEDs.
    public class ModuleLEDConfig {
        public LEDConfig[] LEDs;
    }
    public class LEDConfig {
        public string Name;
        public float[] Location; 
    }
}