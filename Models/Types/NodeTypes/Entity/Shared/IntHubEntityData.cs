﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BlueprintEditorPlugin.Models.Connections;
using BlueprintEditorPlugin.Models.Types.NodeTypes.Entity.ExampleTypes;
using BlueprintEditorPlugin.Utils;
using Frosty.Core.Controls;
using FrostyEditor;
using FrostySdk.Ebx;

namespace BlueprintEditorPlugin.Models.Types.NodeTypes.Entity.Shared
{
    /// <summary>
    /// This is a more advanced demonstration, for a simple demonstration <see cref="CompareBoolEntityData"/>
    /// This demonstrates creating events and properties based off of the property grid
    /// </summary>
    public class IntHubEntityData : FloatHubEntityData
    {
        public override string Name { get; set; } = "Int Hub";
        
        public override string ObjectType { get; set; } = "IntHubEntityData";
    }
}