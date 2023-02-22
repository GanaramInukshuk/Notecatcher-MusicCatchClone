using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour {

    /// <summary>
    /// As per the rules of the original MusicCatch games, there are four note types:
    /// <para>Normal: catching this will reward the player points.</para>
    /// <para>Yellow: catching this will increment the player's point multiplier by 1.</para>
    /// <para>Red: catching this will decrement the player's point multiplier by a random amount.</para>
    /// <para>Purple: catching this will attract all notes (but red) towards the player.</para>
    /// <para>White: catching this note will turn all notes yellow.</para>
    /// </summary>
    public enum NoteType { Normal, Yellow, Red, Purple, White };

    /// <summary>
    /// For coding purposes, a note will be in one of several states:
    /// <para>Waiting: notes in the object pool are waiting to be spawned.</para>
    /// <para>Idle: notes are newly spawned, but their animation coroutine isn't called.</para>
    /// <para>Active: notes that are spawned and are running their animation coroutine.</para>
    /// <para>Following: these notes are in this state if the player touches the purple note; they follow the player.</para>
    /// <para>Ejected: notes in this state are spawned because the player touched a red note; they fly away from the player.</para>
    /// </summary>
    public enum NoteState { Waiting, Idle, Active, Following, Ejected };

    public NoteState CurrentState { get { return _state; }}

    // An array of sprites is used, and one will be randomly selected.
    public Sprite[] sprites;

    public NoteType _noteType = NoteType.Normal;

    private NoteState _state = NoteState.Waiting;

    private const float _constFallAccelerationFactor = -12f;        // SET TO NEGATIVE FOR FALLING DOWN
    private const float _constDefaultAnimationTime = 4f;
    private const float _constDefaultNewNoteHue = 0.4f;
    private const float _constDefaultOldNoteHue = 0.666f;


    private Coroutine _currentCoroutine;


    private void Awake() {
        gameObject.SetActive(false);
        _state = NoteState.Waiting;
    }


    private void OnEnable() {
        _state = NoteState.Idle;
    }

    private void OnDisable() {
        _state = NoteState.Waiting;
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);
    }

    public void ContactPlayer() {

        if (_state != NoteState.Active) return;


        Debug.Log("[NoteController]: Note contacted player. Stopping animation coroutine.");

        // Do something based on what your powerup type is

        // Set self to false for set active
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Launches a single note at a given location. All other stats about the note (rotation rate, powerup type, etc) are randomly generated.
    /// </summary>
    /// <param name="spawnLocation"></param>
    public void StartAnimation(Vector2 spawnLocation) {

        // Use an RNG to determine what color type to be
        // One in 20 notes are red (15 out of 300)
        // One in 50 notes are yellow (6 out of 300)
        // One in 100 notes are purple (3 out of 300)
        // One in 300 notes are white
        int rand = UnityEngine.Random.Range(0, 300);
        if (rand == 0) _noteType = NoteType.White;
        else if (rand < 3) _noteType = NoteType.Purple;
        else if (rand < 6) _noteType = NoteType.Yellow;
        else if (rand < 15) _noteType = NoteType.Red;
        else _noteType = NoteType.Normal;

        // Set the note's color and sprite
        // The sprite doesn't do anything but add variation to the note types
        // The color determines whether the note is a powerup (and what kind) or a regular note
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color color;
        switch (_noteType) {
            case NoteType.Normal: color = Color.cyan; break;
            case NoteType.Yellow: color = Color.yellow; break;
            case NoteType.Red: color = Color.red; break;
            case NoteType.Purple: color = new Color(0.6f, 0, 1); break;
            default: color = Color.white; break;
        }
        sr.color = color;
        sr.sprite = sprites[UnityEngine.Random.Range(0, sprites.Length)];

        if (_state == NoteState.Waiting || _state == NoteState.Idle) {
            gameObject.SetActive(true);
            _state = NoteState.Active;
            _currentCoroutine = StartCoroutine(AnimationCoroutine(spawnLocation, NoteType.Normal, Vector2.up * (30.0f + UnityEngine.Random.Range(-5f, 5f))));

        }
    }

    private IEnumerator EjectedAnimationCoroutine(Vector2 spawnLocation, NoteType noteType, Vector2 launchVelocity) {
        Debug.Log("[NoteController]: Ejected coroutine started.");

        // Countdown timer
        float countdownTimer = _constDefaultAnimationTime;

        // For launch and fall
        Vector2 currentPosition = spawnLocation;
        Vector2 currentVelocity = launchVelocity;

        // For random rotation
        Vector3 rotationRate = new Vector3(0.0f, 0.0f, UnityEngine.Random.Range(-180, 180));

        // For random initial size and shrinking
        float scaleFactor = UnityEngine.Random.Range(0.9f, 1.5f);

        while (!Mathf.Approximately(countdownTimer, 0.0f)) {
            // Update position and velocity
            currentPosition += currentVelocity * Time.deltaTime;
            gameObject.transform.localPosition = currentPosition;

            // Update rotation
            gameObject.transform.eulerAngles += rotationRate * Time.deltaTime;

            // Update countdown
            countdownTimer = Mathf.Max(0f, countdownTimer - Time.deltaTime);

            // Update size
            float percentageLeft = countdownTimer / _constDefaultAnimationTime;
            gameObject.transform.localScale = Vector3.one * scaleFactor * percentageLeft;

            // Update color
            // This is only if the note type is normal
            // Interpolate between the default colors by hue
            if (_noteType == NoteType.Normal) {
                float hue = Mathf.Lerp(_constDefaultNewNoteHue, _constDefaultOldNoteHue, 1.0f - percentageLeft);
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                sr.color = Color.HSVToRGB(hue, 1.0f, 1.0f);
            }

            yield return null;
        }

        gameObject.SetActive(false);
        Debug.Log("[NoteController]: Animation coroutine ended.");
    }

    private IEnumerator AnimationCoroutine(Vector2 spawnLocation, NoteType noteType, Vector2 launchVelocity) {
        Debug.Log("[NoteController]: Animation coroutine started.");

        // Countdown timer
        float countdownTimer = _constDefaultAnimationTime;

        // For launch and fall
        Vector2 currentPosition = spawnLocation;
        Vector2 currentVelocity = launchVelocity;
        Vector2 fallAcceleration = Vector2.up * _constFallAccelerationFactor;

        // For random rotation
        Vector3 rotationRate = new Vector3(0.0f, 0.0f, UnityEngine.Random.Range(-180, 180));

        // For random initial size and shrinking
        float scaleFactor = UnityEngine.Random.Range(0.9f, 1.5f);

        while (!Mathf.Approximately(countdownTimer, 0.0f)) {
            // Update position and velocity
            currentPosition += currentVelocity * Time.deltaTime;
            currentVelocity += fallAcceleration * Time.deltaTime;
            gameObject.transform.localPosition = currentPosition;

            // Update rotation
            gameObject.transform.eulerAngles += rotationRate * Time.deltaTime;

            // Update countdown
            countdownTimer = Mathf.Max(0f, countdownTimer - Time.deltaTime);

            // Update size
            float percentageLeft = countdownTimer / _constDefaultAnimationTime;
            gameObject.transform.localScale = Vector3.one * scaleFactor * percentageLeft;

            // Update color
            // This is only if the note type is normal
            // Interpolate between the default colors by hue
            if (_noteType == NoteType.Normal) {
                float hue = Mathf.Lerp(_constDefaultNewNoteHue, _constDefaultOldNoteHue, 1.0f - percentageLeft);
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                sr.color = Color.HSVToRGB(hue, 1.0f, 1.0f);
            }

            yield return null;
        }

        gameObject.SetActive(false);
        Debug.Log("[NoteController]: Animation coroutine ended.");
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.gameObject.CompareTag("Player")) {
            ContactPlayer();
        }
    }

    private void FixedUpdate() {
        //// Decrement time remaining and use that as a percentage for how big the size is
        //_timeRemaining -= Time.fixedDeltaTime;
        
        //float percentage = _timeRemaining / time;
        //initialSize = percentage;

        //// Destroy the game object if it's zero; otherwise, change the game object's transform
        //if (Mathf.Approximately(initialSize, 0.0f) || initialSize < 0.0f) {
        //    Destroy(gameObject);
        //} else {
        //    _currentVelocity.y -= 0.02f;

        //    gameObject.transform.eulerAngles   += noteRotationRate;             // This governs rotation of the sprite, not which way its up vector is pointing
        //    gameObject.transform.localPosition += (Vector3)_currentVelocity;
        //    gameObject.transform.localScale =  new Vector2(percentage, percentage);
        //}
    }
}