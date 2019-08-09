using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using ImGuiNET;
using ImGuiNET.XNA;

namespace ArtProject
{
    public class GameLoop : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Input inputHandler;
        private ImGuiRenderer guiRenderer;

        private Color[,] texture;
        private Color[,] current;
        private Texture2D render;

        private bool process = false;
        private bool iterate = false;
        private int y = 0;
        private Queue<Point[]> queue = new Queue<Point[]>();
        private bool save = false;
        private bool open = true;

        private float wrap = 0.5f;
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
            texture = new Color[0, 0];
            current = new Color[0, 0];

            guiRenderer = new ImGuiRenderer(this);
            guiRenderer.RebuildFontAtlas();

            inputHandler = new Input(Keyboard.GetState(), Mouse.GetState(), new Dictionary<object, int>() {
                { "save", (int)Keys.S },
                { "process", (int)Keys.P },
                { "iterate", (int)Keys.I },
                { "above", (int)Keys.A },
                { "reverse", (int)Keys.R },
                { "open", (int)Keys.O },
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

            if (process)
            {
                current = (Color[,])texture.Clone();
                Processors.QueueStackPixelSort(ref current, Processors.GetSortQueue(current, wrap, above), reverse);
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
            else if(open)
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
                else throw new Exception("Nothing selected");

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
