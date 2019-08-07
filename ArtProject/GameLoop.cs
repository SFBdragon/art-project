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
        private Texture2D texture;
        
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
            #region Get Image
            OpenFileDialog ofd = new OpenFileDialog
            {
                FileName = "Image",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = "Images|*.png;*.bmp;*.jpg"
            };
            if (ofd.ShowDialog() == true)
                using (System.IO.FileStream stream = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Open))
                    texture = Texture2D.FromStream(GraphicsDevice, stream);
            else Exit();
            #endregion
            #region Configure window
            graphics.PreferredBackBufferWidth = texture.Width;
            graphics.PreferredBackBufferHeight = texture.Height;
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
            guiRenderer.BindTexture(new Texture2D(GraphicsDevice, texture.Width, texture.Height));


        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            for (int i = 0; i < 1000000; i++)
                _ = Math.Sqrt(double.MaxValue);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(clear);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.Draw(texture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
            spriteBatch.End();

            guiRenderer.BeforeLayout(gameTime);
            GuiRender();
            guiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        Color clear = Color.Gray;
        bool window_preferences = false;
        public void GuiRender()
        {
            ImGui.Begin("Debug", ImGuiWindowFlags.MenuBar);
            if(ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Window"))
                {
                    if (ImGui.MenuItem("Preferences")) window_preferences = !window_preferences;
                    if (ImGui.MenuItem("Exit")) { Exit(); }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            ImGui.End();
            if(window_preferences)
            {
                ImGui.Begin("Window Preferences");
                System.Numerics.Vector3 clearVec = new System.Numerics.Vector3(clear.R / 255f, clear.G / 255f, clear.B / 255f);
                ImGui.ColorPicker3("Background", ref clearVec);
                clear = new Color(clearVec.X, clearVec.Y, clearVec.Z);
                ImGui.End();
            }
        }
    }
}
