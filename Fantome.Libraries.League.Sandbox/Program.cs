﻿using Fantome.Libraries.League.Converters;
using Fantome.Libraries.League.Helpers.Structures;
using Fantome.Libraries.League.Helpers.Structures.BucketGrid;
using Fantome.Libraries.League.IO.AnimationFile;
using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.NavigationGridOverlay;
using Fantome.Libraries.League.IO.NVR;
using Fantome.Libraries.League.IO.OBJ;
using Fantome.Libraries.League.IO.ReleaseManifestFile;
using Fantome.Libraries.League.IO.SimpleSkinFile;
using Fantome.Libraries.League.IO.SkeletonFile;
using Fantome.Libraries.League.IO.StaticObjectFile;
using Fantome.Libraries.League.IO.WadFile;
using Fantome.Libraries.League.IO.WGT;
using Fantome.Libraries.League.IO.WorldGeometry;
using ImageMagick;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueAnimation = Fantome.Libraries.League.IO.AnimationFile.Animation;

namespace Fantome.Libraries.League.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleSkin simpleSkin = new SimpleSkin(@"C:\Users\Crauzer\Desktop\New folder\assets\characters\aatrox\skins\base\aatrox.skn");
            Skeleton skeleton = new Skeleton(@"C:\Users\Crauzer\Desktop\New folder\assets\characters\aatrox\skins\base\aatrox.skl");

            var textureMap = new Dictionary<string, MagickImage>() 
            {
                { "Body", new MagickImage(@"C:\Users\Crauzer\Desktop\New folder\assets\characters\aatrox\skins\base\aatrox_base_tx_cm.dds") }
            };

            ModelRoot gltf = simpleSkin.ToGltf(skeleton, textureMap);

            gltf.SaveGLB(@"C:\Users\Crauzer\Desktop\New folder\assets\characters\aatrox\skins\base\aatrox.glb");
        }

        static void TestMapgeo()
        {
            MapGeometry mgeo = new MapGeometry(@"C:/Users/Crauzer/Desktop/data/maps/mapgeometry/sr/base_srx.mapgeo");

            string randomMaterialName = mgeo.Models[180].Submeshes[0].Material;

            mgeo.Models.Clear();

            OBJFile object1 = new OBJFile("room155.obj");
            OBJFile object2 = new OBJFile("room156.obj");
            OBJFile object3 = new OBJFile("room157.obj");

            AddOBJ(object1, "MapGeo_Instance_0");
            AddOBJ(object2, "MapGeo_Instance_1");
            AddOBJ(object3, "MapGeo_Instance_2");

            mgeo.Write("base_srx.mapgeo.edited", 7);

            void AddOBJ(OBJFile obj, string name)
            {
                //We will add each object 2 times just for fun to see how transformation works

                (List<ushort> indices, List<MapGeometryVertex> vertices) = obj.GetMGEOData();

                R3DMatrix44 transformation = R3DMatrix44.FromTranslation(new Vector3(0, 50, 100));

                MapGeometrySubmesh submesh = new MapGeometrySubmesh("", 0, (uint)indices.Count, 0, (uint)vertices.Count);
                MapGeometryModel model1 = new MapGeometryModel(name, vertices, indices, new List<MapGeometrySubmesh>() { submesh }, MapGeometryLayer.AllLayers);

                mgeo.AddModel(model1);
            }
        }

        static void TestWGEO()
        {
            WorldGeometry wgeo = new WorldGeometry("room.wgeo");
            Directory.CreateDirectory("kek");

            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    BucketGridBucket bucket = wgeo.BucketGrid.Buckets[i, j];

                    List<uint> indices = wgeo.BucketGrid.Indices
                        .GetRange((int)bucket.StartIndex, (bucket.InsideFaceCount + bucket.StickingOutFaceCount) * 3)
                        .Select(x => (uint)x)
                        .ToList();

                    if (indices.Count != 0)
                    {
                        int startVertex = (int)indices.Min();
                        int vertexCount = (int)indices.Max() - startVertex;
                        List<Vector3> vertices = wgeo.BucketGrid.Vertices.GetRange(startVertex + (int)bucket.BaseVertex, vertexCount);

                        new OBJFile(vertices, indices).Write(string.Format("kek/bucket{0}_{1}.obj", i, j));
                    }
                }
            }
        }

        static void TestStaticObject()
        {
            StaticObject sco = StaticObject.ReadSCB("aatrox_base_w_ground_ring.scb");
            sco.WriteSCO(@"C:\Users\Crauzer\Desktop\zzzz.sco");

            StaticObject x = StaticObject.ReadSCB(@"C:\Users\Crauzer\Desktop\zzzz.scb");

        }
    }
}
