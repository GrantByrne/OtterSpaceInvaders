using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Otter {
    /// <summary>
    /// Class representing a texture. Can perform pixel operations on the CPU, but those will be
    /// pretty slow and shouldn't be used that much.
    /// </summary>
    public class Texture {

        #region Public Properties

        /// <summary>
        /// The file path source if the texture was loaded from a file.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Determines if the source texture is smoothed when transformed.
        /// </summary>
        public bool Smooth { get { return texture.Smooth; } set { texture.Smooth = value; } }

        /// <summary>
        /// The array of pixels in the texture in bytes.
        /// </summary>
        public byte[] Pixels {
            get {
                CreateImage();
                return image.Pixels;
            }
            set {
                image = new SFML.Graphics.Image((uint)Width, (uint)Height, value);
                Update();
            }
        }

        /// <summary>
        /// The width of the Texture.
        /// </summary>
        public int Width {
            get { return (int)texture.Size.X; }
        }

        /// <summary>
        /// The height of the texture.
        /// </summary>
        public int Height {
            get { return (int)texture.Size.Y; }
        }

        /// <summary>
        /// The rectangle created by the Texture's width and height.
        /// </summary>
        public Rectangle Region {
            get { return new Rectangle(0, 0, Width, Height); }
        }

        #endregion

        #region Private Fields

        internal bool needsUpdate = false;
        internal SFML.Graphics.Texture texture;
        SFML.Graphics.Image image;

        #endregion

        #region Constructors

        /// <summary>
        /// Load a texture from a file path.
        /// </summary>
        /// <param name="source">The file path to load from.</param>
        /// <param name="useCache">Determines if the cache should be checked first.</param>
        public Texture(string source, bool useCache = true) {
            if (useCache) {
                texture = Textures.Load(source);
            }
            else {
                texture = new SFML.Graphics.Texture(source);
            }
            Source = source;

            if (Game.Instance != null) {
                texture.Smooth = Game.Instance.SmoothAll;
            }
        }

        /// <summary>
        /// Create a texture from a stream of bytes.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <param name="useCache">Determines if the cache should be checked first.</param>
        public Texture(Stream stream, bool useCache = true) {
            if (useCache) {
                texture = Textures.Load(stream);
            }
            else {
                texture = new SFML.Graphics.Texture(stream);
            }
            Source = "stream";

            if (Game.Instance != null) {
                texture.Smooth = Game.Instance.SmoothAll;
            }
        }

        /// <summary>
        /// Creates a new texture from a copy of another texture.  No cache option for this.
        /// </summary>
        /// <param name="copy">The texture to copy from.</param>
        public Texture(Texture copy) {
            texture = new SFML.Graphics.Texture(copy.SFMLTexture);

            Source = copy.Source;

            if (Game.Instance != null) {
                texture.Smooth = Game.Instance.SmoothAll;
            }
        }

        /// <summary>
        /// Create a texture from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array to load from.</param>
        /// <param name="useCache">Determines if the cache should be checked first.</param>
        public Texture(byte[] bytes, bool useCache = true) {
            if (useCache) {
                using (MemoryStream ms = new MemoryStream(bytes)) {
                    texture = Textures.Load(ms);
                }
            }
            else {
                using (MemoryStream ms = new MemoryStream(bytes)) {
                    texture = new SFML.Graphics.Texture(ms);
                }
            }
            Source = "byte array";

            if (Game.Instance != null) {
                texture.Smooth = Game.Instance.SmoothAll;
            }
        }

        /// <summary>
        /// Creates an empty texture of width and height.  This does not use the cache.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        public Texture(int width, int height) {
            if (width < 0) throw new ArgumentException("Width must be greater than 0.");
            if (height < 0) throw new ArgumentException("Height must be greater than 0.");

            texture = new SFML.Graphics.Texture((uint)width, (uint)height);

            Source = width + " x " + height + " texture";

            if (Game.Instance != null) {
                texture.Smooth = Game.Instance.SmoothAll;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the Color from a specific pixel on the texture.
        /// Warning: This is slow!
        /// </summary>
        /// <param name="x">The x coordinate of the pixel to get.</param>
        /// <param name="y">The y coordinate of the pixel to get.</param>
        /// <returns>The Color of the pixel.</returns>
        public Color GetPixel(int x, int y) {
            if (x < 0) throw new ArgumentException("X must be greater than 0.");
            if (y < 0) throw new ArgumentException("Y must be greater than 0.");

            CreateImage();

            return new Color(texture.CopyToImage().GetPixel((uint)x, (uint)y));
        }

        /// <summary>
        /// Sets the color of a specific pixel on the texture.
        /// </summary>
        /// <param name="x">The x coordinate of the pixel.</param>
        /// <param name="y">The y coordinate of the pixel.</param>
        /// <param name="color">The Color to set the pixel to.</param>
        public void SetPixel(int x, int y, Color color) {
            if (x < 0) throw new ArgumentException("X must be greater than 0.");
            if (y < 0) throw new ArgumentException("Y must be greater than 0.");
            if (x > Width) throw new ArgumentException("X must be within the texture width.");
            if (y > Height) throw new ArgumentException("Y must be within the texture width.");
           
            CreateImage();
            
            image.SetPixel((uint)x, (uint)y, color.SFMLColor);
            texture = new SFML.Graphics.Texture(image);

            needsUpdate = true;
        }

        /// <summary>
        /// Sets the color of a rectangle of pixels on the texture.
        /// </summary>
        /// <param name="x">The x coordinate of the rectangle.</param>
        /// <param name="y">The y coordinate of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        public void SetRect(int x, int y, int width, int height, Color color) {
            if (x < 0) throw new ArgumentException("X must be greater than 0.");
            if (y < 0) throw new ArgumentException("Y must be greater than 0.");
            if (width < 0) throw new ArgumentException("Width must be greater than 0.");
            if (height < 0) throw new ArgumentException("Height must be greater than 0.");

            for (var xx = x; xx < x + width; xx++) {
                for (var yy = y; yy < y + height; yy++) {
                    SetPixel(xx, yy, color);
                }
            }
        }

        /// <summary>
        /// Copy pixels from one texture to another using blitting.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        public void CopyPixels(Texture from, int fromX, int fromY, int toX, int toY) {
            CreateImage();

            image.Copy(from.image, (uint)toX, (uint)toY, new SFML.Graphics.IntRect(fromX, fromY, from.Width, from.Height));
        }

        /// <summary>
        /// Save the texture to a file. The supported image formats are bmp, png, tga and jpg.
        /// </summary>
        /// <param name="path">The file path to save to. The type of image is deduced from the extension.</param>
        public void SaveToFile(string path) {
            CreateImage();

            image.SaveToFile(path);
        }

        /// <summary>
        /// Loads the image internally in the texture for image manipulation.  This is
        /// handled automatically, but it's exposed so that it can be manually controlled.
        /// </summary>
        public void CreateImage() {
            if (image == null) {
                image = texture.CopyToImage();
            }
        }

        /// <summary>
        /// Updates the texture to reflect changes made from SetPixel.
        /// </summary>
        public void Update() {
            if (needsUpdate) {
                texture.Update(image);
                needsUpdate = false;
            }
        }

        #endregion

        #region Internal

        /// <summary>
        /// Load a texture from an SFML texture.
        /// </summary>
        /// <param name="texture"></param>
        internal Texture(SFML.Graphics.Texture texture) {
            this.texture = texture;
        }

        internal SFML.Graphics.Texture SFMLTexture {
            get { return texture; }
        }

        #endregion

    }
}
