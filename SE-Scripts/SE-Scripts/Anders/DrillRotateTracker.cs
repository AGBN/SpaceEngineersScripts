#region Prelude
using System;
using System.Collections.Generic;
using System.Linq;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
#endregion

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.DrillRotateTracker
{
    public sealed class DrillRotateTracker : MyGridProgram
    {
        // ------- Code Below --------

        public void Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set RuntimeInfo.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public int counter = 0;
        public IMyTextSurface screen = null;
        public IMyMotorAdvancedStator rotor = null;
        public float minAngle = 0;
        public float maxAngle = 0;

        public void Main(string argument, UpdateType updateSource)
        {
            counter++;
            Echo("Run: " + counter);
            //rotor = new List<IMyMotorAdvancedStator>();


            if (updateSource == UpdateType.Terminal)
            {

                string target = argument.Split(' ')[0];

                Runtime.UpdateFrequency = UpdateFrequency.Update10;

                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyCockpit>(blocks);
                screen = (blocks.Where(x => x.CustomData.Contains(target)).FirstOrDefault() as IMyCockpit).GetSurface(int.Parse(argument.Split(' ')[1]));

                GridTerminalSystem.GetBlocksOfType<IMyMotorAdvancedStator>(blocks);
                foreach (var item in blocks)
                {
                    if (item.CustomData.Contains(target))
                    {
                        rotor = item as IMyMotorAdvancedStator;
                    }
                }

                minAngle = rotor.LowerLimitDeg;
                maxAngle = rotor.UpperLimitDeg;
            }

            float curAngle = (rotor.Angle * 180) / (float)(Math.PI);
            string text = "Angle\n" + Math.Round(curAngle - minAngle);
            screen.WriteText(text);

            Echo("Angle: " + curAngle);
            Echo("Min:     " + minAngle);
            Echo("Max:    " + maxAngle);
            Echo("Text:    " + text);
        }


        // ------ Code Above ----------
        #region PreludeFooter
    }
}
#endregion
