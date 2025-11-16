using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Entities.Characters.Navigation;
using SunsetSystems.Equipment;
using SunsetSystems.Game;
using SunsetSystems.Persistence;
using UMA;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace SunsetSystems.Animation
{
    public class AnimationManager : SerializedMonoBehaviour, IPersistentComponent, IAnimationManager
    {
        public enum CreatureAnimationLayer
        {
            Exploration = 0,
            Combat = 1,
            Stealth = 2
        }

        public event Action<string> OnAnimationEvent;

        private const string ANIMATION_MANAGER_ID = "ANIMATION_MANAGER";
        private const string ANIMATOR_PARAM_ON_MOVE = "IsMoving";
        private const string ANIMATOR_PARAM_SPEED = "Speed";
        private const string ANIMATOR_PARAM_DEATH = "IsDead";

        [Title("References")]
        [SerializeField, Required]
        private ICreature owner;
        [SerializeField, Required]
        private Animator animator;
        [SerializeField, Required]
        private INavigationManager agent;
        [SerializeField, Required]
        private RigBuilder rigBuilder;

        [Title("Config")]
        [SerializeField]
        private float _runSpeedThreshold = 6f;
        [SerializeField]
        private float moveThreshold = .5f;
        [SerializeField]
        private string _weaponAnimationTypeParam;
        private int _weaponAnimationTypeParamHash;
        [SerializeField]
        private string _takeHitAnimationParam;
        private int _takeHitAnimationParamHash;
        [SerializeField]
        private string _fireWeaponAnimationParam;
        private int _fireWeaponAnimationParamHash;
        [SerializeField]
        private string _inCoverAnimationParam;
        private int _inCoverAnimationParamHash;

        [SerializeField]
        private bool _enableStateOverride = false;
        [SerializeField, ShowIf("_enableStateOverride")]
        private GameState _stateOverride = GameState.Combat;

        private GameState _stateOverrideLastFrame;
        private bool _combatAnimationOverrideActive = false;
        private WeaponAnimationType _combatAnimationTypeOverride = WeaponAnimationType.Brawl;
        private WeaponAnimationType _currentCombatAnimationType = WeaponAnimationType.Brawl;


        private readonly int _animatorOnMove = Animator.StringToHash(ANIMATOR_PARAM_ON_MOVE);
        private readonly int _animatorSpeed = Animator.StringToHash(ANIMATOR_PARAM_SPEED);

        private readonly int _isDeadAnimationParamHash = Animator.StringToHash(ANIMATOR_PARAM_DEATH);

        private const string RIGHT_ARM = "CC_Base_R_Upperarm", 
                             RIGHT_FOREARM = "CC_Base_R_Forearm", 
                             RIGHT_HAND = "CC_Base_R_Hand", 
                             RIGHT_HINT = "CC_Base_R_Forearm_Hint";
        private const string LEFT_ARM = "CC_Base_L_Upperarm", 
                             LEFT_FOREARM = "CC_Base_L_Forearm", 
                             LEFT_HAND = "CC_Base_L_Hand", 
                             LEFT_HINT = "CC_Base_L_Forearm_Hint";

        private Transform rightHint, leftHint;
        private TwoBoneIKConstraint rightHandConstraint, leftHandConstraint;

        private bool _initializedOnce = false;

        public string ComponentID => ANIMATION_MANAGER_ID;

        private void Awake()
        {
            _weaponAnimationTypeParamHash = Animator.StringToHash(_weaponAnimationTypeParam);
            _takeHitAnimationParamHash = Animator.StringToHash(_takeHitAnimationParam);
            _fireWeaponAnimationParamHash = Animator.StringToHash(_fireWeaponAnimationParam);
            _inCoverAnimationParamHash = Animator.StringToHash(_inCoverAnimationParam);

            _stateOverrideLastFrame = _stateOverride;
        }

        private void Start()
        {
            rigBuilder.layers.Clear();
            rigBuilder.enabled = false;
        }

        private void Update()
        {
            SynchronizeAnimatorWithNavMeshAgent();
            UpdateAnimationStateOverride();
        }

        private void UpdateAnimationStateOverride()
        {
            if (_enableStateOverride is false || _stateOverride == _stateOverrideLastFrame)
            {
                return;
            }
            switch (_stateOverride)
            {
                case GameState.Exploration:
                    SetCombatAnimationsActive(false);
                    break;
                case GameState.Combat:
                    SetCombatAnimationsActive(true);
                    break;
            }
            _stateOverrideLastFrame = _stateOverride;
        }

        private void SynchronizeAnimatorWithNavMeshAgent()
        {
            bool agentOnMove = agent.IsMoving;
            float agentSpeed = agent.CurrentSpeed / _runSpeedThreshold;
            animator.SetBool(_animatorOnMove, agentOnMove);
            animator.SetFloat(_animatorSpeed, agentSpeed);
        }

        private void SetActiveAnimationLayer(CreatureAnimationLayer layer)
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                StartCoroutine(LerpLayerWeight(animator, i, i == (int)layer ? 1f : 0f, 1f));
            }

            static IEnumerator LerpLayerWeight(Animator animator, int index, float targetWeight, float time)
            {
                float lerp = 0f;
                float startWeight = 1 - targetWeight;
                if (Mathf.Approximately(animator.GetLayerWeight(index), targetWeight))
                    yield break;
                while (lerp < time)
                {
                    float value = Mathf.Lerp(startWeight, targetWeight, lerp / time);
                    animator.SetLayerWeight(index, value);
                    lerp += Time.deltaTime;
                    yield return null;
                }
                animator.SetLayerWeight(index, targetWeight);
            }
        }

        public void SetCombatAnimationsActive(bool isCombat)
        {
            animator.SetBool("IsCombat", isCombat);
            if (isCombat)
            {
                SetActiveAnimationLayer(CreatureAnimationLayer.Combat);
            }
            else
            {
                SetActiveAnimationLayer(CreatureAnimationLayer.Exploration);
            }
        }

        public void OnWeaponChanged(IWeaponInstance weaponInstance)
        {
            _currentCombatAnimationType = weaponInstance != null ? weaponInstance.WeaponAnimationData.AnimationType : WeaponAnimationType.Brawl;
            UpdateCombatAnimationType();
        }

        public void SetCombatAnimationTypeOverride(WeaponAnimationType preCastAnimationType)
        {
            _combatAnimationOverrideActive = true;
            _combatAnimationTypeOverride = preCastAnimationType;
            UpdateCombatAnimationType();
        }

        public void ClearCombatAnimationTypeOverride()
        {
            _combatAnimationOverrideActive = false;
            UpdateCombatAnimationType();
        }

        private void UpdateCombatAnimationType()
        {
            if (_combatAnimationOverrideActive)
            {
                SetInteger(_weaponAnimationTypeParamHash, (int)_combatAnimationTypeOverride);
            }
            else
            {
                SetInteger(_weaponAnimationTypeParamHash, (int)_currentCombatAnimationType);
            }
        }

        private Rig InitializeRigLayer()
        {
            Rig layer = new GameObject("RigLayer").AddComponent<Rig>();
            layer.transform.SetParent(rigBuilder.transform);
            UMAData umaData = owner.References.GetCachedComponentInChildren<UMAData>();

            rightHandConstraint = new GameObject("RightHandIK").AddComponent<TwoBoneIKConstraint>();
            rightHandConstraint.transform.parent = layer.transform;
            rightHandConstraint.data.root = umaData.GetBoneGameObject(RIGHT_ARM).transform;
            Transform rightForearm = umaData.GetBoneGameObject(RIGHT_FOREARM).transform;
            rightHandConstraint.data.mid = rightForearm;
            rightHandConstraint.data.tip = umaData.GetBoneGameObject(RIGHT_HAND).transform;
            rightHandConstraint.data.hint = rightHint = new GameObject(RIGHT_HINT).transform;
            rightHint.parent = rightForearm;

            leftHandConstraint = new GameObject("LeftHandIK").AddComponent<TwoBoneIKConstraint>();
            leftHandConstraint.transform.parent = layer.transform;
            leftHandConstraint.data.root = umaData.GetBoneGameObject(LEFT_ARM).transform;
            Transform leftForearm = umaData.GetBoneGameObject(LEFT_FOREARM).transform;
            leftHandConstraint.data.mid = leftForearm;
            leftHandConstraint.data.tip = umaData.GetBoneGameObject(LEFT_HAND).transform;
            leftHandConstraint.data.hint = leftHint = new GameObject(LEFT_HINT).transform;
            leftHint.parent = leftForearm;

            _initializedOnce = true;
            return layer;
        }

        public void EnableIK(WeaponAnimationDataProvider ikData)
        {
            if (!_initializedOnce)
                rigBuilder.layers.Add(new(InitializeRigLayer()));

            rightHandConstraint.data.target = ikData.RightHandIK;
            rightHandConstraint.data.targetPositionWeight = 1;
            rightHandConstraint.data.targetRotationWeight = 1;
            rightHint.localPosition = ikData.RightHintLocalPosition;
            rightHandConstraint.data.hintWeight = 1;

            leftHandConstraint.data.target = ikData.LeftHandIK;
            leftHandConstraint.data.targetPositionWeight = 1;
            leftHandConstraint.data.targetRotationWeight = 1;
            leftHint.localPosition = ikData.RightHintLocalPosition;
            leftHandConstraint.data.hintWeight = 1;

            rigBuilder.enabled = true;
            animator.SetInteger("WeaponAnimationType", (int)ikData.AnimationType);
        }

        public void DisableIK()
        {
            if (!_initializedOnce)
                rigBuilder.layers.Add(new(InitializeRigLayer()));

            rightHandConstraint.data.targetPositionWeight = 0;
            rightHandConstraint.data.targetRotationWeight = 0;
            rightHandConstraint.data.hintWeight = 0;

            leftHandConstraint.data.targetPositionWeight = 0;
            leftHandConstraint.data.targetRotationWeight = 0;
            leftHandConstraint.data.hintWeight = 0;

            rigBuilder.enabled = false;
        }

        public void PlayFireWeaponAnimation()
        {
            animator.SetTrigger(_fireWeaponAnimationParamHash);
        }

        public void PlayTakeHitAnimation()
        {
            animator.SetTrigger(_takeHitAnimationParamHash);
        }

        public void SetCoverAnimationsEnabled(bool enabled)
        {
            animator.SetBool(_inCoverAnimationParamHash, enabled);
        }

        public void SetIsDead(bool dead)
        {
            SetBool(_isDeadAnimationParamHash, dead);
        }

        public void SetTrigger(string name)
        {
            SetTrigger(Animator.StringToHash(name));
        }

        public void SetTrigger(int hash)
        {
            animator.SetTrigger(hash);
        }

        public void SetInteger(int hash, int value)
        {
            animator.SetInteger(hash, value);
        }

        public void SetBool(int hash, bool value)
        {
            animator.SetBool(hash, value);
        }

        public void HandleAnimationEvent(string eventType)
        {
            OnAnimationEvent?.Invoke(eventType);
        }

        public Vector3 GetBonePosition(HumanBodyBones bone)
        {
            return animator.GetBoneTransform(bone).position;
        }

        public object GetComponentPersistenceData()
        {
            return new AnimatorPersistenceData(this);
        }

        public void InjectComponentPersistenceData(object data)
        {
            if (data is not AnimatorPersistenceData animatorData)
                return;
            foreach (int key in animatorData.AnimatorStateData.Keys)
            {
                var stateHash = animatorData.AnimatorStateData[key];
                animator.Play(stateHash, key);
            }
        }

        [Serializable]
        public class AnimatorPersistenceData
        {
            public Dictionary<int, int> AnimatorStateData;

            public AnimatorPersistenceData(AnimationManager animationManager)
            {
                AnimatorStateData = new();
                for (int i = 0; i < animationManager.animator.layerCount; i++)
                {
                    AnimatorStateData[i] = animationManager.animator.GetCurrentAnimatorStateInfo(i).shortNameHash;
                }
            }

            public AnimatorPersistenceData()
            {
                AnimatorStateData = new();
            }
        }
    }
}
