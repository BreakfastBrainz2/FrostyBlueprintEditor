﻿using System.Collections.ObjectModel;
using BlueprintEditorPlugin.Models.Connections;
using BlueprintEditorPlugin.Utils;
using Frosty.Core.Controls;

namespace BlueprintEditorPlugin.Models.Types.NodeTypes.Entity.Shared
{
    public class OrEntityData : EntityNode
    {
        public override string Name { get; set; } = "OR";
        public override string ObjectType { get; set; } = "OrEntityData";

        public override ObservableCollection<InputViewModel> Inputs { get; set; } = new ObservableCollection<InputViewModel>();

        public override ObservableCollection<OutputViewModel> Outputs { get; set; } =
            new ObservableCollection<OutputViewModel>
            {
                new OutputViewModel() { Title = "Out", Type = ConnectionType.Property }
            };

        public override void OnCreation()
        {
            if ((int)Object.InputCount == 0) return;
            
            for (int i = 1; i <= (int)Object.InputCount; i++)
            {
                Inputs.Add(new InputViewModel() {Title = $"In{i}", Type = ConnectionType.Property});
            }
            
            foreach (InputViewModel input in Inputs)
            {
                NodeUtils.PortRealmFromObject(Object, input);
            }

            foreach (OutputViewModel output in Outputs)
            {
                NodeUtils.PortRealmFromObject(Object, output);
            }
        }

        public override void OnModified(ItemModifiedEventArgs args)
        {
            if ((int)Object.InputCount == 0)
            {
                Inputs.Clear();
                return;
            }
            
            //If the InputCount is a larger number that means we are adding
            if ((int)Object.InputCount > Inputs.Count)
            {
                for (int i = Inputs.Count; i <= (int)Object.InputCount; i++)
                {
                    Inputs.Add(new InputViewModel() {Title = $"In{i}", Type = ConnectionType.Property});
                }
            }
            else //This means the list must be smaller
            {
                for (int i = (int)Object.InputCount; i != Inputs.Count; i++)
                {
                    InputViewModel input = Inputs[i];
                    foreach (ConnectionViewModel connection in EditorUtils.CurrentEditor.GetConnections(input))
                    {
                        EditorUtils.CurrentEditor.Disconnect(connection);
                    }

                    Inputs.Remove(input);
                }
            }
            
            //We want to make sure our Inputs and Outputs are the same realm as us, that way our flags compute properly
            foreach (InputViewModel input in Inputs)
            {
                NodeUtils.PortRealmFromObject(Object, input);
            }

            foreach (OutputViewModel output in Outputs)
            {
                NodeUtils.PortRealmFromObject(Object, output);
            }
        }
    }
}