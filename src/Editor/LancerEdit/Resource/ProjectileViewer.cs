// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Linq;
using System.Numerics;
using System.Text;
using LibreLancer;
using LibreLancer.Data;
using LibreLancer.Data.Effects;
using LibreLancer.Data.Equipment;
using LibreLancer.ImUI;
using ImGuiNET;
using LibreLancer.Data.Universe;
using LibreLancer.Fx;

namespace LancerEdit
{
    public class ProjectileViewer : EditorTab
    {
        public static bool Create(MainWindow mw, out ProjectileViewer viewer)
        {
            viewer = null;
            string folder;
            if ((folder = FileDialog.ChooseFolder()) != null)
            {
                if (!GameConfig.CheckFLDirectory(folder)) return false;
                viewer = new ProjectileViewer(mw, folder);
                return true;
            }
            return false;
        }

        private int cameraMode = 0;
        private static readonly DropdownOption[] camModes= new[]
        {
            new DropdownOption("Arcball", Icons.Globe, CameraModes.Arcball),
            new DropdownOption("Walkthrough", Icons.StreetView, CameraModes.Walkthrough),
        };
        private FileSystem vfs;
        private EffectsIni effects;
        private EquipmentIni equipment;
        private Viewport3D viewport;
        private MainWindow mw;
        private LookAtCamera camera = new LookAtCamera();
        private Munition[] projectileList;
        internal ProjectileViewer(MainWindow mw, string folder)
        {
            this.mw = mw;
            Title = "Projectiles";
            vfs = FileSystem.FromFolder(folder);
            var flini = new FreelancerIni(vfs);
            var data = new FreelancerData(flini, vfs);
            effects = new EffectsIni();
            equipment = new EquipmentIni();
            data.Equipment = equipment;
            foreach (var path in flini.EffectPaths) {
                effects.AddIni(path, vfs);
            }
            foreach (var path in flini.EquipmentPaths)  {
                equipment.AddEquipmentIni(path, data);
            }
            projectileList = equipment.Munitions.Where(x => !string.IsNullOrWhiteSpace(x.ConstEffect)).OrderBy(x => x.Nickname).ToArray();
            var fxShapes =   new TexturePanels(flini.EffectShapesPath, vfs);
            foreach (var f in fxShapes.Files)
            {
                var path = vfs.Resolve(flini.DataPath + f);
                mw.Resources.LoadResourceFile(path);
            }
            viewport = new Viewport3D(mw);
            viewport.Background = Color4.Black;
            viewport.DefaultOffset = viewport.CameraOffset = new Vector3(0,0,20);
            viewport.ModelScale = 10f;
            viewport.ResetControls();
            fxPool = new ParticleEffectPool(mw.Commands);
            beams = new BeamsBuffer();
        }

        private Munition currentMunition;
        private Effect constEffect;
        private BeamBolt bolt;
        private BeamSpear beam;
        private ParticleEffectPool fxPool;
        private BeamsBuffer beams;
        public override void Draw()
        {
            ImGui.Columns(2);
            ImGui.BeginChild("##munitions");
            foreach (var m in projectileList)
            {
                if (ImGui.Selectable(m.Nickname, currentMunition == m))
                {
                    currentMunition = m;
                    constEffect = effects.FindEffect(m.ConstEffect);
                    bolt = effects.BeamBolts.FirstOrDefault(x =>
                        x.Nickname.Equals(constEffect.VisBeam, StringComparison.OrdinalIgnoreCase));
                    beam = effects.BeamSpears.FirstOrDefault(x =>
                        x.Nickname.Equals(constEffect.VisBeam, StringComparison.OrdinalIgnoreCase));
                    viewport.ResetControls();
                }
            }
            ImGui.EndChild();
            ImGui.NextColumn();
            ImGui.BeginChild("##rendering");
            ViewerControls.DropdownButton("Camera Mode", ref cameraMode, camModes);
            viewport.Mode = (CameraModes) camModes[cameraMode].Tag;
            ImGui.SameLine();
            if (ImGui.Button("Reset Camera (Ctrl+R)"))
                viewport.ResetControls();
            viewport.Begin();
            Matrix4x4 rot = Matrix4x4.CreateRotationX(viewport.CameraRotation.Y) *
                          Matrix4x4.CreateRotationY(viewport.CameraRotation.X);
            var dirRot = Matrix4x4.CreateRotationX(viewport.ModelRotation.Y) * Matrix4x4.CreateRotationY(viewport.ModelRotation.X);
            var norm = Vector3.TransformNormal(-Vector3.UnitZ, dirRot);
            var dir = Vector3.Transform(-Vector3.UnitZ, rot);
            var to = viewport.CameraOffset + (dir * 10);
            if (viewport.Mode == CameraModes.Arcball)
                to = Vector3.Zero;
            camera.Update(viewport.RenderWidth, viewport.RenderHeight, viewport.CameraOffset, to, rot);
            mw.Commands.StartFrame(mw.RenderContext);
            beams.Begin(mw.Commands, mw.Resources, camera);
            var position = Vector3.Zero;
            if (beam != null)
            {
                beams.AddBeamSpear(position, norm, beam, float.MaxValue);
            } 
            else if (bolt != null)
            {
                beams.AddBeamBolt(position, norm, bolt, float.MaxValue);
            }
            beams.End();
            fxPool.Draw(camera, null, mw.Resources, null);
            mw.Commands.DrawOpaque(mw.RenderContext);
            mw.RenderContext.DepthWrite = false;
            mw.Commands.DrawTransparent(mw.RenderContext);
            mw.RenderContext.DepthWrite = true;
            if (constEffect != null)
            {
                var debugText = new StringBuilder();
                debugText.AppendLine($"ConstEffect: {constEffect.Nickname}");
                if (bolt != null) debugText.AppendLine($"Bolt: {bolt.Nickname}");
                if (beam != null) debugText.AppendLine($"Beam: {beam.Nickname}");
                mw.RenderContext.Renderer2D.DrawString("Arial", 10, debugText.ToString(), Vector2.One, Color4.White);
            }
            viewport.End();
            ImGui.EndChild();
        }
        
        
        
    }
}