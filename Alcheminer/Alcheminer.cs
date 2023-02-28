using Alcheminer.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alcheminer
{
	public class Alcheminer : Microsoft.Xna.Framework.Game
	{
		Texture2D ballTex;
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		public Alcheminer()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		// this will load up and initialise things that donst involve assets or content
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			base.Initialize();
		}

		// loads content and assets such as music, models, etc
		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here

			ballTex = Content.Load<Texture2D>("ball");
		}

		// runs every frame, made for Logic
		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		// runs every frame, made for graphics
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add your drawing code here

			_spriteBatch.Begin();
			_spriteBatch.Draw(ballTex, new Vector2(0, 0), Color.White);
			_spriteBatch.End();

			base.Draw(gameTime);
		}

		protected override void EndRun()
		{
			Engine.Services.IDService.ClearIDs();

			base.EndRun();
		}
	}
}