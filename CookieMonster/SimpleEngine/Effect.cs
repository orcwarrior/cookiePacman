/* 2D C# Graphics Engine
 * Copyright (c) 2008 Lubos Lenco
 * See license.txt for license info
 */

using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics;
using OpenTK.Input;


namespace Engine
{
    class Effect
    {
        int vertex_shader_object, fragment_shader_object, shader_program;
        int vertex_buffer_object, color_buffer_object, element_buffer_object;


        public void Init()
        {
            using (StreamReader vs = new StreamReader("Data/Shaders/Simple_VS.glsl"))
            using (StreamReader fs = new StreamReader("Data/Shaders/Simple_FS.glsl"))
                CreateShaders(vs.ReadToEnd(), fs.ReadToEnd(),
                    out vertex_shader_object, out fragment_shader_object,
                    out shader_program);
        }


        void CreateShaders(string vs, string fs,
            out int vertexObject, out int fragmentObject,
            out int program)
        {
            int status_code;
            string info;

            vertexObject = GL.CreateShader(ShaderType.VertexShader);
            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex shader
            GL.ShaderSource(vertexObject, vs);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out status_code);

            //if (status_code != 1)
            //    throw new ApplicationException(info);

            // Compile vertex shader
            GL.ShaderSource(fragmentObject, fs);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out status_code);

            //if (status_code != 1)
            //    throw new ApplicationException(info);

            program = GL.CreateProgram();
            GL.AttachShader(program, fragmentObject);
            GL.AttachShader(program, vertexObject);

            GL.LinkProgram(program);
            GL.UseProgram(program);
        }
    }
}
