using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IgnoreSolutions.DuelistsOfTheRoses.UI
{
    /// <summary>
    /// Title screen controller is a Menu System capable of 
    /// rolling back state. All it takes to add support for it is to
    /// - Have a CanvasGroup representing the active menu
    /// - Said CanvasGroup GameObject should also have a behaviour that
    ///       implements IAcceptsGenericInput
    /// </summary>
    public class TitleScreenController : MonoBehaviour, IAcceptsGenericInput
    {
        [SerializeField] CanvasGroup activeCanvas;
        [SerializeField] IAcceptsGenericInput _ActiveCanvasInputBehaviour;
        [SerializeField] Stack<CanvasGroup> previousCanvas;

        public bool _InterceptCancel { get => false; set { } }

        [SerializeField] CanvasGroup _ErrorCanvas;

        [SerializeField] CanvasGroup _LastActive;

        [SerializeField] CanvasGroup _LoadingScreen;
        [SerializeField] CanvasGroup _BackgroundScreen;
        [SerializeField] CanvasGroup _LastCanvasAlternative;

        [SerializeField] bool _AllowInput = false;

        [SerializeField] bool _EnableUnityEventSystemSendNavEvents = true;
        
        void Start()
        {
            Application.quitting += Application_quitting;

            //LateInit2();
            EventSystem.current.sendNavigationEvents =
                _EnableUnityEventSystemSendNavEvents;
        }

        void OnDisable()
        {
            Application.quitting -= Application_quitting;
            SceneManager.sceneLoaded -= LateInit;
        }

        private void LateInit2()
        {
            previousCanvas = new Stack<CanvasGroup>();

            if (activeCanvas != null)
            {
                _ActiveCanvasInputBehaviour = activeCanvas.GetComponent<IAcceptsGenericInput>();
                Debug.Log($"[TitleScreenController] Setting Current Input Screen");
                Invoke(nameof(DelayedHandoff), .05f);
            }
        }

        private void LateInit(Scene scene, LoadSceneMode mode)
        {
            if (_LoadingScreen != null) DontDestroyOnLoad(_LoadingScreen);

            LateInit2();

            SceneManager.sceneLoaded -= LateInit;
        }

        public void BeginScene(int sceneHandle)
        {
            BeginScene(sceneHandle, fadeOnEvent: false);
        }

        /// <summary>
        /// Starts a coroutine that fades in the loading screen,
        /// then asynchronously loads a scene after setting up
        /// a handler event. 
        /// </summary>
        /// <param name="sceneHandle"></param>
        public void BeginScene(int sceneHandle, bool fadeOnEvent = false)
        {
            Debug.Log($"[TitleScreenController] Beginning scene with handle {sceneHandle}");
            //_LoadingScreenLogic._PreviousScene = DORScenes.TitleScene;
            StartCoroutine(FadeScreenBeginScene(sceneHandle, fadeOnEvent));
        }

        public IEnumerator PushCanvas_Fade(CanvasGroup canvas)
        {
            _AllowInput = false;
            _ActiveCanvasInputBehaviour = canvas.GetComponent<IAcceptsGenericInput>();
            SetCanvasState(canvas, false);

            // Fade Current Screen
            for(float f = 1.0f; f >= 0; f -= .56f * Time.deltaTime)
            {
                activeCanvas.alpha = f;
                yield return null;
            }
            SetCanvasState(activeCanvas);
            previousCanvas.Push(activeCanvas);
            activeCanvas = canvas;

            yield return new WaitForSeconds(1.5f);

            canvas.GetComponent<Canvas>().enabled = true;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            // Fade Next Screen In
            for(float f = 0.0f; f <= 1.0f; f += .50f * Time.deltaTime)
            {
                canvas.alpha = f;
                
                yield return null;
            }
            SetCanvasState(canvas, true);

            Invoke(nameof(DelayedHandoff), .05f);

            yield return null;
            _AllowInput = true;
        }

        IEnumerator FadeScreenBeginScene(int sceneHandle, bool fadeOnEvent = false)
        {
            _AllowInput = false;
            //yield return _LoadingScreenLogic.FadeMusicRoutine();
            // Fade out active canvas
            for(float f = 0; f < 1.0f; f += 1.25f * Time.deltaTime)
            {
                activeCanvas.alpha = 1 - f;
                _BackgroundScreen.alpha = 1 - f;
                yield return null;
            }

            // Setup loading screen
            // Fade in loading screen.
                _LoadingScreen.alpha = 0;
                _LoadingScreen.gameObject.SetActive(true);
                for (float f = 0; f < 1.0f; f += 1.25f * Time.deltaTime)
                {
                    _LoadingScreen.alpha = f;
                    yield return null;
                }

            // Setup callback for loading screen.
            //_LoadingScreenLogic.NextFadeOnEvent = fadeOnEvent;
            //SceneManager.sceneLoaded += _LoadingScreenLogic.Handle_SceneManagerSceneLoaded;

            SceneManager.LoadSceneAsync(sceneHandle);
            
        }

        public void PushErrorMessage(string header, string message)
        {
            var panel = _ErrorCanvas.transform.Find("Panel");
            var headerTextGo = panel.transform.Find("Header");
            var msgTextGo = panel.transform.Find("ErrorText");

            Text headerText = headerTextGo.GetComponentInChildren<Text>();
            Text msgText = msgTextGo.GetComponent<Text>();

            headerText.text = header;
            msgText.text = message;

            PushCanvas(_ErrorCanvas);
        }

        /// <summary>
        /// Attempts to push the last canvas found in the _LastCanvas variable.
        ///
        /// If fails or the value is incorrect, the fallback will be used.
        /// </summary>
        /// <param name="fallback"></param>
        public void PushLastCanvas(CanvasGroup fallback, bool saveLoaded = false)
        {
            if(saveLoaded)
            {
                PushCanvas(fallback);
            }

            if(PushLastCanvasOnly() == false)
            {
                PushCanvas(fallback);
            }

            string msg = HasErrorMessageToDisplay();
            if(msg != null)
            {
                PushErrorMessage("Client Error", $"Message: {msg}\n\n\nThis usually means the server you want to join is either\nA. Not port forwarded.\nB. Not hosting.");
            }
        }

        protected internal string HasErrorMessageToDisplay()
        {
            GameObject foundMsg = null;
            if( (foundMsg = GameObject.FindGameObjectWithTag("TitleFlag")) != null)
            {
                string msg = foundMsg.name;

                Destroy(foundMsg);
                // We have the message...
                return msg;
            }

            return null;
        }

        protected internal bool PushLastCanvasOnly()
        {
            return false;
            //if (!string.IsNullOrEmpty(_LastActiveSceneID.p_Value))
            //{
            //    var lastO = transform.Find(_LastActiveSceneID.p_Value);
            //    if(lastO != null)
            //    {
            //        _LastActiveSceneID.p_Value = null;
            //        PushCanvas(lastO.GetComponent<CanvasGroup>());
            //        return true;
            //    }
            //    else
            //    {
            //        throw new Exception($"Could not find last CanvasGroup given instance ID {_LastActiveSceneID.p_Value}");
            //    }
            //}

            //return false;
        }

        /// <summary>
        /// Pushes a CanvasGroup to be active and accepting input.
        /// The current active canvas is disabled and pushed down into
        /// a Queue.
        /// </summary>
        public void PushCanvas(CanvasGroup canvas)
        {
            _ActiveCanvasInputBehaviour = canvas.GetComponent<IAcceptsGenericInput>();

            Debug.Log($"Disabling {activeCanvas.name}", activeCanvas);
            SetCanvasState(activeCanvas, false);
            previousCanvas.Push(activeCanvas);
            activeCanvas = canvas;

            Debug.Log($"Enabling {activeCanvas.name}", activeCanvas);
            SetCanvasState(activeCanvas, true);

            Invoke(nameof(DelayedHandoff), .05f);
        }

        public void PushCanvasAdditive(CanvasGroup canvas)
        {
            _ActiveCanvasInputBehaviour = canvas.GetComponent<IAcceptsGenericInput>();

            Debug.Log($"Disabling {activeCanvas.name} without hiding it.", activeCanvas);
            SetCanvasState(activeCanvas, false, true);
            previousCanvas.Push(activeCanvas);
            activeCanvas = canvas;

            SetCanvasState(activeCanvas, true);

            Invoke(nameof(DelayedHandoff), .05f);
        }

        public void PushComponent(CanvasGroup component)
        {
            _ActiveCanvasInputBehaviour = component.GetComponent<IAcceptsGenericInput>();
            Debug.Log($"Setting active component to {component.name}");
            previousCanvas.Push(activeCanvas);
            activeCanvas = component;

            Invoke(nameof(DelayedHandoff), .05f);
        }

        private void DelayedHandoff()
        {
            _ActiveCanvasInputBehaviour?.OnInputHandoff();
        }

        /// <summary>
        /// If a previous canvas exists in the queue,
        ///    - The current active canvas is disabled.
        ///    - The top one is pulled to be the activeCanvas
        ///    - Once the new active canvas has been determined, it 
        ///      is enabled and set to the Active Input Behaviour
        /// </summary>
        public void PreviousGroup()
        {
            // No previous state to go back to.
            if (previousCanvas.Count <= 0) return;

            Selectable hasText = null;
            if ((hasText = activeCanvas.GetComponent<Selectable>()) != null)
            {
                Debug.Log($"Deselecting the InputField {hasText.name}");
                hasText.OnDeselect(new BaseEventData(EventSystem.current));
            }

            SetCanvasState(activeCanvas, enabled: false, noAlpha: false, isControl: hasText != null);
            activeCanvas = previousCanvas.Pop();
            SetCanvasState(activeCanvas, true);

            _ActiveCanvasInputBehaviour = activeCanvas.GetComponent<IAcceptsGenericInput>();

            _ActiveCanvasInputBehaviour.OnInputHandoff();
        }

        /// <summary>
        /// - Enables the canvas
        /// - Sets CanvasGroup alpha
        /// - Sets or disables blocksRaycasts on the CanvasGroup.
        /// </summary>
        private void SetCanvasState(CanvasGroup group, bool enabled = false, bool noAlpha = false, bool isControl = false)
        {
            Canvas _optionalCanvas = group.GetComponent<Canvas>();

            if (enabled)
            {
                if (_optionalCanvas && !noAlpha)
                {
                    group.GetComponent<Canvas>().enabled = true;
                    group.alpha = 1f;
                    group.blocksRaycasts = true;
                    group.interactable = true;
                }
            }
            else
            {
                if (_optionalCanvas && !noAlpha)
                {
                    if(!isControl) group.alpha = 0f;

                    group.blocksRaycasts = false;
                    group.GetComponent<Canvas>().enabled = false;
                    group.interactable = false;
                }
                else group.alpha = 1.0f;

            }

            
        }

        /// <summary>
        /// Translate Input translates input to the
        /// Active Canvas Behaviour while also listening for a 
        /// cancel event.
        /// </summary>
        public void TranslateInput(int type, int playerIndex)
        {
            if (_AllowInput == false) return;


            // TODO: Mechanism

            // Roll back to previous menu
            //if (type == type.Cancel
            //    && ((_ActiveCanvasInputBehaviour == null) ? true : !_ActiveCanvasInputBehaviour._InterceptCancel))
            //{
            //    PreviousGroup();
            //    //SoundManager.p_Instance?.PlaySoundEffect(Vector3.zero, "SelectionCancel");
            //    return;
            //}

            // Pass input down to the Active Canvas Input Behaviour
            if (_ActiveCanvasInputBehaviour != null)
            {
                _ActiveCanvasInputBehaviour.TranslateInput(type, playerIndex);
            }
        }

        public void BeginGame()
        {        }

        public void OnInputHandoff() { }

        public void HandleExitGame()
        {
            if (Application.isEditor == false
                && !Application.isMobilePlatform)
            {
                Invoke(nameof(DelayedExit), 1.5f);
            }
        }

        void DelayedExit()
        {
            Application.Quit();
        }

        private void Application_quitting()
        {        }
    }

}