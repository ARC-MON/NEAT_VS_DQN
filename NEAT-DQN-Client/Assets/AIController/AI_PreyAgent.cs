using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_PreyAgent : MonoBehaviour
{
    public int hp = 200;
    public void move(int direction)
    {
        switch (direction) 
        {
            case 0:
                transform.position += new Vector3(0, 1, 0);
                hp -= 1;
                break;
            case 1:
                transform.position += new Vector3(1, 0, 0);
                hp -= 1;
                break; 
            case 2:
                transform.position += new Vector3(0, -1, 0);
                hp -= 1;
                break;
            case 3:
                transform.position += new Vector3(1, 0, 0);
                hp -= 1;
                break;
        }
    }
}
