using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        IMyProjector projector;
        IMyAssembler assembler;

        Dictionary<string, int> componentsByType = new Dictionary<string, int>();
        BlockDefinitions blockDefinitions;
        StringBuilder sb = new StringBuilder();
        Action<string> originalEcho;

        Queue<IEnumerator<bool>> enumerator = new Queue<IEnumerator<bool>>();

        public Program()
        {
            assembler = GridTerminalSystem.GetBlockWithName("MasterAssembler") as IMyAssembler;
            if (assembler == null)
                throw new Exception("No Assembler found with the name \"MasterAssembler\"!");

            projector = GridTerminalSystem.GetBlockWithName("TallyProjector") as IMyProjector;
            if (projector == null)
                throw new Exception("No Projector found with the name \"TallyProjector\"!");


            blockDefinitions = new BlockDefinitions();

            originalEcho = Echo;
            Echo = EchoOL;
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument)
            {
                case "Start":
                    Start();
                    return;
                case "Queue":
                    if (componentsByType.Count == 0)
                    {
                        Echo("No Projection Processed!");
                        return;
                    }
                    Queue();
                    return;
                case "QStart":
                    Start();
                    Queue();
                    return;
            }
            
            if (enumerator.Count > 0)
            {
                if (!enumerator.Peek().MoveNext())
                {
                    enumerator.Peek().Dispose();
                    enumerator.Dequeue();
                }
            } else
            {
                Runtime.UpdateFrequency = UpdateFrequency.None;

            }

        }

        public void Queue()
        {
            enumerator.Enqueue(QueueLogic());
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Start()
        {
            componentsByType.Clear();
            sb.Clear();

            enumerator.Enqueue(ProcessBPLogic());
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        
        public void EchoOL(string text)
        {
            Me.GetSurface(0).WriteText(text);
            originalEcho(text);
        }

        public IEnumerator<bool> QueueLogic()
        {
            int currentStep = 1;

            foreach (var component in componentsByType)
            {
                MyDefinitionId itemDef;
                if (MyDefinitionId.TryParse(blockDefinitions.GetFullComponentName(component.Key), out itemDef))
                {
                    if (!assembler.CanUseBlueprint(itemDef))
                        throw new Exception("Cant use BP!");

                    assembler.AddQueueItem(itemDef, (MyFixedPoint)(component.Value));
                }

                Echo($"Queueing components...\nStep{currentStep.ToString().PadRight(5).Substring(0, 5)} out of {componentsByType.Count}");
                currentStep++;
                yield return true;
            }

            Echo("Items Queued!");
        }

        public IEnumerator<bool> ProcessBPLogic()
        {
            int currentStep = 1;

            foreach (var block in projector.RemainingBlocksPerType)
            {

                string key = block.Key.ToString();
                var def = blockDefinitions.GetDefinition(key);

                foreach (var component in def.components)
                {
                    if (!componentsByType.ContainsKey(component.id))
                        componentsByType.Add(component.id, 0);

                    componentsByType[component.id] += component.count * block.Value;
                }

                Echo($"converting blocks...\nStep{currentStep.ToString().PadRight(5).Substring(0, 5)} out of {projector.RemainingBlocksPerType.Count}");

                currentStep++;

                yield return true;
            }

            yield return true;

            Echo("Echoing data...");

            foreach (var component in componentsByType)
            {
                sb.Append(component.Key).Append(" : ").AppendLine(component.Value.ToString());
            }

            Me.CustomData = sb.ToString();
        }
    }
}