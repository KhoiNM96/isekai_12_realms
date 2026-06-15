using System;

namespace Isekai12Realms.Tutorial
{
    [Serializable]
    public class TutorialStepData
    {
        public string stepId;
        public TutorialTriggerType triggerType;
        public string targetScreen;
        public string targetUiElementName;
        public string message;
        public TutorialHighlightType highlightType;
        public bool pauseGameplay;
        public bool waitForClickTarget;
        public string nextStepId;
    }
}
