using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Text;

namespace CCN_experiment;

public class CCN_Game : Game
{
	private enum State { Press_Start, Fixation, Stimulus_Presentation, Response_Window, Feedback, Final_Score };
	private enum Stimulus { None, Green, Red };
	private SpriteBatch _batch;
	private int _timer, _score, _top_score, _screen_width;
	private byte _rounds;
	private Texture2D _patch, _checkerboard;
	private SpriteFont _font;
	private State _state;
	private readonly GraphicsDeviceManager _graphics;
	private TextBox _pressStart, _scoreBoard, _helpMessage;
	private readonly Color _textColor = Color.Black, _backColor = Color.White;
	private Rectangle _cb_dest1, _cb_dest2, _fix_center, _progress, _p1, _p2, _p3;
	private bool _profit, _shuffleRG, _alternate, _rewardOnRed, _rewardOnGreen;
	private readonly HashSet<int> _redRewards = new(), _greenRewards = new();
	private readonly Random _randomGen = new();
	private Stimulus _curStimulus, _prevStimulus;
	public const byte Test_Unit = 12, Max_rounds = Test_Unit * 6;
	private readonly StringBuilder _bdr = new();
	public CCN_Game()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
	}
	public void FillRandom(HashSet<int> set)
	{
		int count = 0;
		set.Clear();
		do
		{
			if (set.Add(_randomGen.Next(Max_rounds)))
				count++;
		} while (count < Test_Unit);
	}
	public static Point SetCenter(ref Rectangle a, Point center)
	{
		a.X = center.X - (a.Width / 2);
		a.Y = center.Y - (a.Height / 2);
		return a.Location;
	}
	protected override void LoadContent()
	{
		var w = GraphicsDevice.DisplayMode.Width;
		var h = GraphicsDevice.DisplayMode.Height;
		_graphics.PreferredBackBufferWidth = w;
		_graphics.PreferredBackBufferHeight = h;
		_graphics.IsFullScreen = true;
		_graphics.ApplyChanges();
		_batch = new SpriteBatch(GraphicsDevice);
		_patch = new(GraphicsDevice, 1, 1);
		_patch.SetData(new Color[] { Color.White });
		_checkerboard = Content.Load<Texture2D>("checkerboard");
		_font = Content.Load<SpriteFont>("font");
		Rectangle screenDest = new(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
		int common_height = screenDest.Height / 16;
		Rectangle st = new(screenDest.Width / 4, common_height, screenDest.Width / 2, common_height);
		_scoreBoard = new(st, "Your score:", _font, _textColor, Alignment.Centered);
		st.Y += 2 * st.Height;
		_pressStart = new(st, "Press Enter to start experiment", _font, _textColor, Alignment.Centered);
		st.Y = screenDest.Height - st.Height + 5;
		_progress = new(0, st.Y - 25, 0, 25);
		_helpMessage = new(st, " ", _font, _textColor, Alignment.Centered);
		_score = _top_score = _timer = 0;
		_fix_center = new Rectangle(0, 0, common_height, common_height);
		Point cp = screenDest.Center;
		int sideLen = 7 * screenDest.Width / 20;
		SetCenter(ref _fix_center, cp);
		_cb_dest1 = new(0, 0, sideLen, sideLen);
		_cb_dest2 = new(0, 0, sideLen, sideLen);
		cp.X /= 2;
		SetCenter(ref _cb_dest1, cp);
		cp.X += screenDest.Width / 2;
		SetCenter(ref _cb_dest2, cp);
		_state = State.Press_Start;
		_screen_width = screenDest.Width;
		_rounds = 0;
		_p1 = new((int)(_screen_width * (1250.0f / 3750.0f)), _progress.Y, 5, 25);
		_p2 = new((int)(_screen_width * (2250.0f / 3750.0f)), _progress.Y, 5, 25);
		_p3 = new((int)(_screen_width * (3500.0f / 3750.0f)), _progress.Y, 5, 25);
	}
	protected override void Update(GameTime gameTime)
	{
		KeyboardState input = Keyboard.GetState();
		if (input.IsKeyDown(Keys.Escape))
			Exit();
		_timer += gameTime.ElapsedGameTime.Milliseconds;
		if (_state != State.Press_Start && _state != State.Final_Score)
		{
			_progress.Width = (int)(_screen_width * (_timer / 3750.0f));
		}
		switch (_state)
		{
			default:
			case State.Final_Score:
			case State.Press_Start:
				if (input.IsKeyDown(Keys.Enter))
				{
					_timer = 0;
					_rounds = 0;
					_score = 0;
					_prevStimulus = _curStimulus = Stimulus.None;
					_helpMessage.Text = $"Round 1/{Max_rounds}";
					_alternate = false;
					FillRandom(_redRewards);
					FillRandom(_greenRewards);
					_rewardOnRed = _rewardOnGreen = false;
					_bdr.Clear();
					_state = State.Fixation;
				}
				break;
			case State.Fixation:
				if (_timer >= 1250)
				{
					_shuffleRG = _randomGen.Next(2) == 0;
					_state = State.Stimulus_Presentation;
				}
				break;
			case State.Stimulus_Presentation:
				if (_timer >= 2250)
					_state = State.Response_Window;
				break;
			case State.Response_Window:
				if (_curStimulus == Stimulus.None)
				{
					if (input.IsKeyDown(Keys.Left))
						_curStimulus = _shuffleRG ? Stimulus.Green : Stimulus.Red;
					else if (input.IsKeyDown(Keys.Right))
						_curStimulus = _shuffleRG ? Stimulus.Red : Stimulus.Green;
					_rewardOnGreen = _rewardOnGreen || _greenRewards.Contains(_rounds);
					_rewardOnRed = _rewardOnRed || _redRewards.Contains(_rounds);
				}
				if (_timer >= 3500)
				{
					_state = State.Feedback;
					_bdr.Append($"Round_{_rounds}:\tR(R,G) = ");
					if (_redRewards.Contains(_rounds))
						_bdr.Append("{ ( $");
					else
						_bdr.Append("{ ( _");
					if(_rewardOnRed)
						_bdr.Append('*');
					else
						_bdr.Append(' ');
					if (_greenRewards.Contains(_rounds))
						_bdr.Append(" | $");
					else
						_bdr.Append(" | _");
					if(_rewardOnGreen)
						_bdr.Append('*');
					else
						_bdr.Append(' ');
					_bdr.Append(" ), S = ");
					if (_curStimulus != Stimulus.None)
					{
						if (_curStimulus == Stimulus.Green)
							_bdr.Append("< Red >");
						else
							_bdr.Append("<Green>");
						_alternate = _prevStimulus != Stimulus.None && _curStimulus != _prevStimulus;
						bool selectedReward;
						if (_curStimulus == Stimulus.Red)
						{
							selectedReward = _rewardOnRed;
							_rewardOnRed = false;
						}
						else
						{
							selectedReward = _rewardOnGreen;
							_rewardOnGreen = false;
						}
						_profit = selectedReward && _alternate == false;
						if (_profit)
						{
							_score += 20;
							_top_score = _top_score > _score ? _top_score : _score;
							_bdr.Append(" , P = +");
						}
						else if (selectedReward)
							_bdr.Append(" , P = -");
						else
							_bdr.Append(" , P = 0");
					}
					else
					{
						_profit = false;
						_bdr.Append("<Empty> , P = 0");
					}
					_bdr.Append($"; (alternate penalty: {_alternate} )");
					_bdr.Append(" }\n");
					_prevStimulus = _curStimulus;
					_curStimulus = Stimulus.None;
				}
				break;
			case State.Feedback:
				if (_timer >= 3750)
				{
					_rounds++;
					_timer = 0;
					if (_rounds >= Max_rounds)
					{
						_scoreBoard.Text = $"Your score: {_score}    Top score: {_top_score}";
						_state = State.Final_Score;
						_helpMessage.Text = " ";
						System.IO.File.WriteAllText($"{DateTime.Now:dd-mm-yy-hh-mm-ss}.log", _bdr.ToString());
					}
					else
					{
						_state = State.Fixation;
						_helpMessage.Text = $"Round {_rounds + 1}/{Max_rounds}";
					}
				}
				break;
		}
		base.Update(gameTime);
	}
	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(_backColor);
		_batch.Begin();
		if (_state != State.Press_Start && _state != State.Final_Score)
		{
			_batch.Draw(_patch, _progress, Color.GreenYellow);
			_batch.Draw(_patch, _p1, Color.Black);
			_batch.Draw(_patch, _p2, Color.Black);
			_batch.Draw(_patch, _p3, Color.Black);
		}
		switch (_state)
		{
			case State.Press_Start:
				_pressStart.Draw(_batch);
				break;
			case State.Stimulus_Presentation:
				if (!_shuffleRG)
				{
					_batch.Draw(_checkerboard, _cb_dest1, Color.Green);
					_batch.Draw(_checkerboard, _cb_dest2, Color.Red);
				}
				else
				{
					_batch.Draw(_checkerboard, _cb_dest1, Color.Red);
					_batch.Draw(_checkerboard, _cb_dest2, Color.Green);
				}
				goto case State.Fixation;
			case State.Response_Window:
			case State.Fixation:
				_batch.Draw(_patch, _fix_center, Color.Black);
				break;
			case State.Feedback:
				_batch.Draw(_patch, _fix_center, _profit ? Color.DarkGoldenrod : Color.LightBlue);
				break;
			case State.Final_Score:
			default:
				_pressStart.Draw(_batch);
				_scoreBoard.Draw(_batch);
				break;
		}
		_helpMessage.Draw(_batch);
		base.Draw(gameTime);
		_batch.End();
	}
}
