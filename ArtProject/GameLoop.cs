using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Win32;
using System;
using System.IO;
using ImGuiNET;
using ImGuiNET.XNA;
using Extended.Generic;
using Extended.Generic.Noise;
using Newtonsoft.Json;

namespace ArtProject
{
    public class GameLoop : Game
    {
        // library managers
        readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private ImGuiRenderer guiRenderer;

        // texture data
        private Color[,] original = new Color[0, 0];
        private Color[,] texture = new Color[0, 0];
        private Texture2D render;

        // flags and values for processing
        private bool open = true;
        private bool save = false;

        private bool reset = true;

        private int seed;
        private bool diarrhea_christmas_lights = false;
        private bool open_profile = false;
        private bool save_profile = false;
        private bool generate_noise = false;
        private NoiseProfile noise = new NoiseProfile()
        {
            Amplitude = 1,
            FilterLerp = 0.5f,
            FilterPasses = 0,
            Zero = Color.Black,
            One = Color.White,
            Persistance = 0.5f,
            Octaves = 6,
            OctaveMulti = 2,
            WidthMulti = 5,
            HeightMulti = 3,
        };

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
        private bool colour_shift_down = false;
        private byte colour_shift_iterate = 0;
        private bool filter = false;
        private float lerp = 0.5f;

        // constructor
        public GameLoop()
        {
            Window.Title = "Art Project";
#if DEBUG
            Window.IsBorderless = true;
#else
            Window.IsBorderless = false;
#endif
            Window.AllowUserResizing = false;

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
#if DEBUG
                IsFullScreen = false,
#else
                IsFullScreen = true,
#endif
                SynchronizeWithVerticalRetrace = false
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // setup user interface 
            guiRenderer = new ImGuiRenderer(this);
            guiRenderer.RebuildFontAtlas();

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
            // check to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

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
                    Filter = "Images|*.png;*.bmp;*jpeg;*jpg",
                    RestoreDirectory = true
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
                colour_shift_iterate = 0;

                // focus window
                Window.IsBorderless = true;
            }
            else if (save) render.SaveAsPng(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + $"/{Environment.TickCount}.png", FileMode.Create), render.Width, render.Height);

            else if (reset)
            {
                texture = (Color[,])original.Clone();
                colour_shift_iterate = 0;
            }

            else if (generate_noise)
            {
                var _noise = D2.GenerateValueNoise(noise.Amplitude, 0f, noise.Persistance, (byte)noise.Octaves, (int)noise.WidthMulti, (int)noise.HeightMulti,
                    noise.FilterLerp, (int)noise.FilterPasses, InterpType.Linear, noise.OctaveMulti, seed);
                original = new Color[_noise.GetLength(0), _noise.GetLength(1)];
                for (int x = 0; x < _noise.GetLength(0); x++)
                    for (int y = 0; y < _noise.GetLength(1); y++)
                        original[x, y] = Color.Lerp(noise.Zero, noise.One, _noise[x,y]);
                texture = (Color[,])original.Clone();
                render = new Texture2D(GraphicsDevice, _noise.GetLength(0), _noise.GetLength(1));
                colour_shift_iterate = 0;
            }
            else if (diarrhea_christmas_lights)
            {
                var _noise = D2.GenerateValueNoise(noise.Amplitude, 0f, noise.Persistance, (byte)noise.Octaves, (int)noise.WidthMulti, (int)noise.HeightMulti,
                    noise.FilterLerp, (int)noise.FilterPasses, InterpType.Linear, noise.OctaveMulti, seed);
                original = new Color[_noise.GetLength(0), _noise.GetLength(1)];
                for (int x = 0; x < _noise.GetLength(0); x++)
                    for (int y = 0; y < _noise.GetLength(1); y++)
                        original[x, y] = new ColorHSV(_noise[x, y], _noise[x, y], _noise[x, y]).ToRGB();
                texture = (Color[,])original.Clone();
                render = new Texture2D(GraphicsDevice, _noise.GetLength(0), _noise.GetLength(1));
            }
            else if (open_profile)
            {
                // defocus window
                Window.IsBorderless = false;

                var ofd = new OpenFileDialog
                {
                    FileName = "noise",
                    RestoreDirectory = true,
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    Filter = "Json files|*.json",
                };
                if (ofd.ShowDialog() == true)
                    noise = JsonConvert.DeserializeObject<NoiseProfile>(File.ReadAllText(ofd.FileName));

                // focus window
                Window.IsBorderless = true;
            }
            else if (save_profile)
            {
                // defocus homework
                Window.IsBorderless = false;

                var sfd = new SaveFileDialog
                {
                    FileName = "noise",
                    RestoreDirectory = true,
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    DefaultExt = ".json",
                    Filter = "JSON Files|*.json"
                };
                if (sfd.ShowDialog() == true)
                    File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(noise));

                // focus window
                Window.IsBorderless = true;
            }
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
            else if (colour_shift_down)
            {
                colour_shift_iterate++;
                Processors.ColorFloor(ref texture, colour_shift_iterate);
            }
            // TODO: Filter call implement

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
            if (!Keyboard.GetState().IsKeyDown(Keys.U))
            {
                ImGui.Begin("Processors");
                if (ImGui.Button("Exit [Esc]")) Exit();

                ImGui.Text("File:");
                ImGui.Indent();
                open = ImGui.Button("Open");
                save = ImGui.Button("Save");
                reset = ImGui.Button("Reset");
                ImGui.Unindent();
                ImGui.Separator();

                if (ImGui.CollapsingHeader("Generate:"))
                {
                    diarrhea_christmas_lights = ImGui.Button("Diarrhea Christmas Lights");
                    generate_noise = ImGui.Button("Generate Noise");
                    open_profile = ImGui.Button("Open Profile");
                    save_profile = ImGui.Button("Save Profile");
                    ImGui.Indent();
                    ImGui.SliderInt("Seed", ref seed, 0, int.MaxValue);
                    var _vec3 = new System.Numerics.Vector3(noise.Zero.R / 255f, noise.Zero.G / 255f, noise.Zero.B / 255f);
                    ImGui.ColorEdit3("First Colour", ref _vec3, ImGuiColorEditFlags.InputRGB);
                    noise.Zero = new Color(_vec3.X, _vec3.Y, _vec3.Z);
                    var __vec3 = new System.Numerics.Vector3(noise.One.R / 255f, noise.One.G / 255f, noise.One.B / 255f);
                    ImGui.ColorEdit3("Last Colour", ref __vec3, ImGuiColorEditFlags.InputRGB);
                    noise.One = new Color(__vec3.X, __vec3.Y, __vec3.Z);

                    ImGui.SliderFloat("Amplitude", ref noise.Amplitude, 0f, 1f);
                    ImGui.SliderFloat("Persistence", ref noise.Persistance, 0f, 1f);
                    ImGui.SliderFloat("Bilinear Lerp", ref noise.FilterLerp, 0f, 1f);
                    int temp = (int)noise.FilterPasses;
                    ImGui.InputInt("Bilinear Passes", ref temp);
                    noise.FilterPasses = temp < 0 ? 0 : (uint)temp;

                    ImGui.Text($"Width = 2^octaves*width*multi = {Math.Pow(2, (noise.Octaves - 1)) * noise.WidthMulti * noise.OctaveMulti}");
                    ImGui.Text($"Width = 2^octaves*height*multi = {Math.Pow(2, (noise.Octaves - 1)) * noise.HeightMulti * noise.OctaveMulti}");

                    temp = noise.Octaves;
                    ImGui.InputInt("Octave Count", ref temp);
                    noise.Octaves = temp < 1 ? (byte)1 : (byte)temp;
                    temp = (int)noise.OctaveMulti;
                    ImGui.InputInt("Octave interpolation stretch", ref temp);
                    noise.OctaveMulti = temp < 1 ? 1 : (uint)temp;
                    temp = (int)noise.WidthMulti;
                    ImGui.InputInt("Width", ref temp);
                    noise.WidthMulti = temp < 1 ? 1 : (uint)temp;
                    temp = (int)noise.HeightMulti;
                    ImGui.InputInt("Height", ref temp);
                    noise.HeightMulti = temp < 1 ? 1 : (uint)temp;

                    ImGui.Unindent();
                }
                ImGui.Separator();

                ImGui.Text("Process:");
                ImGui.Indent();
                scramble = ImGui.Button("Scramble");
                descramble = ImGui.Button("Descramble");

                pixel_sort = ImGui.Button("Pixel Sort");
                ImGui.Indent();
                ImGui.SliderFloat("Wrap", ref wrap, 0f, 1f);
                ImGui.Checkbox("Above", ref above);
                ImGui.Checkbox("Reverse", ref reverse);
                ImGui.Unindent();

                // TODO: Filter ui implement

                pixelate = ImGui.Button("Pixelate");
                ImGui.Indent();
                ImGui.InputInt("Pixel size", ref pixel_size);
                ImGui.Unindent();
                pixel_size = MathExtended.Clamp(pixel_size, 1, texture.GetLength(1) - 1);

                colour_split = ImGui.Button("Colour Split");
                ImGui.Indent();
                ImGui.InputInt("Split", ref split);
                ImGui.Unindent();

                greyscale = ImGui.Button("Greyscale");

                colour_shift_down = ImGui.Button("Floor colour by 1-bit");
                ImGui.Indent();
                ImGui.Text($"Colour quality: {8 - colour_shift_iterate}-bits");
                ImGui.Unindent();

                ImGui.End();
            }
        }

        [Serializable]
        struct NoiseProfile
        {
            public float Amplitude;
            public float Persistance;
            public byte Octaves;
            public uint WidthMulti;
            public uint HeightMulti;
            public uint OctaveMulti;
            public float FilterLerp;
            public uint FilterPasses;

            public Color Zero;
            public Color One;
        }
    }
}
