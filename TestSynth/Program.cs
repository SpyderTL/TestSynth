using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
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
		static readonly bool[] MidiNotes = new bool[128];
		static readonly double[] MidiVelocity = new double[128];
		static readonly double[] NoteVelocity = new double[128];
		static readonly int MidiChannel = 0;
		static readonly double Attack = 255;
		static readonly double Release = 255;

		static Key[] Keys;
		static int[] KeyNotes;
		static int[] KeyOctaves;

		static void Main(string[] args)
		{
			Keys = BassKeys;
			KeyNotes = BassKeyNotes;
			KeyOctaves = BassKeyOctaves;

			var devices = Midi.midiInGetNumDevs();
			var deviceHandle = IntPtr.Zero;
			var deviceCaps = new Midi.MidiInCaps();

			for (var device = 0U; device < devices; device++)
			{
				Midi.midiInOpen(out deviceHandle, device, MidiProc, IntPtr.Zero, Midi.CALLBACK_FUNCTION);
				Midi.midiInGetDevCaps(deviceHandle, ref deviceCaps, (uint)Marshal.SizeOf(deviceCaps));

				Console.WriteLine(deviceCaps.name);

				Midi.midiInStart(deviceHandle);
			}

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
					var delta = 1.0 / format.SampleRate;

					var value = 0d;
					var count = 0;

					for (var note = 24; note < MidiNotes.Length; note++)
						MidiNotes[note] = false;

					for (var key = 0; key < Keys.Length; key++)
					{
						var noteIndex = 24 + (KeyOctaves[key] * 12) + KeyNotes[key];

						if (keyboardState.IsPressed(Keys[key]))
						{
							MidiNotes[noteIndex] = true;
							MidiVelocity[noteIndex] = 1.0f;
						}
					}

					for (var note = 24; note < MidiNotes.Length; note++)
					{
						if (MidiNotes[note])
						{
							if (NoteVelocity[note] >= 1.0 - (Attack * delta))
								NoteVelocity[note] = 1.0f;
							else
								NoteVelocity[note] += (Attack * delta);
						}
						else
						{
							if (NoteVelocity[note] <= (Release * delta))
								NoteVelocity[note] = 0.0f;
							else
								NoteVelocity[note] -= (Release * delta);
						}
					}

					for (var octave = 0; octave < 8; octave++)
					{
						for (var note = 0; note < 12; note++)
						{
							var noteIndex = 24 + (octave * 12) + note;

							if (NoteVelocity[noteIndex] != 0.0)
							{
								value += Waves.Sine(time, Notes[note] * MidiOctaves[octave], 0.0) * MidiVelocity[noteIndex] * NoteVelocity[noteIndex];
								//value += Waves.Square(time, Notes[note] * MidiOctaves[octave], 0.0) * MidiVelocity[noteIndex] * NoteVelocity[noteIndex];
								//value += Waves.Triangle(time, Notes[note] * MidiOctaves[octave], 0.0) * MidiVelocity[noteIndex] * NoteVelocity[noteIndex];
								value += Waves.Sawtooth(time, Notes[note] * MidiOctaves[octave], 0.0) * MidiVelocity[noteIndex] * NoteVelocity[noteIndex];
								count++;
							}
						}
					}

					var value2 = (short)((value / 10.0) * short.MaxValue);

					data[x] = (byte)(value2 & 0xff);
					data[x + 1] = (byte)(value2 >> 8);

					time += delta;
				}

				pointers[index].CopyFrom(data);

				source.SubmitSourceBuffer(buffers[index], null);

				index++;

				if (index == buffers.Length)
					index = 0;
			}
		}

		private static void MidiProc(IntPtr handle, uint msg, IntPtr instance, IntPtr param1, IntPtr param2)
		{
			if (msg == Midi.MIM_DATA)
			{
				var timestamp = param2.ToInt32();

				var data = param1.ToInt32();

				var status = data & 0xff;
				var note = (data >> 8) & 0xff;
				var velocity = (data >> 16) & 0xff;
				var unused = data >> 24;

				var message = status >> 4;
				var channel = status & 0xf;

				if (channel == MidiChannel)
				{
					if (message == 0x8)
					{
						MidiNotes[note] = false;
						MidiVelocity[note] = velocity / 255.0;

						Console.WriteLine("Note Off: " + note + " Velocity: " + velocity + " Channel: " + channel);
					}
					else if (message == 0x9)
					{
						MidiNotes[note] = true;
						MidiVelocity[note] = velocity / 255.0;

						Console.WriteLine("Note On: " + note + " Velocity: " + velocity + " Channel: " + channel);
					}
				}
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

		private static readonly Key[] PianoKeys = new Key[]
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

		private static readonly Key[] BassKeys = new Key[]
		{
			Key.Z,
			Key.X,
			Key.C,
			Key.V,
			Key.B,
			Key.N,
			Key.M,
			Key.Comma,
			Key.Period,
			Key.Slash,

			Key.A,
			Key.S,
			Key.D,
			Key.F,
			Key.G,
			Key.H,
			Key.J,
			Key.K,
			Key.L,
			Key.Semicolon,
			Key.Apostrophe,

			Key.Q,
			Key.W,
			Key.E,
			Key.R,
			Key.T,
			Key.Y,
			Key.U,
			Key.I,
			Key.O,
			Key.P,
			Key.LeftBracket,
			Key.RightBracket,
			Key.Backslash,
			Key.D1,
			Key.D2,
			Key.D3,
			Key.D4,
			Key.D5,
			Key.D6,
			Key.D7,
			Key.D8,
			Key.D9,
			Key.D0,
			Key.Minus,
			Key.Equals
		};

		private static readonly int[] PianoKeyNotes = new int[]
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


		private static readonly int[] BassKeyNotes = new int[]
		{
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
			6
		};

		private static readonly double[] PianoKeyOctaves = new double[]
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

		//private static readonly double[] BassKeyOctaves = new double[]
		//{
		//	0.125,
		//	0.125,
		//	0.125,
		//	0.125,
		//	0.125,
		//	0.125,
		//	0.125,
		//	0.125,
		//	0.25,
		//	0.25,

		//	0.125,
		//	0.125,
		//	0.125,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,

		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.5,
		//	0.5,
		//	0.5,

		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.25,
		//	0.5,
		//	0.5,
		//	0.5,
		//	0.5,
		//	0.5,
		//	0.5,
		//	0.5
		//};

		private static readonly int[] BassKeyOctaves = new int[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			1,
			1,

			0,
			0,
			0,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,

			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			2,
			2,
			2,

			1,
			1,
			1,
			1,
			1,
			2,
			2,
			2,
			2,
			2,
			2,
			2
		};

		private static readonly double[] MidiOctaves = new double[]
		{
			0.125,
			0.25,
			0.5,
			1.0,
			2.0,
			4.0,
			8.0,
			16.0
		};
	}
}
