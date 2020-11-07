/*
 ██████   █████   █████████   █████        █████████   █████                   █████  ███          
░░██████ ░░███   ███░░░░░███ ░░███        ███░░░░░███ ░░███                   ░░███  ░░░           
 ░███░███ ░███  ░███    ░███  ░███       ░███    ░░░  ███████   █████ ████  ███████  ████   ██████ 
 ░███░░███░███  ░███████████  ░███       ░░█████████ ░░░███░   ░░███ ░███  ███░░███ ░░███  ███░░███
 ░███ ░░██████  ░███░░░░░███  ░███        ░░░░░░░░███  ░███     ░███ ░███ ░███ ░███  ░███ ░███ ░███
 ░███  ░░█████  ░███    ░███  ░███      █ ███    ░███  ░███ ███ ░███ ░███ ░███ ░███  ░███ ░███ ░███
 █████  ░░█████ █████   █████ ███████████░░█████████   ░░█████  ░░████████░░████████ █████░░██████ 
░░░░░    ░░░░░ ░░░░░   ░░░░░ ░░░░░░░░░░░  ░░░░░░░░░     ░░░░░    ░░░░░░░░  ░░░░░░░░ ░░░░░  ░░░░░░       

Copyright © 2020 NALStudio. All Rights Reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NALStudio.UI
{
    public enum UIAnimationTypes
    {
        Move,
        Scale,
        ScaleX,
        ScaleY,
        Fade
    }

    public class UITweener : MonoBehaviour
    {
        #region Variables

        [Tooltip("Gameobject will default to component gameobject if left empty.")]
        public GameObject objectToAnimate;

        public UIAnimationTypes animationType;
        public LeanTweenType easeType;
        public float duration;
        public float delay;

        public bool loop;
        public bool pingpong;

        public bool startPositionOffset;
        public Vector3 from;
        public bool endPositionOffset;
        public Vector3 to;

        LTDescr _tweenObject;

        public bool showOnEnable;
        public bool workOnDisable;

        bool reversed = false;

        #endregion

        void OnEnable()
        {
            if (showOnEnable)
            {
                DoTween(false, false);
            }
        }

        void OnDisable()
        {
            if (!workOnDisable)
            {
                StopTween();
            }
        }

        public void DoTween(bool reverse = false, bool disableAfterTween = false)
        {
            if (reverse)
            {
                if (!reversed)
                {
                    SwapDirection();
                }
                if (reversed)
                {
                    SwapDirection();
                }
            }
            else
            {
                if (reversed)
                {
                    SwapDirection();
                }
            }
            HandleTween();
            if (disableAfterTween)
            {
                _tweenObject.setOnComplete(() => objectToAnimate.SetActive(false));
            }
        }

        public void StopTween()
        {
            LeanTween.cancel(objectToAnimate);
        }

        void HandleTween()
        {
            switch (animationType)
            {
                case UIAnimationTypes.Fade:
                    Fade();
                    break;
                case UIAnimationTypes.Move:
                    MoveAbsolute();
                    break;
                case UIAnimationTypes.Scale:
                    Scale();
                    break;
                case UIAnimationTypes.ScaleX:
                    Scale();
                    break;
                case UIAnimationTypes.ScaleY:
                    Scale();
                    break;
            }

            _tweenObject.setDelay(delay);
            _tweenObject.setEase(easeType);

            if (loop)
            {
                _tweenObject.setLoopClamp();
            }
            if (pingpong)
            {
                _tweenObject.setLoopPingPong();
            }
        }

        void Fade()
        {
            if (objectToAnimate.GetComponent<CanvasGroup>() == null)
            {
                objectToAnimate.AddComponent<CanvasGroup>();
            }

            objectToAnimate.GetComponent<CanvasGroup>().alpha = from.x;
            _tweenObject = LeanTween.alphaCanvas(objectToAnimate.GetComponent<CanvasGroup>(), to.x, duration);
        }

        void MoveAbsolute()
        {
            objectToAnimate.GetComponent<RectTransform>().anchoredPosition = from;
            _tweenObject = LeanTween.move(objectToAnimate.GetComponent<RectTransform>(), to, duration);
        }

        void Scale()
        {
            objectToAnimate.GetComponent<RectTransform>().localScale = from;
            _tweenObject = LeanTween.scale(objectToAnimate, to, duration);
        }

        void SwapDirection()
        {
            var temp = from;
            from = to;
            to = temp;
            reversed = !reversed;
        }
    }
}