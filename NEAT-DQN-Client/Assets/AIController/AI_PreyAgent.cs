using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AI_PreyAgent : MonoBehaviour
{
    public UnityEngine.UI.Slider hpVisual;
    public float hp = 20;
    private float startingHp = 20;
    private Vector3 myMove;
    public int reward = 0;
    private int who;
    public bool captured = false;

    public float detectionRadius = 0.5f;
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
    private List<Vector3> reyStartingPositions = new List<Vector3> 
    {
        new Vector3(0, 1, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, -1, 0),
        new Vector3(-1, 0, 0)
    };
    private List<Vector2> rayStartingDirections = new List<Vector2>
    {
        Vector2.up,
        Vector2.right,
        Vector2.down,
        Vector2.left
    };

    private void Start()
    {
        hpVisual.value = hp / startingHp;
        captured = false;
    }
    private void Update()
    {
        getState();
    }
    public bool move(int direction)
    {
        switch (direction) 
        {
            case 0:
                myMove = reyStartingPositions[0];
                transform.position += myMove;
                hp -= 1;
                break;
            case 1:
                myMove = reyStartingPositions[1];
                transform.position += myMove;
                hp -= 1;
                break; 
            case 2:
                myMove = reyStartingPositions[2];
                transform.position += myMove;
                hp -= 1;
                break;
            case 3:
                myMove = reyStartingPositions[3];
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
                GameObject.FindWithTag("AI").GetComponent<AI_Controller>().makeFood(1, transform.parent.gameObject);
                Destroy(collider.gameObject);
            }
            else if (collider.gameObject.tag == "Predator")
            {
                captured = true;
            }


        }

        return true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    public (int x, int y, List<int> _types, List<int> _distances, int hp) getState()
    {
        _types.Clear();
        _distances.Clear();

        MyParrentPosition = transform.parent.position;
        MyGlobalPosition = transform.position - MyParrentPosition;

        //Debug.Log("Moja obecna pozycja" + transform.position);

        for (int i = 0; i < 4; i++)
        {
            startingPosition = transform.position + reyStartingPositions[i];
            direction = rayStartingDirections[i];

            hit = Physics2D.Raycast(startingPosition, direction, lengthReycast);
            Debug.DrawRay(startingPosition, direction * lengthReycast, Color.blue);

            if (hit.collider != null)
            {
                who = whoISee(hit.collider.tag);
                _types.Add(who);
                Debug.Log(who);

                //Debug.Log("Collision tag: " + hit.collider.tag);
                if (who == 0)
                    _distances.Add(0);
                else
                    _distances.Add((int)Math.Round(Vector2.Distance(transform.position, hit.point))); //popraw
                //Debug.Log("Dystans do wykrytego" + _distances[_distances.Count - 1]);
            }
            else
            {
                _types.Add(0);
                _distances.Add(0);
            }
        }

        //Debug.Log("Moja pozycja"+MyGlobalPosition);

        return ((int)Math.Round(MyGlobalPosition.x), (int)Math.Round(MyGlobalPosition.y), _types, _distances, (int)hp);
    }
    public int whoISee(string tag)
    {
        switch (tag)
        {
            case "Food":
                return 1;
            case "Wall":
                return 2;
            case "Predator":
                return 3;
            default:
                return 0;
        }
    }

}
