﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using BlueprintEditorPlugin.Editors.GraphEditor.LayoutManager.Algorithms;
using BlueprintEditorPlugin.Editors.GraphEditor.LayoutManager.IO;
using BlueprintEditorPlugin.Editors.NodeWrangler;
using BlueprintEditorPlugin.Models.Nodes;
using Frosty.Controls;
using FrostyEditor;
using FrostySdk;

namespace BlueprintEditorPlugin.Editors.GraphEditor.LayoutManager
{
    public abstract class BaseLayoutManager : ILayoutManager
    {
        public INodeWrangler NodeWrangler { get; set; }
        public virtual int Version => 1000;

        public static string RootLayoutPath => $@"{AppDomain.CurrentDomain.BaseDirectory}BlueprintEditor\BlueprintLayouts\{ProfilesLibrary.ProfileName}\";
        
        public static string CurrentLayoutPath
        {
            get
            {
                //Get the MainWindow so we can grab the Project File
                MainWindow frosty = null;
                
                //Do this to ensure task windows work correctly
                Application.Current.Dispatcher.Invoke(() =>
                {
                    frosty = Application.Current.MainWindow as MainWindow;
                });
                
                //Get the name of the project(make sure to remove .fbproject)
                string projectName = frosty.Project.DisplayName.Split('.')[0];
                return $@"{AppDomain.CurrentDomain.BaseDirectory}BlueprintEditor\BlueprintLayouts\{ProfilesLibrary.ProfileName}\{projectName}\";
            }
        }
        
        public void SortLayout(bool optimized = false)
        {
            SugiyamaMethod sugiyamaMethod = new SugiyamaMethod(NodeWrangler.Connections.ToList(), NodeWrangler.Nodes.ToList());
            sugiyamaMethod.SortGraph();
        }

        public bool LayoutExists(string path)
        {
            return File.Exists($"{CurrentLayoutPath}{path.Replace("/", "\\")}");
        }

        public virtual bool LoadLayout(string path)
        {
            if (!File.Exists(path))
                return false;

            LayoutReader layoutReader = new LayoutReader(new FileStream(path, FileMode.Open));
            if (layoutReader.ReadInt() != Version)
            {
                MessageBoxResult result = FrostyMessageBox.Show(
                    "It appears the layout file associated with this is older then the current version. Would you like me to read it anyway?", 
                    "Graph Layout Manager", MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                {
                    return false;
                }
            }

            // Read through all trans
            for (int i = 0; i < layoutReader.ReadInt(); i++)
            {
                string key = layoutReader.ReadNullTerminatedString();
                if (!ExtensionsManager.TransientNodeExtensions.ContainsKey(key))
                {
                    App.Logger.LogError("Unable to find transient node {0}", key);
                    return false;
                }
                
                Type transType = ExtensionsManager.TransientNodeExtensions[key];
                try
                {
                    ITransient transient = (ITransient)Activator.CreateInstance(transType);
                    if (transient.Load(layoutReader))
                    {
                        NodeWrangler.AddNode(transient);
                    }
                }
                catch (Exception e)
                {
                    App.Logger.LogError("Transient {0} failed with error {1}", transType.ToString(), e.Message);
                    App.Logger.LogError("Stacktrace: {0}", e.StackTrace);
                    return false;
                }
            }

            for (int i = 0; i < layoutReader.ReadInt(); i++)
            {
                IVertex vertex = NodeWrangler.Nodes[layoutReader.ReadInt()];
                vertex.Location = layoutReader.ReadPoint();
                double width = layoutReader.ReadDouble();
                double height = layoutReader.ReadDouble();
                vertex.Size = new Size(width, height);
            }
            
            layoutReader.Dispose();

            return true;
        }

        public bool LoadLayoutRelative(string path)
        {
            return LoadLayout($@"{CurrentLayoutPath}\{path.Replace("/", "\\")}");
        }

        public virtual bool SaveLayout(string path)
        {
            if (!Directory.Exists($@"{CurrentLayoutPath}\{path.Replace("/", "\\")}"))
            {
                Directory.CreateDirectory($@"{CurrentLayoutPath}\{path.Replace("/", "\\")}");
            }
            
            LayoutWriter layoutWriter = new LayoutWriter(new FileStream($@"{CurrentLayoutPath}\{path.Replace("/", "\\")}", FileMode.Create));
            layoutWriter.Write(Version);
            List<IVertex> verts = new List<IVertex>();
            List<ITransient> transients = new List<ITransient>();

            foreach (IVertex vertex in NodeWrangler.Nodes)
            {
                if (vertex is ITransient transient)
                {
                    transients.Add(transient);
                }
                else
                {
                    verts.Add(vertex);
                }
            }

            layoutWriter.Write(transients.Count);

            foreach (ITransient transient in transients)
            {
                try
                {
                    layoutWriter.WriteNullTerminatedString(transient.GetType().Name);
                    transient.Save(layoutWriter);
                }
                catch (Exception e)
                {
                    App.Logger.LogError("Transient {0} failed with error {1}", transient.ToString(), e.Message);
                    App.Logger.LogError("Stacktrace: {0}", e.StackTrace);
                    App.Logger.LogWarning("Careful, layout maybe corrupted!");
                    return false;
                }
            }
            
            layoutWriter.Write(verts.Count);

            foreach (IVertex vertex in verts)
            {
                layoutWriter.Write(NodeWrangler.Nodes.IndexOf(vertex));
                layoutWriter.Write(vertex.Location);
                layoutWriter.Write(vertex.Size.Width);
                layoutWriter.Write(vertex.Size.Height);
            }
            
            layoutWriter.Dispose();

            return true;
        }

        public BaseLayoutManager(INodeWrangler nodeWrangler)
        {
            NodeWrangler = nodeWrangler;
        }
    }
}