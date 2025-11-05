using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SunsetSystems.Audio
{
    public interface IPlaylist
    {
        UniTask<AudioClip> NextTrack();
        UniTask<AudioClip> PreviousTrack();
        AudioClip GetCurrentTrack();

        void ReleaseReferences();
    }
}
