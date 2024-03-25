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
            //if(TryGetComponent(out NetworkTransform rt)) rt.enabled = false;
            _previousTime = Time.time;
        }

        protected override void OnOwnershipChanged(bool state)
        {
            //If we're not the owner and not the 
            _rb.isKinematic = !state;
            enabled = !state;
        }

        protected override int InitializeBuffers()
        {
            //Acceleration needs to be updated, though this can actually be updated once, we may aswell update it constantly because it's less work.
            // Velocity, Angular --?, and Accel
            return 6 * sizeof(float);
        }

        protected override void ReceiveUpdate(ref byte[] bytes, int idx)
        {
       
            //Unfortunately, we need to somehow 
            //rb.angularVelocity = bytes.ToVector3(idx);
            var transform1 = transform;
        
            _previousPosition = transform1.position;
            _previousRotation = transform1.rotation;
            float pt = _previousTime;
            _previousTime = Time.time;
            Vector3 prvvel = _previousVelocity;
            _previousVelocity = bytes.ToVector3(idx);
            _acceleration = (_previousVelocity - prvvel) / (_previousTime - pt);
            _previousAngularVelocity = bytes.ToVector3(idx + sizeof(float) * 3);
            //_previousAcceleration = bytes.ToVector3(idx + sizeof(float) * 6);
        
            // rb.position = bytes.ToVector3(idx + sizeof(float) * 6);
            //rb.rotation = bytes.ToQuaternion(idx + sizeof(float) * 9);

        }

        protected override void SendUpdate(ref byte[] bytes)
        {
            //rb.angularVelocity.ToBytes(ref bytes, 0);
            _rb.velocity.ToBytes(ref bytes, 0 );
            _rb.angularVelocity.ToBytes(ref bytes, 3 * sizeof(float));
            //_previousAcceleration.ToBytes(ref bytes, 6 * sizeof(float));
            //rb.position.ToBytes(ref bytes, sizeof(float) * 6);
            //rb.rotation.ToBytes(ref bytes, sizeof(float) * 9);
        }

    
        //x(t1) = x(t 0) + v*(t1 – t0) + (1/2)a*(t1 –t2) 2
        private void LateUpdate()
        {
            if (IsOwner) return;
            float t = Time.time - _previousTime;
            //Not plus equals, as we aren't actually trying to move

            //Delta velocity / Delta Time... but how do we 
            //Want to be able to factor acceleration...

            Vector3 pos = _previousPosition + _previousVelocity * t + _acceleration * (0.5f * t * t);
            
            //GPT helped me with this, I don't have enough physics experience. (the rotation calc)
            Quaternion rot = Quaternion.Euler(_previousAngularVelocity * (Mathf.Rad2Deg * t)) * _previousRotation;
            
            transform.SetPositionAndRotation(pos, rot); //+ 0.5f * (rb.velocity - previousVelocity) / t * t;
        }
    }
}
