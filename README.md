# SubstanceTween
What does this Unity tool do?
This tool takes exposed parameters from substance designer files(SBAR) and allows you to create multiple key frames by manipulating the exposed Variables, creating transitions and animating them.
You can Write variables to XML files and read from them as well.
When you are done creating your animated object you can save the object as a Prefab for future use.

V2.5 compatible with Unity 5.6 - 2017.2
older versions compatible with Unity 5.3.2 - 5.5 

v2.4(old): https://www.youtube.com/watch?v=REgXbNlAVPA


Controls(v2.5):
* CTRL + Z – Undo
* CTRL + Y – Redo
* R – Randomize values
* G – Repeat last action
Important Notes:
* This tool is meant for simple/optimized materials. if you have a giant Substance Material that is 2048-2048 or has many exposed parameters near the beginning of the input graph in Substance Designer the tool will lag I recommend 512x512 or 1024x1024.
* In Substance Designer the main Output Size on the graph should be at 0x0 and set to ‘Relative to Parent’. The node outputs should be set to 512x512(Unity Standard) and ‘RelativeToParent’
* Allegorithmic has a video and a page about optimizing substances for unity: https://www.youtube.com/watch?v=VnhIDRcfAWA https://support.allegorithmic.com/documentation/display/SD5/Performance+Optimization+Guidelines
* Expect read XML files or prefabs to break when using different versions of this tool. Always save both.
* Try not to delete objects or rename materials while in play mode (if you do, exit playmode and restart).
* Try not to use the tool on animated prefabs already created with this tool
* If you update your material by adding parameters or changing the order in substance designer, you will still be able to read parameters from XML files as long as the names are the same (ex: Wall_Color == Wall_Color).
* You can play animations on different versions of your material.
* If your material name in the XML file is different than the name of the material in the inspector (Crystal 1.5 != Crystal 1-6 ) a dialogue prompt will ask you if you want to change the name of the material in the XML file when you press the ‘Toggle Animation’ button on a different object.
Supported exposed parameters
* Float
* Vector2
* Vector3
* Vector4
* Color (RGB / RGBA)
* Integers
* Enum
* Booleans
* Output Parameters ($Output Size, $Pixel Size and $Random Seed)

Instructions to use tool:
* Open Project
* Select object with a ProceduralMaterial component attached in the Hierarchy
* Add ‘SubstanceTool’ component from inspector
* Press play
* Change sliders, create keyframes
* When you are ready to save your animated object choose ‘Save-Load’ from the toolbar and select ‘Save Prefab’. It might be a good idea to backup keyframes as XML/JSON files before or after you save a prefab.

Possible future features:
* Lerp between Position/Rotation/Scale
* Audio: instead of animated based on time it will also be able to lerp based on audio frequency's. 
* XML preview: when you save a XML file it will possibly save a prefab/preview image (like material icons in your project view)
* Mobile tester: small mobile version where you can edit materials on the go and send XML files/prefabs to a server
* Save material without creating a prefab: at the moment if all you want to do is change a couple variables without animating them or creating a prefab I would use the default sliders in the inspector view and not use this tool.
Bugs/Possible Bugs/Other Notes:

* Error when creating animated prefabs by dragging object to project view from hierarchy instead of using the built in menu.
* If the icon for an animated prefab is pink, right click it and select ‘Reimport’
* My material is Pink after I create Prefabs? – This should never happen, if it does manually reapply your ProceduralMaterial on your object and everything should work again.  
* My material is completely black/white - This should never happen, if it does reset the material (Right click material in inspector view > reset). 
* The tool is blank! - Select a Procedural material and then select the tool window. If that does not work restart unity or the current scene.
* In rare cases the Reset button on the tool will not work - try selecting a second object, then the first and try again. if that doesn’t work try setting the value to minimum/maximum and then press reset. If all else fails restart the tool or exit/enter playmode again. The tool saves the material when the tool is first opened in play mode or a material is selected for the first time in play mode.
* If your XML file is somehow corrupt (0kb file) and you try to read it unity will give you an error saying something like "XmlException: Document element did not appear. Line 1, position 1". If you read a Xml file that is not corrupt after that without restarting the tool your material might look blurry - To fix this, as soon as you read a corrupt xml file restart the tool and read a file that is not corrupt.
* If a prefab in the scene has any errors or missing data try right clicking the PrefabProperties script and select ‘Revert to prefab’
* For v2.5 - The animation curve values/keys are not supposed to be manipulated in the editor curve window. Only the tangents are supposed to be manipulated 
* Unity 2017.1 might crash when displaying dialog boxes after a long period of time. I noticed this was fixed in 2017.1.1p4
Changelog:

2.5.9 – 2/21/18 – 2017.2.0f3  (2017.3.0 works but gives warnings that can be ignored.   )
Will make full release notes soon:
* The tool is now opened by adding a “SubstanceTool” component to an object that has a ProceduralMaterial attached.
* This tool is now used in the inspector instead of a EditorWindow
* Optimization: Broke script up into smaller Utility scripts.
* Feature: Tool now works while in Edit mode
* Feature: Can update animated prefab with tool instead of using prefabProperties script
* Feature: Replaced Lists with Reorderable lists
* Feature: You can now Add LODs to prefabs 
* Feature: Add/Remove Keyframes in the built in curve editor
* Feature: Created gameplay tools (animation trigger, spawn animated prefabs, prefab follow)
* Feature: Added ability to update parameters for all keyframes.
* Feature: Added button for deleting all keyframes
* Feature: Created timeline for scrubbing through animation.
* Feature: Added scripts for Unit testing. 
* Optimization: Increased Performance for animated prefabs (removed nested for loops)
* Optimization: When creating the prefab I delete non animating parameters
* Optimization: If I choose a preset in the curve editor the window will close.
* Optimization: Made prefabProperties more user friendly.
* Fixed Bug: Reading more than 10 keyframes from XML files would mess up the order of keyframes.


2.5 – 10/20/17 – Unity 5.6.0f3 / 2017.1.1p4 /2017.2.0f3
* Feature: Added Toolbar to use for all main functions
* Feature: Tool can now animate Enum and Boolean values
* Feature: Tool can now animate texture offset(_MainTex)
* Feature: Added ability to change substance settings for performance(ProceduralCacheSize/ProceduralProcessorUsage) 
* Feature: Ability for animation to follow an Animation Curve.
* Feature: Write all keyframes to XML files.
* Feature: Create keyframes from multiple XML files.
* Feature: Flicker can make all parameter types to flicker randomly.
* Feature. Undo/Redo now supported
* Feature: Randomize all types of values (Shortcut:R)
* Feature: Repeat last Action (Shortcut G:)
* Feature: Emission Color now supports HDR colors.
* Feature: Added ‘About’ button that displays  
* Feature: Added ‘Rewrite All Keyframes’ Toggle.
* Feature: Added ‘Animation Delay’ to prefab. You can now have the material load before it animates
* Feature: you can now use some functions of the tool when it is paused
* Material: Made a new sample material called ‘Geometry’
* Optimization: Use Dictionaries to store most animation data instead of lists for better performance.
* Optimization: On the prefab I now delete any variables that are not being animated and cache the variables that are being animated.
* Optimization: Changed if/else chain in Update() to switch statements
* Optimized several substance materials 
*  Warning appears on top of the screen if you use tool for a prefab that already has procedural Animation
* Fixed Bug: When animating backwards the animation would follow the graph but it would 
repeat the curve between the first 2 curve points.
* Fixed Bug: When the BackAndForth animation reaches the end the texture would flash for a second.
* Fixed Bug: overwriting a keyframe would overwrite the wrong keyframe
* Fixed Bug: Changed window type to utility to stop errors when dragging the window.
* Fixed Bug: Removed empty path errors when canceling any dialog box (Write/read xml,prefabs,Debug Text)
* Fixed Bug: Emission Color would always reset to whatever is in the Color field
* Fixed Bug: Unity 2017.2 changed when OnValidate() is called. onValidate will now only trigger on the prefab when you are not using the tool
* Fixed Bug: Unity 2017.1 and Unity 2017.2 had diffrences when selecting objects with no Procedural Materials or renderers 
* Removed FPS counter (use built in stats or profiler window instead)

2.4 – 1/29/17- Unity 5.5
* Added Support for Multiple Keyframes!
* Now supports two types of animation, “Loop” and “Back and Forth”. If your animation type is Loop your Animation will play until it hits the last keyFrame, afterwards it will restart and go to the first keyframe.  (example: 1-2-3, 1-2-3, 1-2-3)
* If you have Back and forth selected your animation will go until the last keyframe and then start animating backwards. (example: 1-2-3-2-1, 1-2-3-2-1, 1-2-3-2-1)
* Fixed bug: The tool would allow you to make keyframes with negative animationTime or no animationTime at all, if you do this now a dialogue box will pop up and tell you that you cannot have an animation Time that is 0 or negative.
* Fixed bug: In 2.3 and 2.2 every frame I checked each variable for transition One and transition Two to check for a difference. If there was no difference it wouldn’t change, I did this to hopefully increase performance but if those variables do change later on and the animation restarts, those values will not revert back. To fix this I removed the check and every value will attempt to animate even if they don’t change between keyframes. I have not noticed a performance drop.  

2.3 – 1/5/17 Unity 5.4/5.5 
* Removed old/useless code
* Made new functions called AddProceduralVariablesToList and SetProceduralVariablesFromList so I don’t have to keep writing the same code every time I want to add or set Procedural variables to or from a list. 
* Changed "Lerp Time" label/variable to "Animation time" to make it less confusing to users.
* Made it so creating a second transition would not animate automatically, you now have to create 2 transitions and then click on the "Toggle Animation on/off" button.
* Added colors to the transition and animation buttons. When you create a first or second transition the font color will turn green to indicate you have created a keyframe. When you create two transitions the "Toggle Animation on/off" button will change from red to green. When the button is green it means that you can create an animation from the two keyframes you made.
* Fixed bug where setting Animation Time to 0 or making it have no value would crash unity (It would try to divide by 0) The animation time field also supports basic operations (5*2, 10/2, 10%4).
* When creating a debug log if two lines right next to each other are the same it will only write one of them.
* Fixed bug where creating or reading XML file it would tell you the amount of float properties is the same as the amount of total properties. 
* Added a Header for the tool 
* When you Create a transition or a XML file it will now add the name of the material as “PropertyMaterialName”.
* Fixed bug when creating an animation and selecting another object, the new object would try to animate based on values of the last selected object. Now when you select “Toggle Animation” it will compare the name of the substance material and PropertyMaterialName(you save this when you create a transition) if the values are different it will give you a chance to rename it 
* Added “Rebuild Textures Immediately” Button to Tool and Created Prefab.  Having this on will Trigger an immediate rebuild of this ProceduralMaterial and will Cause FPS to drop.
* Fixed bugs when pausing the game and selecting other objects.
* Fixed bug where you couldn’t create a second transition before the first.
* I created myKeys and myValues list so that I can set the parameters based on the parameter name. This allows the user to switch the order of the parameters in substance designer and bring it back in Unity to make changes without messing up the values, The PropertyName, PropertyFloat, PropertyColor, PropertyVector lists will now only be used for keeping track of the different types and amounts of variables.  At the moment this does not work with variables that have been renamed, you will have to manually rename the variables in the XML file. 
* Added FPS counter near sun controller (worldGUI script) (http://wiki.unity3d.com/index.php?title=FramesPerSecond)
* Fixed bug where I was accidently updating Procedural Material twice per frame.
* Added buttons to select the transitions you have created as well as a button to delete them.
* When you create a prefab it saves the animation time. 
* Added Feature/ Fixed bug: When Writing xml files it would add parameter types with no range ($outputSize, $pixelSize, $RandomSeed) this could slow down your animation and make it choppy (loading a 4096x4096 animation on a material that is meant to be 1024x1024 ) Saving these parameters could  be useful so I made a toggle button called ‘Save Output Parameters’
* Added support for Unity 5.3. In 5.4 and 5.5 I use RebuildTextures() instead of RebuildTexturesImmediately() when updating materials because it is faster.  
RebuildTextures() is not supported in 5.3 when I use it in Update() so I made a conditional statement that detects what version of Unity you are using.  

2.2 – 11/29/16
• Added a button for debug information (lists changes for materials, saves information about written/read Xml files and created prefabs).
• Fixed bug where the created prefab would not have any saved values unless you selected it before exiting play mode.
• Fixed bug when resetting vectors or trying to set vectors to the minimum/maximum would do nothing.
• Fixed bug where if you select objects with no renderer or procedural material in a specific order it would crash the tool. 
• Fixed bug where if you selected objects, restarted the tool and created a prefab it would make the material pink (Unity material error).
• Made it so you cannot open the tool from Window>SubstanceTween if you have nothing selected or if the object does not have the required component(Renderer/ProceduralMaterial).

2.1 - Unity 5.4
• Remade XML read/write (Now using save box instead of manually typing in file path)
• Remade transition/reset buttons
• Remade Prefab button (Now with save box and you are able to name prefabs)
• Remade Sun controls from the old version of the old tool to a separate script on the directional light (Not part of tool yet)
• Recreated _EmissionColor and _MainTex

2.0 - Unity  5.3.2
• Added Position slider
• Remade tool using editor scripting instead of using old OnGUI code (Lacks any buttons including XML read/write, resetting to default or saving object as prefab)
• cant edit _EmissionColor or _MainTex
• Color picker instead of sliders
• Converted all old Arraylists into Lists
1.0 - 1.9
• Old version of tool that was written with Unity 4 OnGUI code on the gameobject instead of in the editor. 
• In these versions you had to manually type the path for XML files instead of using a save/open box.
• Used old Arraylists



