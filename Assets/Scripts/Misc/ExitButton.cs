using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void ExitToStartScene()
    {
        StartCoroutine(PlayerManager.instance.ExitToStartScene());
    }
}
