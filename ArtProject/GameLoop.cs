using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

        public readonly int screenWidth;
        public readonly int screenHeight;
        public const float scale = 2f;

        private ImGuiRenderer guiRenderer;
        private Texture2D drawable;
        
        public GameLoop()
        {
            screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            Window.AllowAltF4 = true;
            Window.Title = "Art Project";
            Window.IsBorderless = true;
            Window.AllowUserResizing = false;

            IsMouseVisible = true;
            IsFixedTimeStep = false;

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = screenWidth,
                PreferredBackBufferHeight = screenHeight,
                IsFullScreen = true,
                SynchronizeWithVerticalRetrace = true
            };
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            guiRenderer = new ImGuiRenderer(this);
            guiRenderer.RebuildFontAtlas();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            guiRenderer.BindTexture(new Texture2D(GraphicsDevice, screenWidth, screenHeight));

            drawable = new Texture2D(GraphicsDevice, (int)(screenWidth / scale), (int)(screenHeight / scale));

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();



            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(clear);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.Draw(drawable, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
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
                //if(ImGui.BeginMenu(""))
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

    public static class Helpers
    {
        public static Stack<Vector2> IterateAngles(Vector2[] lsAngleLength, float iterateInRadians)
        {
            for (int i = 0; i < lsAngleLength.Length; i++)
                lsAngleLength[i].X += iterateInRadians;
            var returns = new Stack<Vector2>();
            foreach(Vector2 i in lsAngleLength)
                returns.Push(new Vector2((float)Math.Cos(i.X) * i.Y, (float)Math.Sin(i.X) * i.Y));
            return returns;
        }
        public static Color[,] PixelSort(Color[,] texture, byte valueThreshhold, bool aboveThreshhold, bool xAxis)
        {
            return;
        }
    }
}
