using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace ld42jam.CCLambda
{
    //----------------------------------------------------------------------------------------------------------------------------------------
    public class Menu : UIBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------
        [SerializeField]
        private GameObject title;
        [SerializeField]
        private GameObject btnStart;
        [SerializeField]
        private GameObject btnQuit;
        [SerializeField]
        private GameObject intro;
        [SerializeField]
        private GameObject gameOver;

        [SerializeField]
        private Universe universe;

        //------------------------------------------------------------------------------------------------------------------------------------
        private volatile bool isBusy;
        private bool introWasShown;

        //------------------------------------------------------------------------------------------------------------------------------------
        protected override void Start()
        {
            base.Start();

            title.SetActive(true);
            btnStart.SetActive(true);
            intro.SetActive(false);

            universe.OnGameOver = OnStopGame;
            universe.gameObject.SetActive(false);

            btnQuit.SetActive(false);
            gameOver.SetActive(false);

            introWasShown = false;
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private static readonly string[] INTRO_TEXT =
        {
            "Ben 'Big' Bang - your oldest brother - has just created the Universe.\nSoon galaxies will spawn and fill the void",
            "Your older sister Gravity is already playing with the new toys,\nand so everything is very attracted to each other.",
            "Your task will be to push the spacetime itself apart\nand make room for new galaxies."
        };

        //------------------------------------------------------------------------------------------------------------------------------------
        public void OnStartGame()
        {
            StartCoroutine(StartGame());
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private IEnumerator StartGame()
        {
            while (isBusy)
            {
                yield return new WaitForSeconds(1.0f);
            }

            isBusy = true;

            title.SetActive(false);
            btnStart.SetActive(false);

            if (!introWasShown)
            {
                intro.SetActive(true);
                yield return null;

                Text introText = intro.GetComponentInChildren<Text>();
                for (int i = 0; i < INTRO_TEXT.Length; ++i)
                {
                    introText.text = INTRO_TEXT[i];
                    while (!Input.GetMouseButtonUp(0))
                    {
                        yield return null;
                    }
                    yield return null;
                }
                intro.SetActive(false);

                introWasShown = true;
            }

            universe.gameObject.SetActive(true);
            btnQuit.SetActive(true);

            isBusy = false;
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        public void OnStopGame()
        {
            StartCoroutine(StopGame());
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private IEnumerator StopGame()
        {
            while(isBusy)
            {
                yield return new WaitForSeconds(1.0f);
            }

            isBusy = true;

            universe.gameObject.SetActive(false);
            btnQuit.SetActive(false);

            gameOver.SetActive(true);

            Text gameOverText = gameOver.GetComponent<Text>();
            gameOverText.text = string.Format("Game Over\n\n{0}\nMax {1:0.000} Hz", universe.Score, universe.MaxRate);
            yield return null;

            while (!Input.GetMouseButtonUp(0))
            {
                yield return null;
            }
            yield return null;

            gameOver.SetActive(false);

            title.SetActive(true);
            btnStart.SetActive(true);

            isBusy = false;
        }
   }
}