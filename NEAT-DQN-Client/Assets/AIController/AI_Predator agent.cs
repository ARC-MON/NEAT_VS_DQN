using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AI_Predatoragent : MonoBehaviour
{
    private List<Vector3> possibleDirections = new List<Vector3>
    {
        new Vector3(0, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, -1, 0),
        new Vector3(-1, 0, 0),
    };
    private List<int> possibleMoves = new List<int>();
    Collider2D[] colliders;
    private int distance;
    private int chosenAction;
    public bool makeMove(GameObject myTarget)
    {
        possibleMoves.Clear();
        
        for (int i = 0; i<4 ; i++) 
        {
            colliders = Physics2D.OverlapCircleAll(transform.position + possibleDirections[i], 0.5f, ~0);
            if (colliders.Length == 0)
                possibleMoves.Add(i);        
        }

        if (possibleMoves.Count > 0)
        {
            if (((int)Math.Round(Vector2.Distance(transform.position, myTarget.transform.position))) > 6)
            {
                chosenAction = possibleMoves[Random.Range(0, possibleMoves.Count)];
            }
            else
            {
                //Przypisz pierwszy dystans jako najmniejszy
                distance = (int)Math.Round(Vector2.Distance(transform.position + possibleDirections[0], myTarget.transform.position));
                chosenAction = 0;
                //SprawdŸ który dystans jest faktycznie najmniejszy
                foreach (var moveNumber in possibleMoves)
                {
                    if (((int)Math.Round(Vector2.Distance(transform.position + possibleDirections[moveNumber], myTarget.transform.position))) < distance)
                    {
                        chosenAction = moveNumber;
                    }
                }
            }      
        }
        else
            chosenAction = 0;

        //Wykonaj ruch z najmniejeszym dystansem
        transform.position += possibleDirections[chosenAction];

        return true;
    }
}
