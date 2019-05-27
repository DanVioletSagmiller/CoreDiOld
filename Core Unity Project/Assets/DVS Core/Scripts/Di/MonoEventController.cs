//using Dvs.Core.IoC.Events;
//using UnityEngine;

//namespace Dvs.Core.IoC.Events
//{
//    /// <summary>
//    /// Used with <see cref="Dvs.Core.IoC.Di.TriggerEvent{T}(T)"/>, 
//    /// to share MonoBehaviour's Start event
//    /// </summary>
//    public class OnMonoStartEvent { }

//    /// <summary>
//    /// Used with <see cref="Dvs.Core.IoC.Di.TriggerEvent{T}(T)"/>, 
//    /// to share MonoBehaviour's first update event
//    /// </summary>
//    public class OnMonoFirstFrameEvent { }

//    /// <summary>
//    /// Used with <see cref="Dvs.Core.IoC.Di.TriggerEvent{T}(T)"/>, 
//    /// to share MonoBehaviour's update event
//    /// </summary>
//    public class OnMonoUpdateEvent { }
//}

//namespace Dvs.Core.IoC
//{
//    public class MonoEventController : MonoBehaviour
//    {
//        void Start()
//        {
//            Di.TriggerEvent(new OnMonoStartEvent());
//            Di.AttachEventListener<OnMonoUpdateEvent>(HandleFirstFrame);
//        }

//        private void HandleFirstFrame(OnMonoUpdateEvent caller)
//        {
//            Di.AttachEventListener<OnMonoUpdateEvent>(HandleFirstFrame, false);
//            Di.TriggerEvent(new OnMonoFirstFrameEvent());
//        }

//        void Update()
//        {
//            Di.TriggerEvent(new OnMonoUpdateEvent());
//        }
//    }
//}