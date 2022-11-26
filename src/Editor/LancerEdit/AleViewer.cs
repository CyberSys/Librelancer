﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Collections.Generic;
using System.Numerics;
using LibreLancer;
using LibreLancer.ImUI;
using LibreLancer.Fx;
using LibreLancer.Utf.Ale;
using LibreLancer.Utf.Mat;
using ImGuiNET;
using LibreLancer.Render;
using LibreLancer.Render.Cameras;

namespace LancerEdit
{
    public partial class AleViewer : EditorTab
    {
        //GL
        Viewport3D aleViewport;
        RenderContext rstate;
        CommandBuffer buffer;
        PolylineRender polyline;
        LineRenderer debug;
        ParticleEffectPool pool;
        ResourceManager res;
        private int cameraMode = 0;
        private static readonly DropdownOption[] camModes= new[]
        {
            new DropdownOption("Arcball", Icons.Globe, CameraModes.Arcball),
            new DropdownOption("Walkthrough", Icons.StreetView, CameraModes.Walkthrough),
        };
        //Tab
        string name;
        ParticleLibrary plib;
        int lastEffect = 0;
        int currentEffect = 0;

        float sparam = 1;
        string[] effectNames;
        public AleViewer(string name, AleFile ale, MainWindow main)
        {
            cameraMode = main.Config.DefaultCameraMode;
            plib = new ParticleLibrary(main.Resources, ale);
            res = main.Resources;
            pool = new ParticleEffectPool(main.Commands);
            effectNames = new string[plib.Effects.Count];
            for (int i = 0; i < effectNames.Length; i++)
                effectNames[i] = string.Format("{0} (0x{1:X})", plib.Effects[i].Name, plib.Effects[i].CRC);
            Title = string.Format("Ale Viewer ({0})", name);
            this.name = name;
            this.rstate = main.RenderContext;
            aleViewport = new Viewport3D(main);
            aleViewport.DefaultOffset = 
            aleViewport.CameraOffset = new Vector3(0, 0, 200);
            aleViewport.ModelScale = 25;
            aleViewport.ResetControls();;
            buffer = main.Commands;
            polyline = main.Polyline;
            debug = main.LineRenderer;
            SetupRender(0);
        }
        ParticleEffectInstance instance;
        void SetupRender(int index)
        {
            selectedReference = null;
            instance = new ParticleEffectInstance(plib.Effects[index]);
            instance.Pool = pool;
            instance.Resources = plib.Resources;
        }

        bool[] openTabs = new bool[] { false, false };
        void TabButton(string name, int idx)
        {
            if (TabHandler.VerticalTab(name, openTabs[idx]))
            {
                if (!openTabs[idx])
                {
                    for (int i = 0; i < openTabs.Length; i++) openTabs[i] = false;
                    openTabs[idx] = true;
                }
                else
                    openTabs[idx] = false;
            }
        }
        void TabButtons()
        {
            ImGuiNative.igBeginGroup();
            TabButton("Hierachy", 0);
            //TabButton("Library", 1);
            ImGuiNative.igEndGroup();
            ImGui.SameLine();
        }

        public override void Draw()
        {
            bool doTabs = false;
            foreach (var t in openTabs) if (t) { doTabs = true; break; }
            var contentw = ImGui.GetContentRegionAvail().X;
            if (doTabs)
            {
                ImGui.Columns(2, "##alecolumns", true);
                ImGui.BeginChild("##leftpanel");
                if (openTabs[0]) NodePanel();
                ImGui.EndChild();
                ImGui.NextColumn();
            }
            TabButtons();
            ImGui.SameLine();
            ImGui.BeginChild("##renderpanel");
            //Fx management
            lastEffect = currentEffect;
            ImGui.Text("Effect:");
            ImGui.SameLine();
            ImGui.PushItemWidth(160);
            ImGui.Combo("##effect", ref currentEffect, effectNames, effectNames.Length);
            ImGui.PopItemWidth();
            if (currentEffect != lastEffect) SetupRender(currentEffect);
            ImGui.SameLine();
            ImGui.PushItemWidth(80);
            ImGui.SliderFloat("SParam", ref sparam, 0, 1, "%f");
            ImGui.PopItemWidth();
            ImGui.Separator();
            //Viewport
            ImGui.BeginChild("##renderchild");
            //Generate render target
            aleViewport.Begin();
            DrawGL(aleViewport.RenderWidth, aleViewport.RenderHeight);
            //Display + Camera controls
            aleViewport.End();
            //Action Bar
            ViewerControls.DropdownButton("Camera Mode", ref cameraMode, camModes);
            aleViewport.Mode = (CameraModes) camModes[cameraMode].Tag;
            ImGui.SameLine();
            if (ImGui.Button("Reset Camera (Ctrl+R)"))
                aleViewport.ResetControls();
            ImGui.SameLine();
            if (ImGui.Button("Reset Fx"))
            {
                instance.Reset();
            }
            ImGui.SameLine();
            ImGui.Text(string.Format("T: {0:0.000}", instance.GlobalTime));
            ImGui.EndChild();
            ImGui.EndChild();
        }

        void NodePanel()
        {
            if (selectedReference != null)
            {
                NodeOptions();
                ImGui.Separator();
            }
            NodeHierachy();
        }
        NodeReference selectedReference = null;
        void NodeHierachy()
        {
            var enabledColor = (Vector4)ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
            var disabledColor = (Vector4)ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];

            foreach (var reference in instance.Effect.References)
            {
                int j = 0;
                if (reference.Parent == null)
                    DoNode(reference, j++, enabledColor, disabledColor);
            }
        }

        void NodeOptions()
        {
            ImGui.Text(string.Format("Selected: {0}", selectedReference.Node.NodeName));
            ImGui.Text(selectedReference.Node.Name);
            //Node enabled
            var enabled = instance.NodeEnabled(selectedReference);
            var wasEnabled = enabled;
            ImGui.Checkbox("Enabled", ref enabled);
            if (enabled != wasEnabled) instance.SetNodeEnabled(selectedReference, enabled);
            //
            if (selectedReference.Node is FxEmitter emitter)
            {
                ImGui.Text($"InitLifeSpan: {emitter.InitLifeSpan.GetValue(sparam, 0)}");
                ImGui.Text($"Frequency: {emitter.Frequency.GetValue(sparam, 0)}");
                ImGui.Text($"Pressure: {emitter.Pressure.GetValue(sparam, 0)}");
            }
            //Normals?

            //Textures?

            //Debug volumes?
        }

        void DoNode(NodeReference reference, int idx, Vector4 enabled, Vector4 disabled)
        {
            var col = instance.NodeEnabled(reference) ? enabled : disabled;
            string label = null;
            if (reference.IsAttachmentNode)
                label = string.Format("Attachment##{0}", idx);
            else
                label = ImGuiExt.IDWithExtra(reference.Node.NodeName, idx);
            ImGui.PushStyleColor(ImGuiCol.Text, col);
            char icon;
            NodeIcon(reference.Node, out icon);
            if (reference.Children.Count > 0)
            {
                if (Theme.IconTreeNode(icon,label, ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    int j = 0;
                    foreach (var child in reference.Children)
                        DoNode(child, j++, enabled, disabled);
                    ImGui.TreePop();
                }
               
            }
            else
            {
                if (ImGui.Selectable($"{icon}  {label}", selectedReference == reference))
                {
                    selectedReference = reference;
                }
            }
            ImGui.PopStyleColor();
        }
        
        void DrawGL(int renderWidth, int renderHeight)
        {
            var cam = new LookAtCamera();
            Matrix4x4 rot = Matrix4x4.CreateRotationX(aleViewport.CameraRotation.Y) *
                Matrix4x4.CreateRotationY(aleViewport.CameraRotation.X);
            var dir = Vector3.Transform(-Vector3.UnitZ, rot);
            var to = aleViewport.CameraOffset + (dir * 10);
            if (aleViewport.Mode == CameraModes.Arcball)
                to = Vector3.Zero;
            cam.Update(renderWidth, renderHeight, aleViewport.CameraOffset, to, rot);
            buffer.StartFrame(rstate);
            polyline.SetCamera(cam);
            debug.StartFrame(cam, rstate);
            instance.Draw(transform, sparam);
            pool.Draw(cam, polyline, res, debug);
            polyline.FrameEnd();
            buffer.DrawOpaque(rstate);
            rstate.DepthWrite = false;
            buffer.DrawTransparent(rstate);
            rstate.DepthWrite = true;
            debug.Render();
        }

        public override void DetectResources(List<MissingReference> missing, List<uint> matrefs, List<string> texrefs)
        {
            foreach (var reference in instance.Effect.References)
            {
                if (reference.Node is FxBasicAppearance)
                {
                    var node = reference.Node;
                    var fx = (FxBasicAppearance)reference.Node;
                    if (fx.Texture != null && !ResourceDetection.HasTexture(texrefs, fx.Texture)) texrefs.Add(fx.Texture);
                    TexFrameAnimation texFrame;
                    if (fx.Texture != null &&  plib.Resources.FindTexture(fx.Texture) == null && 
                        !plib.Resources.TryGetFrameAnimation(fx.Texture, out texFrame))
                    {
                        var str = "Texture: " + fx.Texture; //TODO: This is wrong - handle properly
                        if (!ResourceDetection.HasMissing(missing, str)) missing.Add(new MissingReference(
                            str, string.Format("{0}: {1} ({2})", instance.Effect.Name, node.NodeName, node.Name)));
                    }
                }
            }
        }

        public override void OnHotkey(Hotkeys hk)
        {
            if (hk == Hotkeys.ResetViewport) aleViewport.ResetControls();
        }

        Matrix4x4 transform = Matrix4x4.Identity;
        public override void Update(double elapsed)
        {
            transform = Matrix4x4.CreateRotationX(aleViewport.ModelRotation.Y) * Matrix4x4.CreateRotationY(aleViewport.ModelRotation.X);
            pool.Update(elapsed);
            instance.Update(elapsed, transform, sparam);
        }

        public override void Dispose()
        {
            aleViewport.Dispose();
            pool.Dispose();
        }
    }
}
