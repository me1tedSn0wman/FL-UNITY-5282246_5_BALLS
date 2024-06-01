using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using YG;

public class LeaderboardTry : MonoBehaviour
{
    [SerializeField] LeaderboardYG leaderboardYG;

    public void AddScore(int newHighScore) {
        leaderboardYG.NewScore(newHighScore);
    }

    public void OnEnable()
    {
        leaderboardYG.UpdateLB();
    }
}
