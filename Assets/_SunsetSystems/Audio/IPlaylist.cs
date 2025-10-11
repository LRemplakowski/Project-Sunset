using UnityEngine;

namespace SunsetSystems.Audio
{
    public interface IPlaylist
    {
        Awaitable<AudioClip> NextTrack();
        Awaitable<AudioClip> PreviousTrack();
        AudioClip GetCurrentTrack();

        void ReleaseReferences();
    }
}
