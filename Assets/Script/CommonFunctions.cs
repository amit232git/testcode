using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using Chartboost;

public class CommonFunctions : MonoBehaviour {
	

	private static readonly Dictionary<string, string> appIds = new Dictionary<string, string>() {
		{ "IOS", "52f9be8bfaf6e9854e0001ef" }
	};
	
	private static RevMob revmob;
	public static void initAds()
	{
		if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			AdMobBinding.init("a152f9bf24b2056");
			revmob = RevMob.Start(appIds);
			CBBinding.init("52f9bf78f8975c1cf9e83137","df9c5eca533490ee18934618539c3396e9633cbf");
		}
	}
	
	public static void showBannerAds()
	{
		if(Application.platform == RuntimePlatform.IPhonePlayer) {
			AdMobBinding.createBanner(AdMobBannerType.SmartBannerLandscape, AdMobAdPosition.BottomCenter);
		} 	
	}
	
	public static void hideBannerAds()
	{
		if(Application.platform == RuntimePlatform.IPhonePlayer) {
			AdMobBinding.destroyBanner();
		} 	
	}
	public static int adTurn = 0;
	public static void showTurnAds()
	{
		if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if(adTurn == 0)
			{
				adTurn = 1;
				revmob.ShowFullscreen();
				CBBinding.cacheInterstitial(null);
			}
			else
			{
				adTurn = 0;
				CBBinding.showInterstitial(null);
				revmob.CreateFullscreen();
			}
		}
	}
	
	public static bool CheckInternetConnection()
	{
		bool isConnectedToInternet = false;
		
		if(Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
			iPhoneSettings.internetReachability == iPhoneNetworkReachability.ReachableViaWiFiNetwork) {
			isConnectedToInternet = true;
		}
		
		return isConnectedToInternet;
	}
}
