﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.ExceptionServices;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    /// <summary>
    /// Represents an audio channel to play audio clip.
    /// </summary>
    public sealed class Channel : IDisposable
    {
        private AudioDevice? device;

        private uint alSource;
        private float volume;
        private float pitch;
        private Vector3 position;

        private AudioClip? audioClip;

        /// <summary>
        /// Creates a new audio channel.
        /// </summary>
        /// <param name="device">The <see cref="AudioDevice"/> for which the new channel is to be created.</param>
        public Channel(AudioDevice device)
        {
            try
            {
                if (device == null)
                {
                    throw new ArgumentNullException(nameof(device));
                }

                this.device = device;

                alSource = device.AL.GenSource();
                if (device.AL.GetError() != AudioError.NoError)
                {
                    throw new Exception("Failed to generate an audio source.");
                }

                volume = 1F;
                device.AL.SetSourceProperty(alSource, SourceFloat.Gain, volume);

                pitch = 1F;
                device.AL.SetSourceProperty(alSource, SourceFloat.Pitch, pitch);

                position = device.ListernerPosition - device.ListernerDirection;
                device.AL.SetSourceProperty(alSource, SourceVector3.Position, position);

                device.AddResource(this);
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        /// <summary>
        /// Disposes the resources held by the <see cref="Channel"/>.
        /// </summary>
        public void Dispose()
        {
            if (device == null)
            {
                return;
            }

            device.RemoveResource(this);

            if (alSource != 0)
            {
                device.AL.SourceStop(alSource);
                device.AL.DeleteSource(alSource);
                alSource = 0;
            }

            device = null;
        }

        /// <summary>
        /// Plays the audio clip which is currently attached to the channel.
        /// </summary>
        public void Play()
        {
            if (device == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (audioClip == null)
            {
                return;
            }

            device.AL.SourcePlay(alSource);
        }

        /// <summary>
        /// Plays the specified audio clip.
        /// </summary>
        /// <param name="audioClip">The audio clip to be played.</param>
        public void Play(AudioClip audioClip)
        {
            if (device == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (audioClip == null)
            {
                throw new ArgumentNullException(nameof(audioClip));
            }

            AudioClip = audioClip;

            device.AL.SourcePlay(alSource);
        }

        /// <summary>
        /// Stops playing sound.
        /// </summary>
        public void Stop()
        {
            if (device == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (audioClip == null)
            {
                return;
            }

            device.AL.SourceStop(alSource);
        }

        /// <summary>
        /// Pauses playing sound.
        /// </summary>
        public void Pause()
        {
            if (device == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (audioClip == null)
            {
                return;
            }

            device.AL.SourcePause(alSource);
        }

        /// <summary>
        /// Resets the playback state.
        /// </summary>
        public void Rewind()
        {
            if (device == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (audioClip == null)
            {
                return;
            }

            device.AL.SourceRewind(alSource);
        }

        /// <summary>
        /// Gets or sets the <see cref="DrippyAL.AudioClip"/> to be played.
        /// If set to null, the current <see cref="DrippyAL.AudioClip"/> is detached from the channel.
        /// </summary>
        public AudioClip? AudioClip
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                return audioClip;
            }

            set
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                device.AL.SourceStop(alSource);

                if (value == null)
                {
                    audioClip = null;
                    device.AL.SetSourceProperty(alSource, SourceInteger.Buffer, 0);
                }
                else
                {
                    audioClip = value;
                    device.AL.SetSourceProperty(alSource, SourceInteger.Buffer, audioClip.AlBuffer);
                }
            }
        }

        /// <summary>
        /// Gets or sets the volume.
        /// The value must be between 0 and 1.
        /// </summary>
        public float Volume
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                return volume;
            }

            set
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                volume = value;
                device.AL.SetSourceProperty(alSource, SourceFloat.Gain, volume);
            }
        }

        /// <summary>
        /// Gets or sets the pitch to play the audio clip.
        /// The playback frequency will be multiplied by this value.
        /// </summary>
        public float Pitch
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                return pitch;
            }

            set
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                pitch = value;
                device.AL.SetSourceProperty(alSource, SourceFloat.Pitch, pitch);
            }
        }

        /// <summary>
        /// Gets or sets the position of the sound source.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                return position;
            }

            set
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                position = value;
                device.AL.SetSourceProperty(alSource, SourceVector3.Position, position);
            }
        }

        /// <summary>
        /// Gets the current playback state of the channel.
        /// </summary>
        public PlaybackState State
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                int value;
                device.AL.GetSourceProperty(alSource, GetSourceInteger.SourceState, out value);

                switch ((SourceState)value)
                {
                    case SourceState.Initial:
                    case SourceState.Stopped:
                        return PlaybackState.Stopped;

                    case SourceState.Playing:
                        return PlaybackState.Playing;

                    case SourceState.Paused:
                        return PlaybackState.Paused;

                    default:
                        throw new Exception();
                }
            }
        }

        /// <summary>
        /// Gets the current playing offset of the audio clip.
        /// </summary>
        public TimeSpan PlayingOffset
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                float value;
                device.AL.GetSourceProperty(alSource, SourceFloat.SecOffset, out value);

                return TimeSpan.FromSeconds(value);
            }
        }
    }
}
