# LightshowDopeSheet
Utility for writing P3 Lightshows using Unity Animations. This is for use with developing applications for the P3 Pinball platform. For more information on the P3 and P3 SDK please visit [multimorphic.com](https://www.multimorphic.com)

We are not associated with or endorsed by Multimorphic. Just some enthusiasts trying to build cool stuff.

Join the conversation at the [P3 Owners & Fans Discord](https://discord.gg/GuKGcaDkjd)

# Setup

## Generating configuration files

The utility requires the LED configurations from the P3 modules with the names and locations of the LEDs to work. This information is available at runtime from the module driver.

There is a code snippet in ``DopeSheet/AppSrc/AttractExample.cs`` that can be added to the P3SAAttractMode and then pressing 'L' in attract mode with save a json file with the module config. These written to ``LEDConfigs``. By rerunning with each of the modules, you can generate all the configs.

> Note: As of 2023/04/20, LL-EE does not has LED positions. You will still be able to generate lightshows with this utility, but the previews will not work.

You should only need to do this step once, but if a module updates its definitions in the future, you may need to regenerate them.

## Importing LEDs into Unity

Load the provided Unity project ``DopeSheet`` in Unity 5.6.7 and open the ``LightShowEditor`` Scene.

Under the Op3nPinball dropdown menu, there is an ``LED Definition Import`` item that will open a window. Click ``Select File`` and select one of the json files generated in the previous section. Clicking ``Create Objects From JSON`` will create a hierarchy of objects rooted at the module name under ``LEDManager/LEDs``. You can import multiple modules, but should only have one active at a time. This functions as a standalone project that does not require the P3SDK, but you can copy this code into your project and use it with slight modification in your game if desired.

# Creating a Lightshow

## Important Notes

 * **The first keyframe indicates the start of the script and is otherwise ignored in the generated lightshow.** (Note this should be the frame after the keyframe, I need to change that). This will however impact the visualization. By convention we either set this first frame to black, or our expected last color (aka, our default for the light in our scene).
 * **The final keyframe defines the end of the script** this is less unusual and is probably what you would expect.
 * **Curves are completely ignored.** We use the default fade functionality of the LEDScript for the blends.
 * **Looping does not match between animation and the lightshow.** Unity will loop the animation as a whole, where as we can define our LEDScripts to start and stop at different times using the first 2 conventions. It is generally recommended that you have a keyframe for every LED being changed at the first and last frame of the animation. In this way Unity and the P3 lightshow will match very closely.

## Example of a Lightshow

It is probably easiest to understand how to use it by following an example. Let's create a lightshow for Heist.

 * We assume you have loaded the project, and imported the Heist LEDs.
 * Select the Heist object and in the Animation tab, click ``Create``. Note: The name of the animation will be the name of used in the application for the lightshow. We create ``Animations/WallScoopSweep.anim``.
 ![Screenshot of Creating a new animation.](/assets/images/Create.png)
 * We create a first keyframe for all the Walls and Scoops at frame 0.
 ![Screenshot of keyframe zero.](/assets/images/FirstFrame.png)
 * We will copy and paste that to frame 1 to start our actual animation will all lights off.
 * Copy the all off frame keyframe to frame 61 as well.
 * You have now written your first lightshow! It is super boring, it sets all the walls and scoops to off with no fade, and keeps them in that state for 1 second.
 ![Screenshot of keyframe zero.](/assets/images/OffLightshow.png)
 * Add in new keys frames that are Black, Pink, Pink, Black. You can edit all colors at the same time by multi-selecting the lights. I ended up pushing out my animation time to 2 seconds because I likes it more. Make something that looks like this. The highlighted keyframes are the pink ones.
 ![Screenshot of keyframe zero.](/assets/images/OnOff.png)
 * This now animates something like this.
 ![Screenshot of keyframe zero.](/assets/images/OnOff.gif)
 * We could export this as it is, but we wanted to make a sweep, not just blinking on off. We can select and slide the frames to offset when each light turns on to produce the desired result.
 * The dope sheet allows us to multiselect and slide keyframes. Let's select all everything by scoop1 and wall1 and shift the main animation 5 steps to the right.
 ![Screenshot of keyframe zero.](/assets/images/Slide.png)
 * Continue to cascade them over to the right 5 frames and we end up with the desired lightshow.
 ![Screenshot of keyframe zero.](/assets/images/Sweep.gif)

 # Exporting the lightshow for use in your app.

 Select the Heist object and in the inspector there is a Animation Exporter component. Click the ``Build Lightshow`` button. This will export all the animation clips on that animator to the ``Assets/StreamingAssets`` directory. These json files can be used with the provided example GameModes.

 # Using the lightshow in your app.

 The In ``DopeSheet\AppSrc\Lightshows`` we provide 2 modes for use in your app. We recommend copying these into ``Assets\Op3nPinball\Scripts\Modes\Lightshows`` in your app (or P3SA). These depend on ``Op3nPinball\Scripts\AnimationExporter.cs``, so you will need to copy that over as well. If you add the ``JSONLightShowModeManager`` and start the mode in say your attract mode, you can enable your WallScoopSweep lightshow by calling 

 ```
 PostModeEventToModes(JSONLightShowModeManager.EvtStartLightMode, "WallScoopSweep");
 ```

> Note: Our code depends on Newtonsoft json. We chose to use the same version as is currently included in the SDK. So when if you import everything from this project into P3SA, it may cause problems and you may need to delete ``Assets\Plugins\Newtonsoft``

# License

The code in this project is providing under the MIT License. See [License](LICENSE) for complete details.

Newtonsoft json is included under the terms in the license included [DopeSheet/Assets/Plugins/Newtonsoft/license.txt](/DopeSheet/Assets/Plugins/Newtonsoft/license.txt)