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
namespace SpaceEngineers.UWBlockPrograms.BadgerMiner
{
    public sealed class BadgerMiner : MyGridProgram
    {
        // ------- Code Below --------

        public List<IMyTerminalBlock> blocks;
        public int counter = 0;
        public int blocksCount = 0;
        public string argument;
        public IMyCockpit cockpit;

        public IMyTextSurface cargoScreen;
        public List<IMyInventory> inventories;
        public int BAR_STEPS = 20;
        public int BAR_ROWS = 1;
        
        public IMyTextSurface angleScreen;
        public IMyMotorAdvancedStator rightRotor;

        public void Initialise()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            List<IMyCockpit> cockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cockpits);
            cockpit = cockpits.FirstOrDefault();
           

            //Instantiate Cargo Tracking
            cargoScreen = cockpit.GetSurface(int.Parse(argument.Split(' ')[1]));

            inventories = new List<IMyInventory>();

            foreach (var item in blocks.Where(x => x.HasInventory))
            {
                inventories.Add(item.GetInventory());
            }
            Echo("Inventories found: " + inventories.Count());

            //Instantiate Rotation tracking
            angleScreen = cockpit.GetSurface(int.Parse(argument.Split(' ')[2]));

            rightRotor = blocks.Where(x => x.CustomData.Contains(argument.Split(' ')[0]) && x is IMyMotorAdvancedStator).First() as IMyMotorAdvancedStator;
            Echo((rightRotor != null ? "Rotor found" : "Rotor NOT found"));

        }

        public void Program()
        {
            // Remember to remove void as return type.
            /*if (string.IsNullOrEmpty(argument) && Storage.Length > 0)
            {
                argument = Storage;
                Initialise();
            }*/
        }

        public void Save() 
        {
            //Storage = argument;
        }

        public void Main(string _argument, UpdateType updateSource)
        {
            counter++;
            Echo("Run: " + counter);

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(blocks);

            if (blocks.Count != blocksCount || updateSource == UpdateType.Terminal)
            {
                if (!string.IsNullOrEmpty(_argument))
                {
                    Echo(argument);
                    argument = _argument;
                }

                Initialise();
            }

            PrintCargo();
            PrintRotation();
        }

        public void PrintCargo() 
        {
            long curInv = 0;
            long maxInv = 0;
            double percent = 0;

            foreach (IMyInventory item in inventories)
            {
                curInv += item.CurrentVolume.RawValue;
                maxInv += item.MaxVolume.RawValue;
            }

            percent = curInv / (maxInv / 100);

            string bar = "|";

            double tmp = Math.Round(((BAR_STEPS * BAR_ROWS) * (percent / 100)));

            for (int i = 0; i < (BAR_STEPS * BAR_ROWS); i++)
            {
                if (i % BAR_STEPS == 0 && i != 0 && i != (BAR_STEPS * BAR_ROWS))
                {
                    bar += "|\n|";
                }

                if (i < tmp)
                {
                    bar += "#";
                }
                else
                {
                    bar += "..";
                }
            }
            bar += "|";

            string text = "Inventory: " + Math.Round(percent, 2) + "% \n" + bar;
            cargoScreen.WriteText(text);

            Echo("Inventory: " + curInv);
            Echo("Max:       " + maxInv);
            Echo("Percent:   " + percent);
            Echo("Text:      " + text);
        }

        public void PrintRotation() 
        {
            float curAngle = (rightRotor.Angle * 180) / (float)(Math.PI);

            string text = "Angle\n" + Math.Round(curAngle - rightRotor.LowerLimitDeg);
            angleScreen.WriteText(text);

            Echo("Angle:     " + curAngle);
            Echo("Min:       " + rightRotor.LowerLimitDeg);
            Echo("Max:       " + rightRotor.UpperLimitDeg);
            Echo("Text:      " + text);
        }

        // ------ Code Above ----------
        #region PreludeFooter
    }
}
#endregion
