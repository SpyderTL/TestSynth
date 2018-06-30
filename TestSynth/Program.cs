using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectInput;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace TestSynth
{
	class Program
	{
		static AutoResetEvent BufferEnd;

		static void Main(string[] args)
		{
			var input = new DirectInput();

			var keyboard = new Keyboard(input);
			keyboard.Acquire();

			var audio = new XAudio2();
			audio.StartEngine();

			var master = new MasteringVoice(audio);

			var format = new WaveFormat(44100, 16, 1);

			var source = new SourceVoice(audio, format);

			BufferEnd = new AutoResetEvent(false);

			source.BufferEnd += Source_BufferEnd;

			source.Start();

			var buffers = new AudioBuffer[2];

			var pointers = new DataPointer[buffers.Length];

			for (int buffer = 0; buffer < buffers.Length; buffer++)
			{
				pointers[buffer] = new DataPointer(Utilities.AllocateClearedMemory(1024), 1024);
				buffers[buffer] = new AudioBuffer(pointers[buffer]);

				source.SubmitSourceBuffer(buffers[buffer], null);
			}

			var index = 0;

			var data = new byte[1024];
			var time = 0.0;
			var keyboardState = new KeyboardState();

			while (true)
			{
				BufferEnd.WaitOne();

				keyboard.GetCurrentState(ref keyboardState);

				for (int x = 0; x < data.Length; x += 2)
				{
					var value = 0d;
					var count = 0;

					for (int key = 0; key < Keys.Length; key++)
					{
						if (keyboardState.IsPressed(Keys[key]))
						{
							//value += Waves.Sine(time, Notes[KeyNotes[key]] * KeyOctaves[key], 0.0);
							value += Waves.Square(time, Notes[KeyNotes[key]] * KeyOctaves[key], 0.0);
							//value += Waves.Triangle(time, Notes[KeyNotes[key]] * KeyOctaves[key], 0.0);
							//value += Waves.Sawtooth(time, Notes[KeyNotes[key]] * KeyOctaves[key], 0.0);
							count++;
						}
					}

					var value2 = (short)((value / 10.0) * short.MaxValue);

					data[x] = (byte)(value2 & 0xff);
					data[x + 1] = (byte)(value2 >> 8);

					time += 1.0 / (double)format.SampleRate;
				}

				pointers[index].CopyFrom(data);

				source.SubmitSourceBuffer(buffers[index], null);

				index++;

				if (index == buffers.Length)
					index = 0;
			}
		}

		private static void Source_BufferEnd(IntPtr obj)
		{
			BufferEnd.Set();
		}

		private static readonly double[] Notes = new double[]
		{
			261.6256,
			277.1826,
			293.6648,
			311.1270,
			329.6276,
			349.2282,
			369.9944,
			391.9954,
			415.3047,
			440.0000,
			466.1638,
			493.8833
		};

		private static readonly Key[] Keys = new Key[]
		{
			Key.Q,
			Key.D2,
			Key.W,
			Key.D3,
			Key.E,
			Key.R,
			Key.D5,
			Key.T,
			Key.D6,
			Key.Y,
			Key.D7,
			Key.U,
			Key.I,
			Key.D9,
			Key.O,
			Key.D0,
			Key.P,
			Key.LeftBracket,
			Key.Equals,
			Key.RightBracket,

			Key.Z,
			Key.S,
			Key.X,
			Key.D,
			Key.C,
			Key.V,
			Key.G,
			Key.B,
			Key.H,
			Key.N,
			Key.J,
			Key.M,
			Key.Comma,
			Key.L,
			Key.Period,
			Key.Semicolon,
			Key.Slash
		};

		private static readonly int[] KeyNotes = new int[]
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,

			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			0,
			1,
			2,
			3,
			4,
			5
		};

		private static readonly double[] KeyOctaves = new double[]
		{
			1.0,
			1.0,
			1.0,
			1.0,
			1.0,
			1.0,
			1.0,
			1.0,
			1.0,
			1.0,
			1.0,
			1.0,

			2.0,
			2.0,
			2.0,
			2.0,
			2.0,
			2.0,
			2.0,
			2.0,

			0.5,
			0.5,
			0.5,
			0.5,
			0.5,
			0.5,
			0.5,
			0.5,
			0.5,
			0.5,
			0.5,
			0.5,

			1.0,
			1.0,
			1.0,
			1.0,
			1.0,
		};
	}
}
