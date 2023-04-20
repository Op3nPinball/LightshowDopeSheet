using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Op3nPinball.DopeSheet
{
    public class AnimationExporter : MonoBehaviour
    {
        public readonly string LightshowPath = Path.Combine(Path.Combine("Assets", "StreamingAssets"), "LightShowConfigs");
        
        Dictionary<string, Dictionary<string, SortedDictionary<double, LightData>>> lightShows;

        public void BuildLightshows()
        {
            lightShows = new Dictionary<string, Dictionary<string, SortedDictionary<double, LightData>>>();
            AnimationClip[] clips = AnimationUtility.GetAnimationClips(gameObject);

            foreach(AnimationClip clip in clips)
            {
                EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
                var lightKeys = new  Dictionary<string, SortedDictionary<double, LightData>>();
                lightShows[clip.name] = lightKeys;
                foreach (EditorCurveBinding cBinding in curveBindings)
                {

                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, cBinding);
                    if (!lightKeys.ContainsKey(cBinding.path))
                    {
                        lightKeys[cBinding.path] = new SortedDictionary<double, LightData>();
                    }
                    SortedDictionary<double, LightData> timeDict = lightKeys[cBinding.path];

                    for (int k = 0; k < curve.keys.Length; ++k)
                    {
                        double t = curve.keys[k].time;
                        if (!timeDict.ContainsKey(t))
                        {
                            timeDict[t] = new LightData();
                        }
                        timeDict[t].SetColor(cBinding.propertyName, curve.keys[k].value);
                    }
                }
            }

            foreach (string clip in lightShows.Keys)
            {
                LightshowClip newClip = new LightshowClip();
                newClip.Name = clip;
                newClip.LEDCurves = new List<LightshowLEDCurve>();

                foreach (string ledname in lightShows[clip].Keys)
                {
                    string name = ledname;
                    Match m = Regex.Match(ledname, "^\\w+/(\\w+)$");
                    if (m.Success)
                    {
                        name = m.Groups[1].Value;
                    }
                    LightshowLEDCurve curve = new LightshowLEDCurve();
                    curve.Name = name;
                    curve.Keyframes = new List<LightshowKeyframe>();
                    double prevTime = 0;
                    Color prevCol = new Color(0,0,0,0);
 
                    double fade = 0;
                    double duration = 0;
                    LightshowKeyframe keyframe = new LightshowKeyframe();
                    ushort[] p3Color;

                    var kfEnumerator = lightShows[clip][ledname].GetEnumerator();
                    kfEnumerator.MoveNext();
                    // The first frame defines the script start and the value is ignored
                    curve.delay = kfEnumerator.Current.Key;
                    prevTime = kfEnumerator.Current.Key;

                    kfEnumerator.MoveNext();
                    fade = kfEnumerator.Current.Key - prevTime;
                    keyframe = new LightshowKeyframe();
                    keyframe.Time = prevTime;
                    duration = fade;
                    prevCol = kfEnumerator.Current.Value.color;
                    prevTime = kfEnumerator.Current.Key;

                    while (kfEnumerator.MoveNext())   
                    {
                        if (kfEnumerator.Current.Value.color == prevCol)
                        {
                            duration += kfEnumerator.Current.Key - prevTime;
                            prevCol = kfEnumerator.Current.Value.color;
                            prevTime = kfEnumerator.Current.Key;
                        }
                        else
                        {
                            // close off the last light
                            p3Color = new ushort[] {
                                    (ushort)(255*prevCol.r),
                                    (ushort)(255*prevCol.g),
                                    (ushort)(255*prevCol.b),
                                    (ushort)(255*prevCol.a)
                            };
                            keyframe.LightColor = p3Color;
                            keyframe.duration = duration;
                            keyframe.fadeTime = fade;
                            curve.Keyframes.Add(keyframe);

                            // Start next frame
                            keyframe = new LightshowKeyframe();
                            keyframe.Time = prevTime;
                            fade = kfEnumerator.Current.Key - prevTime;
                            duration = fade;
                            prevCol = kfEnumerator.Current.Value.color;
                            prevTime = kfEnumerator.Current.Key;
                        }
                    }
                    // close off the last light
                    p3Color = new ushort[] {
                            (ushort)(255*prevCol.r),
                            (ushort)(255*prevCol.g),
                            (ushort)(255*prevCol.b),
                            (ushort)(255*prevCol.a)
                    };
                    keyframe.LightColor = p3Color;
                    keyframe.duration = duration;
                    keyframe.fadeTime = fade;
                    curve.Keyframes.Add(keyframe);

                    // Add the curve to the clip.
                    newClip.LEDCurves.Add(curve);
                }

                string jsonString = JsonConvert.SerializeObject(newClip, Formatting.Indented);
                // Write to file
                WriteLightshowJSON(clip, jsonString);
            }
        }

        private void WriteLightshowJSON(string clipname, string jsonStr)
        {
            if(!System.IO.Directory.Exists(LightshowPath))
                System.IO.Directory.CreateDirectory(LightshowPath);

            string path = Path.Combine(LightshowPath, string.Format("{0}.json", clipname));
            File.WriteAllText(path, jsonStr); 
        }
    }

    [System.Serializable]
    public class LightshowClip {
        public string Name;
        [SerializeField]
        public List<LightshowLEDCurve> LEDCurves;
    }

    [System.Serializable]
    public class LightshowLEDCurve {
        public string Name;
        public double delay;
        [SerializeField]
        public List<LightshowKeyframe> Keyframes;
    }

    [System.Serializable]
    public class LightshowKeyframe {
        public double Time;
        public double fadeTime;
        public double duration;
        public ushort[] LightColor;
    }

    public class LightData
    {
        public Color color;
        public LightData()
        {
            color = new Color(0,0,0);
        }
        public void SetColor(string channel, float value)
        {
            if (channel == "m_Color.r")
            {
                color.r = value;
            }
            else if (channel == "m_Color.g")
            {
                color.g = value;
            }
            else if (channel == "m_Color.b")
            {
                color.b = value;
            }
            else if (channel == "m_Color.a")
            {
                color.a = value;
            }
            else {
                Debug.LogError("Unexpected property: " + channel);
            }
        }
    }
        
    #if UNITY_EDITOR
    [CustomEditor(typeof(AnimationExporter))]
    [CanEditMultipleObjects]
    public class AnimationExporter_Editor : Editor
    {
        public void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            AnimationExporter _MyScript = (AnimationExporter)target;

            if (GUILayout.Button("Build Lightshows"))
            {
                _MyScript.BuildLightshows();
                Debug.Log("Light Shows Built");
            }
        }

        public void OnSceneGUI()
        {
        }
    }
#endif
}