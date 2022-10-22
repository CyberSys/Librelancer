﻿// MIT License - Copyright (c) Callum McGing
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using BulletSharp;
using BM = BulletSharp.Math;

namespace LibreLancer.Physics.Sur
{
	//TODO: Sur reader is VERY incomplete & undocumented
	public class SurFile
	{
		const string VERS_TAG = "vers";
		Dictionary<uint, Surface> surfaces = new Dictionary<uint, Surface>();
		Dictionary<uint, ConvexTriangleMeshShape[]> shapes = new Dictionary<uint, ConvexTriangleMeshShape[]>();
        
        public IEnumerable<uint> MeshIds => surfaces.Keys;
        public List<uint> HardpointIds = new List<uint>();

        public void FillMeshHierarchy(SurPart part)
        {
            Surface sfc;
            if (surfaces.TryGetValue(part.Hash, out sfc))
            {
                foreach (var group in sfc.Groups)
                {
                    //Find a list to add the geometry to
                    List<ConvexMesh> hull = null;
                    Matrix4x4 transform = Matrix4x4.Identity;
                    if (group.MeshID == part.Hash && !part.ParentSet)
                    {
                        if (part.DisplayMeshes == null) part.DisplayMeshes = new List<ConvexMesh>();
                        hull = part.DisplayMeshes;
                    }
                    else
                    {
                        var child = IterateChildren(part).FirstOrDefault((x) => x.Hash == group.MeshID);
                        if (child != null)
                        {
                            if (part.DisplayMeshes == null) part.DisplayMeshes = new List<ConvexMesh>();
                            hull = part.DisplayMeshes;
                            //Parent has hull
                            child.ParentSet = true;
                        }
                    }
                    if (hull != null)
                    {
                        //Fill the geometry
                        var verts = new List<Vector3>();
                        foreach (var v in sfc.Vertices)
                            verts.Add(Vector3.Transform(v.Point.Cast(), transform));
                        var indices = new List<int>();
                        if (group.VertexArrayOffset != 0)
                            throw new Exception("tgroupheader vertexarrayoffset wrong");
                        foreach (var tri in group.Triangles)
                        {
                            indices.Add(tri.Vertices[0].Vertex);
                            indices.Add(tri.Vertices[1].Vertex);
                            indices.Add(tri.Vertices[2].Vertex);
                        }
                        hull.Add(new ConvexMesh() {Indices = indices.ToArray(), Vertices = verts.ToArray()});
                    }
                }
            }
            //Go through children
            if(part.Children != null)
                foreach (var c in part.Children)
                    FillMeshHierarchy(c);
        }

        IEnumerable<SurPart> IterateChildren(SurPart parent)
        {
            if (parent.Children != null)
            {
                foreach (var p in parent.Children)
                {
                    yield return p;
                    foreach (var c in IterateChildren(p)) yield return c;
                }
            }
        }
        //For editor display. HACK: Horribly inefficient
        public ConvexMesh[] GetMesh(uint meshId, bool hardpoint)
        {
            List<ConvexMesh> hull = new List<ConvexMesh>();
            foreach (var surface in surfaces.Values)
            {
                for (int i = 0; i < surface.Groups.Length; i++)
                {
                    var th = surface.Groups[i];
                    if (th.MeshID != meshId)
                        continue;
                    var verts = new List<Vector3>();
                    foreach (var v in surface.Vertices)
                        verts.Add(v.Point.Cast());
                    var indices = new List<int>();
                    if (th.VertexArrayOffset != 0)
                        throw new Exception("tgroupheader vertexarrayoffset wrong");
                    foreach (var tri in th.Triangles)
                    {
                        indices.Add(tri.Vertices[0].Vertex);
                        indices.Add(tri.Vertices[1].Vertex);
                        indices.Add(tri.Vertices[2].Vertex);
                    }
                    hull.Add(new ConvexMesh() { Indices = indices.ToArray(), Vertices = verts.ToArray(), ParentCrc = surface.Crc });
                }
            }
            return hull.ToArray();
        }
        
        //I'm assuming this gives me some sort of workable mesh
        public ConvexTriangleMeshShape[] GetShape(uint meshId)
		{
			if (!shapes.ContainsKey(meshId))
			{
				List<ConvexTriangleMeshShape> hull = new List<ConvexTriangleMeshShape>();
				var surface = surfaces[meshId];
                for (int i = 0; i < surface.Groups.Length; i++)
				{
					var th = surface.Groups[i];
					if (th.MeshID != meshId)
						continue;
					var verts = new List<BM.Vector3>();
                    foreach (var v in surface.Vertices)
                        verts.Add(v.Point);
                    var indices = new List<int>();
					if (th.VertexArrayOffset != 0)
						throw new Exception("tgroupheader vertexarrayoffset wrong");
					foreach (var tri in th.Triangles)
					{
                        indices.Add(tri.Vertices[0].Vertex);
                        indices.Add(tri.Vertices[1].Vertex);
                        indices.Add(tri.Vertices[2].Vertex);
					}
                    hull.Add(new ConvexTriangleMeshShape(new TriangleIndexVertexArray(indices, verts)));
				}
				shapes.Add(meshId, hull.ToArray());
			}
			return shapes[meshId];
		}
		public bool HasShape(uint meshId)
		{
			return surfaces.ContainsKey(meshId);
		}
		public SurFile (Stream stream)
		{
			using (var reader = new BinaryReader (stream)) {
				if (reader.ReadTag () != VERS_TAG)
					throw new Exception ("Not a sur file");
				if (reader.ReadSingle() != 2.0)
				{
					throw new Exception("Incorrect sur version");
				}
				while (stream.Position < stream.Length) {
					uint meshid = reader.ReadUInt32 ();
					uint tagcount = reader.ReadUInt32 ();
					while (tagcount-- > 0) {
						var tag = reader.ReadTag ();
						if (tag == "surf") {
							uint size = reader.ReadUInt32 (); //TODO: SUR - What is this?
                            var surf = new Surface(reader, meshid);
							surfaces.Add(meshid, surf);
						} else if (tag == "exts") {
							//TODO: SUR - What are exts used for?
							/*var min = new JVector (
								          reader.ReadSingle (),
								          reader.ReadSingle (),
								          reader.ReadSingle ()
							          );
							var max = new JVector (
								          reader.ReadSingle (),
								          reader.ReadSingle (),
								          reader.ReadSingle ()
							          );*/
							reader.BaseStream.Seek(6 * sizeof(float), SeekOrigin.Current);
						} else if (tag == "!fxd") {
							//TODO: SUR - WTF is this?!
						} else if (tag == "hpid") {
							//TODO: SUR - hpid. What does this do?
							uint count2 = reader.ReadUInt32 ();
							while (count2-- > 0) {
                                HardpointIds.Add(reader.ReadUInt32());
							}
						}
					}
				}
			}
		}


	}
}

