using UnityEngine;

namespace Script
{
    public class AutoDestruct : MonoBehaviour
    {
    
        void Start()
        {
            Destroy(gameObject,10f);
        }

    
    }
}
