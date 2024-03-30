using UnityEngine;

namespace Netcode
{
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkRigidbody : NetworkBehaviour
    {
        private Rigidbody _rb;

        private Vector3 _previousPosition;
        private Quaternion _previousRotation;
        private Vector3 _previousVelocity;
        private Vector3 _previousAngularVelocity;
        private Vector3 _acceleration; //?
        private float _previousTime;
        

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _previousTime = Time.time;
        }

        protected override void OnOwnershipChanged(bool state)
        {
            _rb.isKinematic = !state;
        }

        protected override int InitializeBuffers()
        {
            return 6 * sizeof(float);
        }

        protected override void ReceiveUpdate(ref byte[] bytes, int idx)
        {
            var transform1 = transform;
            _previousPosition = transform1.position;
            _previousRotation = transform1.rotation;
            float pt = _previousTime;
            _previousTime = Time.time;
            Vector3 prvvel = _previousVelocity;
            _previousVelocity = bytes.ToVector3(idx);
            _acceleration = (_previousVelocity - prvvel) / (_previousTime - pt);
            _previousAngularVelocity = bytes.ToVector3(idx + sizeof(float) * 3);
        }

        protected override void SendUpdate(ref byte[] bytes)
        {
            _rb.velocity.ToBytes(ref bytes, 0 );
            _rb.angularVelocity.ToBytes(ref bytes, 3 * sizeof(float));
        }

    
        private void LateUpdate()
        {
            if (IsOwner) return;
            float t = Time.time - _previousTime;
            Vector3 pos = _previousPosition + _previousVelocity * t + _acceleration * (0.5f * t * t);
            Quaternion rot = Quaternion.Euler(_previousAngularVelocity * (Mathf.Rad2Deg * t)) * _previousRotation;
            
            //Let's calculate the difference in rotation and position and see if it's actually worth doing this.
            var transform1 = transform;
            float veloMagnitude = (pos-transform1.position).magnitude;
            //https://stackoverflow.com/questions/20798056/magnitude-of-rotation-between-two-quaternions
            float rotMagnitude = Mathf.Abs(Quaternion.Angle(transform1.rotation, rot));

            
            if (rotMagnitude > 1 && veloMagnitude > 0.01f) transform.SetPositionAndRotation(pos, rot); 
            else if (rotMagnitude > 1) transform.rotation = rot;
            else if (veloMagnitude > 0.01f) transform.position = pos;

           
        }
    }
}
