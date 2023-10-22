﻿using System;
using System.Collections.Generic;
using System.Reflection;
using BlueprintEditor.Models.Connections;
using BlueprintEditor.Models.Types.NodeTypes;
using BlueprintEditor.Utils;
using FrostySdk;
using FrostySdk.Ebx;

namespace BlueprintEditor.Models.Types.EbxEditorTypes
{
    /// <summary>
    /// This class is used to edit the EBX itself
    /// So e.g, whenever removing a node, this class is in charge of removing that node in EBX as well
    /// </summary>
    public class EbxBaseEditor
    {
        /// <summary>
        /// This is the asset type that the Blueprint Editor uses
        /// In this case its set to null, but you would want to set it to the asset type
        /// e.g, LogicPrefabBlueprint
        /// </summary>
        public virtual string AssetType { get; } = "null";

        /// <summary>
        /// Used to add a new Object to ebx
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual object AddNodeObject(Type type)
        {
            dynamic obj = TypeLibrary.CreateObject(type.Name);
            PointerRef pointerRef = new PointerRef(obj);
            AssetClassGuid guid = new AssetClassGuid(FrostySdk.Utils.GenerateDeterministicGuid(
                EditorUtils.CurrentEditor.EditedEbxAsset.Objects,
                type,
                EditorUtils.CurrentEditor.EditedEbxAsset.FileGuid), -1); //TODO: THIS CODE SUCKS! PLEASE UPDATE!
            ((dynamic)pointerRef.Internal).SetInstanceGuid(guid);
            
            //No idea what this does
            if (TypeLibrary.IsSubClassOf(pointerRef.Internal, "DataBusPeer"))
            {
                byte[] b = guid.ExportedGuid.ToByteArray();
                uint value = (uint)((b[2] << 16) | (b[1] << 8) | b[0]);
                pointerRef.Internal.GetType().GetProperty("Flags", BindingFlags.Public | BindingFlags.Instance).SetValue(pointerRef.Internal, value);
            }
            
            EditorUtils.CurrentEditor.EditedProperties.Objects.Add(pointerRef);
            EditorUtils.CurrentEditor.EditedEbxAsset.AddObject(pointerRef.Internal);
            return obj;
        }

        /// <summary>
        /// This method is used to remove a node from the Ebx
        /// </summary>
        /// <param name="nodeToRemove"></param>
        public virtual void RemoveNodeObject(NodeBaseModel nodeToRemove)
        {
            List<PointerRef> pointerRefs = EditorUtils.CurrentEditor.EditedProperties.Objects;
            pointerRefs.RemoveAll(pointer => ((dynamic)pointer.Internal).GetInstanceGuid() == nodeToRemove.Guid);
            EditorUtils.CurrentEditor.EditedEbxAsset.RemoveObject(nodeToRemove.Object);
        }

        /// <summary>
        /// This method is used to remove connections from Ebx
        /// </summary>
        /// <param name="connection"></param>
        public virtual void RemoveConnectionObject(ConnectionViewModel connection)
        {
            //TODO: This code sucks! Please find a faster way to find the connection and remove it
            switch (connection.Type)
            {
                case ConnectionType.Event:
                {
                    foreach (dynamic eventConnection in EditorUtils.CurrentEditor.EditedProperties.EventConnections)
                    {
                        if (!connection.Equals(eventConnection)) continue;
                        EditorUtils.CurrentEditor.EditedProperties.EventConnections.Remove(eventConnection);
                        break;
                    }
                    break;
                }
                case ConnectionType.Property:
                {
                    foreach (dynamic propertyConnection in EditorUtils.CurrentEditor.EditedProperties.PropertyConnections)
                    {
                        if (!connection.Equals(propertyConnection)) continue;
                        EditorUtils.CurrentEditor.EditedProperties.PropertyConnections.Remove(propertyConnection);
                        break;
                    }
                    break;
                }
                case ConnectionType.Link:
                {
                    foreach (dynamic linkConnection in EditorUtils.CurrentEditor.EditedProperties.LinkConnections)
                    {
                        if (!connection.Equals(linkConnection)) continue;
                        EditorUtils.CurrentEditor.EditedProperties.LinkConnections.Remove(linkConnection);
                        break;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// This method is used to create new connections and add them to the ebx
        /// </summary>
        /// <param name="connection"></param>
        public virtual void CreateConnectionObject(ConnectionViewModel connection)
        {
            if (connection == null) return;
            switch (connection.Type)
            {
                case ConnectionType.Event:
                {
                    dynamic eventConnection = TypeLibrary.CreateObject("EventConnection");

                    eventConnection.Source = new PointerRef(connection.SourceNode.Object);
                    eventConnection.Target = new PointerRef(connection.TargetNode.Object);
                    eventConnection.SourceEvent.Name = connection.SourceField;
                    eventConnection.TargetEvent.Name = connection.TargetField;

                    ((dynamic)EditorUtils.CurrentEditor.EditedEbxAsset.RootObject).EventConnections
                        .Add(eventConnection);
                    connection.Object = eventConnection;
                } break;
                
                case ConnectionType.Property:
                {
                    dynamic propertyConnection = TypeLibrary.CreateObject("PropertyConnection");

                    propertyConnection.Source = new PointerRef(connection.SourceNode.Object);
                    propertyConnection.Target = new PointerRef(connection.TargetNode.Object);
                    propertyConnection.SourceField = connection.SourceField;
                    propertyConnection.TargetField = connection.TargetField;

                    ((dynamic)EditorUtils.CurrentEditor.EditedEbxAsset.RootObject).PropertyConnections
                        .Add(propertyConnection);
                    connection.Object = propertyConnection;
                } break;
                
                case ConnectionType.Link:
                {
                    dynamic linkConnection = TypeLibrary.CreateObject("LinkConnection");

                    linkConnection.Source = new PointerRef(connection.SourceNode.Object);
                    linkConnection.Target = new PointerRef(connection.TargetNode.Object);
                    linkConnection.SourceField = connection.SourceField;
                    linkConnection.TargetField = connection.TargetField;

                    ((dynamic)EditorUtils.CurrentEditor.EditedEbxAsset.RootObject).LinkConnections.Add(
                        linkConnection);
                    connection.Object = linkConnection;
                } break;
            }
        }
    }
}