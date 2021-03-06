using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IgnoreSolutions.PsychSodoku
{
    public class CongratulationsScreenController : MonoBehaviour
    {
        [SerializeField] Animator _Animator;
        [SerializeField] Image CompletedArt;
        [SerializeField] TMP_Text _Text;
        [SerializeField] GameTimeManager _GameTime;
        string _StringFormat;

        public void Start()
        {
            _StringFormat = _Text.text;
        }

        public void SetBoardImage(Sprite spr)
        {
            CompletedArt.sprite = spr;
        }

        public void HideCongratulationsScreen()
        {
            _Animator.ResetTrigger("Hide");
            _Animator.SetTrigger("Hide");
        }

        public void ShowCongratulationsScreen()
        {
            TimeSpan playTime = _GameTime.GetPlayTime();
            _Text.text = string.Format(_StringFormat, $"{playTime.Minutes}:{playTime.Seconds.ToString("00")}");

            _Animator.ResetTrigger("Hide");
            _Animator.ResetTrigger("Show");
            _Animator.SetTrigger("Show");
        }
    }
}