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
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;

namespace LibreLancer
{
	public class UIMenuButton : UIElement
	{
		public string Text = "";
		Color4 color;

		public UIMenuButton (UIManager manager, Vector2 position, string text, string tag = null) : base(manager)
		{
			UIScale = new Vector2 (2f, 3f);
			Text = text;
			UIPosition = position;
			Tag = tag;
			color = manager.TextColor;
		}

		public override void DrawBase ()
		{
			Manager.MenuButton.Draw (
				Manager.Game.RenderState,
				GetWorld (UIScale, Position),
				Lighting.Empty
			);
		}
		public override void Update (TimeSpan time)
		{
			var mstate = Manager.Game.Mouse.GetCursorState ();
			var rect = GetTextRectangle ();
			color = Tag != null ? Manager.TextColor : Color4.Gray;
			if (rect.Contains (Manager.Game.Input.MouseX, Manager.Game.Input.MouseY) && Tag != null) {
				color = Color4.Yellow;
				if (mstate.IsButtonDown (MouseButton.Left)) {
					Manager.OnClick (Tag);
				}
			}
		}
		public override void DrawText()
		{
			var r = GetTextRectangle ();
			var sz = GetTextSize (r.Height);
			DrawTextCentered (Manager.GetButtonFont (sz), Text, r, color);
		}

		float GetTextSize (float px)
		{
			return (int)Math.Floor ((px * (72.0f / 96.0f)) - 14);
		}

		Rectangle GetTextRectangle ()
		{
			var topleft = Manager.ScreenToPixel (Position.X - 0.125f * UIScale.X, Position.Y + 0.022f * UIScale.Y);
			var bottomRight = Manager.ScreenToPixel (Position.X + 0.125f * UIScale.X, Position.Y - 0.022f * UIScale.Y);
			var rect = new Rectangle (
				(int)topleft.X,
				(int)topleft.Y,
				(int)(bottomRight.X - topleft.X),
				(int)(bottomRight.Y - topleft.Y)
			);
			return rect;
		}

	}
}

