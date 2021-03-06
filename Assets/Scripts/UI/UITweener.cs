﻿/*
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
    public class UITweener : MonoBehaviour
    {
        public enum AnimationType
        {
            Move,
            Scale,
            ScaleX,
            ScaleY,
            Fade
        }

        public delegate void UITweenerComplete();
        public event UITweenerComplete OnComplete;

        #region Variables

        [Tooltip("Gameobject will default to component gameobject if left empty.")]
        public GameObject objectToAnimate;

        public AnimationType animationType;
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

        public bool startOnEnable;
        public bool stopOnDisable;

        bool reversed = false;
        bool disableOnComplete = false;

        CanvasGroup canvasGroup;
        RectTransform rect;

        public bool debugLogs;
		#endregion

		void Awake()
		{
            canvasGroup = objectToAnimate.GetComponent<CanvasGroup>();
            rect = objectToAnimate.GetComponent<RectTransform>();
		}

		void OnEnable()
        {
            if (startOnEnable)
                DoTween();
        }

		void OnDisable()
        {
            if (stopOnDisable)
                StopTween();
        }

        public void DoTween(bool reverse, bool disableAfterTween)
        {
            StopTween();
            Reverse(reverse);
			HandleTween();
            disableOnComplete = disableAfterTween;
        }

        public void DoTween()
		{
            StopTween();
            if (reversed)
                Reverse(false);
            HandleTween();
            disableOnComplete = false;
        }

        public void DoTween(bool reverse)
		{
            StopTween();
            Reverse(reverse);
            HandleTween();
            disableOnComplete = false;
		}

        public void StopTween()
        {
            if (_tweenObject != null)
            {
                LeanTween.cancel(_tweenObject.id);
                _tweenObject = null;
            }
        }

        void HandleTween()
        {
            if (rect == null)
                rect = objectToAnimate.GetComponent<RectTransform>();

            switch (animationType)
            {
                case AnimationType.Fade:
                    Fade();
                    break;
                case AnimationType.Move:
                    MoveAbsolute();
                    break;
                case AnimationType.Scale:
                    Scale();
                    break;
                case AnimationType.ScaleX:
                    ScaleX();
                    break;
                case AnimationType.ScaleY:
                    ScaleY();
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
            _tweenObject.setOnComplete(() => {
                OnComplete?.Invoke();
                _tweenObject = null;
                if (disableOnComplete)
                {
                    disableOnComplete = false;
                    objectToAnimate.SetActive(false);
                }
            });
        }

        void Fade()
        {
            if (canvasGroup == null)
                canvasGroup = objectToAnimate.AddComponent<CanvasGroup>();
            canvasGroup.alpha = from.x;
            _tweenObject = LeanTween.alphaCanvas(canvasGroup, to.x, duration);
        }

        void MoveAbsolute()
        {
            rect.anchoredPosition = new Vector2(from.x, from.y);
            _tweenObject = LeanTween.move(rect, to, duration);
        }

        void Scale()
        {
            rect.localScale = from;
            _tweenObject = LeanTween.scale(objectToAnimate, to, duration);
        }

        void ScaleX()
        {
            rect.localScale = new Vector3(from.x, rect.localScale.y, rect.localScale.z);
            _tweenObject = LeanTween.scale(objectToAnimate, new Vector3(to.x, rect.localScale.y, rect.localScale.z), duration);
        }

        void ScaleY()
        {
            rect.localScale = new Vector3(rect.localScale.x, from.y, rect.localScale.z);
            _tweenObject = LeanTween.scale(objectToAnimate, new Vector3(rect.localScale.x, to.y, rect.localScale.z), duration);
        }

        void Reverse(bool reverse)
		{
			if (reverse)
			{
				if (!reversed)
					SwapDirection();
			}
			else if (reversed)
			{
				SwapDirection();
			}
		}

        void SwapDirection()
        {
            Vector3 temp = from;
            from = to;
            to = temp;
            reversed = !reversed;
        }
    }
}