using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AppsFlyerSDK
{
    public static class AppsFlyerEventsSuite
    {
        public static void AF_COMPLETE_REGISTRATION(string method)
        {
            Dictionary<string, string> eventParameters0 = new Dictionary<string, string>();
            eventParameters0.Add(AFInAppEvents.REGSITRATION_METHOD, method); // Type of signup method
            AppsFlyer.sendEvent(AFInAppEvents.COMPLETE_REGISTRATION, eventParameters0);
        }

        public static void AF_LOGIN()
        {
            AppsFlyer.sendEvent(AFInAppEvents.LOGIN, null);
        }

        public static void AF_SEARCH(string term, string list)
        {
            Dictionary<string, string> eventParameters2 = new Dictionary<string, string>();
            eventParameters2.Add(AFInAppEvents.SEARCH_STRING, term); // Search term
            eventParameters2.Add(AFInAppEvents.CONTENT_LIST, list); // List of content IDs
            AppsFlyer.sendEvent(AFInAppEvents.SEARCH, eventParameters2);
        }

        public static void AF_CONTENT_VIEW(string price, string id, string category, string currency)
        {
            Dictionary<string, string> eventParameters3 = new Dictionary<string, string>();
            eventParameters3.Add(AFInAppEvents.PRICE, price); // Product price
            eventParameters3.Add("af_content", id); // International Article Number (EAN) when applicable, or other product or content identifier
            eventParameters3.Add(AFInAppEvents.CONTENT_ID, id); // Product ID
            eventParameters3.Add(AFInAppEvents.CONTENT_TYPE, category); // Product category
            eventParameters3.Add(AFInAppEvents.CURRENCY, currency); // Currency in the product details page
            AppsFlyer.sendEvent(AFInAppEvents.CONTENT_VIEW, eventParameters3);
        }

        public static void AF_LIST_VIEW(string type, string list)
        {
            Dictionary<string, string> eventParameters4 = new Dictionary<string, string>();
            eventParameters4.Add(AFInAppEvents.CONTENT_TYPE, type); // Type of list
            eventParameters4.Add(AFInAppEvents.CONTENT_LIST, list); // List of content IDs from the category
            AppsFlyer.sendEvent("", eventParameters4);
        }

        public static void AD_ADD_TO_WISHLIST(string price, string id, string category)
        {
            Dictionary<string, string> eventParameters5 = new Dictionary<string, string>();
            eventParameters5.Add(AFInAppEvents.PRICE, price); // Price of the product
            eventParameters5.Add(AFInAppEvents.CONTENT_ID, id); // Product ID
            eventParameters5.Add(AFInAppEvents.CONTENT_TYPE, category); // Product category
            AppsFlyer.sendEvent(AFInAppEvents.ADD_TO_WISH_LIST, eventParameters5);
        }

        public static void AF_ADD_TO_CART(string price, string id, string category, string currency, string quantity)
        {
            Dictionary<string, string> eventParameters6 = new Dictionary<string, string>();
            eventParameters6.Add(AFInAppEvents.PRICE, price); // Product price
            eventParameters6.Add("af_content", id); // International Article Number (EAN) when applicable, or other product or content identifier
            eventParameters6.Add(AFInAppEvents.CONTENT_ID, id); // Product ID
            eventParameters6.Add(AFInAppEvents.CONTENT_TYPE, category); // Product type
            eventParameters6.Add(AFInAppEvents.CURRENCY, currency); // Product currency
            eventParameters6.Add(AFInAppEvents.QUANTITY, quantity); // How many items of the same product were added to the cart
            AppsFlyer.sendEvent(AFInAppEvents.ADD_TO_CART, eventParameters6);
        }

        public static void AF_INITIATED_CHECKOUT(string price, string id, string category, string currency, string quantity)
        {
            Dictionary<string, string> eventParameters7 = new Dictionary<string, string>();
            eventParameters7.Add(AFInAppEvents.PRICE, price); // Total price in the cart
            eventParameters7.Add(AFInAppEvents.CONTENT_ID, id); // ID of products in the cart
            eventParameters7.Add(AFInAppEvents.CONTENT_TYPE, category); // List of product categories
            eventParameters7.Add(AFInAppEvents.CURRENCY, currency); // Currency during time of checkout
            eventParameters7.Add(AFInAppEvents.QUANTITY, quantity); // Total number of items in the cart
            AppsFlyer.sendEvent(AFInAppEvents.INITIATED_CHECKOUT, eventParameters7);
        }

        public static void AF_PURCHASE(string revenue, string price, string id, string category, string currency, string quantity, string orderID)
        {
            Dictionary<string, string> eventParameters8 = new Dictionary<string, string>();
            eventParameters8.Add(AFInAppEvents.REVENUE, revenue); // Estimated revenue from the purchase. The revenue value should not contain comma separators, currency, special characters, or text.
            eventParameters8.Add(AFInAppEvents.PRICE, price); // Overall purchase sum
            eventParameters8.Add("af_content", id); // International Article Number (EAN) when applicable, or other product or content identifier
            eventParameters8.Add(AFInAppEvents.CONTENT_ID, id); // Item ID
            eventParameters8.Add(AFInAppEvents.CONTENT_TYPE, category); // Item category
            eventParameters8.Add(AFInAppEvents.CURRENCY, currency); // Currency code
            eventParameters8.Add(AFInAppEvents.QUANTITY, quantity); // Number of items in the cart
            eventParameters8.Add("af_order_id", orderID); // ID of the order that is generated after the purchase
            eventParameters8.Add(AFInAppEvents.RECEIPT_ID, orderID); // Order ID
            AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventParameters8);
        }

        public static void AF_FIRST_PURCHASE(string revenue, string price, string id, string category, string currency, string quantity, string orderID)
        {
            Dictionary<string, string> eventParameters9 = new Dictionary<string, string>();
            eventParameters9.Add(AFInAppEvents.REVENUE, revenue); // Revenue from purchase
            eventParameters9.Add(AFInAppEvents.PRICE, price); // Overall purchase sum
            eventParameters9.Add("af_content", id); // International Article Number (EAN) when applicable, or other product or content identifier
            eventParameters9.Add(AFInAppEvents.CONTENT_ID, id); // Item ID
            eventParameters9.Add(AFInAppEvents.CONTENT_TYPE, category); // Item category
            eventParameters9.Add(AFInAppEvents.CURRENCY, currency); // Currency
            eventParameters9.Add(AFInAppEvents.QUANTITY, quantity); // Quantity of items in the cart
            eventParameters9.Add("af_order_id", orderID); // ID of the order that is generated after the purchase
            eventParameters9.Add(AFInAppEvents.RECEIPT_ID, orderID); // Order ID
            AppsFlyer.sendEvent("first_purchase", eventParameters9);
        }

        public static void AF_REMOVE_FROM_CART(string id, string category)
        {
            Dictionary<string, string> eventParameters10 = new Dictionary<string, string>();
            eventParameters10.Add(AFInAppEvents.CONTENT_ID, id); // Item or product ID
            eventParameters10.Add(AFInAppEvents.CONTENT_TYPE, category); // Item or product category
            AppsFlyer.sendEvent("remove_from_cart", eventParameters10);
        }

        public static void AF_LEVEL_ACHIEVED(string level, string score)
        {
            Dictionary<string, string> eventParameters3 = new Dictionary<string, string>();
            eventParameters3.Add(AFInAppEvents.LEVEL, level); // Level the user achieved
            eventParameters3.Add(AFInAppEvents.SCORE, score); // Score associated with user's achievement
            AppsFlyer.sendEvent(AFInAppEvents.LEVEL_ACHIEVED, eventParameters3);
        }

        public static void AF_TUTORIAL_COMPLETION(string level, string tutorialID, string tutorialName)
        {
            Dictionary<string, string> eventParameters4 = new Dictionary<string, string>();
            eventParameters4.Add(AFInAppEvents.SUCCESS, level); // Whether the user completed the tutorial
            eventParameters4.Add("af_tutorial_id", tutorialID); // Tutorial ID
            eventParameters4.Add("af_content", tutorialName); // Tutorial name
            AppsFlyer.sendEvent(AFInAppEvents.TUTORIAL_COMPLETION, eventParameters4);
        }

        public static void AF_SHARE(string reason, string platform)
        {
            Dictionary<string, string> eventParameters5 = new Dictionary<string, string>();
            eventParameters5.Add(AFInAppEvents.DESCRIPTION, reason); // Reason for sharing on social media, for example, a new high score or leveling up
            eventParameters5.Add("platform", platform); // Platform used to post shares
            AppsFlyer.sendEvent(AFInAppEvents.SHARE, eventParameters5);
        }

        public static void AF_INVITE(string context)
        {
            Dictionary<string, string> eventParameters6 = new Dictionary<string, string>();
            eventParameters6.Add(AFInAppEvents.DESCRIPTION, context); // Context of invitation
            AppsFlyer.sendEvent(AFInAppEvents.INVITE, eventParameters6);
        }

        public static void AF_BONUS_CLAIMED(string bonusType)
        {
            Dictionary<string, string> eventParameters7 = new Dictionary<string, string>();
            eventParameters7.Add(bonusType, ""); // Type of bonus user claims
            AppsFlyer.sendEvent("bonus_claimed", eventParameters7);
        }
        
        public static void AF_AD_REVENUE(string country, string id, string category, string placement, string payload = "")
        {
            Dictionary<string, string> eventParameters3 = new Dictionary<string, string>();
            eventParameters3.Add("country", country);
            eventParameters3.Add("ad_unit", id);
            eventParameters3.Add("ad_type", category);
            eventParameters3.Add("placement", placement);
            eventParameters3.Add("ecpm_payload", payload);
            AppsFlyer.sendEvent("AD_REVENUE", eventParameters3);
        }
    }
}
