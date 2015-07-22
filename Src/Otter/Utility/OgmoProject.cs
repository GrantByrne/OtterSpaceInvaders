using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;

namespace Otter {
    /// <summary>
    /// Class used for importing OgmoProject files quickly, and loading levels created in Ogmo Editor
    /// (http://ogmoeditor.com)  Currently OgmoProjects must export in XML Co-ords for Tiles and Entities,
    /// and Bitstring for Grids.
    /// </summary>
    public class OgmoProject {

        #region Private Fields

        Dictionary<string, int> ColliderTags = new Dictionary<string, int>();

        #endregion

        #region Public Fields

        /// <summary>
        /// Determines if grid layers will render in the game.  Only applies at loading time.
        /// </summary>
        public bool DisplayGrids = true;

        /// <summary>
        /// The default image path to search for tilemaps in.
        /// </summary>
        public string ImagePath;

        /// <summary>
        /// Determines if loaded levels will use camera bounds in the Scene.
        /// </summary>
        public bool UseCameraBounds = true;

        /// <summary>
        /// Determines if tilemaps are located in an Atlas.
        /// </summary>
        public bool UseAtlas;

        /// <summary>
        /// The default background color of the Ogmo Project.
        /// </summary>
        public Color BackgroundColor;

        /// <summary>
        /// The default background grid color of the Ogmo Project.
        /// </summary>
        public Color GridColor;

        /// <summary>
        /// The known layers loaded from the Ogmo Editor oep file.
        /// </summary>
        public Dictionary<string, OgmoLayer> Layers = new Dictionary<string, OgmoLayer>();

        /// <summary>
        /// Mapping the tile layers to file paths.
        /// </summary>
        public Dictionary<string, string> TileMaps = new Dictionary<string, string>();

        /// <summary>
        /// The entities stored to create tilemaps and grids.  Cleared every time LoadLevel is called.
        /// </summary>
        public Dictionary<string, Entity> Entities = new Dictionary<string, Entity>();

        /// <summary>
        /// The drawing layer to place the first loaded tile map on.
        /// </summary>
        public int BaseTileDepth;

        /// <summary>
        /// Determines the drawing layers for each subsequently loaded tile map.  For example, the first
        /// tilemap will be at Layer 0, the second at Layer 100, the third at Layer 200, etc.
        /// </summary>
        public int TileDepthIncrement = 100;

        #endregion

        #region Constructors

        /// <summary>
        /// Create an OgmoProject from a source .oep file.
        /// </summary>
        /// <param name="source">The path to the .oep file.</param>
        /// <param name="imagePath">The default image path to use for loading tilemaps.</param>
        public OgmoProject(string source, string imagePath = "") {
            if (!File.Exists(source)) throw new ArgumentException("Ogmo project file could not be found.");

            if (imagePath == "") {
                UseAtlas = true;
            }
            ImagePath = imagePath;

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(source);

            BackgroundColor = new Color(xmlDoc["project"]["BackgroundColor"]);
            GridColor = new Color(xmlDoc["project"]["GridColor"]);

            var xmlLayers = xmlDoc.GetElementsByTagName("LayerDefinition");
            foreach (XmlElement x in xmlLayers) {
                var layer = new OgmoLayer(x);
                Layers.Add(layer.Name, layer);
            }

            //I dont know if I need to do this
            var xmlEntities = xmlDoc.GetElementsByTagName("EntityDefinition");
            foreach (XmlElement x in xmlEntities) {

            }

            var xmlTilesets = xmlDoc.GetElementsByTagName("Tileset");
            foreach (XmlElement x in xmlTilesets) {
                TileMaps.Add(x["Name"].InnerText, x["FilePath"].InnerText);
            }
        }

        #endregion

        #region Private Methods

        void CreateEntity(XmlElement e, Scene scene) {
            Type entityType = Util.GetTypeFromAllAssemblies(e.Name);

            object[] arguments = new object[2];
            arguments[0] = scene;
            arguments[1] = e.Attributes;

            if (entityType != null) {
                MethodInfo method = entityType.GetMethod("CreateFromXML", BindingFlags.Static | BindingFlags.Public);
                if (method != null) {
                    method.Invoke(null, arguments);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load data into a scene from a source .oel file.
        /// </summary>
        /// <param name="source">The oel to load.</param>
        /// <param name="scene">The scene to load into.</param>
        public void LoadLevel(string source, Scene scene) {
            Entities.Clear();

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(source);

            var xmlLevel = xmlDoc["level"];

            scene.Width = int.Parse(xmlDoc["level"].Attributes["width"].Value);
            scene.Height = int.Parse(xmlDoc["level"].Attributes["height"].Value);

            int i = 0;

            foreach (var layer in Layers.Values) {

                if (layer.Type == "GridLayerDefinition") {
                    var Entity = new Entity();

                    var grid = new GridCollider(scene.Width, scene.Height, layer.GridWidth, layer.GridHeight);

                    grid.LoadString(xmlLevel[layer.Name].InnerText);
                    if (ColliderTags.ContainsKey(layer.Name)) {
                        grid.AddTag(ColliderTags[layer.Name]);
                    }

                    if (DisplayGrids) {
                        var tilemap = new Tilemap(scene.Width, scene.Height, layer.GridWidth, layer.GridHeight);
                        tilemap.LoadString(xmlLevel[layer.Name].InnerText, layer.Color);
                        Entity.AddGraphic(tilemap);
                    }

                    Entity.AddCollider(grid);

                    scene.Add(Entity);
                    Entities.Add(layer.Name, Entity);
                }
                if (layer.Type == "TileLayerDefinition") {
                    var Entity = new Entity();

                    var xmlTiles = xmlLevel[layer.Name];

                    var tileset = xmlTiles.Attributes["tileset"].Value;

                    var tilemap = new Tilemap(ImagePath + TileMaps[tileset], scene.Width, scene.Height, layer.GridWidth, layer.GridHeight);

                    foreach (XmlElement t in xmlTiles) {
                        tilemap.SetTile(t);
                    }

                    tilemap.Update();

                    Entity.AddGraphic(tilemap);

                    Entity.Layer = BaseTileDepth - i * TileDepthIncrement;
                    i++;

                    scene.Add(Entity);
                    Entities.Add(layer.Name, Entity);
                }
                if (layer.Type == "EntityLayerDefinition") {
                    var xmlEntities = xmlLevel[layer.Name];

                    foreach (XmlElement e in xmlEntities) {
                        CreateEntity(e, scene);
                    }
                }

            }

            if (UseCameraBounds) {
                scene.CameraBounds = new Rectangle(0, 0, scene.Width, scene.Height);
                scene.UseCameraBounds = true;
            }
        }

        /// <summary>
        /// Register a collision tag on a grid layer loaded from the oel file.
        /// </summary>
        /// <param name="tag">The tag to use.</param>
        /// <param name="layerName">The layer name that should use the tag.</param>
        public void RegisterTag(int tag, string layerName) {
            ColliderTags.Add(layerName, tag);
        }

        /// <summary>
        /// Get the Entity that was created for a specific Ogmo layer.
        /// </summary>
        /// <param name="layerName">The name of the layer to find.</param>
        /// <returns>The Entity created for that layer.</returns>
        public Entity GetEntityFromLayerName(string layerName) {
            return Entities[layerName];
        }

        /// <summary>
        /// Get a list of all the known layer names from the .oep file.
        /// </summary>
        /// <returns></returns>
        public List<string> GetLayerNames() {
            var s = new List<string>();

            foreach (var l in Layers) {
                s.Add(l.Key);
            }

            return s;
        }

        #endregion
        
    }

    /// <summary>
    /// Class representing a layer loaded from Ogmo.
    /// </summary>
    public class OgmoLayer {

        #region Public Fields

        /// <summary>
        /// The name of the layer.
        /// </summary>
        public string Name;

        /// <summary>
        /// The export mode of the layer from Ogmo Editor.
        /// </summary>
        public string ExportMode;

        /// <summary>
        /// The type of the layer from Ogmo Editor.
        /// </summary>
        public string Type;

        /// <summary>
        /// The width of each grid cell.
        /// </summary>
        public int GridWidth;

        /// <summary>
        /// The height of each grid cell.
        /// </summary>
        public int GridHeight;

        /// <summary>
        /// The horizontal parallax of the layer.
        /// </summary>
        public float ScrollX = 1;

        /// <summary>
        /// The vertical parallax of the layer.
        /// </summary>
        public float ScrollY = 1;

        /// <summary>
        /// The color of layer from Ogmo Editor.
        /// </summary>
        public Color Color;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new OgmoLayer.
        /// </summary>
        /// <param name="name">The name of the layer.</param>
        /// <param name="type">The type of the layer.</param>
        public OgmoLayer(string name, string type) {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Create a new OgmoLayer by parsing an XmlElement.
        /// </summary>
        /// <param name="xml">An XmlElement from an Ogmo Editor project file.</param>
        public OgmoLayer(XmlElement xml) {
            Name = xml["Name"].InnerText;
            Type = xml.Attributes["xsi:type"].Value;

            GridWidth = int.Parse(xml["Grid"]["Width"].InnerText);
            GridHeight = int.Parse(xml["Grid"]["Height"].InnerText);

            ScrollX = int.Parse(xml["ScrollFactor"]["X"].InnerText);
            ScrollY = int.Parse(xml["ScrollFactor"]["Y"].InnerText);

            if (Type == "GridLayerDefinition") {
                Color = new Color(xml["Color"]);
            }
        }

        #endregion

    }
}
