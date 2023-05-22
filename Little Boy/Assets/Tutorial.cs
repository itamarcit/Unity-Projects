using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _enemy;
    private enum TutorialTag
    {
        MoveTutorial, SpaceTutorial, EnemyTutorial
    }
    [SerializeField] TutorialTag _tutorialTag;
    private bool _disappear;
    private bool _moveState;
    private bool _spaceState;
    private bool _enemyState;
    
    
    private void Update()
    {
        UpdateTutorialState();
        if (_disappear && CheckTutorialState())
        {
            _spriteRenderer.color = Color.Lerp(_spriteRenderer.color, Color.clear, 0.01f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _disappear = true;
            _moveState = true;
        }
    }

    private void UpdateTutorialState()
    {
        if (Input.GetKey(KeyCode.Space)) _spaceState = true;
        if (_enemy) {
            if (!_enemy.activeSelf) _enemyState = true;
            
        }
    }

    private bool CheckTutorialState()
    {
        if (_tutorialTag.Equals(TutorialTag.MoveTutorial)) return _moveState;
        else if (_tutorialTag.Equals(TutorialTag.SpaceTutorial)) return _spaceState;
        else if (_tutorialTag.Equals(TutorialTag.EnemyTutorial)) return _enemyState;
        return false;
    }
}
