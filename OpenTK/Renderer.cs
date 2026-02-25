using Base.Rendering;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Base.Rendering
{
    public class Renderer
    {
        public struct vaoContents 
        {
            public int vao;
            public int vbo;
            public int ebo;
            public float[] vertices;
            public uint[] indices;
            public int indexCount;
        }
        public struct shaderContents
        {
            public int shaderProgram;
            public string vertexSource;
            public string fragmentSource;
        }

        public struct RenderEntity
        {
            public vaoContents mesh;
            public int shaderProgram;
        }

        public static List<RenderEntity> renderEntities = new List<RenderEntity>();

        public static void RenderAllEntites()
        {
            foreach (RenderEntity entity in renderEntities)
            {
                GL.BindVertexArray(entity.mesh.vao);
                GL.UseProgram(entity.shaderProgram);
                GL.DrawElements(PrimitiveType.Triangles, entity.mesh.indexCount, DrawElementsType.UnsignedInt, 0);
            }
        }

        public static void ChangeShader(ref RenderEntity entity, string vertexSource, string fragmentSource)
        {
            Console.WriteLine("Change Shader Requseted Sender");
            entity.shaderProgram = Renderer.CompileCustomShader(vertexSource, fragmentSource);
        }

        public static RenderEntity GenerateMesh(float[] vertices, uint[] indices, shaderContents shader)
        {
            if (vertices.Length == 0) throw new Exception("GenerateMesh verices was empty");
            int program = Renderer.CompileCustomShader(shader.vertexSource, shader.fragmentSource);
            vaoContents contents = new vaoContents();
            contents.vao = GL.GenVertexArray();
            contents.vbo = GL.GenBuffer();
            contents.ebo = GL.GenBuffer();
            contents.indexCount = indices.Length;
            GL.BindVertexArray(contents.vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, contents.vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, contents.ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            RenderEntity entity = new RenderEntity() { mesh = contents, shaderProgram = program };
            renderEntities.Add(entity);

            return entity;
        }

        public static int CompileCustomShader(string vSource, string fSource)
        {
            Console.WriteLine("Compile Shader Requseted");
            Console.WriteLine("Vertex Source: " + vSource);
            Console.WriteLine("Fragment Source: " + fSource);
            int v = GL.CreateShader(ShaderType.VertexShader); GL.ShaderSource(v, vSource); GL.CompileShader(v);
            int f = GL.CreateShader(ShaderType.FragmentShader); GL.ShaderSource(f, fSource); GL.CompileShader(f);
            int prog = GL.CreateProgram();
            GL.AttachShader(prog, v); GL.AttachShader(prog, f);
            GL.LinkProgram(prog);
            GL.DeleteShader(v); GL.DeleteShader(f);
            return prog;
        }

        public static void DeleteEntity(RenderEntity entity)
        {
            // Listeden çıkar
            renderEntities.Remove(entity);

            // GPU belleğinden sil
            GL.DeleteBuffer(entity.mesh.vbo);
            GL.DeleteBuffer(entity.mesh.ebo);
            GL.DeleteVertexArray(entity.mesh.vao);
            GL.DeleteProgram(entity.shaderProgram);
        }

        public static void ReloadEverything(ref RenderEntity entity, string vertShaderPath, string fragShaderPath, string objPath)
        {
            try
            {
                Renderer.LoadObj(objPath, out entity.mesh.vertices, out entity.mesh.indices);
                Renderer.shaderContents shader = new Renderer.shaderContents
                {
                    vertexSource = File.ReadAllText(Path.Combine("Shaders", vertShaderPath)),
                    fragmentSource = File.ReadAllText(Path.Combine("Shaders", fragShaderPath))
                };
                entity = Renderer.GenerateMesh(entity.mesh.vertices, entity.mesh.indices, shader);
                ReloadShader(vertShaderPath, fragShaderPath, shader);
            }
            catch (Exception e) { Console.WriteLine("Hata: " + e.Message); }
        }

        public static void ReloadShader(string vertShaderPath, string fragShaderPath, shaderContents shader)
        {
            try
            {
                string vSrc = File.ReadAllText(Path.Combine("Shaders", vertShaderPath));
                string fSrc = File.ReadAllText(Path.Combine("Shaders", fragShaderPath));
                shader.shaderProgram = Renderer.CompileCustomShader(vSrc, fSrc);
            }
            catch { /* Dosya okuma hatası */ }
        }

        public static void LoadObj(string path, out float[] outVertices, out uint[] outIndices)
        {
            List<float> vList = new List<float>();
            List<uint> iList = new List<uint>();
            string fullPath = Path.Combine("Meshes", path);

            if (!File.Exists(fullPath)) { outVertices = Array.Empty<float>(); outIndices = Array.Empty<uint>(); return; }

            foreach (var line in File.ReadAllLines(fullPath))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                if (parts[0] == "v")
                {
                    vList.Add(float.Parse(parts[1], CultureInfo.InvariantCulture));
                    vList.Add(float.Parse(parts[2], CultureInfo.InvariantCulture));
                    vList.Add(float.Parse(parts[3], CultureInfo.InvariantCulture));
                }
                else if (parts[0] == "f")
                {
                    for (int i = 1; i <= 3; i++) iList.Add(uint.Parse(parts[i].Split('/')[0]) - 1);
                }
            }
            outVertices = vList.ToArray();
            outIndices = iList.ToArray();
        }
    }
}