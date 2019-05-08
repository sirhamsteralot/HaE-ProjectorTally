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
        Dictionary<string, int> componentsByType = new Dictionary<string, int>();
        BlockDefinitions blockDefinitions;
        StringBuilder sb = new StringBuilder();
        Action<string> originalEcho;

        IEnumerator<bool> enumerator;

        public Program()
        {
            projector = GridTerminalSystem.GetBlockWithName("TallyProjector") as IMyProjector;
            blockDefinitions = new BlockDefinitions();

            originalEcho = Echo;
            Echo = EchoOL;
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument == "Start")
            {
                componentsByType.Clear();
                sb.Clear();

                enumerator = Logic();
                Runtime.UpdateFrequency = UpdateFrequency.Update1;
                return;
            }
            
            if (enumerator != null)
            {
                if (!enumerator.MoveNext())
                {
                    enumerator.Dispose();
                    enumerator = null;
                }
            }

        }
        
        public void EchoOL(string text)
        {
            Me.GetSurface(0).WriteText(text);
            originalEcho(text);
        }

        public IEnumerator<bool> Logic()
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

            Runtime.UpdateFrequency = UpdateFrequency.None;
        }
    }
}