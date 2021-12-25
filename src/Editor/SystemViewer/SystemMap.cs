// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Numerics;
using ImGuiNET;
using LibreLancer;
using LibreLancer.Interface;
using LibreLancer.GameData;
using LibreLancer.ImUI;

namespace SystemViewer
{
    public class SystemMap
    {
        private UiContext ctx;
        private Navmap navmap;
        private MainWindow win;
        public void CreateContext(MainWindow window)
        {
            var uidata = new UiData();
            uidata.FileSystem = window.GameData.VFS;
            uidata.DataPath = window.GameData.Ini.Freelancer.DataPath;
            if (window.GameData.Ini.Navmap != null)
                uidata.NavmapIcons = new IniNavmapIcons(window.GameData.Ini.Navmap);
            else
                uidata.NavmapIcons = new NavmapIcons();
            uidata.Fonts = window.GetService<FontManager>();
            uidata.ResourceManager = window.Resources;
            ctx = new UiContext(uidata);
            ctx.RenderContext = window.RenderContext;
            navmap = new Navmap();
            navmap.Width = 480;
            navmap.Height = 480;
            navmap.LetterMargin = true;
            navmap.MapBorder = true;
            ctx.SetWidget(navmap);
            this.win = window;
        }

        public void SetObjects(StarSystem sys)
        {
            navmap.PopulateIcons(ctx, sys);
        }

        private RenderTarget2D rtarget;
        private int rw = -1, rh = -1, rt = -1;

        static bool NavButton(string icon, string tooltip, bool selected)
        {
            if (selected) {
                ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive]);
            }
            var ret = Theme.IconButton(icon, icon, Color4.White);
            if(selected) ImGui.PopStyleColor();
            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip(tooltip);
            }
            return ret;
        }
        public void Draw(int width, int height, double delta)
        {
            //Set viewport
            height -= 30;
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;
            if (width != rw || height != rh)
            {
                if (rtarget != null) {
                    ImGuiHelper.DeregisterTexture(rtarget.Texture);
                    rtarget.Dispose();
                }
                rtarget = new RenderTarget2D(width, height);
                rw = width;
                rh = height;
                rt = ImGuiHelper.RegisterTexture(rtarget.Texture);
            }
            //Draw
            win.RenderContext.PushViewport(0, 0, width, height);
            ctx.ViewportWidth = width;
            ctx.ViewportHeight = height;
            ctx.RenderContext.RenderTarget = rtarget;
            ctx.RenderContext.ClearColor = Color4.TransparentBlack;
            ctx.RenderContext.ClearAll();
            ctx.RenderWidget(delta);
            ctx.RenderContext.RenderTarget = null;
            win.RenderContext.PopViewport();
            //ImGui
            //TODO: Implement in Navmap then add buttons
            /*
            NavButton("nav_labels", "Show Labels", true);
            ImGui.SameLine();
            ImGui.Dummy(new Vector2(72, 16)); //padding
            ImGui.SameLine();
            NavButton("nav_physical", "Physical Map", false);
            ImGui.SameLine();
            NavButton("nav_political", "Political Map", false);
            ImGui.SameLine();
            NavButton("nav_patrol", "Patrol Paths", false);
            ImGui.SameLine();
            NavButton("nav_mining", "Mining Zones", false);
            ImGui.SameLine();
            NavButton("nav_legend", "Legend", false);
            ImGui.SameLine();
            NavButton("nav_knownbases", "Known Bases", false);
            */
            var cpos = ImGui.GetCursorPos();
            ImGui.Image((IntPtr)rt, new Vector2(width, height), new Vector2(0,1), new Vector2(1,0),
            Color4.White);
            ImGui.SetCursorPos(cpos);
            ImGui.InvisibleButton("##navmap", new Vector2(width, height));
        }
    }
}