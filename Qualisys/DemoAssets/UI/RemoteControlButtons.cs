// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace QualisysRealTime.Unity
{
    
    class RemoteControlButtons : MonoBehaviour
    {
        private class ReleaseControlOnDispose : IDisposable
        {
            static void DiscardResult(QtmCommandResult _) { }
            public void Dispose()
            {
                RTClient.GetInstance().SendReleaseControl(DiscardResult);
            }
        }
        private class RemoteOperation 
        {
            bool complete = false;
            public void OnResult(QtmCommandResult result) 
            {
                complete = true;
                Result = result;
            }
            public IEnumerator Await() 
            {
                while (!complete) {
                    yield return null;
                }
            }
            public QtmCommandResult Result { get; private set; }
        }
#pragma warning disable CS0649
        public Button CaptureButton;
        public Button StopButton;
        public Button SaveButton;
        public InputField FileNameInput;
        public InputField PasswordField;
        public Text StatusText;
#pragma warning disable CS0649


        public enum State 
        { 
            Unknown,
            SendingCommand,
            CaptureStarted,
            CaptureStopped,
            CaptureSaved,
            Error,
        }

        State state = State.Unknown;


        private void Awake()
        {
            if (CaptureButton != null) 
            {
                CaptureButton.onClick.AddListener(()=>StartCoroutine(StartCapture()));
            }
            
            if (StopButton != null)
            {
                StopButton.onClick.AddListener(() => StartCoroutine(StopCapture()));
            }
            
            if (SaveButton != null)
            {
                SaveButton.onClick.AddListener(() => StartCoroutine(SaveCapture()));
            }

            if (FileNameInput != null) 
            {
                FileNameInput.onValueChanged.AddListener(x => PlayerPrefs.SetString("rt_file_ouptut", x));
                FileNameInput.text = PlayerPrefs.GetString("rt_file_ouptut", "UnnamedRecording.qtm");
            }
            if (PasswordField != null)
            {
                PasswordField.onValueChanged.AddListener(x => PlayerPrefs.SetString("rt_master_password", x));
                PasswordField.text = PlayerPrefs.GetString("rt_master_password", "gait1");
            }
            ResetStatusText();
            UpdateEnabledState();
        }

        private void Update()
        {
            if (RTClient.GetInstance().ConnectionState != RTConnectionState.Connected) 
            {
                state = State.Error;
                StopAllCoroutines();
            }

            if (state != State.SendingCommand)
            {
                UpdateEnabledState();
            }
        }

        void UpdateEnabledState() 
        {
            if (state == State.SendingCommand || RTClient.GetInstance().ConnectionState != RTConnectionState.Connected)
            {
                if (CaptureButton != null)
                {
                    CaptureButton.interactable = false;
                }
                if (StopButton != null)
                {
                    StopButton.interactable = false;
                }
                if (SaveButton != null)
                {
                    SaveButton.interactable = false;
                }
            }
            else 
            { 
                switch (state) 
                {
                    case State.CaptureStarted:
                        if (CaptureButton != null)
                        {
                            CaptureButton.interactable = false;
                        }
                        if (StopButton != null)
                        {
                            StopButton.interactable = true;
                        }
                        if (SaveButton != null)
                        {
                            SaveButton.interactable = false;
                        }
                        break;
                    case State.CaptureStopped:
                        if (CaptureButton != null)
                        {
                            CaptureButton.interactable = true;
                        }
                        if (StopButton != null)
                        {
                            StopButton.interactable = false;
                        }
                        if (SaveButton != null)
                        {
                            SaveButton.interactable = true;
                        }
                        break;
                    case State.CaptureSaved:
                    case State.Unknown:
                    case State.Error:
                
                        if (CaptureButton != null)
                        {
                            CaptureButton.interactable = true;
                        }
                        if (StopButton != null)
                        {
                            StopButton.interactable = false;
                        }
                        if (SaveButton != null)
                        {
                            SaveButton.interactable = false;
                        }
                        break;
                    case State.SendingCommand:
                    default:
                        throw new NotImplementedException(state.ToString());
                }
            }
        }

        void SetStatusText(QtmCommandResult result) 
        {
            if (StatusText != null)
            {
                if (!result.Success)
                {
                    StatusText.text = "Failed to " + result.CommandName + "\n(" + result.Message + ")";
                }
                else
                {
                    StatusText.text = result.CommandName + "\n(OK)";
                }
            }
        }

        void ResetStatusText() 
        {
            if (StatusText != null)
            {
                StatusText.text = "";
            }
        }


        IEnumerator AwaitTakeControl() 
        {
            ResetStatusText();
            string password = GetPassword();
            RemoteOperation o = new RemoteOperation();
            RTClient.GetInstance().SendTakeControl(password, o.OnResult);
            yield return o.Await();
            if (!o.Result.Success)
            {
                SetStatusText(o.Result);
                state = State.Error;
                yield break;
            }
        }

        IEnumerator AwaitNewmeasurement()
        {
            ResetStatusText();
            RemoteOperation o = new RemoteOperation();
            RTClient.GetInstance().SendNewMeasurement(o.OnResult);
            yield return o.Await();
            if (!o.Result.Success)
            {
                SetStatusText(o.Result);
                state = State.Error;
                yield break;
            }
        }
        IEnumerator AwaitStartNewCapture()
        {
            ResetStatusText();
            RemoteOperation o = new RemoteOperation();
            RTClient.GetInstance().SendStartCapture(o.OnResult);
            yield return o.Await();
            if (!o.Result.Success)
            {
                SetStatusText(o.Result);
                state = State.Error;
                yield break;
            }
        }
        IEnumerator AwaitStopCapture()
        {
            ResetStatusText();
            RemoteOperation o = new RemoteOperation();
            RTClient.GetInstance().SendStop(o.OnResult);
            yield return o.Await();
            if (!o.Result.Success)
            {
                SetStatusText(o.Result);
                state = State.Error;
                yield break;
            }
        }

        IEnumerator AwaitSave()
        {
            ResetStatusText();
            RemoteOperation o = new RemoteOperation();
            RTClient.GetInstance().SendSaveFile(GetFileName(), o.OnResult);
            yield return o.Await();
            if (!o.Result.Success)
            {
                SetStatusText(o.Result);
                state = State.Error;
                yield break;
            }
        }
        static void DiscardResult(QtmCommandResult _) { }
        IEnumerator StartCapture()
        {
            if (state == State.SendingCommand)
            {
                yield break;
            }
            state = State.SendingCommand;

            yield return AwaitTakeControl();
            if (state == State.Error)
            {
                yield break;
            }
            using (var _ = new ReleaseControlOnDispose()) 
            { 
                RTClient.GetInstance().SendCloseFile(DiscardResult);
                yield return AwaitNewmeasurement();
                if (state == State.Error)
                {
                    yield break;
                }

                yield return AwaitStartNewCapture();
                if (state == State.Error)
                {
                    yield break;
                }
            }
            state = State.CaptureStarted;
            UpdateEnabledState();
        }

        IEnumerator StopCapture()
        {
            if (state == State.SendingCommand)
            {
                yield break;
            }
            state = State.SendingCommand;

            yield return AwaitTakeControl();
            if (state == State.Error)
            {
                yield break;
            }
            
            using (var _ = new ReleaseControlOnDispose()) 
            { 
                yield return AwaitStopCapture();
                if (state == State.Error)
                {
                    yield break;
                }
            }
            state = State.CaptureStopped;
            UpdateEnabledState();
        }

        IEnumerator SaveCapture()
        {
            if (state == State.SendingCommand)
            {
                yield break;
            }
            state = State.SendingCommand;

            yield return AwaitTakeControl();
            if (state == State.Error)
            {
                yield break;
            }
            using (var _ = new ReleaseControlOnDispose()) 
            { 
                RTClient.GetInstance().SendStop(DiscardResult);
                yield return AwaitSave();
                if (state == State.Error)
                {
                    yield break;
                }
            }
            state = State.CaptureSaved;
            UpdateEnabledState();
        }

        private string GetPassword() 
        {
            if (PasswordField == null) 
            {
                return "gait1";
            }
            return PasswordField.text;
        }

        private string GetFileName()
        {
            if (FileNameInput == null) 
            {
                return "UnnamedCapture.qtm";
            }
            return FileNameInput.text;
        }
    }
}

