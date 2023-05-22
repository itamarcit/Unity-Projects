using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ManegeLevelsLocks : ScriptableObject
{
    public int levelsFinish;

    public void AddLevel()
    {
        levelsFinish++;
    }

    public int GetLevel()
    {
        return levelsFinish;
    }
    
    public void Reset()
    {
        levelsFinish = 1;
    }
}
