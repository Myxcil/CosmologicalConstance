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
        private GameObject universe;

        //------------------------------------------------------------------------------------------------------------------------------------
        private bool isBusy;
        private bool introWasShown;

        //------------------------------------------------------------------------------------------------------------------------------------
        protected override void Start()
        {
            base.Start();
            title.SetActive(true);
            btnStart.SetActive(true);
            intro.SetActive(false);
            universe.SetActive(false);
            btnQuit.SetActive(false);
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
            if (isBusy)
                return;

            StartCoroutine(StartGame());
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        public void OnStopGame()
        {
            universe.SetActive(false);
            btnQuit.SetActive(false);

            title.SetActive(true);
            btnStart.SetActive(true);
        }

        //------------------------------------------------------------------------------------------------------------------------------------
        private IEnumerator StartGame()
        {
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

            universe.SetActive(true);
            btnQuit.SetActive(true);
        }
    }
}