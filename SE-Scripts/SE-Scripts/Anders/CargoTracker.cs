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
namespace SpaceEngineers.UWBlockPrograms.CargoTracker
{
    public sealed class CargoTracker : MyGridProgram
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
        public List<IMyTextSurface> screens = null;
        public List<IMyInventory> cargo = null;    

        public void Main(string argument, UpdateType updateSource)
        {
            counter++;
            Echo("Run: " + counter);
            //rotor = new List<IMyMotorAdvancedStator>();


            if (updateSource == UpdateType.Terminal)
            {
                Runtime.UpdateFrequency = UpdateFrequency.Update10;

                screens = new List<IMyTextSurface>();
                cargo = new List<IMyInventory>();


                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocks(blocks);
                blocks = blocks.Where(x => x.CustomData.Contains(argument)).ToList();

                foreach (var item in blocks)
                {
                    if (item is IMyTextSurface)
                    {
                        screens.Add((IMyTextSurface)item);
                    }
                    else if (item is IMyTextSurfaceProvider)
                    {
                        for (int i = 0; i < ((IMyTextSurfaceProvider)item).SurfaceCount; i++)
                        {
                            screens.Add(((IMyTextSurfaceProvider)item).GetSurface(i));
                        }
                    }
                }

                GridTerminalSystem.GetBlocksOfType<IMyEntity>(blocks);

                foreach (var item in blocks)
                {
                    if (item.HasInventory)
                    {
                        cargo.Add(item.GetInventory());
                    }
                }
            }

            long curInv = 0;
            long maxInv = 0;
            double percent = 0;

            foreach (IMyInventory item in cargo)
            {
                curInv += item.CurrentVolume.RawValue;
                maxInv += item.MaxVolume.RawValue;
            }

            percent = curInv / (maxInv / 100);

            int solidBlocks = 20;
            string bar = "|";

            double tmp = Math.Round((solidBlocks * (percent / 100)));
            for (int i = 0; i < solidBlocks; i++)
            {
                if (i < tmp)
                {
                    bar += "¤";
                }
                else
                {
                    bar += " ";
                }
            }
            bar += "|";

            string text = "Inventory: " + Math.Round(percent,2) + "% \n"+ bar;
            screens.ForEach(x => x.WriteText(text));

            Echo("Inventory: " + curInv);
            Echo("Max:       " + maxInv);
            Echo("Percent:   " + percent);
            Echo("Text:      " + text);
        }


        // ------ Code Above ----------
        #region PreludeFooter
    }
}
#endregion
