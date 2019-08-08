using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Win32;
using System;
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
        private Texture2D render;
        private Color[,] texture;

        private float wrap = 0.05f;
        private bool above = true;
        private bool reverse = false;
        
        public GameLoop()
        {
            Window.AllowAltF4 = true;
            Window.Title = "Art Project";
            Window.IsBorderless = false;
            Window.AllowUserResizing = false;

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = false
            };

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Texture2D image;
            #region Get Image
            OpenFileDialog ofd = new OpenFileDialog
            {
                FileName = "Image",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = "Images|*.png;*.bmp;*.jpg"
            };
            if (ofd.ShowDialog() == true)
                using (System.IO.FileStream stream = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Open))
                    image = Texture2D.FromStream(GraphicsDevice, stream);
            else throw new Exception("Nothing selected");

            texture = new Color[image.Width, image.Height];
            {
                var array = new Color[image.Width * image.Height];
                image.GetData(array);
                for (int i = 0; i < array.Length; i++)
                    texture[i / image.Height, i % image.Height] = array[i];
            }
            #endregion
            #region Configure window
            graphics.PreferredBackBufferWidth = image.Width;
            graphics.PreferredBackBufferHeight = image.Height;
            graphics.ApplyChanges();
            #endregion

            guiRenderer = new ImGuiRenderer(this);
            guiRenderer.RebuildFontAtlas();

            inputHandler = new Input(Keyboard.GetState(), Mouse.GetState(), new Dictionary<object, int>());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            render = new Texture2D(GraphicsDevice, texture.GetLength(0), texture.GetLength(1));
            guiRenderer.BindTexture(new Texture2D(GraphicsDevice, render.Width, render.Height));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var localarr = (Color[,])texture.Clone();

            // tex, 0.9f, false, false
            Processors.PixelSort(ref localarr, wrap, above, reverse);

            {
                var array = new Color[render.Width * render.Height];
                for (int x = 0; x < render.Width; x++)
                    for (int y = 0; y < render.Height; y++)
                        array[x * render.Height + y] = localarr[x, y];
                render.SetData(array);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.Draw(render, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            spriteBatch.End();

            guiRenderer.BeforeLayout(gameTime);
            GuiRender();
            guiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        public void GuiRender()
        {
            ImGui.Begin("Debug", ImGuiWindowFlags.MenuBar);
            if(ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Window"))
                {
                    if (ImGui.MenuItem("Exit")) { Exit(); }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            ImGui.SliderFloat("Wrap value", ref wrap, 0f, 1f);
            ImGui.Checkbox("Above", ref above);
            ImGui.Checkbox("Reverse", ref reverse);
            ImGui.End();
        }
    }
}
