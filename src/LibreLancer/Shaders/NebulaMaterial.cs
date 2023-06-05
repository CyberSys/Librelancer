﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibreLancer.Shaders
{
    using System;
    
    public class NebulaMaterial
    {
        static ShaderVariables[] variants;
        private static bool iscompiled = false;
        private static int GetIndex(ShaderFeatures features)
        {
            ShaderFeatures masked = (features & ((ShaderFeatures)(8)));
            if ((masked == ((ShaderFeatures)(8))))
            {
                return 1;
            }
            return 0;
        }
        public static ShaderVariables Get(ShaderFeatures features)
        {
            AllShaders.Compile();
            return variants[GetIndex(features)];
        }
        public static ShaderVariables Get()
        {
            AllShaders.Compile();
            return variants[0];
        }
        internal static void Compile(string sourceBundle)
        {
            if (iscompiled)
            {
                return;
            }
            iscompiled = true;
            ShaderVariables.Log("Compiling NebulaMaterial");
            variants = new ShaderVariables[2];
            // No GL4 variants detected
            variants[0] = ShaderVariables.Compile(sourceBundle.Substring(331645, 549), sourceBundle.Substring(332194, 308));
            variants[1] = ShaderVariables.Compile(sourceBundle.Substring(332502, 574), sourceBundle.Substring(332194, 308));
        }
    }
}
