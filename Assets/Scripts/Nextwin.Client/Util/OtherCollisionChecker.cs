using System.Collections.Generic;
using UnityEngine;

namespace Nextwin.Client.Util
{
    public enum CollisionEvent
    {
        OnCollisionEnter,
        OnCollisionExit,
        OnCollisionStay,
        OnTriggerEnter,
        OnTriggerExit,
        OnTriggerStay
    }

    public class OtherCollisionChecker : MonoBehaviour
    {
        public delegate void CollisionEventHandler(Collider collider);

        private event CollisionEventHandler CollisionEnterHandler;
        private event CollisionEventHandler CollisionExitHandler;
        private event CollisionEventHandler CollisionStayHandler;
        private event CollisionEventHandler TriggerEnterHandler;
        private event CollisionEventHandler TriggerExitHandler;
        private event CollisionEventHandler TriggerStayHandler;

        private Dictionary<CollisionEvent, CollisionEventHandler> _events = new Dictionary<CollisionEvent, CollisionEventHandler>();

        private void Awake()
        {
            _events.Add(CollisionEvent.OnCollisionEnter, CollisionEnterHandler);
            _events.Add(CollisionEvent.OnCollisionExit, CollisionExitHandler);
            _events.Add(CollisionEvent.OnCollisionStay, CollisionStayHandler);
            _events.Add(CollisionEvent.OnTriggerEnter, TriggerEnterHandler);
            _events.Add(CollisionEvent.OnTriggerExit, TriggerExitHandler);
            _events.Add(CollisionEvent.OnTriggerStay, TriggerStayHandler);
        }

        public void AddCollisionEvent(CollisionEvent collisionEvent, CollisionEventHandler handler)
        {
            _events[collisionEvent] += handler;
        }

        private void OnCollisionEnter(Collision collision)
        {
            _events[CollisionEvent.OnCollisionEnter]?.Invoke(collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            _events[CollisionEvent.OnCollisionExit]?.Invoke(collision.collider);
        }

        private void OnCollisionStay(Collision collision)
        {
            _events[CollisionEvent.OnCollisionStay]?.Invoke(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            _events[CollisionEvent.OnTriggerEnter]?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            _events[CollisionEvent.OnTriggerExit]?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            _events[CollisionEvent.OnTriggerStay]?.Invoke(other);
        }
    }
}