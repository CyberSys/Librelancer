// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

#include "imgui.h"
#include "imgui_freetype.h"
#include "cimgui_ext.h"
#include "ftshim.h"

static bool functionsLoaded = false;
extern "C" {
IGEXPORT bool igBuildFontAtlas(void* atlas)
{
	if(!functionsLoaded) {
		igLoadFreetype();
		functionsLoaded = true;
	}
	return ImGuiFreeType::BuildFontAtlas((ImFontAtlas*)atlas);
}


IGEXPORT void igMapGlyph(int glyph, int actual)
{
    ImGuiFreeType::MapGlyph(glyph, actual);
}
}
/* DYNAMICALLY LOADED FREETYPE USING FREETYPESHIM */
