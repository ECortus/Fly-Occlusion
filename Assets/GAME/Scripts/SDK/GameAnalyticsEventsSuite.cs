using Cysharp.Threading.Tasks;
using GameAnalyticsSDK;

namespace AppsFlyerSDK
{
    public static class GameAnalyticsEventsSuite
    {
        public static async void AppInit()
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.NewDesignEvent("APP_SUCCESSFUL_INIT");
        }
        
        public static async void GameReady()
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.StartSession();
        }
        
        public static async void GameEnd()
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.EndSession();
        }
        
        public static async void UserConsentInteraction()
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.NewDesignEvent("SUCCESSFUL_USER_CONSENT_ACCEPT");
        }
        
        public static async void FirstInteraction(string name)
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.NewDesignEvent("FIRST_INTERACTION:" + name);
        }
        
        public static async void EngagementWithCore(string name)
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.NewDesignEvent("ENGAGEMENT:" + name);
        }
        
        public static async void LevelProgressionStart()
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "0 m");
        }
        
        public static async void LevelProgressionComplete(string level)
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, level);
        }
        
        public static async void AdInteractionBanner(string name, string placement)
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Banner, "adMod", name + "_" + placement);
        }
        
        public static async void AdInteractionInterstitial(string name)
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Interstitial, "adMod", $"{name}_Fullscreen");
        }
        
        public static async void AdInteractionRewardedVideo(string name)
        {
            await UniTask.WaitUntil(() => GameAnalytics.Initialized);
            GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.RewardedVideo, "adMod", $"{name}_Fullscreen");
        }
    }
}