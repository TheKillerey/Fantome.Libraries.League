﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Libraries.League.Helpers.Structures;

namespace Fantome.Libraries.League.IO.MapGeometry
{
    public class MGEOVertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }

        public MGEOVertex(Vector3 position, Vector3 normal)
        {
            this.Position = position;
            this.Normal = normal;
        }

        public MGEOVertex(BinaryReader br)
        {
            this.Position = new Vector3(br);
            this.Normal = new Vector3(br);
        }
    }
}
