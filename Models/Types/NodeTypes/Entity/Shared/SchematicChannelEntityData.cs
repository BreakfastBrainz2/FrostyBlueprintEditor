﻿using System.Collections.ObjectModel;
using BlueprintEditorPlugin.Models.Connections;
using Frosty.Core;
using Frosty.Core.Controls;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Managers;

namespace BlueprintEditorPlugin.Models.Types.NodeTypes.Entity.Shared
{
    public class SchematicChannelEntityData : EntityNode
    {
        public override string Name { get; set; } = "Schematic Channel (null ref, 0 events)";
        public override string ObjectType { get; set; } = "SchematicChannelEntityData";

        public override ObservableCollection<InputViewModel> Inputs { get; set; } = new ObservableCollection<InputViewModel>();

        public override ObservableCollection<OutputViewModel> Outputs { get; set; } = new ObservableCollection<OutputViewModel>();

        public override void OnCreation()
        {
            PointerRef ptr = Object.Channel;

            EbxAssetEntry blueprintAssetEntry = App.AssetManager.GetEbxEntry(ptr.External.FileGuid);
            EbxAsset blueprint = App.AssetManager.GetEbx(blueprintAssetEntry);

            Name = $"Schematic Channel ({blueprintAssetEntry.Filename}, {((dynamic)blueprint.RootObject).Events.Count} events)";

            //Populate interface outpts/inputs
            //TODO: Enumerate over links too
            //TODO: Add in proper support for getting realms of inputs/outputs
            for (int i = 0; i < ((dynamic)blueprint.RootObject).Properties.Count; i++)
            {
                dynamic property = ((dynamic)blueprint.RootObject).Properties[i];
                if (Object.OutputProperties.Contains(i) || Object.OutputRefProperties.Contains(i))
                {
                    Outputs.Add(new OutputViewModel() { Title = FrostySdk.Utils.GetString(property.Id), Type = ConnectionType.Property});
                }
                else
                {
                    Inputs.Add(new InputViewModel() { Title = FrostySdk.Utils.GetString(property.Id), Type = ConnectionType.Property});
                }
            }
        }
        
        public override void OnModified(ItemModifiedEventArgs args) => OnCreation();
    }
}