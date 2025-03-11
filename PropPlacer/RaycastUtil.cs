using UnityEngine;

namespace PropPlacer
{
    public static class RaycastUtil
    {
        /// <summary>
        /// Returns true if it hit something
        /// </summary>
        /// <param name="position">Outputs the global position of the raycast spot</param>
        /// <param name="rotation">Outputs the rotation of what you are trying to place</param>
        /// <param name="normal">Outputs the normal vector of the raycast hit</param>
        /// <returns></returns>
        public static bool Raycast(out Vector3 position, out Quaternion rotation, out Vector3 normal, out GameObject hitObject)
        {
            var playerBody = Locator.GetPlayerBody();

            playerBody.DisableCollisionDetection();
            try
            {
                int layerMask = OWLayerMask.physicalMask;
                var origin = Locator.GetActiveCamera().transform.position;
                var direction = Locator.GetActiveCamera().transform.forward;

                var hit = Physics.Raycast(origin, direction, out var hitInfo, 100f, layerMask);
                if (hit)
                {
                    position = hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.point);
                    normal = hitInfo.rigidbody.transform.InverseTransformDirection(hitInfo.normal);

                    var toOrigin = Vector3.ProjectOnPlane((origin - hitInfo.point).normalized, hitInfo.normal);
                    var worldSpaceRot = Quaternion.LookRotation(toOrigin, hitInfo.normal);
                    rotation = hitInfo.rigidbody.transform.InverseTransformRotation(worldSpaceRot);

                    hitObject = hitInfo.rigidbody.gameObject;

                    playerBody.EnableCollisionDetection();

                    return true;
                }
            }
            catch { }
            playerBody.EnableCollisionDetection();

            position = Vector3.zero;
            rotation = Quaternion.identity;
            normal = Vector3.up;
            hitObject = null;

            return false;
        }
    }
}
