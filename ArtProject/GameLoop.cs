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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Input inputHandler;
        private ImGuiRenderer guiRenderer;

        private Color[,] texture = new Color[0, 0];
        private Color[,] current = new Color[0, 0];
        private Texture2D render;

        private bool process = false;
        private bool iterate = false;
        private int y = 0;
        private Queue<Point[]> queue = new Queue<Point[]>();
        private bool save = false;
        private bool open = true;

        private float wrap = 0.5f;
        private int split = 0;
        private bool above = false;
        private bool reverse = false;

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
            guiRenderer = new ImGuiRenderer(this);
            guiRenderer.RebuildFontAtlas();

            inputHandler = new Input(Keyboard.GetState(), Mouse.GetState(), new Dictionary<object, int>() {
                { "save", (int)Keys.S },
                { "process", (int)Keys.P },
                { "iterate", (int)Keys.I },
                { "above", (int)Keys.A },
                { "reverse", (int)Keys.R },
                { "open", (int)Keys.O },
                { "splitup", (int)Keys.Up },
                { "splitdown", (int)Keys.Down },
                { "wrapup", (int)Keys.Right },
                { "wrapdown", (int)Keys.Left},
            });

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            guiRenderer.BindTexture(new Texture2D(GraphicsDevice,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height));

            render = new Texture2D(GraphicsDevice, 1, 1);
        }

        protected override void Update(GameTime gameTime)
        {
            inputHandler.Update(Keyboard.GetState(), Mouse.GetState());

            if (inputHandler.keyboardState.IsKeyDown(Keys.Escape)) Exit();
            if (inputHandler.OnBindingPressed("process")) process = true;
            if (inputHandler.OnBindingPressed("iterate")) iterate = true;
            if (inputHandler.OnBindingPressed("save")) save = true;
            if (inputHandler.OnBindingPressed("open")) open = true;
            if (inputHandler.OnBindingPressed("above")) above = !above;
            if (inputHandler.OnBindingPressed("reverse")) reverse = !reverse;
            if (inputHandler.OnBindingPressed("wrapup")) wrap = Math.Min(wrap + 0.05f, 1f);
            if (inputHandler.OnBindingPressed("wrapdown")) wrap = Math.Max(wrap - 0.05f, 0f);
            if (inputHandler.OnBindingPressed("splitup")) split = Math.Min(split + 1, 5);
            if (inputHandler.OnBindingPressed("splitdown")) split = Math.Max(split - 1, -5);


            if (process)
            {
                current = (Color[,])texture.Clone();
                Processors.QueueStackPixelSort(ref current, Processors.GetSortQueue(current, wrap, above), reverse);
                Processors.ColorSplit(ref current, split);

                var width = current.GetLength(0);
                var height = current.GetLength(1);
                var map = Procedural.Perlin2D.CompileOctaves(width, height,
                    //Procedural.GenerateNoiseMap(0.5f, width / 16, height / 16, Environment.TickCount),
                    //Procedural.GenerateNoiseMap(0.25f, width / 8, height / 8, Environment.TickCount),
                    Procedural.GenerateNoiseMap(1f, width / 4, height / 4, Environment.TickCount));
                ////map = Procedural.Perlin2D.BillinearFilter(map, 0.1f);
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        var hsv = current[x, y].ToHSV();
                        // TODO: text V
                        hsv.V += (map[x, y] - 0.5f)/4;
                        current[x, y] = new Color(map[x, y], map[x, y], map[x, y]); // hsv.ToRGB();
                    }
            }
            else if (iterate)
            {
                current = (Color[,])texture.Clone();
                queue = Processors.GetSortQueue(current, wrap, above);
            }
            else if (save)
            {
                render.SaveAsPng(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + $"/{Environment.TickCount}.png", FileMode.Create), render.Width, render.Height);
            }
            else if (open)
            {
                Window.IsBorderless = false;

                // get texture
                OpenFileDialog ofd = new OpenFileDialog
                {
                    FileName = "Image",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    Filter = "Images|*.png;*.bmp;*jpeg;*jpg"
                };
                if (ofd.ShowDialog() == true)
                    using (FileStream stream = new FileStream(ofd.FileName, FileMode.Open))
                        render = Texture2D.FromStream(GraphicsDevice, stream);
                else render = new Texture2D(GraphicsDevice, 10, 10);

                // init maps
                texture = new Color[render.Width, render.Height];
                var array = new Color[render.Width * render.Height];
                render.GetData(array);
                for (int i = 0; i < array.Length; i++)
                    texture[i / render.Height, i % render.Height] = array[i];

                current = (Color[,])texture.Clone();
                open = false;

                Window.IsBorderless = true;
            }

            if (queue.Count > 0)
            {
                Processors.ArrayPixelSort(ref current, queue.Dequeue(), y, reverse);
                y++;
            }
            else y = 0;

            // current -> render
            {
                var array = new Color[render.Width * render.Height];
                for (int x = 0; x < render.Width; x++)
                    for (int y = 0; y < render.Height; y++)
                        array[x * render.Height + y] = current[x, y];
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
                ImGui.Begin("Debug", ImGuiWindowFlags.MenuBar);

                ImGui.SliderFloat("Wrap value", ref wrap, 0f, 1f);
                ImGui.SliderInt("Split", ref split, -5, 5);
                ImGui.Checkbox("Above [A]", ref above);
                ImGui.Checkbox("Reverse [R]", ref reverse);

                ImGui.Separator();

                process = ImGui.Button("Process [P]");
                iterate = ImGui.Button("Continuous Process [I]");
                open = ImGui.Button("Open [O]");
                save = ImGui.Button("Save [S]");

                ImGui.End();
            }
        }
    }
}
