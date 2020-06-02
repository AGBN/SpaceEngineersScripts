#region Prelude
using System;
using System.Collections;
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
namespace SpaceEngineers.UWBlockPrograms.TunnelProject
{    

    public sealed class TunnelProject : MyGridProgram
    {
        // ------- Code Below --------

        // Status texts:
        public string SaveStatus;

        // End---
        public float PISTON_SPEED = 0.2f;

        public int counter;
        public string argument;
        public string dataIdentifier;
        public bool ReconnectReady = true;
        public bool test = false;
        public bool firstRun = true;

        public string FRONT_IDENTIFIER = "front";
        public IMyPistonBase PistonFront1;
        public IMyPistonBase PistonFront2;
        public IMyShipConnector ConnectorFront;

        public string BACK_IDENTIFIER = "back";
        public IMyPistonBase PistonBack1;
        public IMyPistonBase PistonBack2;
        public IMyShipConnector ConnectorBack;

        public List<IMyAssembler> Assemblers;
        public List<IMyShipDrill> Drills;
        public int drillCount;
        public List<IMyShipWelder> Welders;
        public int welderCount;
        public List<IMyShipGrinder> Grinders;
        public int grinderCount;

        public IMyTextPanel Screen;
        private string message = "";
        public string Message { get { return message; } set { message += (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(value) ? value : "\n" + value); } }

        #region Component definitions
        //https://3dpeg.net/archives/425
        public string baseStr = "MyObjectBuilder_BlueprintDefinition/";
        public MyDefinitionId SteelPlate { get { return MyDefinitionId.Parse(baseStr + "SteelPlate"); } }
        public MyDefinitionId ConstructionComponent { get { return MyDefinitionId.Parse(baseStr + "ConstructionComponent"); } }
        public MyDefinitionId InteriorPlate { get { return MyDefinitionId.Parse(baseStr + "InteriorPlate"); } }
        public MyDefinitionId SmallTube { get { return MyDefinitionId.Parse(baseStr + "SmallTube"); } }
        public MyDefinitionId MotorComponent { get { return MyDefinitionId.Parse(baseStr + "MotorComponent"); } }
        public MyDefinitionId ComputerComponent { get { return MyDefinitionId.Parse(baseStr + "ComputerComponent"); } }
        public MyDefinitionId Display { get { return MyDefinitionId.Parse(baseStr + "Display"); } }

        // Amount need for 1 segment:
        // Attempt to keep this amount in assemblers.

        public double RATIO = 0.25;
        public double SteelPlateQuota { get { return 29250 * RATIO; } }
        public double ConstructionComponentQuota { get { return 962 * RATIO; } }
        public double InteriorPlateQuota { get { return 522 * RATIO; } }
        public double SmallTubeQuota { get { return 492 * RATIO; } }
        public double MotorComponentQuota { get { return 240 * RATIO; } }
        public double ComputerComponentQuota { get { return 72 * RATIO; } }
        public double DisplayQuota { get { return 4 * RATIO; } }

        public class AssemblerContent
        {
            public string componentName;
            public double amount;
        }
        #endregion

        public void Save()
        {
            SaveStatus = "Saved at run " + counter + ". Value: " + argument;
            Storage = argument;
        }

        public Program()
        {
            if (Storage.Length > 0)
            {
                argument = Storage;
                Echo("Retrieved '" + argument + "' from Storage");
            }
            else
                Echo("No Saved argument. Proceding as normal.");
        }

        public void Initialise()
        {
            if (argument.Contains("Test"))
            {
                Runtime.UpdateFrequency = UpdateFrequency.None;
                Echo("Test Run:");
                argument = argument.Split(' ')[0];
                test = true;
            }
            else
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
            
            Echo("Initialise started:");

            dataIdentifier = argument;
            Echo("Identifier: " + dataIdentifier);

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(blocks);
            blocks.RemoveAll(x => !x.CustomData.Contains(dataIdentifier));
            Echo("Blocks found: " + blocks.Count());

            Drills = new List<IMyShipDrill>();
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(Drills, (x => x.CustomData.Contains(dataIdentifier)));
            drillCount = Drills.Count();
            Echo("Drills found: " + drillCount);

            Welders = new List<IMyShipWelder>();
            GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(Welders, (x => x.CustomData.Contains(dataIdentifier)));
            welderCount = Welders.Count();
            Echo("Welders found: " + welderCount);

            Grinders = new List<IMyShipGrinder>();
            GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(Grinders, (x => x.CustomData.Contains(dataIdentifier)));
            grinderCount = Grinders.Count();
            Echo("Grinders found: " + grinderCount);

            Assemblers = new List<IMyAssembler>();
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Assemblers, (x => x.CustomData.Contains(dataIdentifier)));
            Echo("Assemblers found: " + Assemblers.Count());

            List<IMyPistonBase> pistons = new List<IMyPistonBase>();
            GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(pistons, (x => x.CustomData.Contains(dataIdentifier)));
            Echo("Pistons found: " + pistons.Count());

            foreach (var item in pistons)
            {
                if (item.CustomData.Contains(FRONT_IDENTIFIER))
                {
                    Echo("Front Piston found");
                    if (PistonFront1 == null)
                        PistonFront1 = item;
                    else
                        PistonFront2 = item;
                }
                else if (item.CustomData.Contains(BACK_IDENTIFIER))
                {
                    Echo("Back Piston found");
                    if (PistonBack1 == null)
                        PistonBack1 = item;
                    else
                        PistonBack2 = item;
                }
                else
                {
                    Message += "PISTON FOUND OUTSIDE OF SCOPE!!! \n Piston name: " + item.DisplayName;
                }
            }

            List<IMyShipConnector> connectors = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connectors, (x => x.CustomData.Contains(dataIdentifier)));
            Echo("Connectors found: " + connectors.Count());

            foreach (var item in connectors)
            {
                if (item.CustomData.Contains(FRONT_IDENTIFIER))
                {
                    Echo("Front Connector found");
                    ConnectorFront = item;
                }
                else if (item.CustomData.Contains(BACK_IDENTIFIER))
                {
                    Echo("Back Connector found");
                    ConnectorBack = item;
                }
                else
                {
                    Message += "CONNECTOR FOUND OUTSIDE OF SCOPE!!! \n Connector name: " + item.DisplayName;
                }
            }

            // Set settings for blocks
            // NOTE: Max Impulse should be set to 250'000 manually for pistons
            // NOTE: Enable 'Share Inertia Tension' manually pistons

            if (!test)
            {
                PistonFront1.Velocity = PISTON_SPEED * -1;
                PistonFront2.Velocity = PISTON_SPEED * -1;
                PistonBack1.Velocity = PISTON_SPEED * -1;
                PistonBack2.Velocity = PISTON_SPEED * -1;

                Drills.ForEach(x => x.Enabled = true);
                Welders.ForEach(x => x.Enabled = true);
                Grinders.ForEach(x => x.Enabled = true);
                Assemblers.ForEach(x => x.Enabled = true);
                
            }

            Welders.ForEach(x => x.HelpOthers = true);

            firstRun = false;
        }

        public void Main(string _argument, UpdateType updateSource)
        {
            counter++;
            
            if (updateSource == UpdateType.Terminal || firstRun)
            {
                argument = _argument;
                Initialise();
            }

            if (!test)
            {
                switch (ConnectorFront.Status)
                {
                    case MyShipConnectorStatus.Unconnected:
                        ExtendPistons();
                        break;

                    case MyShipConnectorStatus.Connectable:
                        if (PistonBack1.Status == PistonStatus.Extended)
                        {
                            ConnectorFront.Connect();
                            ConnectorBack.Disconnect();
                        }
                        break;

                    case MyShipConnectorStatus.Connected:

                        break;
                }

                switch (ConnectorBack.Status)
                {
                    case MyShipConnectorStatus.Unconnected:
                        RetractPistons();
                        break;

                    case MyShipConnectorStatus.Connectable:
                        if (PistonBack1.Status == PistonStatus.Retracted)
                        {
                            ConnectorBack.Connect();
                            ConnectorFront.Disconnect();
                        }
                        break;

                    case MyShipConnectorStatus.Connected:
                        if (ConnectorFront.Status == MyShipConnectorStatus.Connected)
                        {
                            ConnectorBack.Disconnect();
                            RetractPistons();
                        }
                        break;
                }
            }

            UpdateAssemblers();

            PrintStatus();
        }

        public void UpdateAssemblers()
        {
            List<AssemblerContent> assemblerContents = GetAssemblerInv();

            assemblerContents[0].amount = GetDifference(assemblerContents[0].amount, SteelPlateQuota);
            assemblerContents[1].amount = GetDifference(assemblerContents[1].amount, ConstructionComponentQuota);
            assemblerContents[2].amount = GetDifference(assemblerContents[2].amount, InteriorPlateQuota);
            assemblerContents[3].amount = GetDifference(assemblerContents[3].amount, SmallTubeQuota);
            assemblerContents[4].amount = GetDifference(assemblerContents[4].amount, MotorComponentQuota);
            assemblerContents[5].amount = GetDifference(assemblerContents[5].amount, ComputerComponentQuota);
            assemblerContents[6].amount = GetDifference(assemblerContents[6].amount, DisplayQuota);

            foreach (var item in assemblerContents)
            {
                if (item.amount > 0)
                {
                    AddToQueue(item.amount, (MyDefinitionId.Parse(baseStr + item.componentName)));
                }
            }

        }

        public void AddToQueue(double amount, MyDefinitionId component) 
        {
            double max_vol = 4000.00;
            List<IMyAssembler> sortedAssemblers = new List<IMyAssembler>();

            foreach (var assembler in Assemblers)
            {

                if (assembler.GetInventory(1).CurrentVolume.RawValue/1000 < max_vol * 0.9)
                {
                    if (assembler.IsQueueEmpty)
                        sortedAssemblers.Insert(0, assembler);
                    else
                    {
                        List<MyProductionItem> tmp = new List<MyProductionItem>();
                        assembler.GetQueue(tmp);

                        if (tmp.Count() < 50)
                        {
                            sortedAssemblers.Add(assembler);
                        }
                    }
                }
            }

            foreach (var item in sortedAssemblers)
            {
                if (item != null)
                {
                    double i = (amount < 50 ? amount : 50);
                    amount -= i;
                    item.AddQueueItem(component, i);

                    if (amount <= 0)
                        return;
                }
            }
        }

        public double GetDifference(double amount, double quota) 
        {
            if (amount < quota)
                return quota - amount;
            else
                return 0;
        }

        public List<AssemblerContent> GetAssemblerInv()
        {
            List<AssemblerContent> assemblerContents = new List<AssemblerContent>();
            assemblerContents.Add(new AssemblerContent() { componentName = "" + SteelPlate.ToString().Split('/')[1], amount = 0 });
            assemblerContents.Add(new AssemblerContent() { componentName = "" + ConstructionComponent.ToString().Split('/')[1], amount = 0 });
            assemblerContents.Add(new AssemblerContent() { componentName = "" + InteriorPlate.ToString().Split('/')[1], amount = 0 });
            assemblerContents.Add(new AssemblerContent() { componentName = "" + SmallTube.ToString().Split('/')[1], amount = 0 });
            assemblerContents.Add(new AssemblerContent() { componentName = "" + MotorComponent.ToString().Split('/')[1], amount = 0 });
            assemblerContents.Add(new AssemblerContent() { componentName = "" + ComputerComponent.ToString().Split('/')[1], amount = 0 });
            assemblerContents.Add(new AssemblerContent() { componentName = "" + Display.ToString().Split('/')[1], amount = 0 });


            foreach (var assembler in Assemblers)
            {
                // Output inventory
                List<MyInventoryItem> iTmp = new List<MyInventoryItem>();
                assembler.GetInventory(1).GetItems(iTmp);

                foreach (var item in iTmp)
                {
                    string name = item.Type.ToString().Split('/')[1];
                    var ac = assemblerContents.Where(y => y.componentName.Equals(name)).FirstOrDefault();

                    if (ac != null)
                    {
                        ac.amount += item.Amount.ToIntSafe();
                    }
                    else
                        Message = "Unusuable component in assembler inventory. \nComponent: " + name;

                }

                // Production Queue
                List<MyProductionItem> qTmp = new List<MyProductionItem>();
                assembler.GetQueue(qTmp);

                foreach (var item in qTmp)
                {
                    string name = item.BlueprintId.ToString().Split('/')[1];
                    var ac = assemblerContents.Where(y => y.componentName.Equals(name)).FirstOrDefault();

                    if (ac != null)
                    {
                        ac.amount += item.Amount.ToIntSafe();
                    }
                    else
                        Message = "Unusuable component in assembler queue. \nComponent: " + name;

                }
            }

            return assemblerContents;
        }

        public void ExtendPistons()
        {
            if (PistonBack1.Status == PistonStatus.Extending || PistonBack2.Status == PistonStatus.Extending || PistonFront1.Status == PistonStatus.Extending || PistonFront2.Status == PistonStatus.Extending)
            {
                // Nothing.. something is already happening
            }
            else
            {
                if (PistonBack1.Status != PistonStatus.Extended)
                    PistonBack1.Extend();
                else if (PistonBack2.Status != PistonStatus.Extended)
                    PistonBack2.Extend();
                else if (PistonFront1.Status != PistonStatus.Extended)
                    PistonFront1.Extend();
                else if (PistonFront2.Status != PistonStatus.Extended)
                    PistonFront2.Extend();
                else
                {
                    Message += "ERROR: Pistons are stuck in extended position.";
                }
            }
        }

        public void RetractPistons()
        {
            if (PistonBack1.Status == PistonStatus.Retracting || PistonBack2.Status == PistonStatus.Retracting || PistonFront1.Status == PistonStatus.Retracting || PistonFront2.Status == PistonStatus.Retracting)
            {
                // Nothing.. something is already happening
            }
            else
            {
                if (PistonBack1.Status != PistonStatus.Retracted)
                    PistonBack1.Retract();
                else if (PistonBack2.Status != PistonStatus.Retracted)
                    PistonBack2.Retract();
                else if (PistonFront1.Status != PistonStatus.Retracted)
                    PistonFront1.Retract();
                else if (PistonFront2.Status != PistonStatus.Retracted)
                    PistonFront2.Retract();
                else
                {
                    Message += "ERROR: Pistons are stuck in retracted position.";
                }
            }
        }

        public void PrintStatus()
        {
            if (Screen == null)
            {
                List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(textPanels, (x => x.CustomData.Contains(dataIdentifier)));
                Screen = textPanels.FirstOrDefault();

                Echo("Screen " + (Screen != null ? "" : "not ") + "found");
            }


            Echo("Run: " + counter);
            Echo(SaveStatus);
            Echo(Message);
            Echo(GetDrillStatus());
            Echo(GetWelderStatus());
            Echo(GetGrinderStatus());
            Echo("Piston 1: " + PistonBack1.Status);
            Echo("Piston 2: " + PistonBack2.Status);
            Echo("Piston 3: " + PistonFront1.Status);
            Echo("Piston 4: " + PistonBack2.Status);

            if (Screen != null)
            {
                Screen.WriteText(Message + "\n");
                Screen.WriteText(GetDrillStatus(), true);
                Screen.WriteText(GetWelderStatus(), true);
                Screen.WriteText(GetGrinderStatus(), true);
                Screen.WriteText("Piston 1: " + PistonBack1.Status + ".\n", true);
                Screen.WriteText("Piston 2: " + PistonBack2.Status + ".\n", true);
                Screen.WriteText("Piston 3: " + PistonFront1.Status + ".\n", true);
                Screen.WriteText("Piston 4: " + PistonBack2.Status + ".\n", true);

                for(int i = 0; i < Assemblers.Count(); i++)
                {
                    Screen.WriteText("Assembler " + (i+1) + ": " + GetAssemblerStatus(Assemblers[i]), true);
                }
            }

            Message = "";
        }

        public string GetDrillStatus()
        {
            int nr = 0;

            foreach (var drill in Drills)
            {
                if (drill.Enabled)
                    nr++;
            }

            return "Drills: " + nr + " of " + drillCount + " running.\n";
        }

        public string GetWelderStatus()
        {
            int nr = 0;

            foreach (var welder in Welders)
            {
                if (welder.Enabled)
                    nr++;
            }

            return "Welders: " + nr + " of " + welderCount + " running.\n";
        }

        public string GetGrinderStatus()
        {
            int nr = 0;

            foreach (var grinder in Grinders)
            {
                if (grinder.Enabled)
                    nr++;
            }

            return "Grinders: " + nr + " of " + grinderCount + " running.\n";
        }

        public string GetAssemblerStatus(IMyAssembler assembler) 
        {
            if (assembler.IsProducing)
                return assembler.CurrentProgress + "%.\n";
            else if (assembler.IsQueueEmpty)
                return " idle.\n";
            else
                return assembler.CurrentProgress + "% - Waiting....\n";
                
        }

        // ------ Code Above ----------
        #region PreludeFooter
    }
}
#endregion
