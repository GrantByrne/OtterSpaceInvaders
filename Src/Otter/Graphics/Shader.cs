using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.Graphics;
using SFML;
using SFML.Window;
using System.IO;

namespace Otter {
    /// <summary>
    /// Class representing a shader written in GLSL.
    /// </summary>
    public class Shader {

        #region Constructors

        /// <summary>
        /// Creates a Shader using a file as the source for the vertex and fragment shader.
        /// </summary>
        /// <param name="vertexFile">The file path to the vertex shader.</param>
        /// <param name="fragmentFile">The file path to the fragment shader.</param>
        public Shader(string vertexFile, string fragmentFile) {
            shader = new SFML.Graphics.Shader(vertexFile, fragmentFile);
        }

        /// <summary>
        /// Create a Shader using a stream as the source of the vertex and fragment shader.
        /// </summary>
        /// <param name="vertexStream">The stream for the vertex shader.</param>
        /// <param name="fragmentStream">The stream for the fragment shader.</param>
        public Shader(Stream vertexStream, Stream fragmentStream) {
            shader = new SFML.Graphics.Shader(vertexStream, fragmentStream);
        }

        /// <summary>
        /// Create a shader using a stream as the source and a ShaderType parameter.
        /// </summary>
        /// <param name="shaderType">The shader type (fragment or vertex)</param>
        /// <param name="source">The stream for the shader.</param>
        public Shader(ShaderType shaderType, Stream source) {
            if (shaderType == ShaderType.Vertex) {
                shader = new SFML.Graphics.Shader(source, null);
            }
            else {
                shader = new SFML.Graphics.Shader(null, source);
            }
        }

        /// <summary>
        /// Creates a Shader using a file path source, and auto detects which type of shader
        /// it is.  If the file path contains ".frag" or ".fs" it is assumed to be a fragment shader.
        /// </summary>
        /// <param name="source">The file path.</param>
        public Shader(string source) {
            if (source.Contains(".frag") || source.Contains(".fs")) {
                shader = new SFML.Graphics.Shader(null, source);
            }
            else {
                shader = new SFML.Graphics.Shader(source, null);
            }
        }

        /// <summary>
        /// Creates a shader using a copy of another shader.
        /// </summary>
        /// <param name="copy">The shader to copy.</param>
        public Shader(Shader copy) : this(copy.shader) { }

        /// <summary>
        /// Creates a shader using a file path and a ShaderType parameter.
        /// </summary>
        /// <param name="shaderType">The shader type (fragment or vertex)</param>
        /// <param name="source">The file path.</param>
        public Shader(ShaderType shaderType, string source) {
            if (shaderType == ShaderType.Vertex) {
                shader = new SFML.Graphics.Shader(source, null);
            }
            else {
                shader = new SFML.Graphics.Shader(null, source);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set a parameter on the shader.
        /// </summary>
        /// <param name="name">The parameter in the shader to set.</param>
        /// <param name="color">The color to set it to.</param>
        public void SetParameter(string name, Color color) {
            shader.SetParameter(name, color.SFMLColor);
        }

        /// <summary>
        /// Set a parameter on the shader.
        /// </summary>
        /// <param name="name">The parameter in the shader to set.</param>
        /// <param name="x">The value to set it to.</param>
        public void SetParameter(string name, float x) {
            shader.SetParameter(name, x);
        }

        /// <summary>
        /// Set a parameter on the shader.
        /// </summary>
        /// <param name="name">The parameter in the shader to set.</param>
        /// <param name="x">The first value of a vec2.</param>
        /// <param name="y">The first value of a vec2.</param>
        public void SetParameter(string name, float x, float y) {
            shader.SetParameter(name, x, y);
        }

        /// <summary>
        /// Set a parameter on the shader.
        /// </summary>
        /// <param name="name">The parameter in the shader to set.</param>
        /// <param name="x">The first value of a vec3.</param>
        /// <param name="y">The second value of a vec3.</param>
        /// <param name="z">The third value of a vec3.</param>
        public void SetParameter(string name, float x, float y, float z) {
            shader.SetParameter(name, x, y);
        }

        /// <summary>
        /// Set a parameter on the shader.
        /// </summary>
        /// <param name="name">The parameter in the shader to set.</param>
        /// <param name="x">The first value of a vec4.</param>
        /// <param name="y">The second value of a vec4.</param>
        /// <param name="z">The third value of a vec4.</param>
        /// <param name="w">The fourth value of a vec4.</param>
        public void SetParameter(string name, float x, float y, float z, float w) {
            shader.SetParameter(name, x, y, z, w);
        }

        /// <summary>
        /// Set a parameter on the shader.
        /// </summary>
        /// <param name="name">The parameter in the shader to set.</param>
        /// <param name="texture">The texture to set it to.</param>
        public void SetParameter(string name, Texture texture) {
            shader.SetParameter(name, texture.SFMLTexture);
        }

        #endregion

        #region Internal

        internal SFML.Graphics.Shader shader;

        internal Shader(SFML.Graphics.Shader shader) {
            this.shader = shader;
        }

        #endregion

    }

    #region Enum

    public enum ShaderType {
        Vertex,
        Fragment
    }

    #endregion

}
