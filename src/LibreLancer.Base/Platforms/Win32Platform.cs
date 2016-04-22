﻿/* The contents of this file are subject to the Mozilla Public License
 * Version 1.1 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS"
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
 * License for the specific language governing rights and limitations
 * under the License.
 * 
 * 
 * The Initial Developer of the Original Code is Callum McGing (mailto:callum.mcging@gmail.com).
 * Portions created by the Initial Developer are Copyright (C) 2013-2016
 * the Initial Developer. All Rights Reserved.
 */
using System;
using SharpFont;
using LibreLancer.Platforms.Win32;
namespace LibreLancer.Platforms
{
	class Win32Platform : IPlatform
	{
		public bool IsDirCaseSensitive(string directory)
		{
			return false;
		}

		public Face LoadSystemFace (Library library, string face)
		{
			byte[] buffer;
			//Get font data from GDI
			unsafe {
				var hfont = GDI.CreateFont (0, 0, 0, 0, GDI.FW_REGULAR,
					0, 0, 0, GDI.DEFAULT_CHARSET, GDI.OUT_OUTLINE_PRECIS,
					GDI.CLIP_DEFAULT_PRECIS, GDI.DEFAULT_QUALITY,
					GDI.DEFAULT_PITCH, face);
				//get data
				var hdc = GDI.CreateCompatibleDC(IntPtr.Zero);
				GDI.SelectObject (hdc, hfont);
				var size = GDI.GetFontData (hdc, 0, 0, IntPtr.Zero, 0);
				buffer = new byte[size];
				fixed(byte* ptr = buffer) {
					GDI.GetFontData (hdc, 0, 0, (IntPtr)ptr, size);
				}
				GDI.DeleteDC (hdc);
				//delete font
				GDI.DeleteObject (hfont);
			}
			//create font object
			return new Face(library, buffer,0);
		}
	}
}

