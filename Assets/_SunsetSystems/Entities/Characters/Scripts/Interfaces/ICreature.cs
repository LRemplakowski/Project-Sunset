using SunsetSystems.ActionSystem;
using SunsetSystems.Entities.Interfaces;
using UnityEngine;

namespace SunsetSystems.Entities.Characters
{
    public interface ICreature : IEntity, IActionPerformer, ICreatureTemplateProvider
    {
        new ICreatureReferences References { get; }

        void ForceToPosition(Vector3 position, Quaternion rotation);
        void ForceToPosition(Transform positionTransform);
        void FacePointInSpace(Vector3 point);
        void FaceTransform(Transform transform);
        void InjectDataFromTemplate(ICreatureTemplate template);
    }
}
