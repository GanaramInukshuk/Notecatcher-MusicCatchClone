using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public float spawnerRotationRate = 5f;
    public int poolSize = 10;
    public GameObject objectAsPrefab;

    private NoteController[] _notes;

    private int _indexOfLastSpawnedNote = 0;
    private float _timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Create an empty array of gameobjects (the music notes)
        _notes = new NoteController[poolSize];

        for (int i = 0; i < poolSize; i++) {
            // Create an instnce of the gameobject prefab
            GameObject prefabInstance = Instantiate(objectAsPrefab, new Vector2(0, 0), Quaternion.identity);

            // Get a reference to the transform to set this to be its parent
            Transform transformReference = prefabInstance.GetComponent<Transform>();
            transformReference.SetParent(gameObject.transform);

            // Save a reference to the note's note controller
            NoteController noteController = prefabInstance.GetComponent<NoteController>();
            _notes[i] = noteController;
        }
    }
    public void SpawnNote(int amount = 1) {

        for (int i = 0; i < amount; i++) {
            NoteController note = GetNextAvailableNote();
            if (note != null) {
                note.StartAnimation(new Vector2(UnityEngine.Random.Range(-40f, 40f), -30f));
            } else {
                Debug.Log("[NoteSpawner]: Object pool is empty.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0f, 0f, 1f) * Time.deltaTime * spawnerRotationRate;

        _timer += Time.deltaTime;

        // Spawn some note every second
        if (_timer >= .5f) {
            SpawnNote(UnityEngine.Random.Range(5, 15));
            _timer = 0;
        }
    }

    /// <summary>
    /// Gets the index of the next available note.
    /// </summary>
    private NoteController GetNextAvailableNote() {

        int prevIndex = _indexOfLastSpawnedNote;
        NoteController noteToReturn = null;


        // Iterate through the entire array of note controllers using the _currentNoteIndex as the start
        for (int i = prevIndex; i < _notes.Length + prevIndex; i++) {
            int current_index = i % _notes.Length;

            NoteController current_note = _notes[current_index];
            if (current_note.CurrentState == NoteController.NoteState.Waiting) {
                noteToReturn = current_note;
                _indexOfLastSpawnedNote = current_index;
                break;
            }
        }

        return noteToReturn;
    }
}
