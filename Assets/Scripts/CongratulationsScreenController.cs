using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IgnoreSolutions.PsychSodoku
{
    public class CongratulationsScreenController : MonoBehaviour
    {
        [SerializeField] Animator _Animator;
        [SerializeField] Image CompletedArt;
        [SerializeField] TMP_Text _Text;
        [SerializeField] ModifyShaderOffset _MainGame;
        [SerializeField] GameTimeManager _GameTime;
        [SerializeField] GameObject _NextButton;
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
            _MainGame._DontUpdate = false;
            _Animator.ResetTrigger("Hide");
            _Animator.SetTrigger("Hide");
        }

        public void SetNextButtonSelected()
        {
            if(_NextButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_NextButton);

                Button b = _NextButton.GetComponent<Button>();
                if (b != null) b.Select();
            }
        }

        public void ShowCongratulationsScreen()
        {
            _MainGame._DontUpdate = true;
            TimeSpan playTime = _GameTime.GetPlayTime();
            int levelIndex = SudokuParametersInjest.p_Instance.GetLevelIndex();
            _Text.text = string.Format(_StringFormat, levelIndex + 1, $"{playTime.Minutes}:{playTime.Seconds.ToString("00")}");

            _Animator.ResetTrigger("Hide");
            _Animator.ResetTrigger("Show");
            _Animator.SetTrigger("Show");
        }
    }
}