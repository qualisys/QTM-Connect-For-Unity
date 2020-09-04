using System;
using System.Collections.Generic;
using System.Linq;
using QTMRealTimeSDK.Data;
using UnityEngine;

namespace QualisysRealTime.Unity
{

    internal class QtmCommandHandler
    {
        internal struct RtCommandCompleteTrigger
        {
            internal QTMEvent triggerEvent;
            internal string resultString;

            internal bool Matches(List<string> commandStrings, List<QTMEvent> events)
            {
                var resultString = this.resultString;
                var riggerEvent = this.triggerEvent;

                bool stringMatch = string.IsNullOrEmpty(resultString)
                    || commandStrings.FindIndex(x => x.StartsWith(resultString)) != -1;
            
                bool eventMatch = triggerEvent == QTMEvent.None
                    || events.Contains(triggerEvent);

                return stringMatch && eventMatch;
            }
        }

        internal interface ICommand
        {
            Action<QtmCommandResult> OnResult { get; }
            string CommandString { get; }
            RtCommandCompleteTrigger[] Triggers { get; }
        }

        internal struct CommandAndResultPair
        {
            internal ICommand command;
            internal QtmCommandResult result;
        }

        internal class QtmCommandAwaiter
        {
            const float timeout = 5;
            ICommand command;
            List<QTMEvent> events = new List<QTMEvent>();
            List<string> commandStrings = new List<string>();
            internal bool IsAwaiting { get; private set; } = false;
            public string CurrentCommand { get { return IsAwaiting ? command.CommandString : ""; } }

            DateTime start;

            internal void Await(ICommand command)
            {
                this.command = command;
                this.commandStrings.Clear();
                this.events.Clear();
                this.IsAwaiting = true;
                start = DateTime.Now;
            }

            internal void AppendEvent(QTMEvent qtmEvent)
            {
                //Debug.Log(command.GetType().Name + " response event: " + qtmEvent.ToString());
                events.Add(qtmEvent);
            }
            internal void AppendCommandString(string resultString)
            {
                //Debug.Log(command.GetType().Name + " response: " + resultString);
                commandStrings.Add(resultString);
            }

            
            
            internal bool TryGetResult(out CommandAndResultPair result)
            {
                var triggers = command.Triggers;
                
                //Look for error string
                for (int i = 0; i < commandStrings.Count; ++i)
                {
                    if (!triggers.Any(x => x.resultString == commandStrings[i]))
                    {
                        result = new CommandAndResultPair()
                        {
                            command = command,
                            result = new QtmCommandResult
                            {
                                Command = command.CommandString,
                                Success = false,
                                Message = commandStrings[i],
                                CommandName = command.GetType().Name,
                            }
                        };
                        IsAwaiting = false;
                        return true;
                    }
                }

                //Look for timeout
                var deltaTime = (DateTime.Now - start).TotalSeconds;
                if (deltaTime >= timeout)
                {
                    result = new CommandAndResultPair()
                    {
                        command = command,
                        result = new QtmCommandResult
                        {
                            Command = command.CommandString,
                            Success = false,
                            Message = "timeout",
                            CommandName = command.GetType().Name,
                        }
                    };
                    IsAwaiting = false;
                    return true;
                }
                
                //Look for match
                for (int i = 0; i < triggers.Length; ++i)
                {
                    if (triggers[i].Matches(commandStrings, events))
                    {
                        result = new CommandAndResultPair()
                        {
                            command = command,
                            result = new QtmCommandResult {
                                Command = command.CommandString,
                                Success = true,
                                Message = string.Empty,
                                CommandName = command.GetType().Name,
                            }
                        };
                        IsAwaiting = false;
                        return true;
                    }
                }

                result = default(CommandAndResultPair);
                return false;
            }

            internal void CancelAwait()
            {
                IsAwaiting = false;
            }
        }


        internal struct StartCapture : ICommand
        {
            static RtCommandCompleteTrigger[] trigger_cache = new RtCommandCompleteTrigger[]{
                 new RtCommandCompleteTrigger{ resultString = "Starting measurement", triggerEvent = QTMEvent.CaptureStarted },
                 new RtCommandCompleteTrigger{ resultString = "Measurement is already running", triggerEvent = QTMEvent.None },
            };
            public string CommandString { get => "Start"; }
            public RtCommandCompleteTrigger[] Triggers { get => trigger_cache; }
            public Action<QtmCommandResult> OnResult { get => onResult; }

            Action<QtmCommandResult> onResult;
            public StartCapture(Action<QtmCommandResult> onResult)
            {
                this.onResult = onResult;
            }
        }
        internal struct StartRtFromFile : ICommand
        {
            static RtCommandCompleteTrigger[] trigger_cache = new RtCommandCompleteTrigger[]{
                 new RtCommandCompleteTrigger{ resultString = "Starting measurement", triggerEvent = QTMEvent.CaptureStarted },
                 new RtCommandCompleteTrigger{ resultString = "Measurement is already running", triggerEvent = QTMEvent.None },
            };
            public string CommandString { get => "Start rtfromfile"; }
            public RtCommandCompleteTrigger[] Triggers { get => trigger_cache; }
            public Action<QtmCommandResult> OnResult { get => onResult; }

            Action<QtmCommandResult> onResult;
            public StartRtFromFile(Action<QtmCommandResult> onResult)
            {
                this.onResult = onResult;
            }
        }

        internal struct Stop : ICommand
        {
            static RtCommandCompleteTrigger[] trigger_cache = new RtCommandCompleteTrigger[]{
                 new RtCommandCompleteTrigger{ resultString = "Stopping measurement", triggerEvent = QTMEvent.None },
            };
            public string CommandString { get => "Stop"; }
            public RtCommandCompleteTrigger[] Triggers { get => trigger_cache; }
            public Action<QtmCommandResult> OnResult { get => onResult; }

            Action<QtmCommandResult> onResult;
            public Stop(Action<QtmCommandResult> onResult)
            {
                this.onResult = onResult;
            }
        }


        internal struct Close : ICommand
        {
            static RtCommandCompleteTrigger[] trigger_cache = new RtCommandCompleteTrigger[]{
                 new RtCommandCompleteTrigger{ resultString = "Closing connection", triggerEvent = QTMEvent.ConnectionClosed },
                 new RtCommandCompleteTrigger{ resultString = "No connection to close", triggerEvent = QTMEvent.None },
                 new RtCommandCompleteTrigger{ resultString = "File closed", triggerEvent = QTMEvent.None },
            };
            public string CommandString { get => "Close"; }
            public RtCommandCompleteTrigger[] Triggers { get => trigger_cache; }
            public Action<QtmCommandResult> OnResult { get => onResult; }
            Action<QtmCommandResult> onResult;
            public Close(Action<QtmCommandResult> onResult)
            {
                this.onResult = onResult;
            }
        }


        internal struct New : ICommand
        {
            static RtCommandCompleteTrigger[] trigger_cache = new RtCommandCompleteTrigger[]{
                new RtCommandCompleteTrigger{ resultString = "Creating new connection", triggerEvent = QTMEvent.Connected },
                new RtCommandCompleteTrigger{ resultString = "Already connected", triggerEvent = QTMEvent.None },
            };
            public string CommandString { get => "New"; }
            public RtCommandCompleteTrigger[] Triggers { get => trigger_cache; }
            public Action<QtmCommandResult> OnResult { get => onComplete; }
            Action<QtmCommandResult> onComplete;
            public New(Action<QtmCommandResult> onResult)
            {
                this.onComplete = onResult;
            }
        }

        internal struct Save : ICommand
        {
            static RtCommandCompleteTrigger[] trigger_cache = new RtCommandCompleteTrigger[]
            {
                new RtCommandCompleteTrigger{ resultString = "Measurement saved", triggerEvent = QTMEvent.CaptureSaved },
            };

            public string CommandString { get => "save " + fileName + " overwrite"; }
            public RtCommandCompleteTrigger[] Triggers { get => trigger_cache; }
            public Action<QtmCommandResult> OnResult { get => onResult; }

            Action<QtmCommandResult> onResult;
            string fileName;
            public Save(string fileName, Action<QtmCommandResult> onResult)
            {
                this.onResult = onResult;
                this.fileName = fileName;
            }
        }

        internal struct TakeControl : ICommand
        {
            static RtCommandCompleteTrigger[] trigger_cache = new RtCommandCompleteTrigger[]
            {
                new RtCommandCompleteTrigger{ resultString = "You are now master", triggerEvent = QTMEvent.None },
            };

            public string CommandString { get => "TakeControl " + password; }
            public RtCommandCompleteTrigger[] Triggers { get => trigger_cache; }
            public Action<QtmCommandResult> OnResult { get => onResult; }

            Action<QtmCommandResult> onResult;
            string password;
            public TakeControl(string password, Action<QtmCommandResult> onResult)
            {
                this.onResult = onResult;
                this.password = password;
            }
        }

        internal struct ReleaseControl : ICommand
        {
            static RtCommandCompleteTrigger[] trigger_cache = new RtCommandCompleteTrigger[]
            {
                new RtCommandCompleteTrigger{ resultString = "You are now a regular client", triggerEvent = QTMEvent.None },
            };

            public string CommandString { get => "releaseControl "; }
            public RtCommandCompleteTrigger[] Triggers { get => trigger_cache; }
            public Action<QtmCommandResult> OnResult { get => onResult; }

            Action<QtmCommandResult> onResult;
            public ReleaseControl(Action<QtmCommandResult> onResult)
            {
                this.onResult = onResult;
            }
        }

    }
}
