// Add this in your Attract mode and press "L" to export the light locations

#if UNITY_EDITOR
        public bool sw_launch_active(Switch sw)
		{            
            Op3nPinball.DopeSheet.ModuleLEDConfig config = new Op3nPinball.DopeSheet.ModuleLEDConfig();
            List<Op3nPinball.DopeSheet.LEDConfig> leds = new List<Op3nPinball.DopeSheet.LEDConfig>();
            
            foreach (LED led in p3.LEDs.Values)
            {
                Op3nPinball.DopeSheet.LEDConfig ledConfig = new Op3nPinball.DopeSheet.LEDConfig();
                ledConfig.Name = led.Name;
                if (led.Location != null && led.Location.Length == 2)
                    ledConfig.Location = new float[]{led.Location[0], led.Location[1], 0};
                else
                    ledConfig.Location = led.Location;
                leds.Add(ledConfig);
            }
            config.LEDs = leds.ToArray();
            string moduleName = p3.Config.Game.Name;
            if(!System.IO.Directory.Exists("LEDConfigs"))
                System.IO.Directory.CreateDirectory("LEDConfigs");
            string path = System.IO.Path.Combine("LEDConfigs", moduleName + ".json");

            // Write to file
            System.IO.File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
            Multimorphic.P3.Logging.Logger.Log(path + " written with LEDConfig.");
			return SWITCH_CONTINUE;
		}
    }
#endif