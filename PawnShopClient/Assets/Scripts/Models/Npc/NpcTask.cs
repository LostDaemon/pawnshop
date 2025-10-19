using UnityEngine;

namespace PawnShop.Models.Npc
{
    [System.Serializable]
    public class NpcTask
    {
        //for Unity Editor
        [SerializeField] private NpcTaskType type;
        [SerializeField] private Transform target;
        [SerializeField] private float value;
        [SerializeField] private NpcAction trigger;

        public NpcTaskType Type 
        { 
            get => type; 
            set => type = value; 
        }
        
        public Transform Target 
        { 
            get => target; 
            set => target = value; 
        }
        
        public float Value 
        { 
            get => value; 
            set => this.value = value; 
        }

        public NpcAction Trigger 
        { 
            get => trigger; 
            set => trigger = value; 
        }
    }
}
