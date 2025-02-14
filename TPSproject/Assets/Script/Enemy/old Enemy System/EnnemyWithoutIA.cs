using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script
{
    public class EnnemyWithoutIA : MonoBehaviour
    {
        [SerializeField] private List<Vector3> listWaypoint;
        [SerializeField] private bool wayPointDone= false;
        [SerializeField] private float speed;

        private void Start()
        {
            
        }

        public enum EnnemyState
        {
            Walk,
            Idle
        }

        private void Update()
        {
            WalkingToWayPoints();
        }

        private void WalkingToWayPoints()
        {
          transform.position = Vector3.MoveTowards(transform.position,listWaypoint[0],Time.deltaTime*speed);
        }   
    }
}
