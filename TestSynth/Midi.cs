using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security;

namespace TestSynth
{
	[SuppressUnmanagedCodeSecurity]
	public static class Midi
	{
		// Represents the method that handles messages from a midi driver.
		public delegate void MidiProc(IntPtr handle, uint msg, IntPtr instance, IntPtr param1, IntPtr param2);

		[DllImport("winmm.dll")]
		public static extern int midiInGetErrorText(int errCode, StringBuilder errMsg, int sizeOfErrMsg);

		[DllImport("winmm.dll")]
		public static extern int midiInOpen(out IntPtr handle, uint deviceID, MidiProc proc, IntPtr instance, uint flags);

		[DllImport("winmm.dll")]
		public static extern int midiInClose(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern int midiInStart(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern int midiInStop(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern int midiInReset(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern int midiInPrepareHeader(IntPtr handle, IntPtr header, uint sizeOfmidiHeader);

		[DllImport("winmm.dll")]
		public static extern int midiInUnprepareHeader(IntPtr handle, IntPtr header, uint sizeOfmidiHeader);

		[DllImport("winmm.dll")]
		public static extern int midiInAddBuffer(IntPtr handle, IntPtr header, uint sizeOfmidiHeader);

		[DllImport("winmm.dll")]
		public static extern int midiInGetDevCaps(IntPtr handle, ref MidiInCaps caps, uint sizeOfmidiInCaps);

		[DllImport("winmm.dll")]
		public static extern int midiInGetNumDevs();

		[DllImport("winmm.dll")]
		public static extern int midiConnect(IntPtr inHandle, IntPtr outHandle,
			IntPtr reserved);

		[DllImport("winmm.dll")]
		public static extern int midiDisconnect(IntPtr inHandle, IntPtr outHandle,
			IntPtr reserved);

		[DllImport("winmm.dll")]
		public static extern int midiOutOpen(out IntPtr handle, uint deviceID, MidiProc proc, IntPtr instance, uint flags);

		[DllImport("winmm.dll")]
		public static extern int midiOutClose(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern int midiOutReset(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern int midiOutShortMsg(IntPtr handle, uint message);

		[DllImport("winmm.dll")]
		public static extern int midiOutPrepareHeader(IntPtr handle, IntPtr header, uint sizeOfmidiHeader);

		[DllImport("winmm.dll")]
		public static extern int midiOutUnprepareHeader(IntPtr handle, IntPtr header, uint sizeOfmidiHeader);

		[DllImport("winmm.dll")]
		public static extern int midiOutLongMsg(IntPtr handle, IntPtr header, uint sizeOfmidiHeader);

		[DllImport("winmm.dll")]
		public static extern int midiOutGetErrorText(int errCode, StringBuilder errMsg, int sizeOfErrMsg);

		[DllImport("winmm.dll")]
		public static extern int midiOutGetDevCaps(IntPtr handle, ref MidiOutCaps caps, uint sizeOfmidiOutCaps);

		[DllImport("winmm.dll")]
		public static extern int midiOutGetNumDevs();

		[DllImport("winmm.dll")]
		public static extern int midiStreamClose(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern int midiStreamOpen(out IntPtr handle, ref uint deviceID, uint cMidi, MidiProc proc, IntPtr instance, uint flags);

		[DllImport("winmm.dll")]
		public static extern int midiStreamOut(IntPtr handle, IntPtr header, uint sizeOfmidiHeader);

		[DllImport("winmm.dll")]
		public static extern int midiStreamPause(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern int midiStreamRestart(IntPtr handle);

		[DllImport("winmm.dll")]
		public static extern int midiStreamStop(IntPtr handle);

		//[DllImport("winmm.dll")]
		//public static extern int midiStreamPosition(IntPtr handle, ref MmTime time, uint sizeOfMmTime);

		//[DllImport("winmm.dll")]
		//public static extern int midiStreamProperty(IntPtr handle, ref MidiStreamOutPortProperty p, uint flags);

		public const int MAXERRORLENGTH = 256;
		public const int MMSYSERR_NOERROR = 0;
		public const int MIDIERR_STILLPLAYING = 65;

		public const uint CALLBACK_FUNCTION = 0x30000;
		public const uint MIDI_IO_STATUS = 32;
		public const uint MIM_OPEN = 0x3C1;
		public const uint MIM_CLOSE = 0x3C2;
		public const uint MIM_DATA = 0x3C3;
		public const uint MIM_LONGDATA = 0x3C4;
		public const uint MIM_ERROR = 0x3C5;
		public const uint MIM_LONGERROR = 0x3C6;
		public const uint MOM_OPEN = 0x3C7;
		public const uint MOM_CLOSE = 0x3C8;
		public const uint MOM_DONE = 0x3C9;
		public const uint MOM_POSITIONCB = 0x3CA;
		public const uint MIM_MOREDATA = 0x3CC;
		public const uint MHDR_DONE = 1;
		public const uint MHDR_PREPARED = 2;
		public const uint MHDR_INQUEUE = 4;
		public const uint MHDR_ISSTRM = 8;

		public const uint MM_STREAM_OPEN = 0x3D4;
		public const uint MM_STREAM_CLOSE = 0x3D5;
		public const uint MM_STREAM_DONE = 0x3D6;
		public const uint MM_STREAM_ERROR = 0x3D7;

		// stream properties
		public const uint MIDIPROP_SET = 0x80000000;
		public const uint MIDIPROP_GET = 0x40000000;
		public const uint MIDIPROP_TIMEDIV = 0x00000001;
		public const uint MIDIPROP_TEMPO = 0x00000002;

		/// <summary>
		/// Represents the Windows Multimedia MIDIINCAPS structure.
		/// </summary>
		public struct MidiInCaps
		{
			public ushort mid;
			public ushort pid;
			public uint driverVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string name;
			public uint support;
		}

		/// <summary>
		/// Represents the Windows Multimedia MIDIOUTCAPS structure.
		/// </summary>
		public struct MidiOutCaps
		{
			public ushort mid;
			public ushort pid;
			public uint driverVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string name;
			public ushort technology;
			public ushort voices;
			public ushort notes;
			public ushort channelMask;
			public uint support;
		}
	}
}