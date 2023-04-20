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
        
        private Dictionary<string, Dictionary<string, SortedDictionary<int, LightData>>> _lightShows;
        private Dictionary<string, float> _frameRates;

        public void BuildLightshows()
        {
            _lightShows = new Dictionary<string, Dictionary<string, SortedDictionary<int, LightData>>>();
            _frameRates = new Dictionary<string, float>();
            AnimationClip[] clips = AnimationUtility.GetAnimationClips(gameObject);

            foreach(AnimationClip clip in clips)
            {
                EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
                var lightKeys = new  Dictionary<string, SortedDictionary<int, LightData>>();
                _lightShows[clip.name] = lightKeys;
                _frameRates[clip.name] = clip.frameRate;
                foreach (EditorCurveBinding cBinding in curveBindings)
                {

                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, cBinding);
                    if (!lightKeys.ContainsKey(cBinding.path))
                    {
                        lightKeys[cBinding.path] = new SortedDictionary<int, LightData>();
                    }
                    SortedDictionary<int, LightData> timeDict = lightKeys[cBinding.path];

                    for (int k = 0; k < curve.keys.Length; ++k)
                    {
                        int frameNum = Mathf.RoundToInt((curve.keys[k].time)*((float)clip.frameRate));
                        if (!timeDict.ContainsKey(frameNum))
                        {
                            timeDict[frameNum] = new LightData();
                        }
                        timeDict[frameNum].SetColor(cBinding.propertyName, curve.keys[k].value);
                    }
                }
            }

            foreach (string clip in _lightShows.Keys)
            {
                LightshowClip newClip = new LightshowClip();
                newClip.Name = clip;
                newClip.FrameRate = _frameRates[clip];
                newClip.LEDCurves = new List<LightshowLEDCurve>();

                foreach (string ledname in _lightShows[clip].Keys)
                {
                    int offset = 0;
                    string name = ledname;
                    Match m = Regex.Match(ledname, "^\\w+/(\\w+)$");
                    if (m.Success)
                    {
                        name = m.Groups[1].Value;
                    }
                    LightshowLEDCurve curve = new LightshowLEDCurve();
                    curve.Name = name;
                    curve.Keyframes = new List<LightshowKeyframe>();
                    int prevTime = 0;
                    Color prevCol = new Color(0,0,0,0);

                    var kfEnumerator = _lightShows[clip][ledname].GetEnumerator();
                    kfEnumerator.MoveNext();
                    // The first frame defines the script start and the value is ignored
                    int fade = 0;
                    int duration = 0;
                    LightshowKeyframe keyframe = new LightshowKeyframe(_frameRates[clip]);
                    ushort[] p3Color;
                    offset = 1;
                    curve.delay = (double)kfEnumerator.Current.Key / (double)_frameRates[clip];
                    prevTime = kfEnumerator.Current.Key + offset;

                    kfEnumerator.MoveNext();
                    fade = (kfEnumerator.Current.Key - prevTime);
                    keyframe = new LightshowKeyframe(_frameRates[clip]);
                    keyframe.SetTime(prevTime - offset);
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
                            keyframe.SetDuration(duration);
                            keyframe.SetFadeTime(fade);
                            curve.Keyframes.Add(keyframe);

                            // Start next frame
                            keyframe = new LightshowKeyframe(_frameRates[clip]);
                            keyframe.SetTime(prevTime - offset);
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
                    keyframe.SetDuration(duration);
                    keyframe.SetFadeTime(fade);
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
        public float FrameRate;
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
        public double FadeTime;
        public double Duration;
        private float FrameRate;
        public ushort[] LightColor;
        public LightshowKeyframe() {}
        public LightshowKeyframe(float frameRate)
        {
            FrameRate = frameRate;
        }

        public void SetTime(int frameNum) {
            if (FrameRate == 0) {
                return;
            }
            Time = frameNum / (double) FrameRate;
        }
        public void SetFadeTime(int frameNum) {
            if (FrameRate == 0) {
                return;
            }
            FadeTime = frameNum / (double) FrameRate;
        }
        public void SetDuration(int frameNum) {
            if (FrameRate == 0) {
                return;
            }
            Duration = frameNum / (double) FrameRate;
        }
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