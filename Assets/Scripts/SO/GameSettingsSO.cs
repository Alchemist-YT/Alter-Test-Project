using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game Data/Settings")]
public class GameSettingsSO : ScriptableObject
{
    [field: SerializeField] public int MaxTurns = 6;
    [field: SerializeField] public int TurnDuration = 30;
}
