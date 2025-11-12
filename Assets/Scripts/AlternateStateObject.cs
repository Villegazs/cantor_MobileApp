using System.Collections.Generic;
using UnityEngine;

public class AlternateStateObject : MonoBehaviour
{
    bool isActive = false;

    public void ToggleObjectState(GameObject obj)
    {
        isActive = !isActive;
        if (obj != null)
        {
            obj.SetActive(isActive);
        }
    }
    

}
