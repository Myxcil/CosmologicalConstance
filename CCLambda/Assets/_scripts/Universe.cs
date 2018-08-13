using UnityEngine;
using System.Collections.Generic;

namespace ld42jam.CCLambda
{
    //----------------------------------------------------------------------------------------------------------------------------------------
    public class Universe : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------
        [SerializeField]
        private Material matGalaxy;
        [SerializeField]
        private float pushValue = 1.0f;
        [SerializeField]
        private float velocityDiffusion = 0.96f;
        [SerializeField]
        private float mergeGalaxyFactor = 0.2f;
        [SerializeField]
        private float galaxyCollapseSize = 12.0f;
        [SerializeField]
        private float diameterScoreScale = 0.1f;

        [SerializeField]
        private UnityEngine.UI.Text txtScore;
        [SerializeField]
        private AudioSource asPush;
        [SerializeField]
        private AudioSource asMerge;
        [SerializeField]
        private GameObject clickFX;

        //------------------------------------------------------------------------------------------------------------------------------------
        private List<Galaxy> galaxies;
        private int maxGalaxies;

        private List<int> deleteList;

        private bool pusherActive;
        private Vector3 pusherPosition;

        private float nextGalaxySpawn;

        private float score;
        public int Score { get { return (int)score; } }

        public float MaxRate { get; private set; }

        //------------------------------------------------------------------------------------------------------------------------------------
        public delegate void GameOver();
        public GameOver OnGameOver = null;

        //------------------------------------------------------------------------------------------------------------------------------------
        private void Awake()
        {
            galaxies = new List<Galaxy>();
            deleteList = new List<int>();
            clickFX.SetActive(false);
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private void OnEnable()
        {
            deleteList.Clear();
            pusherActive = false;
            nextGalaxySpawn = 0.5f;

            clickFX.SetActive(false);

            score = 0;
            MaxRate = 0;

            txtScore.text = string.Format("{0} (d={1:0.000})", 0, 0.0f);
            txtScore.gameObject.SetActive(true);
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private void OnDisable()
        {
            if (clickFX != null)
            {
                clickFX.SetActive(false);
            }

            if (txtScore != null && txtScore.gameObject != null)
            {
                txtScore.gameObject.SetActive(false);
            }

            for (int i = 0; i < galaxies.Count; ++i)
            {
                galaxies[i].OnDestroy();
            }
            galaxies.Clear();
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private void Update()
        {
            if (nextGalaxySpawn > 0)
            {
                nextGalaxySpawn -= Time.deltaTime;
            }
            else
            {
                SpawnGalaxy();
                nextGalaxySpawn = GetNextGalaxyCountdown();
            }

            bool wasActive = pusherActive;
            if (Input.GetMouseButton(0))
            {
                pusherActive = GetClickPosition(out pusherPosition);
                if (pusherActive)
                {
                    clickFX.transform.position = pusherPosition;
                }
            }
            else
            {
                pusherActive = false;
            }
            if (!wasActive && pusherActive)
            {
                asPush.Play();
                clickFX.SetActive(true);
            }
            else if (wasActive && !pusherActive)
            {
                asPush.Stop();
                clickFX.SetActive(false);
            }

            for (int i = 0; i < galaxies.Count - 1; ++i)
            {
                for (int j = i + 1; j < galaxies.Count; ++j)
                {
                    if (Galaxy.UpdateVelocities(galaxies[i], galaxies[j], mergeGalaxyFactor, i, j, deleteList))
                    {
                        asMerge.Play();
                    }
                }
            }

            float totalSize = 0;
            for (int i = 0; i < galaxies.Count; ++i)
            {
                if (pusherActive)
                {
                    galaxies[i].ApplyPush(pusherPosition, pushValue);
                }
                galaxies[i].Update(Time.deltaTime, velocityDiffusion, ref totalSize, galaxyCollapseSize);
            }

            if (deleteList.Count > 0)
            {
                for (int i = 0; i < deleteList.Count; ++i)
                {
                    int index = deleteList[i];
                    if (index >= 0 && index < galaxies.Count)
                    {
                        galaxies[index].OnDestroy();
                        galaxies.RemoveAt(index);
                    }
                }
                deleteList.Clear();
            }
            maxGalaxies = Mathf.Max(maxGalaxies, galaxies.Count);

            int oldScore = (int)score;
            float deltaScore = diameterScoreScale * totalSize * Mathf.Max(0, galaxies.Count - 1);
            score += deltaScore * Time.deltaTime;
            int newScore = (int)score;
            if (oldScore != newScore)
            {
                txtScore.text = string.Format("{0} (d={1:0.000} Hz)", newScore, deltaScore);
            }
            MaxRate = Mathf.Max(MaxRate, deltaScore);

            if (galaxies.Count == 1 && maxGalaxies > 1)
            {
                if (galaxies[0].IsGreaterOrEqual(maxGalaxies))
                {
                    OnGameOver.Invoke();
                }
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private void SpawnGalaxy()
        {
            float galaxySize = Random.Range(0.7f, 1.3f);

            int numTries = 8;
            while (numTries-- > 0)
            {
                Vector3 screenPos = Vector3.zero;
                screenPos.x = Random.Range(0, Screen.width);
                screenPos.y = Random.Range(0, Screen.height);

                Vector3 hitPoint;
                if (GetClickPosition(screenPos, out hitPoint))
                {
                    if (IsFree(hitPoint, 0.5f * galaxySize))
                    {
                        Galaxy galaxy = CreateGalaxy(galaxySize, hitPoint);
                        galaxies.Add(galaxy);
                        StartCoroutine(galaxy.Spawn());
                        break;
                    }
                }
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private float GetNextGalaxyCountdown()
        {
            return Random.Range(2.0f, 5.0f);
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private bool IsFree(Vector3 pos, float radius)
        {
            for (int i = 0; i < galaxies.Count; ++i)
            {
                if (galaxies[i].Intersect(pos, radius))
                    return false;
            }
            return true;
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private static bool GetClickPosition(out Vector3 hitPoint)
        {
            return GetClickPosition(Input.mousePosition, out hitPoint);
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private static bool GetClickPosition(Vector3 screenPoint, out Vector3 hitPoint)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPoint);

            Plane plane = new Plane(Vector3.up, 0);
            float enter;
            if (plane.Raycast(ray, out enter))
            {
                hitPoint = ray.GetPoint(enter);
                return true;
            }
            else
            {
                hitPoint = Vector3.zero;
                return false;
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private int galaxyIndex = 0;

        //------------------------------------------------------------------------------------------------------------------------------------
        private Galaxy CreateGalaxy(float size, Vector3 position)
        {
            GameObject galaxyObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            galaxyObject.name = string.Format("Galaxy_{0}", galaxyIndex++);

            MeshRenderer mr = galaxyObject.GetComponent<MeshRenderer>();
            mr.material = new Material(matGalaxy);

            Color color = Random.ColorHSV(0.5f, 1.0f, 0.3f, 0.6f, 0.7f, 1.0f, 1.0f, 1.0f);
            mr.material.SetColor("_TintColor", color);

            Transform tm = galaxyObject.transform;
            tm.position = position;
            tm.localRotation = Quaternion.Euler(90, 0, 0);
            tm.localScale = Vector3.one * size;

            return new Galaxy(galaxyObject, size);
        }
    }
}