using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cheats : MonoBehaviour
{
    public void PlusGold()
    {
        Gold.Instance.Plus(50000);
    }
    
    public void PlusGem()
    {
        Gem.Instance.Plus(50000);
    }
}
