using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AnimationState { Idle, Walk, Run, Attack, Hurt, Die }

public class EnemyAnimator : MonoBehaviour
{
    public float FrameRate = 12f;
    private SpriteRenderer _spriteRenderer;
    
    private Dictionary<AnimationState, Sprite[]> _animations = new Dictionary<AnimationState, Sprite[]>();
    private AnimationState _currentState = AnimationState.Idle;
    private int _monsterID = 1;
    private System.Action _onComplete;
    private Coroutine _animationCoroutine;
    private bool _isLooping = true;

    public void Init(int monsterID, SpriteRenderer renderer)
    {
        _monsterID = monsterID;
        _spriteRenderer = renderer;
        LoadAnimations();
        Play(AnimationState.Walk);
    }

    private void LoadAnimations()
    {
        _animations.Clear();
        _animations[AnimationState.Idle] = LoadSequence("idle");
        _animations[AnimationState.Walk] = LoadSequence("walk");
        _animations[AnimationState.Run] = LoadSequence("run");
        _animations[AnimationState.Attack] = LoadSequence("attack");
        _animations[AnimationState.Hurt] = LoadSequence("hurt");
        _animations[AnimationState.Die] = LoadSequence("die");
    }

    private Sprite[] LoadSequence(string action)
    {
        string folderPath = $"Enemies/Monster{_monsterID}";
        List<Sprite> sprites = new List<Sprite>();
        
        for (int i = 0; i < 20; i++)
        {
            string fileName = $"{_monsterID}_enemies_1_{action}_{i:D3}";
            Sprite s = Resources.Load<Sprite>($"{folderPath}/{fileName}");
            if (s != null) sprites.Add(s);
        }
        
        return sprites.ToArray();
    }

    public void Play(AnimationState state, bool loop = true, System.Action onComplete = null)
    {
        if (_currentState == state && _animationCoroutine != null && _isLooping == loop && _isLooping) return;
        
        _onComplete = onComplete;
        _currentState = state;
        _isLooping = loop;

        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        
        if (_animations.ContainsKey(state) && _animations[state].Length > 0)
        {
            _animationCoroutine = StartCoroutine(Animate(state));
        }
    }

    IEnumerator Animate(AnimationState state)
    {
        Sprite[] frames = _animations[state];
        int frameIndex = 0;
        float waitTime = 1f / FrameRate;

        while (true)
        {
            if (_spriteRenderer != null && frameIndex < frames.Length)
            {
                _spriteRenderer.sprite = frames[frameIndex];
            }

            frameIndex++;
            if (frameIndex >= frames.Length)
            {
                if (_isLooping) frameIndex = 0;
                else
                {
                    _onComplete?.Invoke();
                    break;
                }
            }

            yield return new WaitForSeconds(waitTime);
        }
    }
}
