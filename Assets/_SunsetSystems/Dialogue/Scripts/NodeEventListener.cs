using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Dialogue
{
    public class NodeEventListener : MonoBehaviour
    {
        [Title("Config")]
        [SerializeField]
        private string _listenForNode = "";

        [Title("Events")]
        public UltEvent OnNodeEventMatch = new();

        public void OnNodeEvent(string nodeName)
        {
            if (_listenForNode.Equals(nodeName))
            {
                Debug.Log($"{nameof(NodeEventListener)} >>> Node event triggered! Node: {_listenForNode}", this);
                OnNodeEventMatch?.InvokeSafe();
            }
        }
    }
}
