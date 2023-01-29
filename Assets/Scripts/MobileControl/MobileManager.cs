using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class MobileManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        GameObject.FindGameObjectWithTag(Constants.TAG_MOBILE_PANEL).SetActive(false);
#elif UNITY_ANDROID || UNITY_IOS 
        GameObject.FindGameObjectWithTag(Constants.TAG_MOBILE_PANEL).SetActive(true);
#endif
    }

}
