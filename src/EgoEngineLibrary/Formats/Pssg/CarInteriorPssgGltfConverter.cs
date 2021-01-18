﻿using EgoEngineLibrary.Graphics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace EgoEngineLibrary.Formats.Pssg
{
	public class CarInteriorPssgGltfConverter : PssgGltfConverter
	{
		private class ExportState : PssgModelReaderState
		{
			public ExportState()
			{
			}
		}

		public static bool SupportsPssg(PssgFile pssg)
		{
			return pssg.FindNodes("RENDERNODE").Any();
		}

		public ModelRoot Convert(PssgFile pssg)
		{
			var sceneBuilder = new SceneBuilder();

			var state = new ExportState();

			// F1 games use lib YYY
			var parent = pssg.FindNodes("LIBRARY", "type", "NODE").FirstOrDefault();
			if (parent is null)
			{
				parent = pssg.FindNodes("LIBRARY", "type", "YYY").FirstOrDefault();
				state.IsF1 = true;
			}
			if (parent is null)
				throw new InvalidDataException("Could not find library with scene nodes.");

			foreach (var child in parent.ChildNodes)
			{
				CreateNode(sceneBuilder, child, null, state);
			}

			return sceneBuilder.ToGltf2();
		}

		private static void CreateNode(SceneBuilder sceneBuilder, PssgNode node, NodeBuilder? parent, ExportState state)
		{
			// only consider a scene node if it has a transform child node
			if (!node.ChildNodes.Any(c => c.Name == "TRANSFORM")) return;

			NodeBuilder gltfNode;
			if (parent is null)
			{
				string name = (string)node.Attributes["id"].Value;
				gltfNode = new NodeBuilder(name);
				gltfNode.LocalTransform = GetTransform(node);
			}
			else if (node.Name == "RENDERNODE")
			{
				gltfNode = CreateMeshNode(sceneBuilder, node, parent, state);
			}
			else if (node.Name == "NODE")
			{
				string name = (string)node.Attributes["id"].Value;
				gltfNode = parent.CreateNode(name);
				gltfNode.LocalTransform = GetTransform(node);
			}
			else
			{
				throw new NotImplementedException($"Support for node {node.Name} not implemented.");
			}

			foreach (var child in node.ChildNodes)
			{
				CreateNode(sceneBuilder, child, gltfNode, state);
			}
		}

		private static NodeBuilder CreateMeshNode(SceneBuilder sceneBuilder, PssgNode renderNode, NodeBuilder parent, ExportState state)
		{
			string name = (string)renderNode.Attributes["id"].Value;
			NodeBuilder node = parent.CreateNode(name);
			node.LocalTransform = GetTransform(renderNode);

			var mesh = ConvertMesh(renderNode, state);
			sceneBuilder.AddRigidMesh(mesh, node);

			return node;
		}

		private static IMeshBuilder<MaterialBuilder> ConvertMesh(PssgNode renderNode, ExportState state)
		{
			IEnumerable<PssgNode> primitives = renderNode.FindNodes("RENDERSTREAMINSTANCE");

			var primitiveDatas = new List<PrimitiveData>();
			var texCoordSets = 0;
			foreach (var prim in primitives)
			{
				var shaderName = ((string)prim.Attributes["shader"].Value).Substring(1);
				var material = CreateMaterialBuilder(shaderName, state, out var createdNew);

				string rdsId = ((string)prim.Attributes["indices"].Value).Substring(1);
				var rdsNode = prim.File.FindNodes("RENDERDATASOURCE", "id", rdsId).First();

				var rds = new RenderDataSourceReader(rdsNode);
				texCoordSets = Math.Max(texCoordSets, rds.TexCoordSetCount);

				primitiveDatas.Add(new PrimitiveData(prim, material, createdNew, rds));
			}

			string name = (string)renderNode.Attributes["id"].Value;
			var mb = CreateMeshBuilder(name, texCoordSets);
			foreach (var prim in primitiveDatas)
			{
				if (prim.CreatedNewMaterial)
					ConvertMaterial(prim.Node.File, prim.Material, texCoordSets);

				var pb = mb.UsePrimitive(prim.Material);
				var rds = prim.Rds;

				var triangles = rds.GetTriangles();
				foreach (var tri in triangles)
				{
					pb.AddTriangle(
						CreateVertexBuilder(rds, tri.A, state),
						CreateVertexBuilder(rds, tri.B, state),
						CreateVertexBuilder(rds, tri.C, state));
				}
			}

			return mb;
		}

		private static void ConvertMaterial(PssgFile pssg, MaterialBuilder mat, int texCoordSets)
		{
			var shader = pssg.FindNodes("SHADERINSTANCE", "id", mat.Name).FirstOrDefault();
			if (shader is null)
				throw new InvalidDataException($"Could not find shader instance {mat.Name} referenced by the model.");

			var shaderGroupId = shader.Attributes["shaderGroup"].GetValue<string>().Substring(1);
			var sgNode = shader.File.FindNodes("SHADERGROUP", "id", shaderGroupId).FirstOrDefault();
			var textureInputs = shader.FindNodes("SHADERINPUT", "type", "texture");

			mat.WithMetallicRoughnessShader()
				.WithMetallicRoughness(0.1f, 0.5f)
				.WithBaseColor(new Vector4(1, 1, 1, 1));

			if (texCoordSets > 0)
			{
				mat.UseChannel(KnownChannel.BaseColor).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetDiffuseTexture(sgNode, textureInputs)))
					.WithCoordinateSet(0);
			}

			if (texCoordSets > 1)
			{
				mat.UseChannel(KnownChannel.Occlusion).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetOcclusionTexture(sgNode, textureInputs)))
					.WithCoordinateSet(1);
			}

			if (texCoordSets > 2)
			{
				mat.UseChannel(KnownChannel.Emissive).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetEmissiveTexture(sgNode, textureInputs)))
					.WithCoordinateSet(2);
			}

			if (texCoordSets > 3)
			{
				mat.UseChannel(KnownChannel.Normal).UseTexture()
					.WithPrimaryImage(new MemoryImage(GetNormalTexture(sgNode, textureInputs)))
					.WithCoordinateSet(3);
			}
		}
	}
}
