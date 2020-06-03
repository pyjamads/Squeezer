_Game Feel Descriptions_

A small "non-invasive" tool for easily adding effects of many different types to a game prototype. The tool is "non-invasive" in the sense that descriptions describe effects to be executed when some event happens in the game. Collisions, movement, destruction, startup, and custom events can all trigger various effects. Effects can be executed on many different targets, relative or absolute, general or singular.

If you are looking to quickly add tween effects, manipulate material colors, manipulate time scale, add screen shake, play sound effects and other simple tricks to make your prototype feel better, this tool can do it.

__Requirements__

This tool requires Unity 2019.3 or above.

__Usage__

Game Feel Descriptions are meant to be used for executing various collections of effects when certain events happen.
A Description attaches itself to Game Objects with a given Tag or ComponentType attached. Each Description contains a list of Triggers, that determine when effects should be executed. Each Trigger in turn contains a list of EffectGroups, that determine where the effects are applied. Lastly each EffectGroup contains a list of effects to execute.
This structure allows the definition of groups of effects to execute, when events such as collision, movement, destruction, startup etc. happen on the attached object and execute them on targets such as Self, Collider (Other), Objects with Tag or a specific Component. 

It is also possible to create and execute effects directly from code, using the GameFeelEffectExecutor instance. Simply create and initialize and effect with a given target, and Queue the effect.

Effects can contain a list of effects to execute once they complete, which allows you to create small effect trees, to be executed in the specified order.

___UI___

The custom Description inspector, gives a great overview of the triggers, groups and effect trees. You can right click on any item, to add or remove elements in that part of the tree, and left click to open the specific element in the tree, when you want to adjust the settings of that element.

__Setup__

The tool has a small untiy men item "GameFeelDescriptions > Setup From Tags", which allows you to automatically create descriptions that attach to each of the Tags defined in your project. Default tags can be excluded, and you can choose to enabled "Step Through Mode" on the generated descriptions.

__Step Through Mode__

Descriptions can have "Step Through Mode" enabled, which allows you to add list of generated effects to execute when a specific collision happens, or simply pause your game in the editor and allow you to edit the effects manually.
When "Step Through Mode" is enabled, all changes to the Description are saved when you exit Play Mode in Unity, but they can be reverted by using the standard Unity Undo functionality. "Step Through Mode" only applies to collision triggers, but it can alternatively be enabled directly on a specific EffectGroup, which allows the same addition or manual changes to be made when that specific event happens. Any changes to the EffectGroup will be saved when you exit Play Mode.