using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace ld42jam.CCLambda
{
    //------------------------------------------------------------------------------------------------------------------------------------
    class Galaxy
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        private readonly GameObject gameObject;
        private float diameter;
        private readonly float speed;

        private Vector3 velocity;
        private float angle;

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool IsSpawning { get; private set; }

        //--------------------------------------------------------------------------------------------------------------------------------
        public Galaxy(GameObject go, float size)
        {
            gameObject = go;
            diameter = size;
            speed = Random.Range(3.0f, 5.0f) * diameter;
            angle = Random.Range(-180, 180);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private const float SPAWN_DURATION = 1.5f;
        private const float RCP_SPAWN_DURATION = 1.0f / SPAWN_DURATION;

        //--------------------------------------------------------------------------------------------------------------------------------
        public IEnumerator Spawn()
        {
            IsSpawning = true;
            float sizeStep = diameter * RCP_SPAWN_DURATION;
            float size = 0;
            while (size < diameter)
            {
                if (gameObject == null)
                    yield break;

                gameObject.transform.localScale = Vector3.one * size;
                yield return null;

                size += sizeStep * Time.deltaTime;
            }
            gameObject.transform.localScale = Vector3.one * diameter;
            IsSpawning = false;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public void ChangeSize(float diff)
        {
            diameter += diff;
            gameObject.transform.localScale = Vector3.one * diameter;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool IsGreaterOrEqual(float size)
        {
            return diameter >= size;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public void OnDestroy()
        {
            if (gameObject != null)
            {
                Object.Destroy(gameObject);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool UpdateVelocities(Galaxy galaxyA, Galaxy galaxyB, float mergeGalaxyFactor, int i, int j, List<int> deleteList)
        {
            if (galaxyA.IsSpawning || galaxyB.IsSpawning)
                return false;

            Vector3 diff = GetDifference(galaxyA, galaxyB);

            float dist = diff.magnitude;
            if (dist < 0.5f * (galaxyA.diameter + galaxyB.diameter))
            {
                if (galaxyA.diameter > galaxyB.diameter)
                {
                    deleteList.Add(j);
                    galaxyA.ChangeSize(mergeGalaxyFactor * galaxyB.diameter);
                }
                else
                {
                    deleteList.Add(i);
                    galaxyB.ChangeSize(mergeGalaxyFactor * galaxyA.diameter);
                }
                return true;
            }
            else
            {
                Vector3 dir = diff / dist;
                float ratio = galaxyB.diameter / galaxyA.diameter;

                Vector3 deltaV = dir / (dist * dist) * Time.deltaTime;
                galaxyA.velocity += deltaV * ratio;
                galaxyB.velocity -= deltaV / ratio;
            }

            return false;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private static Vector3 GetDifference(Galaxy galaxyA, Galaxy galaxyB)
        {
            Vector3 diff = galaxyB.gameObject.transform.position - galaxyA.gameObject.transform.position;
            diff.y = 0;
            return diff;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public void ApplyPush(Vector3 pusherPosition, float strength)
        {
            Vector3 diff = gameObject.transform.position - pusherPosition;
            diff.y = 0;

            float sqDist = diff.sqrMagnitude;
            sqDist = Mathf.Max(sqDist, 0.25f * diameter * diameter);
            diff.Normalize();
            diff.y = 0;

            velocity += diff * strength / sqDist * Time.deltaTime;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public void Update(float timeStep, float vDiffusion, ref float totalSize, float collapseSize)
        {
            gameObject.transform.position += velocity * timeStep;
            velocity *= vDiffusion;

            angle += speed * timeStep;
            if (angle > 180)
                angle -= 360;

            gameObject.transform.rotation = Quaternion.Euler(90, angle, 0);

            if (diameter >= (0.85f * collapseSize))
            {
                float speed = diameter / collapseSize;
                float wave = 0.975f + 0.05f * Mathf.Sin(angle * speed);
                gameObject.transform.localScale = diameter * wave * Vector3.one;
            }

            totalSize += diameter;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool Intersect(Vector3 position, float radius)
        {
            Vector3 diff = gameObject.transform.position - position;
            float minDist = 0.5f * diameter + radius;
            if (diff.sqrMagnitude < minDist * minDist)
                return true;

            return false;
        }
    }
}