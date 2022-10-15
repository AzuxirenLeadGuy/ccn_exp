using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace CCN_experiment
{
	/// <summary>The paramter for alignment of a text in a TextBox</summary>
	public enum Alignment : byte
	{
		/// <summary>Aligns the text to the left of textbox</summary>
		Left,
		/// <summary>Aligns the text to the center of TextBox</summary>
		Centered,
		/// <summary>Aligns the text to the right of TextBox</summary>
		Right
	}
	/// <summary>Maintains the drawing of text in the given Rectangle</summary>
	public struct TextBox
	{
		/// <summary>
		/// The Construtctor for Textbox
		/// </summary>
		/// <param name="bd">The rectangle where the text is displayed</param>
		/// <param name="txt">The text to display</param>
		/// <param name="fnt">The font used</param>
		public TextBox(Rectangle bd, string txt, SpriteFont fnt)
		{
			_bounds = bd;
			_text = txt;
			_font = fnt;
			LayerDepth = 0;
			TextColor = Color.Black;
			_pos = default;
			_scale = default;
			_alignment = Alignment.Left;
			FitText();
		}
		/// <summary>
		/// The Construtctor for Textbox
		/// </summary>
		/// <param name="bd">The rectangle where the text is displayed</param>
		/// <param name="txt">The text to display</param>
		/// <param name="fnt">The font used</param>
		/// <param name="c">The color of the text</param>
		/// <param name="align">The alignment of the text</param>
		public TextBox(Rectangle bd, string txt, SpriteFont fnt, Color c, Alignment align = Alignment.Centered)
		{
			_bounds = bd;
			_text = txt;
			_font = fnt;
			LayerDepth = 0;
			TextColor = c;
			_pos = default;
			_scale = default;
			_alignment = align;
			FitText();
		}
		/// <summary>
		/// This is the Destination rectangle where the text is to be drawn 
		/// </summary>
		/// <value></value>
		public Rectangle Bounds
		{
			get => _bounds;
			set
			{
				_bounds = value;
				FitText();
			}
		}
		/// <summary>
		/// This is the text to display
		/// </summary>
		/// <value></value>
		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				FitText();
			}
		}
		/// <summary>
		/// This is the font used
		/// </summary>
		/// <value></value>
		public SpriteFont Font
		{
			get => _font;
			set
			{
				_font = value;
				FitText();
			}
		}
		/// <summary>
		/// The Alignment of the text within the box
		/// </summary>
		/// <value></value>
		public Alignment Alignment
		{
			get => _alignment;
			set
			{
				_alignment = value;
				FitText();
			}
		}
		/// <summary>The LayerDepth of the text</summary>
		public float LayerDepth;
		/// <summary>The color of the text</summary>
		public Color TextColor;
		private SpriteFont _font;
		private Rectangle _bounds;
		private string _text;
		private Vector2 _pos;
		private float _scale;
		private Alignment _alignment;
		internal void FitText()
		{
			FitText(_bounds, _font, _text, out _scale, out _pos);
			var fontlength = _font.MeasureString(_text) * _scale;
			_pos.Y += (_bounds.Height - fontlength.Y) / 2;
			if (_alignment != Alignment.Left)
			{
				var diff = _bounds.Width - fontlength.X;
				if (_alignment == Alignment.Right) _pos.X += diff;
				else _pos.X += diff / 2;
			}
		}
		/// <summary>
		/// Draws the string (Color of text is white, by default)
		/// </summary>
		/// <param name="batch">The spritebatch for the game</param>
		public void Draw(SpriteBatch batch)
		{
			batch.DrawString(_font, _text, _pos, TextColor, 0, Vector2.Zero, _scale, SpriteEffects.None, LayerDepth);
		}
		/// <summary>
		/// Draws the string
		/// </summary>
		/// <param name="batch">The spritebatch for the game</param>
		/// <param name="rotation">The value of rotation</param>
		/// <param name="origin">The point to rotate the text about</param>
		public void Draw(SpriteBatch batch, float rotation, Vector2 origin)
		{
			batch.DrawString(_font, _text, _pos, TextColor, rotation, origin, _scale, SpriteEffects.None, LayerDepth);
		}
		/// <summary>
		/// Draws the string
		/// </summary>
		/// <param name="batch">The spritebatch for the game</param>
		/// <param name="rotation">The value of rotation</param>
		/// <param name="origin">The point to rotate the text about</param>
		/// <param name="effects">The SpriteEffects to use</param>
		public void Draw(SpriteBatch batch, float rotation, Vector2 origin, SpriteEffects effects)
		{
			batch.DrawString(_font, _text, _pos, TextColor, rotation, origin, _scale, effects, LayerDepth);
		}
		/// <summary>Initalizes the Vector2 and Float value of poition and 
		/// scale to fit the text in the rectangle</summary>
		/// <param name="dest">The rectangle to fit the string</param>
		/// <param name="font">The Spritefont object</param>
		/// <param name="message">The content of the string</param>
		/// <param name="scale">The scale to fit</param>
		/// <param name="position">The position of fitting</param>
		public static void FitText(Rectangle dest, SpriteFont font, string message, 
			out float scale, out Vector2 position)
		{
			Vector2 size = font.MeasureString(message);
			float xScale = dest.Width / size.X;
			float yScale = dest.Height / size.Y;
			// Taking the smaller scaling value will result in the text always fitting in the boundaires.
			scale = Math.Min(xScale, yScale);
			// Figure out the location to absolutely-center it in the boundaries rectangle.
			position = dest.Location.ToVector2();
		}
	}
}