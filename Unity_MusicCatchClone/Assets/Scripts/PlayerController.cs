using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Sprite playerSprite;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move the player sprite to wherever the cursor is
        gameObject.transform.position = CursorToWorldPosition();
    }

    private Vector2 CursorToWorldPosition() {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        
    }
}
