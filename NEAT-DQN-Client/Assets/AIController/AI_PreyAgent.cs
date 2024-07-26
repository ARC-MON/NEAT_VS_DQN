using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AI_PreyAgent : MonoBehaviour
{
    public UnityEngine.UI.Slider hpVisual;
    public float hp = 10;
    private float startingHp = 10;
    private Vector3 myMove;
    public int reward = 0;

    public float detectionRadius = 1.0f;
    public Color gizmoColor = Color.red;

    //Reycast variables
    private Vector2 startingPosition;
    private Vector2 direction;
    private int lengthReycast = 3;
    private RaycastHit2D hit;
    private List<int> _types = new List<int>();
    private List<int> _distances = new List<int>();
    private Vector3 MyGlobalPosition;
    private Vector3 MyParrentPosition;

    private void Start()
    {
        hpVisual.value = hp / startingHp;
    }
    private void Update()
    {
        gateState();
    }
    public bool move(int direction)
    {
        switch (direction) 
        {
            case 0:
                myMove = new Vector3(0, 1, 0);
                transform.position += myMove;
                hp -= 1;
                break;
            case 1:
                myMove = new Vector3(1, 0, 0);
                transform.position += myMove;
                hp -= 1;
                break; 
            case 2:
                myMove = new Vector3(0, -1, 0);
                transform.position += myMove;
                hp -= 1;
                break;
            case 3:
                myMove = new Vector3(-1, 0, 0);
                transform.position += myMove;
                hp -= 1;
                break;
        }

        if(hp > 0)
            hpVisual.value = hp / startingHp;

        return true;
    }
    public bool detect()
    {
        Vector2 detectionCenter = transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(detectionCenter, detectionRadius, ~0);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.tag == "Wall")
            {
                reward = -5;
                transform.position += (-myMove);
            }
            else if (collider.gameObject.tag == "Food")
            {
                hp = startingHp;
                hpVisual.value = hp / startingHp;
                reward = 20;
                Destroy(collider.gameObject);
            }
            
        }

        return true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    public void gateState()
    {
        MyParrentPosition = transform.parent.position;
        MyGlobalPosition = transform.position - MyParrentPosition;

        _types.Clear();
        _distances.Clear();

        startingPosition = MyGlobalPosition + new Vector3(0, 1);
        direction = Vector2.up;

        hit = Physics2D.Raycast(transform.position, direction, lengthReycast);
        Debug.DrawRay(transform.position, direction * lengthReycast, Color.blue);

        if (hit.collider != null)
        {
            _types.Add(whoISee(hit.collider.tag));
            _distances.Add((int)(Vector2.Distance(MyGlobalPosition, hit.transform.position))); //popraw
        }
        else
        {
            _types.Add(0);
            _distances.Add(0);
        }

        Debug.Log(MyGlobalPosition);
    }
    public int whoISee(string tag)
    {
        switch (tag)
        {
            case "Food":
                return 1;
            case "Wall":
                return 2;
            default:
                return 0;
        }
    }

}
