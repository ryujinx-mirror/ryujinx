using SoundIOSharp;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Ryujinx.Audio.SoundIo
{
    /// <summary>
    /// An object pool containing a set of audio tracks
    /// </summary>
    internal class SoundIoAudioTrackPool : IDisposable
    {
        /// <summary>
        /// The current size of the <see cref="SoundIoAudioTrackPool"/>
        /// </summary>
        private int m_Size;

        /// <summary>
        /// The maximum size of the <see cref="SoundIoAudioTrackPool"/>
        /// </summary>
        private int m_MaxSize;

        /// <summary>
        /// The <see cref="SoundIO"/> audio context this track pool belongs to
        /// </summary>
        private SoundIO m_Context;

        /// <summary>
        /// The <see cref="SoundIODevice"/> audio device this track pool belongs to
        /// </summary>
        private SoundIODevice m_Device;

        /// <summary>
        /// The queue that keeps track of the available <see cref="SoundIoAudioTrack"/> in the pool.
        /// </summary>
        private ConcurrentQueue<SoundIoAudioTrack> m_Queue;

        /// <summary>
        /// The dictionary providing mapping between a TrackID and <see cref="SoundIoAudioTrack"/>
        /// </summary>
        private ConcurrentDictionary<int, SoundIoAudioTrack> m_TrackList;

        /// <summary>
        /// Gets the current size of the <see cref="SoundIoAudioTrackPool"/>
        /// </summary>
        public int Size { get => m_Size; }

        /// <summary>
        /// Gets the maximum size of the <see cref="SoundIoAudioTrackPool"/>
        /// </summary>
        public int MaxSize { get => m_MaxSize; }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="SoundIoAudioTrackPool"/> is empty
        /// </summary>
        public bool IsEmpty { get => m_Queue.IsEmpty; }

        /// <summary>
        /// Constructs a new instance of a <see cref="SoundIoAudioTrackPool"/> that is empty
        /// </summary>
        /// <param name="maxSize">The maximum amount of tracks that can be created</param>
        public SoundIoAudioTrackPool(SoundIO context, SoundIODevice device, int maxSize)
        {
            m_Size    = 0;
            m_Context = context;
            m_Device  = device;
            m_MaxSize = maxSize;

            m_Queue     = new ConcurrentQueue<SoundIoAudioTrack>();
            m_TrackList = new ConcurrentDictionary<int, SoundIoAudioTrack>();
        }

        /// <summary>
        /// Constructs a new instance of a <see cref="SoundIoAudioTrackPool"/> that contains
        /// the specified amount of <see cref="SoundIoAudioTrack"/>
        /// </summary>
        /// <param name="maxSize">The maximum amount of tracks that can be created</param>
        /// <param name="initialCapacity">The initial number of tracks that the pool contains</param>
        public SoundIoAudioTrackPool(SoundIO context, SoundIODevice device, int maxSize, int initialCapacity)
            : this(context, device, maxSize)
        {
            var trackCollection = Enumerable.Range(0, initialCapacity)
                                            .Select(TrackFactory);

            m_Size  = initialCapacity;
            m_Queue = new ConcurrentQueue<SoundIoAudioTrack>(trackCollection);
        }

        /// <summary>
        /// Creates a new <see cref="SoundIoAudioTrack"/> with the proper AudioContext and AudioDevice
        /// and the specified <paramref name="trackId" />
        /// </summary>
        /// <param name="trackId">The ID of the track to be created</param>
        /// <returns>A new AudioTrack with the specified ID</returns>
        private SoundIoAudioTrack TrackFactory(int trackId)
        {
            // Create a new AudioTrack
            SoundIoAudioTrack track = new SoundIoAudioTrack(trackId, m_Context, m_Device);

            // Keep track of issued tracks
            m_TrackList[trackId] = track;

            return track;
        }

        /// <summary>
        /// Retrieves a <see cref="SoundIoAudioTrack"/> from the pool
        /// </summary>
        /// <returns>An AudioTrack from the pool</returns>
        public SoundIoAudioTrack Get()
        {
            // If we have a track available, reuse it
            if (m_Queue.TryDequeue(out SoundIoAudioTrack track))
            {
                return track;
            }

            // Have we reached the maximum size of our pool?
            if (m_Size >= m_MaxSize)
            {
                return null;
            }

            // We don't have any pooled tracks, so create a new one
            return TrackFactory(m_Size++);
        }

        /// <summary>
        /// Retrieves the <see cref="SoundIoAudioTrack"/> associated with the specified <paramref name="trackId"/> from the pool
        /// </summary>
        /// <param name="trackId">The ID of the track to retrieve</param>
        public SoundIoAudioTrack Get(int trackId)
        {
            if (m_TrackList.TryGetValue(trackId, out SoundIoAudioTrack track))
            {
                return track;
            }

            return null;
        }

        /// <summary>
        /// Attempts to get a <see cref="SoundIoAudioTrack"/> from the pool
        /// </summary>
        /// <param name="track">The track retrieved from the pool</param>
        /// <returns>True if retrieve was successful</returns>
        public bool TryGet(out SoundIoAudioTrack track)
        {
            track = Get();

            return track != null;
        }

        /// <summary>
        /// Attempts to get the <see cref="SoundIoAudioTrack" /> associated with the specified <paramref name="trackId"/> from the pool
        /// </summary>
        /// <param name="trackId">The ID of the track to retrieve</param>
        /// <param name="track">The track retrieved from the pool</param>
        public bool TryGet(int trackId, out SoundIoAudioTrack track)
        {
            return m_TrackList.TryGetValue(trackId, out track);
        }

        /// <summary>
        /// Returns an <see cref="SoundIoAudioTrack"/> back to the pool for reuse
        /// </summary>
        /// <param name="track">The track to be returned to the pool</param>
        public void Put(SoundIoAudioTrack track)
        {
            // Ensure the track is disposed and not playing audio
            track.Close();

            // Requeue the track for reuse later
            m_Queue.Enqueue(track);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SoundIoAudioTrackPool" />
        /// </summary>
        public void Dispose()
        {
            foreach (var track in m_TrackList)
            {
                track.Value.Close();
                track.Value.Dispose();
            }

            m_Size = 0;
            m_Queue.Clear();
            m_TrackList.Clear();
        }
    }
}
