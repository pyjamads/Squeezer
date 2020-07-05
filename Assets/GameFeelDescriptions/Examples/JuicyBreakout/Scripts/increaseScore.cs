using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameFeelDescriptions.Examples
{
    public class increaseScore : MonoBehaviour
    {
        public int scorePerBlock = 3;

        private int currentScore;

        void Start()
        {
            currentScore = 0;
        }

        public void IncreaseScore()
        {
            currentScore += scorePerBlock;

            var text = GetComponent<Text>();

            text.text = "" + currentScore;
        }
    }
}
