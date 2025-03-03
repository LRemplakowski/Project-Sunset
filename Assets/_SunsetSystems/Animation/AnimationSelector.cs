using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace SunsetSystems.Animation
{
    [RequireComponent(typeof(Animator))]
    public class AnimationSelector : StateMachineBehaviour
    {
        private enum ParamType
        {
            Float, Integer
        }

        [SerializeField]
        private ParamType _paramType = ParamType.Integer;
        [SerializeField]
        private int _stateCount;
        [SerializeField]
        private string animatorParameter = "selected_animation";

        private static readonly System.Random _random = new();

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            switch (_paramType)
            {
                case ParamType.Float:
                    animator.SetFloat(animatorParameter, _random.Next(0, _stateCount));
                    break;
                case ParamType.Integer:
                    animator.SetInteger(animatorParameter, _random.Next(0, _stateCount));
                    break;
            }
        }
    }
}
