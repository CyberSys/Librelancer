﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Numerics;
using LibreLancer.Infocards;
using LibreLancer;
using LibreLancer.ImUI;
using ImGuiNET;
namespace SystemViewer
{
    public class InfocardControl : IDisposable
    {
        BuiltRichText icard;
        MainWindow window;
        RenderTarget2D renderTarget;
        int renderWidth = -1, renderHeight = -1, rid = -1;
        public InfocardControl(MainWindow win, Infocard infocard, float initWidth)
        {
            window = win;
            icard = win.RichText.BuildText(infocard.Nodes, (int)initWidth, 0.8f);
            //icard = new InfocardDisplay(win, new Rectangle(0, 0, (int)initWidth, int.MaxValue), infocard);
            //icard.FontScale = 0.8f;
        }
        public void SetInfocard(Infocard infocard)
        {
            icard.Dispose();
            icard = window.RichText.BuildText(infocard.Nodes, renderWidth > 0 ? renderWidth : 400, 0.8f);
        }
        public void Draw(float width)
        {
            icard.Recalculate(width);
            if (icard.Height < 1 || width < 1) {
                ImGui.Dummy(new Vector2(1, 1));
                return;
            }
            if (icard.Height != renderHeight || (int)width != renderWidth)
            {
                renderWidth = (int)width;
                renderHeight = (int)icard.Height;
                if (renderTarget != null)
                {
                    ImGuiHelper.DeregisterTexture(renderTarget.Texture);
                    renderTarget.Dispose();
                }
                renderTarget = new RenderTarget2D(renderWidth, renderHeight);
                rid = ImGuiHelper.RegisterTexture(renderTarget.Texture);
            }

            window.RenderContext.RenderTarget = renderTarget;
            window.RenderContext.PushViewport(0, 0, renderWidth, renderHeight);
            var cc = window.RenderContext.ClearColor;
            window.RenderContext.ClearColor = Color4.Transparent;
            window.RenderContext.ClearAll();
            window.RenderContext.ClearColor = cc;
            window.RichText.RenderText(icard, 0, 0);
            window.RenderContext.RenderTarget = null;
            window.RenderContext.PopViewport();

            //ImGui. Base off ImageButton so we can get input for selection later
            var style = ImGui.GetStyle();
            var btn = style.Colors[(int)ImGuiCol.Button];
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, btn);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, btn);
            ImGui.ImageButton((IntPtr)rid, new Vector2(renderWidth, icard.Height),
                                 new Vector2(0, 1), new Vector2(1, 0),
                                 0,
                                 Vector4.Zero, Vector4.One);
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            //Somehow keep track of selection? (idk if InfocardDisplay should do this)
        }
        public void Dispose()
        {
            renderTarget.Dispose();
        }
    }
}
