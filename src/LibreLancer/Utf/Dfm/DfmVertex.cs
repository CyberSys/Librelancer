﻿// MIT License - Copyright (c) Malte Rupprecht
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Runtime.InteropServices;
using LibreLancer.Vertices;
namespace LibreLancer.Utf.Dfm
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DfmVertex : IVertexType
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 TextureCoordinate;
		public Vector4 BoneWeights;
		public float BoneId1;
        public float BoneId2;
        public float BoneId3;
        public float BoneId4;
		public DfmVertex(Vector3 pos, Vector3 normal, Vector2 texcoord, Vector4 boneWeights, int id1, int id2, int id3, int id4)
		{
			Position = pos;
			Normal = normal;
			TextureCoordinate = texcoord;
            BoneWeights = boneWeights;
            BoneId1 = id1;
            BoneId2 = id2;
            BoneId3 = id3;
            BoneId4 = id4;
		}

		public VertexDeclaration GetVertexDeclaration()
		{
			return new VertexDeclaration(
				sizeof(float) * 3 + sizeof(float) * 3 + sizeof(float) * 2 + sizeof(float) * 4 + sizeof(float) * 4,
				new VertexElement(VertexSlots.Position, 3, VertexElementType.Float, false, 0),
				new VertexElement(VertexSlots.Normal, 3, VertexElementType.Float, false, sizeof(float) * 3),
				new VertexElement(VertexSlots.Texture1, 2, VertexElementType.Float, false, sizeof(float) * 6),
				new VertexElement(VertexSlots.BoneWeights, 1, VertexElementType.Float, false, sizeof(float) * 8),
				new VertexElement(VertexSlots.BoneIds, 4, VertexElementType.Float, false, sizeof(float) * 12)
			);
		}
	}
}

