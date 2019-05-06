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

        public Program()
        {
            projector = GridTerminalSystem.GetBlockWithName("Projector") as IMyProjector;
            blockDefinitions = new BlockDefinitions();
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            foreach (var block in projector.RemainingBlocksPerType)
            {
                var def = blockDefinitions.GetDefinition(block.Key.ToString());

                foreach(var component in def.components)
                {
                    componentsByType[component.Item1] += component.Item2;
                }
            }
        }
    }
}