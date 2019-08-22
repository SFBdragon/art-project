using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using ImGuiNET;
using ImGuiNET.XNA;
using Extended.Generic;

namespace ArtProject
{
    public class GameLoop : Game
    {
        // library managers
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // managers
        private Input inputHandler;
        private ImGuiRenderer guiRenderer;

        // texture data
        private Color[,] original = new Color[0, 0];
        private Color[,] texture = new Color[0, 0];
        private Texture2D render;

        // flags and values for processing
        private bool open = true;
        private bool save = false;

        private bool modify_brightness = false;
        private string seed = "default";
        private float billinear_lerp = .1f;
        private int billinear_iterations = 0;

        private bool reset = true;
        private bool scramble = false;
        private bool descramble = false;
        private bool pixel_sort = false;
        private float wrap = 0.5f;
        private bool above = false;
        private bool reverse = false;
        private bool colour_split = false;
        private int split = 0;
        private bool pixelate = false;
        private int pixel_size = 1;
        private bool greyscale = false;


        // constructor
        public GameLoop()
        {
            Window.Title = "Art Project";
            Window.IsBorderless = true;
            Window.AllowUserResizing = false;

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = false
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // setup user interface 
            guiRenderer = new ImGuiRenderer(this);
            guiRenderer.RebuildFontAtlas();

            // setup texture manager
            inputHandler = new Input(Keyboard.GetState(), Mouse.GetState(), new Dictionary<object, int>() {
                { "exit", (int)Keys.Escape }
            });

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            guiRenderer.BindTexture(new Texture2D(GraphicsDevice,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height));
        }

        protected override void Update(GameTime gameTime)
        {
            // update the input manager
            inputHandler.Update(Keyboard.GetState(), Mouse.GetState());
            // check inputs
            if (inputHandler.OnBindingPressed("exit")) Exit();

            // complete tasks
            else if (open)
            {
                // defocus window
                Window.IsBorderless = false;

                // get texture from filesystem ("render" is just used for temporary purposes, it gets overwritten immediately after)
                var ofd = new OpenFileDialog
                {
                    FileName = "Image",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    Filter = "Images|*.png;*.bmp;*jpeg;*jpg"
                };
                if (ofd.ShowDialog() == true)
                    using (FileStream stream = new FileStream(ofd.FileName, FileMode.Open))
                        render = Texture2D.FromStream(GraphicsDevice, stream);
                else render = new Texture2D(GraphicsDevice, 10, 10);

                // set texture maps
                original = new Color[render.Width, render.Height];
                var array = new Color[render.Width * render.Height];
                render.GetData(array);
                for (int i = 0; i < array.Length; i++)
                    original[i % render.Width, i / render.Width] = array[i];

                texture = (Color[,])original.Clone();
                open = false;

                // focus window
                Window.IsBorderless = true;
            }
            else if (save) render.SaveAsPng(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + $"/{Environment.TickCount}.png", FileMode.Create), render.Width, render.Height);
            else if (reset) texture = (Color[,])original.Clone();

            else if (scramble) Processors.TextureScramble(ref texture);
            else if (descramble) Processors.TextureDescramble(ref texture);
            else if (pixel_sort) Processors.QueueStackPixelSort(ref texture, Processors.GetSortQueue(texture, wrap, above), reverse);
            else if (pixelate) Processors.Pixelate(ref texture, pixel_size);
            else if (colour_split) Processors.ColorSplit(ref texture, split);
            else if (greyscale)
            {
                for (int x = 0; x < texture.GetLength(0); x++)
                    for (int y = 0; y < texture.GetLength(1); y++)
                    {
                        var color = texture[x, y];
                        var avrg = (byte)((color.R + color.G + color.B) / 3f);
                        texture[x, y].R = avrg;
                        texture[x, y].G = avrg;
                        texture[x, y].B = avrg;
                    }
            }

            // processing texture -> render texture
            {
                var array = new Color[render.Width * render.Height];
                for (int x = 0; x < render.Width; x++)
                    for (int y = 0; y < render.Height; y++)
                        array[x + y * render.Width] = texture[x, y];
                render.SetData(array);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
            spriteBatch.Draw(render, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
                Math.Min(1f, Math.Min(
                    (float)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / render.Width,
                    (float)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / render.Height)),
                SpriteEffects.None, 0f);
            spriteBatch.End();

            guiRenderer.BeforeLayout(gameTime);
            GuiRender();
            guiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        public void GuiRender()
        {
            if (!inputHandler.keyboardState.IsKeyDown(Keys.D))
            {
                ImGui.Begin("Processors");

                ImGui.Text("File:");
                ImGui.Indent();
                open = ImGui.Button("Open");
                save = ImGui.Button("Save");
                if (ImGui.Button("Exit [Esc]")) Exit();
                ImGui.Unindent();
                ImGui.Separator();

                ImGui.Text("Processing:");
                ImGui.Indent();
                reset = ImGui.Button("Reset");
                scramble = ImGui.Button("Scramble");
                descramble = ImGui.Button("Descramble");

                pixel_sort = ImGui.Button("Pixel Sort");
                ImGui.Indent();
                ImGui.SliderFloat("Wrap", ref wrap, 0f, 1f);
                ImGui.Checkbox("Above", ref above);
                ImGui.Checkbox("Reverse", ref reverse);
                ImGui.Unindent();

                pixelate = ImGui.Button("Pixelate");
                ImGui.Indent();
                ImGui.InputInt("Pixel size", ref pixel_size);
                ImGui.Unindent();
                pixel_size = ExtendedMath.Clamp(pixel_size, 1, texture.GetLength(1) - 1);

                colour_split = ImGui.Button("Colour Split");
                ImGui.Indent();
                ImGui.InputInt("Split", ref split);
                ImGui.Unindent();

                greyscale = ImGui.Button("Greyscale");

                ImGui.End();
            }
        }
    }
}

//else if (modify_brightness)
//{
//    // cache dimensions
//    var width = texture.GetLength(0);
//    var height = texture.GetLength(1);

//    // generate an unfiltered noisemap
//    var map = Procedural.Perlin2D.CompileOctaves(width, height,
//        Procedural.GenerateNoiseMap(.5f, (width + 63) / 64, (height + 63) / 64, seed.GetHashCode())
//        , Procedural.GenerateNoiseMap(.25f, (width + 15) / 16, (height + 15) / 16, seed.GetHashCode() + 42)
//        , Procedural.GenerateNoiseMap(.125f, (width + 3) / 4, (height + 3) / 4, seed.GetHashCode() + "42".GetHashCode())
//        , Procedural.GenerateNoiseMap(.75f, width, height, seed.GetHashCode() + 42.0.GetHashCode())
//        );

//    // filter billinearly
//    for (int i = 0; i < billinear_iterations; i++)
//        map = Procedural.Perlin2D.BillinearFilter(map, billinear_lerp);

//    // overwrite 'value' parameters with generated
//    for (int x = 0; x < width; x++)
//        for (int y = 0; y < height; y++)
//        {
//            var t = texture[x, y].ToHSV();
//            t.V = map[x, y];
//            texture[x, y] = t.ToRGB();
//        }
//}


//modify_brightness = ImGui.Button("Modify brightness map");
//ImGui.InputText("Seed", ref seed, 15);
//ImGui.InputInt("Billinear Iterations", ref billinear_iterations);
//ImGui.InputFloat("Billinear Lerp", ref billinear_lerp);
//ImGui.Separator();