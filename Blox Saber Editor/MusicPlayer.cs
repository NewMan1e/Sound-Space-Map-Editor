﻿using System;
using System.IO;
using Blox_Saber_Editor.SoundTouch;
using NAudio.Wave;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace Blox_Saber_Editor
{
	class MusicPlayer : IDisposable
	{
		private object locker = new object();

		private int streamID;

		public MusicPlayer()
		{
			Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
		}

		public void Load(string file)
		{
			var stream = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_FX_FREESOURCE);
			var volume = Volume;
			var tempo = Tempo;

			Bass.BASS_StreamFree(streamID);

			streamID = BassFx.BASS_FX_TempoCreate(stream, BASSFlag.BASS_DEFAULT);

			Volume = volume;
			Tempo = tempo;

			Reset();
		}

		public void Play()
		{
			//lock (locker)
			{
				Bass.BASS_ChannelPlay(streamID, false);
			}
		}

		public void Pause()
		{
			//lock (locker)
			{
				Stop();
			//	Bass.BASS_ChannelPause(streamID);
			}
		}

		public void Stop()
		{
			//lock (locker)
			{
				Bass.BASS_ChannelStop(streamID);
			}
		}

		public float Tempo
		{
			set => Bass.BASS_ChannelSetAttribute(streamID, BASSAttribute.BASS_ATTRIB_TEMPO, value * 100 - 100);
			get
			{
				float val = 0;

				Bass.BASS_ChannelGetAttribute(streamID, BASSAttribute.BASS_ATTRIB_TEMPO, ref val);

				return -(val + 95) / 100;
			}
		}

		public float Volume
		{
			set => Bass.BASS_ChannelSetAttribute(streamID, BASSAttribute.BASS_ATTRIB_VOL, value);
			get
			{
				float val = 1;

				Bass.BASS_ChannelGetAttribute(streamID, BASSAttribute.BASS_ATTRIB_VOL, ref val);

				return val;
			}
		}

		public void Reset()
		{
			Stop();

			CurrentTime = TimeSpan.Zero;
		}

		public bool IsPlaying => Bass.BASS_ChannelIsActive(streamID) == BASSActive.BASS_ACTIVE_PLAYING;
		public bool IsPaused => Bass.BASS_ChannelIsActive(streamID) == BASSActive.BASS_ACTIVE_PAUSED;

		public TimeSpan TotalTime
		{
			get
			{
				long len = Bass.BASS_ChannelGetLength(streamID, BASSMode.BASS_POS_BYTES);
				var time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(streamID, len));

				return time;
			}
		}

		public TimeSpan CurrentTime
		{
			get
			{
				long pos = Bass.BASS_ChannelGetPosition(streamID, BASSMode.BASS_POS_BYTES);
				var time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(streamID, pos));

				return time;
			}
			set
			{
				//lock (locker)
				{
					var pos = Bass.BASS_ChannelSeconds2Bytes(streamID, value.TotalSeconds);

					Bass.BASS_ChannelSetPosition(streamID, pos, BASSMode.BASS_POS_BYTES);
				}
			}
		}

		public double Progress
		{
			get
			{
				var pos = Bass.BASS_ChannelGetPosition(streamID, BASSMode.BASS_POS_BYTES);
				var length = Bass.BASS_ChannelGetLength(streamID, BASSMode.BASS_POS_BYTES);

				return (double)(pos / (decimal)length);
			}
		}

		public void Dispose()
		{
			Bass.BASS_Free();
		}
	}
}