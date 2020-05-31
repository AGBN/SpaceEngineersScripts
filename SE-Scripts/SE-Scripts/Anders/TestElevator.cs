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
namespace SpaceEngineers.UWBlockPrograms.TestElevator
{
    public sealed class TestElevator : MyGridProgram // K miner
    {
        // ------- Code Below --------

        int counter;
        string argument;
        List<IMyTerminalBlock> blocks;
        IMyPistonBase piston1;
        Vector3D lastPos;
        double DISTANCE_DIFF = 0.05;
        bool lastPosSet = false;

        /*IMyTextPanel screen;
        List<string> screenText;*/

        public void Write(string t, int nr) 
        {
            /*
            for (int i = 0; i <= nr; i++)
            {
                if (screenText.Count() <= nr)
                {
                    screenText.Add("");
                }
            }

            screenText[nr] = t;
            string text = "";

            foreach (var item in screenText)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    text += item + "\n";
                }
            }
            screen.WriteText(text);
            */

            Echo(t);
        }

        public void Initialise()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            /*
            screenText = new List<string>();
            List<IMyTextPanel> myTextPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(myTextPanels);
            screen = myTextPanels.Where(x => x.CustomData.Contains(argument)).FirstOrDefault();
            */

            List<IMyPistonBase> pistons = new List<IMyPistonBase>();
            GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(pistons);
            piston1 = pistons.Where(x => x.CustomData.Contains(argument)).FirstOrDefault();
            
            Write((piston1 != null ? "Piston found" : "Piston NOT found"), 2);            
        }

        public void Main(string _argument, UpdateType updateSource)
        {
            counter++;
            Echo("Run: " + counter);

            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(blocks);

            if (updateSource == UpdateType.Terminal)
            {
                if (!string.IsNullOrEmpty(_argument))
                {
                    Echo(argument);
                    argument = _argument;
                }

                Initialise();
            }

            Write("Piston Status: " + piston1.Status, 8);

            Vector3D currentPos = piston1.Top.GetPosition();

            Write("Cur Post: " + currentPos, 9);
            Write("Last position: " + lastPos.IsValid() + " | " + lastPos, 10);
            Write("Last Pos Set: " + lastPosSet, 11);
            Write("Dist Difference: " + Vector3D.Distance(lastPos, currentPos), 12);
            Write("Piston Ext Range: " + piston1.CurrentPosition, 13);

            if (piston1.Status == PistonStatus.Extending)
            {
                if (Vector3D.Distance(lastPos, currentPos) < DISTANCE_DIFF && lastPosSet && piston1.CurrentPosition > 5)
                {
                    piston1.MaxLimit = piston1.CurrentPosition;
                }
                else
                {
                    lastPos = currentPos;
                    lastPosSet = true;
                }
            }
            else if (piston1.Status == PistonStatus.Retracting)
            {
                piston1.MaxLimit = 10.0f;
                lastPosSet = false;
                
            }
        }

        // ------ Code Above ----------
        #region PreludeFooter
    }
}
#endregion
