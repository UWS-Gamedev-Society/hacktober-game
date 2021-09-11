using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines
{
    [AddComponentMenu("Dreamteck/Splines/Users/Spline Positioner")]
    public class SplinePositioner : SplineTracer
    {
        public enum Mode { Percent, Distance }
        
        public GameObject targetObject
        {
            get
            {
                if (_targetObject == null) return gameObject;
                return _targetObject;
            }

            set
            {
                if (value != _targetObject)
                {
                    _targetObject = value;
                    RefreshTargets();
                    Rebuild();
                }
            }
        }

        public double position
        {
            get
            {
                return _result.percent;
            }
            set
            {
                if (value != _position)
                {
                    _position = (float)value;
                    if (mode == Mode.Distance)
                    {
                        SetDistance(_position, true);
                    }
                    else
                    {
                        SetPercent(value, true);
                    }
                }
            }
        }

        public Mode mode
        {
            get { return _mode;  }
            set
            {
                if (value != _mode)
                {
                    _mode = value;
                    Rebuild();
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private GameObject _targetObject;
        [SerializeField]
        [HideInInspector]
        private float _position = 0f;
        [SerializeField]
        [HideInInspector]
        private Mode _mode = Mode.Percent;
        private float _lastPosition = 0f;

        protected override void OnDidApplyAnimationProperties()
        {
            if (_lastPosition != _position)
            {
                _lastPosition = _position;
                if (mode == Mode.Distance)
                {
                    SetDistance(_position, true);
                }
                else
                {
                    SetPercent(_position, true);
                }
            }
            base.OnDidApplyAnimationProperties();
        }

        protected override Transform GetTransform()
        {
            return targetObject.transform;
        }

        protected override Rigidbody GetRigidbody()
        {
            return targetObject.GetComponent<Rigidbody>();
        }

        protected override Rigidbody2D GetRigidbody2D()
        {
            return targetObject.GetComponent<Rigidbody2D>();
        }

        protected override void PostBuild()
        {
            base.PostBuild();
            if (mode == Mode.Distance) SetDistance((float)_position, true);
            else SetPercent(_position, true);
        }

        public override void SetPercent(double percent, bool checkTriggers = false, bool handleJuncitons = false)
        {
            base.SetPercent(percent, checkTriggers, handleJuncitons);
            _position = (float)percent;
        }

        public override void SetDistance(float distance, bool checkTriggers = false, bool handleJuncitons = false)
        {
            base.SetDistance(distance, checkTriggers, handleJuncitons);
            _position = distance;
        }
    }
}
